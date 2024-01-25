using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hackbox;
using Hackbox.Parameters;
using Hackbox.UI;
using TMPro;
using System.Linq;
using NaughtyAttributes;
using static UnityEditor.Progress;

public class HackboxManager : SingletonMonoBehaviour<HackboxManager>
{
    public string operatorName;
    public string contestantName;

    public Member operatorControl;
    public Member contestantControl;

    public Host hackboxHost;

    public Theme wonderwallTheme;
    public Preset header;
    public Preset wallOrientation;
    public Preset wallSpeed;
    public Preset helps;
    public Preset operatorMark;
    public Preset operatorQuestion;
    public Preset launchGame;

    private string currentQ;
    private string currentA;

    public void OnCreatedRoom()
    {
        DebugLog.Print("Welcome to Wonderwall", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        DebugLog.Print($"Players can join the game at HACKBOX.CA using the room code {hackboxHost.RoomCode}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
    }

    public void OnPlayerJoins(Member member)
    {
        if (member.Name == operatorName && operatorControl == null)
        {
            operatorControl = member;
            SendHostLaunchGame();
        }
        else if (member.Name == contestantName && contestantControl == null)
        {
            contestantControl = member;
            SendContestantControls();
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
            Name = "textBox",
            Preset = header,
        };

        box.SetParameterValue("text", message);
        state.Components.Add(box);
    }

    public void AddWallOrientation(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "grid",
            Preset = wallOrientation,
        };
        state.Components.Add(options);
    }

    public void AddWallSpeed(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "grid",
            Preset = wallSpeed,
        };
        state.Components.Add(options);
    }

    public void AddHelps(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "grid",
            Preset = helps,
        };
        state.Components.Add(options);
    }

    public void AddQuestion(State state, string[] qData)
    {
        UIComponent options = new UIComponent()
        {
            Name = "main",
            Preset = operatorQuestion,
        };

        ChoicesParameter optionsInfo = options.GetParameter<ChoicesParameter>("choices");
        optionsInfo.Value = new List<ChoicesParameter.Choice>();
        for (int i = 0; i < qData.Length; i++)
        {
            optionsInfo.Value.Add(new ChoicesParameter.Choice()
            {
                Label = qData[i], //This is what will be displayed on the label
                Value = qData[i], //This is the value that will return to the build
                Keys = new string[0],
            });
        }
        state.Components.Add(options);
    }

    public void AddMark(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "grid",
            Preset = operatorMark,
        };
        state.Components.Add(options);
    }

    public void AddLaunchGame(State state)
    {
        UIComponent options = new UIComponent()
        {
            Name = "grid",
            Preset = launchGame,
        };
        state.Components.Add(options);
    }

    #endregion

    #region Send States

    public void InvalidUser(Member mem)
    {
        State s = GenerateBaseState("INVALID");
        hackboxHost.UpdateMemberState(mem, s);
    }

    public void SendContestantControls()
    {
        State s = GenerateBaseState("CONTROL");
        AddHeader(s, "WALL CONTROLS");
        AddWallOrientation(s);
        AddWallSpeed(s);
        AddHeader(s, "HELP OPTIONS");
        AddHelps(s);
        hackboxHost.UpdateMemberState(contestantControl, s);
    }

    public void SendOperatorMimic(string q, string answer)
    {
        currentQ = q;
        currentA = answer;
        State s = GenerateBaseState("OPERATOR");
        string[] qData = new string[2] { q, answer };
        AddQuestion(s, qData);
        AddMark(s);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void SendHostLaunchGame()
    {
        State s = GenerateBaseState("OPERATOR");
        AddLaunchGame(s);
        hackboxHost.UpdateMemberState(operatorControl, s);
    }

    public void RefreshContestant()
    {
        if(contestantControl != null)
            SendContestantControls();
    }

    public void RefreshOperator()
    {
        SendOperatorMimic(currentQ, currentA);
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
            switch(mes.Value)
            {
                case "LAUNCH":
                    State s = GenerateBaseState("OPERATOR");
                    AddHeader(s, "LAUNCHING WALL");
                    hackboxHost.UpdateMemberState(operatorControl, s);
                    Operator.Get.StartTheWall();
                    break;

                case "CORRECT":
                    Operator.Get.Correct();
                    break;

                case "INCORRECT":
                    Operator.Get.Incorrect();
                    break;

                default:
                    RefreshOperator();
                    break;
            }
        }
    }
}
