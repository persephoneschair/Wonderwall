using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class DatabaseManager : SubMenuManager
{
    public TMP_Dropdown profileDropdown;
    public TMP_InputField profileNameInput;
    public Button deleteButton;

    public RectTransform mainWindowRT;

    public GameObject editQToInstance;
    public Transform instanceTransformTarget;

    public List<EditQuestionObj> editQuestionsList;

    public Toggle playWithUnusedToggle;
    public Toggle playWithUsedToggle;

    public TextMeshProUGUI databaseReadoutMesh;
    private const string databaseReadout =
        "<color=green>Unused Questions: {0}\n" +
        "<color=red>Used Questions: {1}\n" +
        "<color=yellow>Total Questions: {2}\n" +
        "<color=#00FFFF>Available Questions: {3}";

    private bool activated = false;

    public override void OnRoomConnected()
    {
        activated = true;
        base.OnRoomConnected();
        BuildDropdown();
        mainWindowRT.localPosition = new Vector3(0, -2500, 0);
    }

    public void BuildQuestionObjects()
    {
        foreach (EditQuestionObj obj in editQuestionsList)
            Destroy(obj.gameObject);
        editQuestionsList.Clear();

        foreach(Question q in QuestionDatabase.Questions)
        {
            var x = Instantiate(editQToInstance, instanceTransformTarget);
            var y = x.GetComponent<EditQuestionObj>();
            y.containedQuestion = q;
            y.questionInput.text = q.question;
            y.correctInput.text = q.correctAnswer;
            y.incorrectInput.text = q.incorrectAnswer;
            editQuestionsList.Add(y);
        }
    }

    public void SetProfileSpecifics()
    {
        foreach(EditQuestionObj eqo in editQuestionsList)
            eqo.usedToggle.isOn = PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Contains(eqo.containedQuestion.ID);
    }

    public void BuildDropdown()
    {
        if (!activated)
            return;
        var cdp = PersistenceManager.CurrentDatabaseProfile;
        profileDropdown.ClearOptions();
        profileDropdown.AddOptions(PersistenceManager.storedDatabaseProfiles.Select(x => x.ProfileName).ToList());
        int index = Array.IndexOf(PersistenceManager.storedDatabaseProfiles.ToArray(), cdp);
        profileDropdown.value = index;
        OnChangeProfile(index);
    }

    public void OnChangeProfile(int index)
    {
        PersistenceManager.CurrentDatabaseProfile = PersistenceManager.storedDatabaseProfiles[index];
        ApplyValues();
        UpdateReadoutMesh();
    }
    public void OnChangeProfileName(string s)
    {
        PersistenceManager.CurrentDatabaseProfile.ProfileName = s;
    }

    public void UpdateReadoutMesh()
    {

        databaseReadoutMesh.text = string.Format(databaseReadout,
            QuestionDatabase.Questions.Count(x => !PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Contains(x.ID)),
            QuestionDatabase.Questions.Count(x => PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Contains(x.ID)),
            QuestionDatabase.Questions.Count(),
            GetAvailableQCount());
    }

    public int GetAvailableQCount()
    {
        var cdp = PersistenceManager.CurrentDatabaseProfile;
        if (cdp.PlayWithUsed && cdp.PlayWithUnused)
            return QuestionDatabase.Questions.Count();
        else if (cdp.PlayWithUsed)
            return QuestionDatabase.Questions.Count(x => PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Contains(x.ID));
        else if (cdp.PlayWithUnused)
            return QuestionDatabase.Questions.Count(x => !PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Contains(x.ID));
        else
            return 0;
    }

    public void OnClearFlags()
    {
        foreach (Question q in QuestionDatabase.Questions)
            PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Remove(q.ID);
        ApplyValues();
        UpdateReadoutMesh();
    }

    public void OnDeleteProfile()
    {
        PersistenceManager.OnDeleteProfile();
        BuildDropdown();
    }

    public void OnDeleteQuestion(Question q)
    {
        var y = editQuestionsList.FirstOrDefault(x => x.containedQuestion == q);
        Destroy(y.gameObject);
        editQuestionsList.Remove(y);
        BuildQuestionObjects();
        SetProfileSpecifics();
        UpdateReadoutMesh();
    }

    private void ApplyValues()
    {
        var cdp = PersistenceManager.CurrentDatabaseProfile;
        profileNameInput.text = cdp.ProfileName;
        playWithUnusedToggle.isOn = cdp.PlayWithUnused;
        playWithUsedToggle.isOn = cdp.PlayWithUsed;
        SetProfileSpecifics();
    }

    private void Update()
    {

        if (PersistenceManager.CurrentDatabaseProfile != null)
        {
            var cgc = PersistenceManager.CurrentDatabaseProfile;

            if (cgc.LockConfig)
            {
                profileNameInput.interactable = false;
                deleteButton.interactable = false;
            }
            else
            {
                profileNameInput.interactable = true;
                deleteButton.interactable = true;
            }
        }
    }

    public void OnToggle(int toggleRef)
    {
        if (toggleRef == 0)
            PersistenceManager.CurrentDatabaseProfile.PlayWithUnused = playWithUnusedToggle.isOn;
        else if (toggleRef == 1)
            PersistenceManager.CurrentDatabaseProfile.PlayWithUsed = playWithUsedToggle.isOn;

        UpdateReadoutMesh();
    }

    public void OnNewProfile()
    {
        var def = new DatabaseProfile()
        {
            ProfileName = "New Profile",
        };
        PersistenceManager.storedDatabaseProfiles.Add(def);
        PersistenceManager.CurrentDatabaseProfile = def;
        BuildDropdown();
    }

    public void OnCloseMenu()
    {
        PersistenceManager.WriteDatabaseProfiles();
        PersistenceManager.WriteQuestionDatabase();
    }
}
