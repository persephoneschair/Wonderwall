using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public MainMenuManager.ButtonType buttonType;
    public Button button;

    public void OnClickButton()
    {
        if (buttonType == MainMenuManager.ButtonType.Quit)
        {
            DebugLog.Print("QUITTING", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            Application.Quit();
            return;
        }
        MainMenuManager.Get.OnClickMenuButton(this.buttonType);
    }

    private void Update()
    {
        AutoEnablingOfButtons();
    }

    private void AutoEnablingOfButtons()
    {
        var hb = HackboxManager.Get;
        switch (buttonType)
        {
            case MainMenuManager.ButtonType.PlayRandomWall:
                if (hb.operatorControl == null || hb.contestantControl == null)
                {
                    button.interactable = false;
                    return;
                }
                button.interactable = (MainMenuManager.Get.GetDBMan() as DatabaseManager).GetAvailableQCount() >= 25;
                break;

            case MainMenuManager.ButtonType.PlayBespokeWall:
            case MainMenuManager.ButtonType.PlayRemoteWall:
                if (hb.operatorControl == null || hb.contestantControl == null)
                {
                    button.interactable = false;
                    return;
                }
                else
                    button.interactable = true;
                break;
        }
    }
}
