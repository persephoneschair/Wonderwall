using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceConfig
{
    public DeviceConfig()
    {

    }

    private string _operatorName = "Operator";
    public string OperatorName
    {
        get { return _operatorName; }
        set
        {
            _operatorName = value;
            OnPropertyChanged();
        }
    }

    private string _controlName = "Control";
    public string ControlName
    {
        get { return _controlName; }
        set
        {
            _controlName = value;
            OnPropertyChanged();
        }
    }

    private int _qTextSize = 30;
    public int QTextSize
    {
        get { return _qTextSize; }
        set
        {
            _qTextSize = value;
            OnPropertyChanged();
        }
    }

    private int _statsTextSize = 20;
    public int StatsTextSize
    {
        get { return _statsTextSize; }
        set
        {
            _statsTextSize = value;
            OnPropertyChanged();
        }
    }



    public void OnPropertyChanged()
    {
        PersistenceManager.CurrentDeviceConfig = this;
    }
}
