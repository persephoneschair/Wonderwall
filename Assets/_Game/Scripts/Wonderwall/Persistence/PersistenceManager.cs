using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PersistenceManager
{
    public static string persistentDataPath;
    public static string localDataPath;
    private const string gameplayConfigExtension = "GameplayConfigs";
    private const string databaseProfileExtension = "DatabaseProfiles";
    private const string prizeValues = "2,500|5,000|7,500|10,000|15,000|20,000|25,000|30,000|40,000|50,000|" +
        "60,000|70,000|80,000|90,000|100,000|200,000|300,000|400,000|500,000|1,000,000";

    private const string demoWall = "question,correctAnswer,incorrectAnswer\r\nIn which year was the first iPhone released?,2007,2005\r\nWhat is 9 squared?,Eighty One,Sixty Four\r\nWhat is the French word for strawberry?,Fraise,Mange tout\r\nHow long is a snooker table?,Twelve feet,Sixteen feet\r\n'I've started so I'll finish' is a phrase heard in which quiz show?,Mastermind,Only Connect\r\nWhat is the capital city of Iceland?,Reykjavík,Ljubljana\r\nWhat is the ninth letter of the NATO Phonetic alphabet?,India,Indigo\r\nWho was the Greek god of gods?,Zeus,Hercules\r\nWho was the first person to run a four-minute mile?,Roger Bannister,Stanley Matthews\r\n\"In the bible, who was the father of Isaac?\",Abraham,Jacob\r\nWho presents the TV show 'The Traitors'?,Claudia Winkleman,Davina McCall\r\nWho played Chewbacca in Star Wars?,Peter Mayhew,David Prowse\r\nWhat is the name for sparkling Italian wine?,Prosecco,Champagne\r\nWhat medal is presented to winners at the Olympics?,Gold,Platinum\r\nWhat is the capital city of France?,Paris,Hamburg\r\nWhich magical double act always force the three of clubs?,Penn & Teller,Siegfried & Roy\r\nWho was Henry VIII's first wife?,Catherine of Aragon,Anne Boleyn\r\nWhich animal features in the award-winning game 'Untitled'?,Goose,Goat\r\nIn which game engine is this game made?,Unity,Unreal\r\n\"On a standard Rubik's Cube, what colour is opposite white?\",Yellow,Black\r\nWho wrote the war poem 'Dulce et Decorum est'?,Wilfred Owen,Siegfried Sassoon\r\nHow many keys are there on a full-size piano?,Eighty eight,Ninety Nine\r\nWho created the iPad and iMac?,Apple,Samsung\r\nIn which country are the Spanish Steps?,Italy,Spain\r\nWho directed the film 'Gladiator'?,Ridley Scott,Martin Scorsese\r\n";
    private const string demoCSV = "question,correctAnswer,incorrectAnswer\r\nInsert questions into this column,Insert correct answers here,Insert incorrect answers here\r\nWhat is the capital city of Spain?,Madrid,Paris\r\nWhat colour is Sonic the Hedgehog?,Blue,Red";

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
        persistentDataPath = Application.persistentDataPath;
        localDataPath = Directory.GetParent(Application.dataPath).ToString().Replace("\\", "/");
        if (!Directory.Exists(persistentDataPath))
        {
            Directory.CreateDirectory(persistentDataPath);
            DebugLog.Print("Data path directory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        persistentDataPath += "/";
        if (!Directory.Exists(persistentDataPath + gameplayConfigExtension))
        {
            Directory.CreateDirectory(persistentDataPath + gameplayConfigExtension);
            DebugLog.Print("Gameplay config subdirectory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        if (!Directory.Exists(persistentDataPath + databaseProfileExtension))
        {
            Directory.CreateDirectory(persistentDataPath + databaseProfileExtension);
            DebugLog.Print("Database profile subdirectory created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }

        if (!Directory.Exists(localDataPath + "/Question Data"))
        {
            Directory.CreateDirectory(localDataPath + "/Question Data");
            File.WriteAllText(localDataPath + "/Question Data/Question Data Template.csv", demoCSV);
            DebugLog.Print("Assets subdirectory and demo created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        if (!Directory.Exists(localDataPath + "/Question Data/Bespoke Walls"))
        {
            Directory.CreateDirectory(localDataPath + "/Question Data/Bespoke Walls");
            File.WriteAllText(localDataPath + "/Question Data/Bespoke Walls/Bespoke Example.csv", demoWall);
            DebugLog.Print("Bespoke walls subdirectory and demo created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
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
        if (!File.Exists(persistentDataPath + "DeviceConfig.json"))
        {
            File.WriteAllText(persistentDataPath + "DeviceConfig.json", JsonConvert.SerializeObject(new DeviceConfig(), Formatting.Indented));
            DebugLog.Print("Default device config created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
            DebugLog.Print("Default device config restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);

        JsonConvert.PopulateObject(File.ReadAllText(persistentDataPath + "DeviceConfig.json"), CurrentDeviceConfig);
    }

    private static void WriteDeviceConfig()
    {
        File.WriteAllText(persistentDataPath + "DeviceConfig.json", JsonConvert.SerializeObject(CurrentDeviceConfig, Formatting.Indented));
    }

    #endregion

    #region Gameplay Config

    private static void ReadGameplayConfigs()
    {
        var storedConfigs = Directory.GetFiles(persistentDataPath + gameplayConfigExtension, "*.json");

        if(storedConfigs.Length <= 1)
        {
            var def = new GameplayConfig()
            {
                LockConfig = true,
                PrizeLadder = prizeValues.Split('|').Select(s => "$" + s).ToArray()
            };
            File.WriteAllText(persistentDataPath + gameplayConfigExtension + $"/{def.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
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
            File.WriteAllText(persistentDataPath + gameplayConfigExtension + $"/{def2.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
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
            File.WriteAllText(persistentDataPath + gameplayConfigExtension + $"/{gcf.ID}.json", JsonConvert.SerializeObject(gcf, Formatting.Indented));
    }

    public static void OnDeleteConfig()
    {
        File.Delete(persistentDataPath + gameplayConfigExtension + $"/{CurrentGameplayConfig.ID}.json");
        storedGameplayConfigs.Remove(CurrentGameplayConfig);
        CurrentGameplayConfig = storedGameplayConfigs.FirstOrDefault();
    }

    #endregion

    #region Question Database

    private static void ReadQuestionDatabase()
    {
        if (!File.Exists(persistentDataPath + "QuestionDatabase.json"))
        {
            File.WriteAllText(persistentDataPath + "QuestionDatabase.json", JsonConvert.SerializeObject(QuestionDatabase.Questions, Formatting.Indented));
            DebugLog.Print("Empty question database created", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
            DebugLog.Print("Question database restored", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);

        JsonConvert.PopulateObject(File.ReadAllText(persistentDataPath + "QuestionDatabase.json"), QuestionDatabase.Questions);

        (MainMenuManager.Get.GetDBMan() as DatabaseManager).BuildQuestionObjects();
    }

    private static void ReadDatabaseProfiles()
    {
        var storedDbProfiles = Directory.GetFiles(persistentDataPath + databaseProfileExtension, "*.json");

        if (storedDbProfiles.Length < 1)
        {
            var def = new DatabaseProfile()
            {
                LockConfig = true
            };
            File.WriteAllText(persistentDataPath + databaseProfileExtension + $"/{def.ID}.json", JsonConvert.SerializeObject(def, Formatting.Indented));
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
        File.WriteAllText(persistentDataPath + "QuestionDatabase.json", JsonConvert.SerializeObject(QuestionDatabase.Questions, Formatting.Indented));
    }

    public static void WriteDatabaseProfiles()
    {
        foreach (DatabaseProfile dbp in storedDatabaseProfiles)
            File.WriteAllText(persistentDataPath + databaseProfileExtension + $"/{dbp.ID}.json", JsonConvert.SerializeObject(dbp, Formatting.Indented));
    }

    private static void SetDatabaseProfile()
    {
        (MainMenuManager.Get.GetDBMan() as DatabaseManager).SetProfileSpecifics();
    }

    public static void OnDeleteProfile()
    {
        File.Delete(persistentDataPath + databaseProfileExtension + $"/{CurrentDatabaseProfile.ID}.json");
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
