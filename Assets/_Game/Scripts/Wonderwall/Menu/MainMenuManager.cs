using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenuManager : SingletonMonoBehaviour<MainMenuManager>
{
    public enum ButtonType
    {
        Home,
        PlayBespokeWall,
        PlayRandomWall,
        GameConfiguration,
        DeviceConfiguration,
        ImportQuestions,
        QuestionDatabase,
        Quit
    };

    public GameObject titleLogo;
    public GameObject readoutObj;
    public GameObject[] subMenus;
    public SubMenuManager[] subMenuManagers;

    public Animator menuAnim;

    public void OnRoomConnected()
    {
        foreach(var s in subMenuManagers)
            s.OnRoomConnected();
    }

    public void ToggleMenu()
    {
        menuAnim.SetTrigger("toggle");
    }

    public void OnClickMenuButton(ButtonType buttonType)
    {
        if(buttonType == ButtonType.PlayRandomWall)
        {
            LoadRandomWall();
            return;
        }
        KillAllMenus();
        ActivateMenu(buttonType);
        switch(buttonType)
        {
            case ButtonType.Home:
                break;

            case ButtonType.PlayBespokeWall:
                ImportManager.Get.OnClickImportQuestions(true);
                break;

            case ButtonType.GameConfiguration:
                break;

            case ButtonType.DeviceConfiguration:
                break;

            case ButtonType.ImportQuestions:
                ImportManager.Get.OnClickImportQuestions(false);
                break;

            case ButtonType.QuestionDatabase:
                break;

            case ButtonType.Quit:
                break;
        }
    }

    private void KillAllMenus()
    {
        titleLogo.SetActive(false);
        readoutObj.SetActive(false);
        foreach (GameObject g in subMenus)
            g.gameObject.SetActive(false);
    }

    private void ActivateMenu(ButtonType menu)
    {
        subMenus[(int)menu].gameObject.SetActive(true);
        if(menu == ButtonType.Home)
        {
            titleLogo.SetActive(true);
            readoutObj.SetActive(true);
        }            
    }

    private void LoadRandomWall()
    {
        var cdp = PersistenceManager.CurrentDatabaseProfile;
        var qdb = QuestionDatabase.Questions;
        List<Question> availableQs = new List<Question>();
        if (cdp.PlayWithUsed)
            availableQs.AddRange(qdb.Where(x => cdp.UsedQsOnThisProfile.Contains(x.ID)));
        if (cdp.PlayWithUnused)
            availableQs.AddRange(qdb.Where(x => !cdp.UsedQsOnThisProfile.Contains(x.ID)));

        availableQs.Shuffle();
        List<Question> selectedQs = availableQs.Take(25).ToList();

        QuestionManager.LoadPack(selectedQs, false);
        ToggleMenu();
        ImportManager.Get.TriggerAlert(string.Format("<color=#00FF00>RANDOM WALL LOADED\nREADY TO PLAY"));
    }

    public Object GetDBMan()
    {
        return subMenuManagers.FirstOrDefault(x => x.GetType() == typeof(DatabaseManager));
    }

    public Object GetGameplayConfig()
    {
        return subMenuManagers.FirstOrDefault(x => x.GetType() == typeof(GameplayConfigManager));
    }
}
