using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NaughtyAttributes;
using SimpleFileBrowser;
using System.IO;
using TMPro;

public class ImportManager : SingletonMonoBehaviour<ImportManager>
{
    public Animator errorAnim;
    public TextMeshProUGUI errorMesh;

    private const string importErrorAlert = "<color=#FF0000>IMPORT ERROR";
    private const string notEnoughQsAlert = "<color=#FF8D00>DATA SHEET CONTAINED {0} QUESTIONS\nBESPOKE WALLS MUST CONTAIN EXACTLY 25 QUESTIONS";
    private const string importSuccessful = "<color=#00FF00>{0} {1} ADDED TO DATABASE";
    private const string noNewQs = "<color=#FF8D00>IMPORTED DATA CONTAINED NO NEW QUESTIONS";
    private const string wallImportSuccessful = "<color=#00FF00>BESPOKE WALL LOADED\nREADY TO PLAY";

    public void OnClickImportQuestions(bool bespokeWall)
    {
        SetFiltersToAsset();
        StartCoroutine(ShowLoadDialogCoroutine(bespokeWall));
    }
    public void SetFiltersToAsset()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Question Sheet", ".csv"));
        FileBrowser.SetDefaultFilter(".csv");
    }
    public IEnumerator ShowLoadDialogCoroutine(bool bespokeWall)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.dataPath, null, "Import Question Data", "Import");
        try
        {
            if (FileBrowser.Success)
                ImportQuestions(File.ReadAllText(FileBrowser.Result.FirstOrDefault()), bespokeWall);


            (MainMenuManager.Get.GetDBMan() as DatabaseManager).BuildQuestionObjects();
            MainMenuManager.Get.OnClickMenuButton(MainMenuManager.ButtonType.Home);
        }
        catch (Exception ex)
        {
            TriggerAlert(importErrorAlert);
            DebugLog.Print(ex.Message, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            MainMenuManager.Get.OnClickMenuButton(MainMenuManager.ButtonType.Home);
        }
        (MainMenuManager.Get.GetDBMan() as DatabaseManager).UpdateReadoutMesh();
    }

    private void ImportQuestions(string csv, bool bespokeWall)
    {
        Question[] qs = CSVSerializer.Deserialize<Question>(csv);

        if (!bespokeWall)
        {
            List<Question> filteredQs = qs.Where(x => !QuestionDatabase.Questions.Any(y => y.question == x.question)).ToList();
            foreach (Question q in filteredQs)
                QuestionDatabase.Questions.Add(q);

            if(filteredQs.Count == 0)
                TriggerAlert(noNewQs);
            else
                TriggerAlert(string.Format(importSuccessful, filteredQs.Count, filteredQs.Count == 1 ? "QUESTION" : "QUESTIONS"));

            PersistenceManager.WriteQuestionDatabase();
        }
        else
        {
            if (qs.Length != 25)
                TriggerAlert(string.Format(notEnoughQsAlert, qs.Length));
            else
            {
                List<Question> filteredQs = qs.ToList();
                QuestionManager.LoadPack(filteredQs, true);
                MainMenuManager.Get.ToggleMenu();
                TriggerAlert(string.Format(wallImportSuccessful));
            }
        }
    }

    public void TriggerAlert(string message)
    {
        errorMesh.text = message;
        errorAnim.SetTrigger("alert");
    }    
}
