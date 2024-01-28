using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{
    private float startTime;
    private float pitTime;
    private float elapsedTime;
    private float elapsedPitTime;

    public bool clockRunning;
    private bool pitRunning;

    public Slider pitSlider;
    public Slider mainSlider;

    public Image[] leftSideLights;
    public Image[] rightSideLights;
    public Color[] sideLightColors;

    private int resumeFinalCountdown;

    private float timeSinceOperatorRefresh;

    private void Update()
    {
        if (pitRunning)
            PitClock();
        else if (clockRunning)
            QuestionTimer();
    }

    public void OnWallStart(float totalTime, float pitTime)
    {
        ToggleAllSideLights(false);
        startTime = totalTime;
        this.pitTime = pitTime;
        elapsedTime = 0;
        elapsedPitTime = 0;
        resumeFinalCountdown = 0;
        mainSlider.value = 1;
        StartCoroutine(SyncClockStart());
    }

    IEnumerator SyncClockStart()
    {
        WonderwallManager.Get.timerMesh.text = (startTime - elapsedTime < 60.01f) ? GetSecondsAndTenths() : GetMinutesAndSeconds();
        yield return new WaitUntil(() => WonderwallManager.Get.CurrentQuestion != null);
        OnPit();
        clockRunning = true;
    }

    public void OnPit()
    {
        if (AudioManager.Get.finalCountdown.isPlaying)
            AudioManager.Get.finalCountdown.Pause();
        pitSlider.value = 1;
        pitRunning = true;
        StartCoroutine(PitLightsRoutine());
    }

    private void ToggleAllSideLights(bool on, int final = 0)
    {
        StartCoroutine(ToggleRoutine(on, final));
    }

    private IEnumerator ToggleRoutine(bool on, int final)
    {
        for (int i = leftSideLights.Length - 1; i >= 0; i--)
        {
            leftSideLights[i].color = sideLightColors[on ? 1 + final : 0];
            rightSideLights[i].color = sideLightColors[on ? 1 + final : 0 + final];
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator PitLightsRoutine()
    {
        ToggleAllSideLights(true);
        for(int i = 0; i < leftSideLights.Length; i++)
        {
            yield return new WaitForSeconds(1f);
            leftSideLights[i].color = sideLightColors[0];
            rightSideLights[i].color = sideLightColors[0];
        }
    }

    private IEnumerator FinalLightsRoutine(int fringe = 0)
    {
        if(fringe == 0)
            ToggleAllSideLights(true, 2);
        else
        {
            yield return new WaitForSeconds(0.5f);
            for (int i = leftSideLights.Length - 1; i >= 0; i--)
            {
                leftSideLights[i].color = sideLightColors[i >= fringe ? 3 : 2];
                rightSideLights[i].color = sideLightColors[i >= fringe ? 3 : 2];
                yield return new WaitForSeconds(0.02f);
            }
        }

        for (int i = fringe; i < leftSideLights.Length; i++)
        {
            yield return new WaitForSeconds(1f);
            if (pitRunning || !clockRunning)
            {
                resumeFinalCountdown = i + 1;
                yield break;
            }
            leftSideLights[i].color = sideLightColors[2];
            rightSideLights[i].color = sideLightColors[2];
        }
    }

    public void OnEnd()
    {
        clockRunning = false;
    }

    private void QuestionTimer()
    {
        elapsedTime += (1f * Time.deltaTime);
        timeSinceOperatorRefresh += (1f * Time.deltaTime);
        WonderwallManager.Get.timerMesh.text = (startTime - elapsedTime < 60.01f) ? GetSecondsAndTenths() : GetMinutesAndSeconds();
        mainSlider.value = 1f - (elapsedTime / startTime);

        if(timeSinceOperatorRefresh > PersistenceManager.CurrentGameplayConfig.OperatorRefreshInterval)
        {
            timeSinceOperatorRefresh = 0f;
            WonderwallManager.Get.UpdateOperatorMimic();
        }            

        if ((GetRemainingTime() < PersistenceManager.CurrentGameplayConfig.EnableBailOutAt) && !WonderwallManager.Get.bailoutActive && WonderwallManager.Get.playWithBail)
        {
            WonderwallManager.Get.bailoutActive = WonderwallManager.Get.CheckForBailout();
            WonderwallManager.Get.UpdateOperatorMimic();
        }            
        if (GetRemainingTime() < 15f && !AudioManager.Get.finalCountdown.isPlaying)
        {
            AudioManager.Get.finalCountdown.Play();
            StartCoroutine(FinalLightsRoutine());
        }
        if (elapsedTime >= startTime)
        {
            clockRunning = false;
            WonderwallManager.Get.TimeUpPermaClose();
            WonderwallManager.Get.GameOver(true);
        }
    }

    private void PitClock()
    {
        elapsedPitTime += (1f * Time.deltaTime);
        WonderwallManager.Get.timerMesh.text = GetPitTime();
        pitSlider.value = 1f - (elapsedPitTime / pitTime);
        if (elapsedPitTime >= pitTime)
        {
            //If pit is taking place during final countdown
            if (startTime - elapsedTime < 15f && !AudioManager.Get.finalCountdown.isPlaying)
            {
                AudioManager.Get.finalCountdown.UnPause();
                StartCoroutine(FinalLightsRoutine(resumeFinalCountdown));
            }                
            pitRunning = false;
            elapsedPitTime = 0;
        }
    }

    public string GetFormattedTime()
    {
        float f = GetRemainingTime();

        int minutes = Mathf.FloorToInt(f / 60f);
        int seconds = Mathf.RoundToInt(f % 60f);
        return $"{minutes}:{seconds.ToString("00")}";
    }

    public float GetRemainingTime()
    {
        return startTime - elapsedTime;
    }

    public string GetMinutesAndSeconds()
    {
        return TimeSpan.FromSeconds(startTime - elapsedTime).ToString(@"m\:ss");
    }

    public string GetSecondsAndTenths()
    {
        return TimeSpan.FromSeconds(startTime - elapsedTime).ToString(@"s\.f");
    }

    public string GetPitTime()
    {
        return TimeSpan.FromSeconds(pitTime - elapsedPitTime).ToString(@"s\.f");
    }
}
