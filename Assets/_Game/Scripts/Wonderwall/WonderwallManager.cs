using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WonderwallManager : SingletonMonoBehaviour<WonderwallManager>
{
    [Header("GFX Config")]
    public float shortBoxLength = 160f;
    public float longBoxLength = 330f;

    [Header("Prize Config")]
    public string[] prizes;
    public string[] holidayPrizes;

    [Header("Time Config")]
    public float delayBeforeWallReveal = 8f;
    public float delayAfterWallReveal = 3.5f;
    public float countdownLength = 15f;
    public float delayBeforeQBoxCloses = 1f;
    public float delayBeforeQBoxReopens = 0.5f;
    public float pitstopLength = 15f;
    public float timeAvailable = 180f;
    public float enableBailOutAt = 30f;

    [Header("Gameplay Config")]
    [Range(1,20)] public int targetQs = 20;
    [Range(0,3)] public int strikesAvailable = 3;
    [Range(0,2)] public int passesAvailable = 2;
    [Range(0,2)] public int pitsAvailable = 2;
    [ShowOnly] public int questionsCorrect = 0;
    [ShowOnly] public bool answeringEnabled = false;

    private int startStrikes;
    private int startPasses;
    private int startPits;
    private bool playWithBail = true;
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

    public Image[] pitstopImages;
    public Color[] pitstopColors;

    public Animator[] helpStraps;

    [Header("Misc Scene Objects")]
    public AnswerBox[] answerBoxes;
    public Animator wonderwallAnim;
    public Animator sideBarsAnim;
    public GameObject questionBox;
    public Animator[] resultAnims;
    public TextMeshProUGUI[] resultMeshes;

    private Question _currentQuestion;
    public Question CurrentQuestion
    {
        get
        {
            return _currentQuestion; 
        }
        set
        {
            if (value != null)
            {
                //Question has changed - update host
                DebugLog.Print($"{Array.IndexOf(QuestionManager.currentPack.questions.ToArray(), value) + 1}) {value.question}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                DebugLog.Print($"[{answerBoxes.FirstOrDefault(x => x.answer == value.correctAnswer).boxIndex + 1}] {value.correctAnswer}", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
            }
            _currentQuestion = value;
        }
    }

    public void InitWall()
    {
        LatticeManager.Get.SetDefault();
        sideBarsAnim.SetTrigger("toggle");
        gameActive = true;
        answeringEnabled = false;
        wonderwallAnim.speed = 1;
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

        SetHelps();
        GlobalTimeManager.Get.OnWallStart(timeAvailable, pitstopLength);
        if (startStrikes == 0)
            playWithBail = false;
        else
            playWithBail = true;
        bailoutActive = false;
        StartCoroutine(FadeWallIn());
    }

    private void SetHelps()
    {
        startStrikes = strikesAvailable;
        startPasses = passesAvailable;
        startPits = pitsAvailable;

        for (int i = 0; i < strikesAvailable; i++)
            strikeImages[i].color = strikeColors[1];
        for (int i = strikesAvailable; i < strikeImages.Length; i++)
            strikeImages[i].gameObject.SetActive(false);

        for (int i = 0; i < passesAvailable; i++)
            passImages[i].color = passColors[1];
        for (int i = passesAvailable; i < passImages.Length; i++)
            passImages[i].gameObject.SetActive(false);

        for (int i = 0; i < pitsAvailable; i++)
            pitstopImages[i].color = pitstopColors[1];
        for (int i = pitsAvailable; i < pitstopImages.Length; i++)
            pitstopImages[i].gameObject.SetActive(false);

        strikesLabel.text = startStrikes == 0 ? "" : "Strikes";
        passesLabel.text = startPasses == 0 ? "" : "Passes";
        pitstopsLabel.text = startPits == 0 ? "" : "Pitstops";
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
        LatticeManager.Get.SetCountdown();
        yield return new WaitForSeconds(countdownLength);
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
        bailoutActive = CheckForBailout();
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
        if (!answeringEnabled || pitsAvailable == 0)
            return;
        bailoutActive = false;
        LatticeManager.Get.SetPit();
        //Update device here
        answeringEnabled = false;
        helpStraps[1].SetTrigger("toggle");
        GlobalTimeManager.Get.OnPit();
        pitstopImages[startPits - pitsAvailable].color = pitstopColors[0];
        pitsAvailable--;
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(pitsAvailable == 1 ? AudioManager.LoopClip.Theme2 : AudioManager.LoopClip.Theme3, true, pitstopLength);
        AudioManager.Get.Play(pitsAvailable == 1 ? AudioManager.OneShotClip.Pit1 : AudioManager.OneShotClip.Pit2);
        Invoke("UpdateQuestionMesh", pitstopLength);
    }

    [Button]
    public void Pass()
    {
        if (!answeringEnabled || passesAvailable == 0)
            return;
        helpStraps[0].SetTrigger("toggle");
        answeringEnabled = false;
        passImages[startPasses - passesAvailable].color = passColors[0];
        passesAvailable--;
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
        prizeMesh.text = prizes[questionsCorrect];
        questionsCorrect++;
        qCounterMesh.text = $"{Math.Min(questionsCorrect, targetQs)}/{targetQs}";
        answerBoxes.FirstOrDefault(x => x.answer == CurrentQuestion.correctAnswer).OnRevealAnswer(true);
        CurrentQuestion = QuestionManager.GetNextQuestion();
        if (questionsCorrect == targetQs)
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
        CurrentQuestion = QuestionManager.GetNextQuestion();
        if (startStrikes > 0)
        {
            strikeImages[startStrikes - strikesAvailable].color = strikeColors[0];
            strikesAvailable--;
        }
        if ((strikesAvailable == 0 && startStrikes > 0) || (startStrikes == 0 && CurrentQuestion == null))
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
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU BAILED OUT WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : prizes[questionsCorrect - 1].ToUpperInvariant())}";
    }

    private void TriggerJackpotStrap()
    {
        resultAnims[0].SetTrigger("lock");
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU COMPLETED THE WONDERWALL WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : prizes[questionsCorrect - 1].ToUpperInvariant())}";
    }

    public void TriggerTimeOutWinStrap()
    {
        resultAnims[0].SetTrigger("lock");
        resultMeshes[0].text = $"<size=200%>CONGRATULATIONS</size>\n\nYOU ANSWERED {questionsCorrect} QUESTIONS CORRECTLY\n\nYOU HAVE WON {(questionsCorrect == 0 ? "NOTHING" : prizes[questionsCorrect - 1].ToUpperInvariant())}";
    }

    private void TriggerStrikeOutStrap()
    {
        resultAnims[1].SetTrigger("lock");
        resultMeshes[1].text = $"<size=200%>BAD LUCK</size>\n\nYOU STRUCK OUT WITH {GlobalTimeManager.Get.GetFormattedTime()} REMAINING";
    }

    private void TriggerTimeOutLoseStrap()
    {
        resultAnims[1].SetTrigger("lock");
        resultMeshes[1].text = $"<size=200%>BAD LUCK</size>\n\nYOU RAN OUT OF TIME";
    }

    public bool CheckForBailout()
    {
        if (!playWithBail)
            return false;

        else if ((GlobalTimeManager.Get.GetRemainingTime() < enableBailOutAt) || strikesAvailable == 1)
        {
            LatticeManager.Get.SetBailoutActive();
            //Update player/host device here
            return true;
        }
        else
            return false;
    }

    public void GameOver(bool instantStop = false)
    {
        GlobalTimeManager.Get.OnEnd();
        StartCoroutine(FadeOutRoutine(instantStop));
    }

    IEnumerator FadeOutRoutine(bool instantStop)
    {
        if(!instantStop)
            yield return new WaitForSeconds(1f);

        if(startStrikes > 0 && strikesAvailable == 0)
        {
            TriggerStrikeOutStrap();
            LatticeManager.Get.SetLose();
        }
        else if(questionsCorrect == targetQs)
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
