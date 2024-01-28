using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeviceConfigManager : SubMenuManager
{
    private const string instructionsString = "Two additional devices are required in order to play Wonderwall; an <color=yellow>operator</color> and a <color=purple>control</color>.\n\n" +
        "The <color=yellow>operator</color> is used by the host to mark progress. The <color=purple>control</color> is used by the contestant to control the wall and use any available helps.\n\n" +
        "You can connect both of these devices now by visiting <color=green>https://hackbox.ca</color> from any browser on each device and joining with the names set below and the room code <color=green>{0}</color>.";

    public TextMeshProUGUI instructsMesh;

    public TMP_InputField operatorInputField;
    public TextMeshProUGUI operatorDetectionMesh;

    public TMP_InputField controlInputField;
    public TextMeshProUGUI controlDetectionMesh;

    public GenericSlider[] mimicSlider;

    public Button testOperatorButton;
    private bool opTestActive;
    

    public override void OnRoomConnected()
    {
        base.OnRoomConnected();
        instructsMesh.text = string.Format(instructionsString, HackboxManager.Get.hackboxHost.RoomCode);
        SetInitialValues();
    }

    private void SetInitialValues()
    {
        if(PersistenceManager.CurrentDeviceConfig != null)
        {
            mimicSlider[0].slider.value = PersistenceManager.CurrentDeviceConfig.QTextSize;
            mimicSlider[1].slider.value = PersistenceManager.CurrentDeviceConfig.StatsTextSize;

            operatorInputField.text = PersistenceManager.CurrentDeviceConfig.OperatorName;
            controlInputField.text = PersistenceManager.CurrentDeviceConfig.ControlName;
        }        
    }

    public void OnOperatorValueChanged(string s)
    {
        PersistenceManager.CurrentDeviceConfig.OperatorName = s.ToUpperInvariant();
    }

    public void OnControlValueChanged(string s)
    {
        PersistenceManager.CurrentDeviceConfig.ControlName = s.ToUpperInvariant();
    }

    public void OnChangeMimicTextSize(int conf)
    {
        if(PersistenceManager.CurrentDeviceConfig != null)
        {
            if (conf == 0)
                PersistenceManager.CurrentDeviceConfig.QTextSize = (int)mimicSlider[conf].slider.value;
            else if (conf == 1)
                PersistenceManager.CurrentDeviceConfig.StatsTextSize = (int)mimicSlider[conf].slider.value;
        }
    }

    public void Update()
    {
        CheckForOperator();
        CheckForControl();
    }

    void CheckForOperator()
    {
        if(HackboxManager.Get.hackboxHost.AllMembers.Any(x => x.Name.ToUpperInvariant() == PersistenceManager.CurrentDeviceConfig.OperatorName))
        {
            operatorDetectionMesh.text = "<color=green>OPERATOR ONLINE";
            operatorInputField.interactable = false;
            testOperatorButton.interactable = !opTestActive;
        }
        else
        {
            operatorDetectionMesh.text = "<color=red>NO DEVICE DETECTED";
            operatorInputField.interactable = true;
            testOperatorButton.interactable = false;
        }
    }

    void CheckForControl()
    {
        if (HackboxManager.Get.hackboxHost.AllMembers.Any(x => x.Name.ToUpperInvariant() == PersistenceManager.CurrentDeviceConfig.ControlName))
        {
            controlDetectionMesh.text = "<color=green>CONTROL ONLINE";
            controlInputField.interactable = false;
        }
        else
        {
            controlDetectionMesh.text = "<color=red>NO DEVICE DETECTED";
            controlInputField.interactable = true;
        }
    }

    public void OnForceNewRoom()
    {
        parentButton.GetComponent<Button>().interactable = false;
        HackboxManager.Get.ResetConnection();
        MainMenuManager.Get.OnClickMenuButton(MainMenuManager.ButtonType.Home);
    }

    public void OnTestOperator()
    {
        opTestActive = true;
        HackboxManager.Get.SendTestQAndA(this);
    }

    public void OnCloseMenu()
    {
        OnEndTest();
    }

    public void OnEndTest()
    {
        if(HackboxManager.Get.operatorControl != null)
            HackboxManager.Get.SendGenericMessage(HackboxManager.Get.operatorControl, "CONNECTED<br>AWAITING PACK INGESTION");
        opTestActive = false;
    }
}
