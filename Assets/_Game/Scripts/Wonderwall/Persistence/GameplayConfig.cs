using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GameplayConfig
{
    public GameplayConfig()
    {
        ID = Guid.NewGuid();
        Epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private Guid _id;
    public Guid ID
    {
        get { return _id; }
        set { _id = value; }
    }

    private long _epoch;
    public long Epoch
    {
        get { return _epoch; }
        set { _epoch = value; }
    }

    private bool _lockConfig = false;
    public bool LockConfig
    {
        get { return _lockConfig; }
        set
        {
            _lockConfig = value;
        }
    }

    private string _configName = "US Rules";
    public string ConfigName
    {
        get { return _configName; }
        set
        {
            _configName = value;
            (MainMenuManager.Get.GetGameplayConfig() as GameplayConfigManager).BuildDropdown();
        }
    }

    private bool _isCurrent = true;
    public bool IsCurrent
    {
        get { return _isCurrent; }
        set
        {
            _isCurrent = value;
        }
    }

    private int _numberOfStrikes = 3;
    public int NumberOfStrikes
    {
        get { return _numberOfStrikes; }
        set
        {
            _numberOfStrikes = value;
            OnPropertyChanged();
        }
    }

    private int _numberOfPasses = 3;
    public int NumberOfPasses
    {
        get { return _numberOfPasses; }
        set
        {
            _numberOfPasses = value;
            OnPropertyChanged();
        }
    }

    private int _numberOfPits = 2;
    public int NumberOfPits
    {
        get { return _numberOfPits; }
        set
        {
            _numberOfPits = value;
            OnPropertyChanged();
        }
    }

    private int _targetQuestions = 20;
    public int TargetQuestions
    {
        get { return _targetQuestions; }
        set
        {
            _targetQuestions = value;
            OnPropertyChanged();
        }
    }

    private string[] _prizeLadder = new string[20];
    public string[] PrizeLadder
    {
        get { return _prizeLadder; }
        set
        {
            _prizeLadder = value;
            OnPropertyChanged();
        }
    }


    private float _enableBailOutAt = 30f;
    public float EnableBailOutAt
    {
        get { return _enableBailOutAt; }
        set
        {
            _enableBailOutAt = value;
            OnPropertyChanged();
        }
    }

    private float _timeAvailable = 180f;
    public float TimeAvailable
    {
        get { return _timeAvailable; }
        set
        {
            _timeAvailable = value;
            OnPropertyChanged();
        }
    }

    private float _operatorRefreshInterval = 5f;
    public float OperatorRefreshInterval
    {
        get { return _operatorRefreshInterval; }
        set
        {
            _operatorRefreshInterval = value;
            OnPropertyChanged();
        }
    }

    private bool _useWebcam = true;
    public bool UseWebcam
    {
        get { return _useWebcam; }
        set
        {
            _useWebcam = value;
            OnPropertyChanged();
        }
    }

    private bool _shuffleQuestionOrder = true;
    public bool ShuffleQuestionOrder
    {
        get { return _shuffleQuestionOrder; }
        set
        {
            _shuffleQuestionOrder = value;
            OnPropertyChanged();
        }
    }


    public void OnPropertyChanged()
    {
        //PersistenceManager.CurrentGameplayConfig = this;
    }
}
