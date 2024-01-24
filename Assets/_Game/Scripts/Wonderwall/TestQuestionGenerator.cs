using NaughtyAttributes;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

public class TestQuestionGenerator : SingletonMonoBehaviour<TestQuestionGenerator>
{
    public int requiredQuestions = 25;
    public Pack downloadedPack = new Pack();

    public string webhookRequest = "https://opentdb.com/api.php?amount=50&category=9";
    [Button]
    public void DownloadAWall()
    {
        downloadedPack.questions.Clear();
        StartCoroutine(DownloadWall(webhookRequest));
    }

    IEnumerator DownloadWall(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    ProcessDownload(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    private void ProcessDownload(string json)
    {
        OTDBObj otdb = new OTDBObj();
        JsonConvert.PopulateObject(json, otdb);
        foreach (Result r in otdb.results)
        {
            if (IsGarbage(r))
                continue;

            Question q = new Question(HttpUtility.HtmlDecode(r.question),
                HttpUtility.HtmlDecode(r.correct_answer),
                HttpUtility.HtmlDecode(r.incorrect_answers.FirstOrDefault()));
            downloadedPack.questions.Add(q);

            if (downloadedPack.questions.Count == requiredQuestions)
                break;
        }
        if (downloadedPack.questions.Count == requiredQuestions)
        {
            DebugLog.Print($"{downloadedPack.questions.Count} questions downloaded! Wall is ready...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
            QuestionManager.LoadPack(downloadedPack);
        }
            
        else
        {
            DebugLog.Print($"{downloadedPack.questions.Count} questions downloaded - finding more...", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
            Invoke("RecursiveDownload", 5f);
        }            
    }

    private void RecursiveDownload()
    {
        StartCoroutine(DownloadWall(webhookRequest));
    }

    private bool IsGarbage(Result r)
    {
        if (r.correct_answer.ToLowerInvariant() == "true" ||
            r.correct_answer.ToLowerInvariant() == "false" ||
            r.correct_answer.Length > 20 ||
            r.incorrect_answers.FirstOrDefault().Length > 20 ||
            downloadedPack.questions.Any(x => x.correctAnswer == r.correct_answer) ||
            downloadedPack.questions.Any(x => x.incorrectAnswer == r.correct_answer) ||
            downloadedPack.questions.Any(x => x.correctAnswer == r.incorrect_answers.FirstOrDefault()) ||
            downloadedPack.questions.Any(x => x.incorrectAnswer == r.incorrect_answers.FirstOrDefault()) ||
            r.question.Contains("following") || r.question.Contains("these"))
            return true;
        else
            return false;
    }
}
