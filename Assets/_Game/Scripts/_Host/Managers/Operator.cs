using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class Operator : SingletonMonoBehaviour<Operator>
{
    [Header("Game Settings")]
    [Tooltip("Supresses Twitch chat messages and will store Pennys and medals in a separate test file")]
    public bool testMode;
    [Tooltip("Skips opening titles")]
    public bool skipOpeningTitles;

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {

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
