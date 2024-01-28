using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WonderwallManager : SingletonMonoBehaviour<WonderwallManager>
{
    [Header("GFX Config")]
    public float shortBoxLength = 160f;
    public float longBoxLength = 330f;

    [Header("Time Config")]
    public float delayBeforeWallReveal = 8f;
    public float delayAfterWallReveal = 3.5f;
    public float countdownLength = 15f;
    public float delayBeforeQBoxCloses = 1f;
    public float delayBeforeQBoxReopens = 0.5f;
    public float pitstopLength = 15f;

    [Header("Gameplay Data")]
    public string playerName = "PLAYER";
    [ShowOnly] public int questionsCorrect = 0;
    [ShowOnly] public bool answeringEnabled = false;

    [ShowOnly] public int currentStrikes;
    [ShowOnly] public int currentPasses;
    [ShowOnly] public int currentPits;

    [ShowOnly] public bool playWithBail = true;
    [ShowOnly] public bool bailoutActive;
    [ShowOnly] public bool gameActive;

    [Header("Text Meshes")]
    public TextMeshProUGUI questionMesh;
    public TextMeshProUGUI nameMesh;
    public TextMeshProUGUI qCounterMesh;
    public TextMeshProUGUI prizeMesh;
    public TextMeshProUGUI timerMesh;

    public TextMeshProUGUI strikesLabel;
    public TextMeshProUGUI passesLabel;
    public TextMeshProUGUI pitstopsLabel;

    [Header("Helps")]
    public Image[] strikeImages;
    public Color[] strikeColors;

    public Image[] passImages;
    public Color[] passColors;
    private bool triggerPassAlert = false;

    public Image[] pitstopImages;
    public Color[] pitstopColors;

    public Animator[] helpStraps;

    [Header("Misc Scene Objects")]
    public AnswerBox[] answerBoxes;
    public Animator wonderwallAnim;
    public Animator sideBarsAnim;
    public GameObject questionBox;
    public Animator[] resultAnims;
    [HideInInspector] public Animator triggeredStrap;
    public TextMeshProUGUI[] resultMeshes;
    public GameObject noWebcamPlaceholder;

    private Question _currentQuestion;
    public Question CurrentQuestion
    {
        get
        {
            return _currentQuestion; 
        }
        set
        {
            _currentQuestion = value;
            if (value != null)
            {
                UpdateOperatorMimic(triggerPassAlert ? "PASSED" : "");
                triggerPassAlert = false;
                DebugLog.Print($"{Array.IndexOf(QuestionManager.currentPack.questions.ToArray(), value) + 1}) {value.question}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                DebugLog.Print($"[{answerBoxes.FirstOrDefault(x => x.answer == value.correctAnswer).boxIndex + 1}] {value.correctAnswer}", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
            }
        }
    }

    public void UpdateOperatorMimic(string alertMessage = "")
    {
        string qData = GetQData(CurrentQuestion);
        string statsData = GetStatsString(CurrentQuestion, alertMessage);
        HackboxManager.Get.SendOperatorMimic(qData, statsData);
    }

    private string GetStatsString(Question value, string topAlert = "")
    {
        string s = $"{(string.IsNullOrEmpty(topAlert) ? "" : topAlert + "<br>")}" +

                    $"{(bailoutActive ? "BAILOUT ACTIVE<br>" : "")}" +
                    $"Time: {GlobalTimeManager.Get.GetFormattedTime()}<br>" +

                    $"{(PersistenceManager.CurrentGameplayConfig.NumberOfStrikes != 0 ? $"Strikes: {currentStrikes} | " : "")}" +
                    $"{(PersistenceManager.CurrentGameplayConfig.NumberOfPasses != 0 ? $"Passes: {currentPasses} | " : "")}" +
                    $"{(PersistenceManager.CurrentGameplayConfig.NumberOfPits != 0 ? $"Pits: {currentPits} | " : "")}";

        if(PersistenceManager.CurrentGameplayConfig.NumberOfStrikes > 0 || PersistenceManager.CurrentGameplayConfig.NumberOfPasses > 0 || PersistenceManager.CurrentGameplayConfig.NumberOfPits > 0)
        {
            s = s.Substring(0, s.Length - 2);
            s += "<br>";
        }
        s += $"Qs: {QuestionManager.currentPack.questions.Count - Array.IndexOf(QuestionManager.currentPack.questions.ToArray(), value)}" +
                    $" | Correct: {questionsCorrect}/{PersistenceManager.CurrentGameplayConfig.TargetQuestions}";

        return s;
    }

    private string GetQData(Question value)
    {
        return string.Join("|", $"{value.question}", $"[{answerBoxes.FirstOrDefault(x => x.answer == value.correctAnswer).boxIndex + 1}] {value.correctAnswer}");
    }

    public void InitWall()
    {
        AudioManager.Get.StopLoop();
        noWebcamPlaceholder.SetActive(!PersistenceManager.CurrentGameplayConfig.UseWebcam);
        LatticeManager.Get.SetDefault();
        sideBarsAnim.SetTrigger("toggle");
        gameActive = true;
        answeringEnabled = false;
        questionsCorrect = 0;
        wonderwallAnim.speed = 1;
        nameMesh.text = playerName;
        qCounterMesh.text = "";
        prizeMesh.text = "";
        List<string> allAnswers = new List<string>();
        foreach(Question q in QuestionManager.currentPack.questions)
        {
            allAnswers.Add(q.correctAnswer);
            if (allAnswers.Count == 49)
                break;
            allAnswers.Add(q.incorrectAnswer);
        }
        allAnswers.Shuffle();
        for (int i = 0; i < answerBoxes.Length; i++)
            answerBoxes[i].Init(allAnswers[i], i);

        playWithBail = PersistenceManager.CurrentGameplayConfig.NumberOfStrikes != 0;
        SetHelps();
        GlobalTimeManager.Get.OnWallStart(PersistenceManager.CurrentGameplayConfig.TimeAvailable, pitstopLength);
        bailoutActive = false;

        if (PersistenceManager.CurrentGameplayConfig.ShuffleQuestionOrder)
            QuestionManager.currentPack.questions.Shuffle();

        StartCoroutine(FadeWallIn());
        HackboxManager.Get.BuildContestantState();
    }

    private void SetHelps()
    {
        currentStrikes = PersistenceManager.CurrentGameplayConfig.NumberOfStrikes;
        currentPasses = PersistenceManager.CurrentGameplayConfig.NumberOfPasses;
        currentPits = PersistenceManager.CurrentGameplayConfig.NumberOfPits;

        for (int i = 0; i < PersistenceManager.CurrentGameplayConfig.NumberOfStrikes; i++)
            strikeImages[i].color = strikeColors[1];
        for (int i = PersistenceManager.CurrentGameplayConfig.NumberOfStrikes; i < strikeImages.Length; i++)
            strikeImages[i].gameObject.SetActive(false);

        for (int i = 0; i < PersistenceManager.CurrentGameplayConfig.NumberOfPasses; i++)
            passImages[i].color = passColors[1];
        for (int i = PersistenceManager.CurrentGameplayConfig.NumberOfPasses; i < passImages.Length; i++)
            passImages[i].gameObject.SetActive(false);

        for (int i = 0; i < PersistenceManager.CurrentGameplayConfig.NumberOfPits; i++)
            pitstopImages[i].color = pitstopColors[1];
        for (int i = PersistenceManager.CurrentGameplayConfig.NumberOfPits; i < pitstopImages.Length; i++)
            pitstopImages[i].gameObject.SetActive(false);

        strikesLabel.text = PersistenceManager.CurrentGameplayConfig.NumberOfStrikes == 0 ? "" : "Strikes";
        passesLabel.text = PersistenceManager.CurrentGameplayConfig.NumberOfPasses == 0 ? "" : "Passes";
        pitstopsLabel.text = PersistenceManager.CurrentGameplayConfig.NumberOfPits == 0 ? "" : "Pitstops";
    }

    private IEnumerator FadeWallIn()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.ChairRotation, false);
        yield return new WaitForSeconds(delayBeforeWallReveal);
        AudioManager.Get.Play(AudioManager.OneShotClip.RevealAnswers);
        wonderwallAnim.SetTrigger("start");
        List<int> fadeOrder = new List<int>(Enumerable.Range(0, 49));
        fadeOrder.Shuffle();

        for (int i = 0; i < 16; i++)
            answerBoxes[fadeOrder[i]].ToggleBoxFade();
        yield return new WaitForSeconds(1.5f);
        for (int i = 16; i < 32; i++)
            answerBoxes[fadeOrder[i]].ToggleBoxFade();
        yield return new WaitForSeconds(1.5f);
        for (int i = 32; i < 49; i++)
            answerBoxes[fadeOrder[i]].ToggleBoxFade();

        yield return new WaitForSeconds(delayAfterWallReveal);

        AudioManager.Get.Play(AudioManager.OneShotClip.CountdownToStart);

        CurrentQuestion = QuestionManager.GetNextQuestion();
        UpdateOperatorMimic($"{countdownLength} SECOND COUNTDOWN...");
        LatticeManager.Get.SetCountdown();
        yield return new WaitForSeconds(countdownLength);
        UpdateOperatorMimic($"PLAY!");
        UpdateQuestionMesh();
        AudioManager.Get.Play(AudioManager.OneShotClip.StartCrack);
        AudioManager.Get.Play(AudioManager.LoopClip.Theme1, true);
    }

    public void ToggleWallSpeed()
    {
        wonderwallAnim.speed = wonderwallAnim.speed > 1.5f ? 1 : 2;
    }

    public void WallLock(int pos)
    {
        wonderwallAnim.SetTrigger(pos == 0 ? "lockLeft" : pos == 1 ? "lockCentre" : "lockRight");
    }

    private void UpdateQuestionMesh()
    {
        if (!GlobalTimeManager.Get.clockRunning)
            return;
        questionBox.SetActive(true);
        answeringEnabled = true;
        questionMesh.text = CurrentQuestion.question;
        if (!bailoutActive)
            LatticeManager.Get.SetDefault();
        else
            LatticeManager.Get.SetBailoutActive();
    }

    private void CloseQuestionBox()
    {
        questionBox.SetActive(false);
        Invoke("UpdateQuestionMesh", delayBeforeQBoxReopens);
    }

    public void TimeUpPermaClose()
    {
        PermanentlyCloseQuestionBox();
        if (playWithBail)
        {
            TriggerTimeOutLoseStrap();
            LatticeManager.Get.SetLose();
        }            
        else
        {
            TriggerTimeOutWinStrap();
            LatticeManager.Get.SetBail();
        }            
    }

    private void PermanentlyCloseQuestionBox()
    {
        questionBox.SetActive(false);
    }

    public void SetWallRToL()
    {
        wonderwallAnim.SetBool("LToR", false);
    }

    public void SetWallLToR()
    {
        wonderwallAnim.SetBool("LToR", true);
    }

    [Button]
    public void Pitstop()
    {
        if (!answeringEnabled || currentPits == 0)
            return;
        bailoutActive = false;
        LatticeManager.Get.SetPit();
        answeringEnabled = false;
        helpStraps[1].SetTrigger("toggle");
        GlobalTimeManager.Get.OnPit();
        pitstopImages[PersistenceManager.CurrentGameplayConfig.NumberOfPits - currentPits].color = pitstopColors[0];
        currentPits--;
        UpdateOperatorMimic($"{pitstopLength} SECOND PITSTOP");
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(currentPits == 1 ? AudioManager.LoopClip.Theme2 : AudioManager.LoopClip.Theme3, true, pitstopLength);
        AudioManager.Get.Play(currentPits == 1 ? AudioManager.OneShotClip.Pit1 : AudioManager.OneShotClip.Pit2);
        Invoke("UpdateQuestionMesh", pitstopLength);
        Invoke("PostPitMimic", pitstopLength);
    }

    private void PostPitMimic()
    {
        bailoutActive = CheckForBailout();
        UpdateOperatorMimic($"PLAY!");
    }

    [Button]
    public void Pass()
    {
        if (!answeringEnabled || currentPasses == 0)
            return;
        helpStraps[0].SetTrigger("toggle");
        answeringEnabled = false;
        passImages[PersistenceManager.CurrentGameplayConfig.NumberOfPasses - currentPasses].color = passColors[0];
        currentPasses--;
        triggerPassAlert = true;
        AudioManager.Get.Play(AudioManager.OneShotClip.Pass);
        CurrentQuestion = QuestionManager.GetNextQuestion();
        Invoke("CloseQuestionBox", delayBeforeQBoxCloses);
    }

    [Button]
    public void Correct()
    {
        if (CurrentQuestion == null || !answeringEnabled)
            return;
        answeringEnabled = false;
        prizeMesh.text = PersistenceManager.CurrentGameplayConfig.PrizeLadder[questionsCorrect];
        questionsCorrect++;
        qCounterMesh.text = $"{Math.Min(questionsCorrect, PersistenceManager.CurrentGameplayConfig.TargetQuestions)}/{PersistenceManager.CurrentGameplayConfig.TargetQuestions}";
        answerBoxes.FirstOrDefault(x => x.answer == CurrentQuestion.correctAnswer).OnRevealAnswer(true);
        CurrentQuestion = QuestionManager.GetNextQuestion();
        if (questionsCorrect == PersistenceManager.CurrentGameplayConfig.TargetQuestions)
        {
            Invoke("PermanentlyCloseQuestionBox", delayBeforeQBoxCloses);
            GameOver(false);
        }            
        else
            Invoke("CloseQuestionBox", delayBeforeQBoxCloses);
    }

    [Button]
    public void Incorrect()
    {
        if (CurrentQuestion == null || !answeringEnabled)
            return;
        answeringEnabled = false;

        answerBoxes.FirstOrDefault(x => x.answer == CurrentQuestion.correctAnswer).OnRevealAnswer(false);
        if (PersistenceManager.CurrentGameplayConfig.NumberOfStrikes > 0)
        {
            strikeImages[PersistenceManager.CurrentGameplayConfig.NumberOfStrikes - currentStrikes].color = strikeColors[0];
            currentStrikes--;
        }
        bailoutActive = CheckForBailout();
        CurrentQuestion = QuestionManager.GetNextQuestion();
        if ((currentStrikes == 0 && PersistenceManager.CurrentGameplayConfig.NumberOfStrikes > 0) || (PersistenceManager.CurrentGameplayConfig.NumberOfStrikes == 0 && CurrentQuestion == null))
        {
            Invoke("PermanentlyCloseQuestionBox", delayBeforeQBoxCloses);
            GameOver(false);
        }
        else
            Invoke("CloseQuestionBox", delayBeforeQBoxCloses);
    }

    [Button]
    public void BailOut()
    {
        if (!bailoutActive || !answeringEnabled)
            return;
        LatticeManager.Get.SetBail();
        answeringEnabled = false;
        PermanentlyCloseQuestionBox();
        TriggerBailStrap();
        GameOver(true);
    }

    private void TriggerBailStrap()
    {
        resultAnims[0].SetTrigger("lock");
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU BAILED OUT WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : PersistenceManager.CurrentGameplayConfig.PrizeLadder[questionsCorrect - 1].ToUpperInvariant())}";
        triggeredStrap = resultAnims[0];
    }

    private void TriggerJackpotStrap()
    {
        resultAnims[0].SetTrigger("lock");
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU COMPLETED THE WONDERWALL WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : PersistenceManager.CurrentGameplayConfig.PrizeLadder[questionsCorrect - 1].ToUpperInvariant())}";
        triggeredStrap = resultAnims[0];
    }

    public void TriggerTimeOutWinStrap()
    {
        resultAnims[0].SetTrigger("lock");
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU ANSWERED {questionsCorrect} QUESTIONS CORRECTLY\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : PersistenceManager.CurrentGameplayConfig.PrizeLadder[questionsCorrect - 1].ToUpperInvariant())}";
        triggeredStrap = resultAnims[0];
    }

    private void TriggerStrikeOutStrap()
    {
        resultAnims[1].SetTrigger("lock");
        resultMeshes[1].text = $"<size=200%>BAD LUCK</size>\n\nYOU STRUCK OUT WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING";
        triggeredStrap = resultAnims[1];
    }

    private void TriggerTimeOutLoseStrap()
    {
        resultAnims[1].SetTrigger("lock");
        resultMeshes[1].text = $"<size=200%>BAD LUCK</size>\n\nYOU RAN OUT OF TIME";
        triggeredStrap = resultAnims[1];
    }

    public bool CheckForBailout()
    {
        if (!playWithBail)
            return false;

        else if ((GlobalTimeManager.Get.GetRemainingTime() < PersistenceManager.CurrentGameplayConfig.EnableBailOutAt) || currentStrikes == 1)
        {
            LatticeManager.Get.SetBailoutActive();
            return true;
        }
        else
            return false;
    }

    public void GameOver(bool instantStop = false)
    {
        (MainMenuManager.Get.GetDBMan() as DatabaseManager).UpdateReadoutMesh();
        gameActive = false;
        GlobalTimeManager.Get.OnEnd();
        StartCoroutine(FadeOutRoutine(instantStop));
        HackboxManager.Get.SendEndGame();
    }

    IEnumerator FadeOutRoutine(bool instantStop)
    {
        if(!instantStop)
            yield return new WaitForSeconds(1f);

        if(PersistenceManager.CurrentGameplayConfig.NumberOfStrikes > 0 && currentStrikes == 0)
        {
            TriggerStrikeOutStrap();
            LatticeManager.Get.SetLose();
        }
        else if(questionsCorrect == PersistenceManager.CurrentGameplayConfig.TargetQuestions)
        {
            LatticeManager.Get.SetJackpot();
            TriggerJackpotStrap();
        }
        sideBarsAnim.SetTrigger("toggle");
        AudioManager.Get.StopLoop();
        AudioManager.Get.finalCountdown.Stop();
        AudioManager.Get.Play(AudioManager.OneShotClip.EndSting);
        List<int> fadeOrder = new List<int>(Enumerable.Range(0, 49));
        fadeOrder.Shuffle();

        for (int i = 0; i < 16; i++)
        {
            if (answerBoxes[fadeOrder[i]].revealed)
                continue;
            else
                answerBoxes[fadeOrder[i]].ToggleBoxFade();
        }            
        yield return new WaitForSeconds(0.75f);
        for (int i = 16; i < 32; i++)
        {
            if (answerBoxes[fadeOrder[i]].revealed)
                continue;
            else
                answerBoxes[fadeOrder[i]].ToggleBoxFade();
        }
        yield return new WaitForSeconds(0.75f);
        for (int i = 32; i < 49; i++)
        {
            if (answerBoxes[fadeOrder[i]].revealed)
                continue;
            else
                answerBoxes[fadeOrder[i]].ToggleBoxFade();
        }
        yield return new WaitForSeconds(1f);

        foreach (AnswerBox a in answerBoxes)
            a.revealed = false;

        wonderwallAnim.SetTrigger("end");
    }
}
