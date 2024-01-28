using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayConfigManager : SubMenuManager
{
    public TMP_Dropdown profileDropdown;
    public TMP_InputField profileNameInput;
    public GenericSlider gameTimeSlider;
    public GenericSlider targetQsSlider;
    public Toggle shuffleQOrderToggle;
    public GenericSlider passesSlider;
    public GenericSlider pitsSlider;
    public GenericSlider strikesSlider;
    public GenericSlider enableBailSlider;
    public Button setLadderButton;
    public Toggle webcamToggle;
    public GenericSlider operatorRefreshSlider;
    public Button deleteButton;

    public GameObject mainWindow;
    public RectTransform mainWindowRT;
    public GameObject ladderObj;
    public RectTransform ladderRT;
    public PrizeField[] prizeLadder;

    public Button backButton;
    public Button newConfigButton;

    private const string prizeValues = "2,500|5,000|7,500|10,000|15,000|20,000|25,000|30,000|40,000|50,000|" +
        "60,000|70,000|80,000|90,000|100,000|200,000|300,000|400,000|500,000|1,000,000";

    private bool activated = false;

    public override void OnRoomConnected()
    {
        activated = true;
        base.OnRoomConnected();
        BuildDropdown();
        mainWindowRT.localPosition = new Vector3(0, -1500, 0);
        ladderRT.localPosition = new Vector3(0, -1500, 0);
    }

    public void BuildDropdown()
    {
        if (!activated)
            return;
        var cgc = PersistenceManager.CurrentGameplayConfig;
        profileDropdown.ClearOptions();
        profileDropdown.AddOptions(PersistenceManager.storedGameplayConfigs.Select(x => x.ConfigName).ToList());
        int index = Array.IndexOf(PersistenceManager.storedGameplayConfigs.ToArray(), cgc);
        profileDropdown.value = index;
        OnChangeProfile(index);
    }

    private void Update()
    {

        if(PersistenceManager.CurrentGameplayConfig != null)
        {
            var cgc = PersistenceManager.CurrentGameplayConfig;

            if(cgc.LockConfig)
            {
                profileNameInput.interactable = false;
                gameTimeSlider.slider.interactable = false;
                targetQsSlider.slider.interactable = false;
                passesSlider.slider.interactable = false;
                pitsSlider.slider.interactable = false;
                strikesSlider.slider.interactable = false;
                enableBailSlider.slider.interactable = false;
                foreach (PrizeField tx in prizeLadder)
                    tx.field.interactable = false;
                deleteButton.interactable = false;
            }
            else
            {
                profileNameInput.interactable = true;
                gameTimeSlider.slider.interactable = true;
                targetQsSlider.slider.interactable = true;
                pitsSlider.slider.interactable = true;
                strikesSlider.slider.interactable = true;
                setLadderButton.interactable = true;
                foreach (PrizeField tx in prizeLadder)
                    tx.field.interactable = true;
                deleteButton.interactable = true;
                if (cgc.NumberOfStrikes == 0)
                {
                    enableBailSlider.slider.interactable = false;
                    passesSlider.slider.interactable = false;
                    passesSlider.slider.value = 0;
                    OnChangeSliderValue(2);
                }
                else
                {
                    enableBailSlider.slider.interactable = true;
                    passesSlider.slider.interactable = true;
                }
            }                         
        }
    }

    public void OnChangeProfile(int index)
    {
        PersistenceManager.CurrentGameplayConfig = PersistenceManager.storedGameplayConfigs[index];
        ApplyValues();
    }

    private void ApplyValues()
    {
        var cgc = PersistenceManager.CurrentGameplayConfig;
        profileNameInput.text = cgc.ConfigName;
        gameTimeSlider.slider.value = cgc.TimeAvailable;
        targetQsSlider.slider.value = cgc.TargetQuestions;
        shuffleQOrderToggle.isOn = cgc.ShuffleQuestionOrder;
        passesSlider.slider.value = cgc.NumberOfPasses;
        pitsSlider.slider.value = cgc.NumberOfPits;
        strikesSlider.slider.value = cgc.NumberOfStrikes;
        enableBailSlider.slider.value = cgc.EnableBailOutAt;
        webcamToggle.isOn = cgc.UseWebcam;
        operatorRefreshSlider.slider.value = cgc.OperatorRefreshInterval;

        for (int i = 0; i < cgc.PrizeLadder.Length; i++)
        {
            prizeLadder[i].index = i;
            prizeLadder[i].label.text = $"Question #{i + 1}";
            prizeLadder[i].field.text = cgc.PrizeLadder[i];
        }            
    }

    public void OnChangeProfileName(string s)
    {
        PersistenceManager.CurrentGameplayConfig.ConfigName = s;
    }

    public void OnToggle(int toggleRef)
    {
        if (toggleRef == 0)
            PersistenceManager.CurrentGameplayConfig.ShuffleQuestionOrder = shuffleQOrderToggle.isOn;
        else if(toggleRef == 1)
            PersistenceManager.CurrentGameplayConfig.UseWebcam = webcamToggle.isOn;
    }

    public void OnChangeSliderValue(int sliderRef)
    {
        switch (sliderRef)
        {
            case 0:
                PersistenceManager.CurrentGameplayConfig.TimeAvailable = gameTimeSlider.slider.value;
                break;

            case 1:
                PersistenceManager.CurrentGameplayConfig.TargetQuestions = (int)targetQsSlider.slider.value;
                break;

            case 2:
                PersistenceManager.CurrentGameplayConfig.NumberOfPasses = (int)passesSlider.slider.value;
                break;

            case 3:
                PersistenceManager.CurrentGameplayConfig.NumberOfPits = (int)pitsSlider.slider.value;
                break;

            case 4:
                PersistenceManager.CurrentGameplayConfig.NumberOfStrikes = (int)strikesSlider.slider.value;
                break;

            case 5:
                PersistenceManager.CurrentGameplayConfig.EnableBailOutAt = enableBailSlider.slider.value;
                break;

            case 6:
                PersistenceManager.CurrentGameplayConfig.OperatorRefreshInterval = operatorRefreshSlider.slider.value;
                break;
        }
    }

    public void OnChangePrizeValue(int index, string s)
    {
        var x = PersistenceManager.CurrentGameplayConfig.PrizeLadder;
        x[index] = s;
        PersistenceManager.CurrentGameplayConfig.PrizeLadder = x;
    }

    public void OnNewConfig()
    {
        var def = new GameplayConfig()
        {
            ConfigName = "New Config",
            PrizeLadder = prizeValues.Split('|').ToArray()
        };
        PersistenceManager.storedGameplayConfigs.Add(def);
        PersistenceManager.CurrentGameplayConfig = def;
        BuildDropdown();
    }

    public void OnDeleteConfig()
    {
        PersistenceManager.OnDeleteConfig();
        BuildDropdown();
    }

    public void OnSetLadder()
    {
        mainWindow.SetActive(false);
        backButton.interactable = false;
        newConfigButton.interactable = false;
        ladderObj.SetActive(true);

        for(int i = 0; i < prizeLadder.Length; i++)
            prizeLadder[i].obj.SetActive(i + 1 <= PersistenceManager.CurrentGameplayConfig.TargetQuestions);
    }

    public void CloseLadder()
    {
        mainWindow.SetActive(true);
        backButton.interactable = true;
        newConfigButton.interactable = true;
        ladderObj.SetActive(false);
    }

    public void OnCloseMenu()
    {
        PersistenceManager.WriteGameplayConfigs();
    }
}
