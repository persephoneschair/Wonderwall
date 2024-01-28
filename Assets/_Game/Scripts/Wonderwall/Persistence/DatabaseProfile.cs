using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DatabaseProfile
{
    public DatabaseProfile()
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

    private bool _playWithUsed = false;
    public bool PlayWithUsed
    {
        get { return _playWithUsed; }
        set
        {
            _playWithUsed = value;
        }
    }

    private bool _playWithUnused = true;
    public bool PlayWithUnused
    {
        get { return _playWithUnused; }
        set
        {
            _playWithUnused = value;
        }
    }

    private string _profileName = "Default";
    public string ProfileName
    {
        get { return _profileName; }
        set
        {
            _profileName = value;
            (MainMenuManager.Get.GetDBMan() as DatabaseManager).BuildDropdown();
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

    private HashSet<Guid> _usedQsOnThisProfile = new HashSet<Guid>();
    public HashSet<Guid> UsedQsOnThisProfile
    {
        get { return _usedQsOnThisProfile; }
        set
        {
            _usedQsOnThisProfile = value;
        }
    }
}
