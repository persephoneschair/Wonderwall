using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hackbox;
using Hackbox.Parameters;
using Hackbox.UI;
using TMPro;
using System.Linq;
using NaughtyAttributes;
using UnityEngine.UIElements;
using System;

public class HackboxManager : SingletonMonoBehaviour<HackboxManager>
{

    [Header("Hackbox Config")]
    public Host hackboxHost;
    public bool allowHotkeyRefresh;

    [Header("Wonderwall Control")]
    public Theme wonderwallTheme;
    public Preset header;
    public Preset wallOrientation;
    public Preset wallSpeed;

    [Header("Operator")]
    public Preset operatorMark;
    public Preset operatorQuestion;
    public Preset launchGame;
    public Preset statsBox;
    public Preset inputBox;
    public Preset abandonGame;
    public Preset endGame;

    [Header("Helps")]
    public Preset threeHelps;
    public Preset pitOnly;
    public Preset pitBail;
    public Preset passBail;
    public Preset bailOnly;
    public Preset returnTest;

    [Header("Live Data")]
    public Member operatorControl;
    public Member contestantControl;
    public State contestantState;
    private string currentQData;
    private string currentStatsData;
    private DeviceConfigManager dcm;

    public void OnConnected(string code)
    {
        DebugLog.Print("Welcome to Wonderwall", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        DebugLog.Print($"Players can join the game at HACKBOX.CA using the room code {code}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        MainMenuManager.Get.OnRoomConnected();
        Operator.Get.OnConnectedToRoom();
    }

    public void ResetConnection()
    {
        foreach(Member mem in hackboxHost.AllMembers)
            SendGenericMessage(mem, "ROOM DESTROYED<br>PLEASE REFRESH YOUR BROWSER AND RECONNECT");

        operatorControl = null;
        contestantControl = null;
        hackboxHost.Disconnect();
        hackboxHost.Connect(true);
    }

    public void OnPlayerJoins(Member member)
    {
        if (member.Name == PersistenceManager.CurrentDeviceConfig.OperatorName && operatorControl == null)
        {
            operatorControl = member;
            SendGenericMessage(operatorControl, "CONNECTED<br>AWAITING PACK INGESTION");
            //SendOperatorLaunchGame();
        }
        else if (member.Name == PersistenceManager.CurrentDeviceConfig.ControlName && contestantControl == null)
        {
            contestantControl = member;
            SendGenericMessage(contestantControl, "CONNECTED<br>AWAITING PACK INGESTION");
            //SendContestantControls();
        }
        else
            InvalidUser(member);
    }

    #region Hackbox Input Generation

    public State GenerateBaseState(string headerText)
    {
        State newState = new State() { Theme = wonderwallTheme };
        newState.SetHeaderText(headerText);
        newState.SetHeaderParameter("align", "center");
        return newState;
    }

    public void AddHeader(State state, string message)
    {
        UIComponent box = new UIComponent()
        {
            Name = "header",
            Preset = header,
        };

        box.SetParameterValue("text", message);
        state.Components.Add(box);
    }

    public void AddWallOrientation(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "orientation",
            Preset = wallOrientation,
        };
        state.Components.Add(options);
    }

    public void AddWallSpeed(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "speed",
            Preset = wallSpeed,
        };
        state.Components.Add(options);
    }

    public void AddThreeHelps(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "helps",
            Preset = threeHelps,
        };
        state.Components.Add(options);
    }
    public void AddPitOnly(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "helps",
            Preset = pitOnly,
        };
        state.Components.Add(options);
    }

    public void AddPitBail(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "helps",
            Preset = pitBail,
        };
        state.Components.Add(options);
    }

    public void AddPassBail(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "helps",
            Preset = passBail,
        };
        state.Components.Add(options);
    }
    public void AddBailOnly(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "helps",
            Preset = bailOnly,
        };
        state.Components.Add(options);
    }

    public void AddQAndA(State state, string qData)
    {
        UIComponent options = new UIComponent()
        {
            Name = "qA",
            Preset = operatorQuestion,
        };
        options.SetParameterValue("text", qData);
        options.SetStyleParameterValue("fontSize", $"{PersistenceManager.CurrentDeviceConfig.QTextSize}px");
        state.Components.Add(options);
    }

    public void AddMark(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "mark",
            Preset = operatorMark,
        };
        state.Components.Add(options);
    }

    public void AddLaunchGame(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "launch",
            Preset = launchGame,
        };
        state.Components.Add(options);
    }

    public void AddStatsBox(State state, string message)
    {
        UIComponent options = new UIComponent()
        {
            Name = "stats",
            Preset = statsBox,
        };
        options.SetParameterValue("text", message);
        options.SetStyleParameterValue("fontSize", $"{PersistenceManager.CurrentDeviceConfig.StatsTextSize}px");
        state.Components.Add(options);
    }

    public void AddReturnFromTest(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "test",
            Preset = returnTest,
        };
        state.Components.Add(options);
    }

    public void AddInputBox(State state, string ev = "")
    {
        UIComponent name = new UIComponent()
        {
            Name = "name",
            Preset = inputBox,
        };
        if(!string.IsNullOrEmpty(ev))
            name.SetParameterValue("event", ev);
        state.Components.Add(name);
    }

    public void AddAbandonGame(State state, bool endOfGame)
    {
        UIComponent comp;
        if(endOfGame)
            comp = new UIComponent()
            {
            Name = "abandon",
            Preset = endGame,
            };
        else
            comp = new UIComponent()
            {
                Name = "abandon",
                Preset = abandonGame,
            };
        state.Components.Add(comp);
    }

    #endregion

    #region Send States

    public void InvalidUser(Member mem)
    {
        State s = GenerateBaseState("ERROR");
        AddQAndA(s, "AN ERROR OCCURED<br><br>PLEASE ENSURE THAT THE NAME YOU SET FOR THIS DEVICE MATHCES WITH ONE OF THE DEVICE NAMES DEFINED IN THE CONFIG OR THAT ANOTHER DEVICE IS NOT ALREADY CONNECTED WITH THIS NAME");
        hackboxHost.UpdateMemberState(mem, s);
    }

    public void BuildContestantState()
    {
        contestantState = GenerateBaseState("CONTROL");
        AddHeader(contestantState, "WONDERWALL CONTROLS");
        AddWallOrientation(contestantState);
        AddWallSpeed(contestantState);

        var cgc = PersistenceManager.CurrentGameplayConfig;

        //At least one help is available; add the header
        if (cgc.NumberOfStrikes > 0 || cgc.NumberOfPits > 0 || cgc.NumberOfPasses > 0)
            AddHeader(contestantState, "HELP OPTIONS");

        //Add the necessary preset
        if (cgc.NumberOfStrikes > 0 && cgc.NumberOfPasses > 0 && cgc.NumberOfPits > 0)
            AddThreeHelps(contestantState);
        else if (cgc.NumberOfStrikes == 0 && cgc.NumberOfPasses == 0 && cgc.NumberOfPits > 0)
            AddPitOnly(contestantState);
        else if (cgc.NumberOfStrikes > 0 && cgc.NumberOfPits > 0 && cgc.NumberOfPasses == 0)
            AddPitBail(contestantState);
        else if (cgc.NumberOfStrikes > 0 && cgc.NumberOfPasses > 0 && cgc.NumberOfPits == 0)
            AddPassBail(contestantState);
        else if (cgc.NumberOfStrikes > 0 && cgc.NumberOfPasses == 0 && cgc.NumberOfPits == 0)
            AddBailOnly(contestantState);

        SendContestantControls();
    }

    public void SendContestantControls()
    {
        hackboxHost.UpdateMemberState(contestantControl, contestantState);
    }

    public void SendOperatorMimic(string qData, string statsData)
    {
        currentQData = qData;
        currentStatsData = statsData;
        string[] qx = currentQData.Split('|');
        string rejoined = string.Join("<br><br>", qx[0], qx[1]);
        State s = GenerateBaseState("OPERATOR");
        AddStatsBox(s, statsData);
        AddQAndA(s, rejoined);
        AddMark(s);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendOperatorGetName()
    {
        State s = GenerateBaseState("OPERATOR");
        AddQAndA(s, "ENTER PLAYER'S NAME IN THE BOX BELOW AND HIT SEND");
        AddInputBox(s);
        AddAbandonGame(s, false);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendRemotePackIngest()
    {
        State s = GenerateBaseState("OPERATOR");
        AddQAndA(s, "CONVERT A BESPOKE PACK AND PASTE THE RESULTING STRING DATA INTO THE BOX BELOW AND HIT SEND TO LOAD IT REMOTELY");
        AddInputBox(s, "REMOTEINGEST");
        AddAbandonGame(s, false);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendLaunchGame()
    {
        State s = GenerateBaseState("OPERATOR");
        AddLaunchGame(s);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendGenericMessage(Member mem, string message)
    {
        State s = GenerateBaseState(operatorControl == mem ? "OPERATOR" : "CONTROL");
        AddQAndA(s, message);
        hackboxHost.UpdateMemberState(mem, s);
    }

    public void SendTestQAndA(DeviceConfigManager dc)
    {
        if (operatorControl == null)
            return;

        dcm = dc;
        State s = GenerateBaseState("OPERATOR");
        AddStatsBox(s, "There could potentially<br>be up to four<br>lines of statistics<br>in this display");
        AddQAndA(s, string.Join("<br><br>", "This is what the operator layout will look like. Press either button to return.", "[#] OK"));
        AddMark(s);
        //AddReturnFromTest(s);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendEndGame()
    {
        Invoke("EndDelay", 1f);
    }

    private void EndDelay()
    {
        State s = GenerateBaseState("OPERATOR");
        AddQAndA(s, "GAME OVER");
        AddAbandonGame(s, true);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    private void Update()
    {
        if (allowHotkeyRefresh)
        {
            if (Input.GetKeyDown(KeyCode.C))
                RefreshContestant();
            if (Input.GetKeyDown(KeyCode.O))
                RefreshOperator();
        }        
    }

    [Button]
    public void RefreshContestant()
    {
        if(contestantControl != null)
            SendContestantControls();
    }

    [Button]
    public void RefreshOperator()
    {
        if(operatorControl != null)
        {
            if (string.IsNullOrEmpty(currentQData) || string.IsNullOrEmpty(currentStatsData))
                SendOperatorGetName();
            else
                SendOperatorMimic(currentQData, currentStatsData);
        }
    }

    #endregion

    public void OnMessageReceived(Message mes)
    {
        if (mes.Member == contestantControl)
        {
            switch (mes.Value)
            {
                case "Left":
                    Operator.Get.LockWallLeft();
                    break;

                case "Centre":
                    Operator.Get.LockWallCentre();
                    break;

                case "Right":
                    Operator.Get.LockWallRight();
                    break;

                case "Speed":
                    Operator.Get.ToggleWallSpeed();
                    break;

                case "Pit":
                    Operator.Get.Pitstop();
                    break;

                case "Pass":
                    Operator.Get.Pass();
                    break;

                case "Bail":
                    Operator.Get.BailOut();
                    break;
            }
            RefreshContestant();
        }


        else if(mes.Member == operatorControl)
        {
            switch(mes.Event)
            {
                case "GETNAME":
                    WonderwallManager.Get.playerName = mes.Value.ToString();
                    SendLaunchGame();
                    break;

                case "REMOTEINGEST":
                    ImportManager.Get.OnRemoteImport(mes.Value);
                    break;

                default:
                    switch (mes.Value)
                    {
                        case "LAUNCH":
                            SendGenericMessage(operatorControl, "LAUNCHING WALL");
                            Operator.Get.StartTheWall();
                            break;

                        case "ABANDON":
                            if (ImportManager.Get.errorAnim.GetBool("persist"))
                                ImportManager.Get.TogglePersist();

                            QuestionManager.ClearDownPack();
                            ImportManager.Get.TriggerAlert("<color=#FF0000>GAME ABANDONED");
                            MainMenuManager.Get.ToggleMenu();
                            SendGenericMessage(operatorControl, "CONNECTED<br>AWAITING PACK INGESTION");
                            break;

                        case "END":
                            WonderwallManager.Get.triggeredStrap.SetTrigger("lock");
                            WonderwallManager.Get.triggeredStrap = null;
                            LatticeManager.Get.SetMenu();
                            MainMenuManager.Get.ToggleMenu();
                            QuestionManager.ClearDownPack();
                            AudioManager.Get.Play(AudioManager.LoopClip.Setup, true);

                            SendGenericMessage(operatorControl, "CONNECTED<br>AWAITING PACK INGESTION");
                            SendGenericMessage(contestantControl, "CONNECTED<br>AWAITING PACK INGESTION");
                            break;

                        case "CORRECT":
                            if (dcm != null && dcm.testOperatorButton)
                                EndTest();
                            else
                            {
                                Operator.Get.Correct();
                                RefreshOperator();
                            }
                            break;

                        case "INCORRECT":
                            if (dcm != null && dcm.testOperatorButton)
                                EndTest();
                            else
                            {
                                Operator.Get.Incorrect();
                                RefreshOperator();
                            }
                            break;

                        default:
                            RefreshOperator();
                            break;
                    }
                    break;
            }
        }
    }

    private void EndTest()
    {
        dcm.OnEndTest();
        dcm = null;
    }
}
