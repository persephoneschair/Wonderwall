using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PersistenceManager
{
    private static string dataPath;
    private const string gameplayConfigExtension = "GameplayConfigs";
    private const string databaseProfileExtension = "DatabaseProfiles";
    private const string prizeValues = "2,500|5,000|7,500|10,000|15,000|20,000|25,000|30,000|40,000|50,000|" +
        "60,000|70,000|80,000|90,000|100,000|200,000|300,000|400,000|500,000|1,000,000";

    public static List<GameplayConfig> storedGameplayConfigs = new List<GameplayConfig>();
    public static List<DatabaseProfile> storedDatabaseProfiles = new List<DatabaseProfile>();

    private static DeviceConfig _currentDeviceConfig = new DeviceConfig();
    public static DeviceConfig CurrentDeviceConfig
    {
        get { return _currentDeviceConfig; }
        set
        {
            _currentDeviceConfig = value;
            WriteDeviceConfig();
        }
    }

    private static GameplayConfig _currentGameplayConfig = new GameplayConfig();
    public static GameplayConfig CurrentGameplayConfig
    {
        get { return _currentGameplayConfig; }
        set
        {
            _currentGameplayConfig = value;
            foreach (GameplayConfig gpf in storedGameplayConfigs.Where(x => x != value))
                gpf.IsCurrent = false;
            value.IsCurrent = true;
            WriteGameplayConfigs();
        }
    }

    private static DatabaseProfile _currentDatabaseProfile = new DatabaseProfile();
    public static DatabaseProfile CurrentDatabaseProfile
    {
        get { return _currentDatabaseProfile; }
        set
        {
            _currentDatabaseProfile = value;
            foreach (DatabaseProfile dbp in storedDatabaseProfiles.Where(x => x != value))
                dbp.IsCurrent = false;
            value.IsCurrent = true;
            WriteDatabaseProfiles();
        }
    }

    public static void OnStartup()
    {
        SetDataPath();
        ReadDeviceConfig();
        ReadGameplayConfigs();
        ReadQuestionDatabase();
        ReadDatabaseProfiles();
    }

    public static void SetDataPath()
    {
        dataPath = Application.persistentDataPath;
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
            DebugLog.Print("Data path directory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        dataPath += "/";
        if (!Directory.Exists(dataPath + gameplayConfigExtension))
        {
            Directory.CreateDirectory(dataPath + gameplayConfigExtension);
            DebugLog.Print("Gameplay config subdirectory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        if (!Directory.Exists(dataPath + databaseProfileExtension))
        {
            Directory.CreateDirectory(dataPath + databaseProfileExtension);
            DebugLog.Print("Database profile subdirectory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
    }

    public static void WriteAllData()
    {
        WriteDeviceConfig();
        WriteGameplayConfigs();
    }

    #region Device Config

    private static void ReadDeviceConfig()
    {
        if (!File.Exists(dataPath + "DeviceConfig.json"))
        {
            File.WriteAllText(dataPath + "DeviceConfig.json", JsonConvert.SerializeObject(new DeviceConfig(), Formatting.Indented));
            DebugLog.Print("Default device config created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
            DebugLog.Print("Default device config restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);

        JsonConvert.PopulateObject(File.ReadAllText(dataPath + "DeviceConfig.json"), CurrentDeviceConfig);
    }

    private static void WriteDeviceConfig()
    {
        File.WriteAllText(dataPath + "DeviceConfig.json", JsonConvert.SerializeObject(CurrentDeviceConfig, Formatting.Indented));
    }

    #endregion

    #region Gameplay Config

    private static void ReadGameplayConfigs()
    {
        var storedConfigs = Directory.GetFiles(dataPath + gameplayConfigExtension, "*.json");

        if(storedConfigs.Length <= 1)
        {
            var def = new GameplayConfig()
            {
                LockConfig = true,
                PrizeLadder = prizeValues.Split('|').Select(s => "$" + s).ToArray()
            };
            File.WriteAllText(dataPath + gameplayConfigExtension + $"/{def.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
            storedGameplayConfigs.Add(def);

            var def2 = new GameplayConfig()
            {
                LockConfig = true,
                ConfigName = "UK Rules",
                PrizeLadder = prizeValues.Split('|').Select(s => "£" + s).ToArray(),
                NumberOfStrikes = 0,
                NumberOfPasses = 0,
                EnableBailOutAt = 0
            };
            File.WriteAllText(dataPath + gameplayConfigExtension + $"/{def2.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
            storedGameplayConfigs.Add(def2);

            CurrentGameplayConfig = def;
            DebugLog.Print("US & UK gameplay configs created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
        {
            storedGameplayConfigs = storedConfigs.Select(config => JsonConvert.DeserializeObject<GameplayConfig>(File.ReadAllText(config))).OrderBy(x => x.Epoch).ToList();
            if(storedGameplayConfigs.FirstOrDefault(x => x.IsCurrent) == null)
                storedGameplayConfigs.FirstOrDefault().IsCurrent = true;
            CurrentGameplayConfig = storedGameplayConfigs.FirstOrDefault(x => x.IsCurrent);
            DebugLog.Print("Gameplay configs restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
        }
    }

    public static void WriteGameplayConfigs()
    {
        foreach(GameplayConfig gcf in storedGameplayConfigs)
            File.WriteAllText(dataPath + gameplayConfigExtension + $"/{gcf.ID}.json", JsonConvert.SerializeObject(gcf, Formatting.Indented));
    }

    public static void OnDeleteConfig()
    {
        File.Delete(dataPath + gameplayConfigExtension + $"/{CurrentGameplayConfig.ID}.json");
        storedGameplayConfigs.Remove(CurrentGameplayConfig);
        CurrentGameplayConfig = storedGameplayConfigs.FirstOrDefault();
    }

    #endregion

    #region Question Database

    private static void ReadQuestionDatabase()
    {
        if (!File.Exists(dataPath + "QuestionDatabase.json"))
        {
            File.WriteAllText(dataPath + "QuestionDatabase.json", JsonConvert.SerializeObject(QuestionDatabase.Questions, Formatting.Indented));
            DebugLog.Print("Empty question database created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
            DebugLog.Print("Question database restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);

        JsonConvert.PopulateObject(File.ReadAllText(dataPath + "QuestionDatabase.json"), QuestionDatabase.Questions);

        (MainMenuManager.Get.GetDBMan() as DatabaseManager).BuildQuestionObjects();
    }

    private static void ReadDatabaseProfiles()
    {
        var storedDbProfiles = Directory.GetFiles(dataPath + databaseProfileExtension, "*.json");

        if (storedDbProfiles.Length < 1)
        {
            var def = new DatabaseProfile()
            {
                LockConfig = true
            };
            File.WriteAllText(dataPath + databaseProfileExtension + $"/{def.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
            storedDatabaseProfiles.Add(def);

            CurrentDatabaseProfile = def;
            DebugLog.Print("Default database profile created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
        {
            storedDatabaseProfiles = storedDbProfiles.Select(profile => JsonConvert.DeserializeObject<DatabaseProfile>(File.ReadAllText(profile))).OrderBy(x => x.Epoch).ToList();
            if (storedDatabaseProfiles.FirstOrDefault(x => x.IsCurrent) == null)
                storedDatabaseProfiles.FirstOrDefault().IsCurrent = true;
            CurrentDatabaseProfile = storedDatabaseProfiles.FirstOrDefault(x => x.IsCurrent);
            DebugLog.Print("Database profiles restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
        }

        SetDatabaseProfile();
    }

    public static void WriteQuestionDatabase()
    {
        File.WriteAllText(dataPath + "QuestionDatabase.json", JsonConvert.SerializeObject(QuestionDatabase.Questions, Formatting.Indented));
    }

    public static void WriteDatabaseProfiles()
    {
        foreach (DatabaseProfile dbp in storedDatabaseProfiles)
            File.WriteAllText(dataPath + databaseProfileExtension + $"/{dbp.ID}.json", JsonConvert.SerializeObject(dbp, Formatting.Indented));
    }

    private static void SetDatabaseProfile()
    {
        (MainMenuManager.Get.GetDBMan() as DatabaseManager).SetProfileSpecifics();
    }

    public static void OnDeleteProfile()
    {
        File.Delete(dataPath + databaseProfileExtension + $"/{CurrentDatabaseProfile.ID}.json");
        storedDatabaseProfiles.Remove(CurrentDatabaseProfile);
        CurrentDatabaseProfile = storedDatabaseProfiles.FirstOrDefault();
    }

    public static void OnDeleteQuestion(Question q)
    {
        QuestionDatabase.Questions.Remove(q);
        (MainMenuManager.Get.GetDBMan() as DatabaseManager).OnDeleteQuestion(q);

        foreach (DatabaseProfile p in storedDatabaseProfiles)
            p.UsedQsOnThisProfile.Remove(q.ID);

        WriteQuestionDatabase();
        WriteDatabaseProfiles();

        //Test builds
    }

    #endregion
}
