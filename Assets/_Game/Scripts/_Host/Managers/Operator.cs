using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using TMPro;

public class Operator : SingletonMonoBehaviour<Operator>
{
    public float forcedDelayOnLoadingScreen = 2f;
    public GameObject loadingScreen;
    public Animator loadingAnim;
    public bool reloadHackboxHost;
    public TextMeshProUGUI debugger;

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        HackboxManager.Get.hackboxHost.ReloadHost = reloadHackboxHost;
        PersistenceManager.OnStartup();
    }

    public void OnConnectedToRoom()
    {
        Invoke("KillLoadingScreen", forcedDelayOnLoadingScreen);
    }

    private void KillLoadingScreen()
    {
        //AudioManager.Get.Play(AudioManager.LoopClip.MainTheme, false);
        AudioManager.Get.Play(AudioManager.LoopClip.Setup, true/*, 38.5f*/);
        loadingAnim.SetTrigger("loaded");
        Invoke("KillLoadingObject", 2f);
    }

    private void KillLoadingObject()
    {
        loadingScreen.SetActive(false);
    }

    [Button]
    public void LoadTestWall()
    {
        if (WonderwallManager.Get.gameActive)
            return;
        TestQuestionGenerator.Get.DownloadAWall();
    }

    [Button]
    public void StartTheWall()
    {
        if (WonderwallManager.Get.gameActive)
        {
            DebugLog.Print("A GAME IS ALREADY UNDERWAY!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            return;
        }
                
        if (QuestionManager.currentPack == null)
        {
            DebugLog.Print("NO QUESTION PACK HAS BEEN INGESTED!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            if (HackboxManager.Get.operatorControl != null)
                HackboxManager.Get.SendOperatorGetName();
            return;
        }    
        
        if(HackboxManager.Get.contestantControl == null)
        {
            HackboxManager.Get.SendOperatorGetName();
            DebugLog.Print($"NO CONTESTANT DEVICE CONNECTED: CONNECT A DEVICE WITH THE ROOM CODE {HackboxManager.Get.hackboxHost.RoomCode}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            return;
        }
        else
            WonderwallManager.Get.InitWall();
    }

    [Button]
    public void Correct()
    {
        WonderwallManager.Get.Correct();
    }

    [Button]
    public void Incorrect()
    {
        WonderwallManager.Get.Incorrect();
    }

    [Button]
    public void Pass()
    {
        WonderwallManager.Get.Pass();
    }

    [Button]
    public void Pitstop()
    {
        WonderwallManager.Get.Pitstop();
    }

    [Button]
    public void BailOut()
    {
        WonderwallManager.Get.BailOut();
    }

    [Button]
    public void ToggleWallSpeed()
    {
        WonderwallManager.Get.ToggleWallSpeed();
    }

    [Button]
    public void LockWallLeft()
    {
        WonderwallManager.Get.WallLock(0);
    }

    [Button]
    public void LockWallCentre()
    {
        WonderwallManager.Get.WallLock(1);
    }

    [Button]
    public void LockWallRight()
    {
        WonderwallManager.Get.WallLock(2);
    }
}
