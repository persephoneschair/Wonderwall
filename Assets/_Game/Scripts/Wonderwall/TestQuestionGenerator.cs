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
    public Pack activePack = new Pack();

    public string webhookRequest = "https://opentdb.com/api.php?amount=50&category=9";

    public bool useRaw;
    [TextArea(5,20)] public string rawJson;

    [Button]
    public void DownloadAWall()
    {
        activePack.questions.Clear();
        if (!useRaw)
            StartCoroutine(DownloadWall(webhookRequest));
        else
            ProcessRaw();
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
            activePack.questions.Add(q);

            if (activePack.questions.Count == requiredQuestions)
                break;
        }
        if (activePack.questions.Count == requiredQuestions)
        {
            DebugLog.Print($"{activePack.questions.Count} questions downloaded! Wall is ready...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
            QuestionManager.LoadPack(activePack);
        }
            
        else
        {
            DebugLog.Print($"{activePack.questions.Count} questions downloaded - finding more...", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
            Invoke("RecursiveDownload", 5f);
        }            
    }

    private void ProcessRaw()
    {
        JsonConvert.PopulateObject(rawJson, activePack);
        if(activePack.questions.Count == requiredQuestions)
        {
            DebugLog.Print($"{activePack.questions.Count} questions loaded! Wall is ready...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
            QuestionManager.LoadPack(activePack);
        }
        else
            DebugLog.Print($"Hmmm...something went wrong...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
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
            activePack.questions.Any(x => x.correctAnswer == r.correct_answer) ||
            activePack.questions.Any(x => x.incorrectAnswer == r.correct_answer) ||
            activePack.questions.Any(x => x.correctAnswer == r.incorrect_answers.FirstOrDefault()) ||
            activePack.questions.Any(x => x.incorrectAnswer == r.incorrect_answers.FirstOrDefault()) ||
            r.question.Contains("following") || r.question.Contains("these"))
            return true;
        else
            return false;
    }
}
