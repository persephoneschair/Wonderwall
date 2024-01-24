using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerBox : MonoBehaviour
{
    public enum Bank
    {
        LeftBank,
        CentralBank,
        RightBank
    };

    public Bank bank;

    [ShowOnly] public int boxIndex;
    public bool shortBox;
    public bool longBox;
    public string answer;
    public TextMeshProUGUI numberMesh;
    public TextMeshProUGUI answerMesh;

    public bool revealed;
    public Animator anim;

    public void Init(string ans, int index)
    {
        RectTransform rt = answerMesh.rectTransform;
        float defaultHeight = rt.sizeDelta.y;
        revealed = false;
        answer = ans;
        boxIndex = index;
        numberMesh.text = (boxIndex + 1).ToString();
        if (shortBox)
            rt.sizeDelta = new Vector2(WonderwallManager.Get.shortBoxLength, defaultHeight);
        else if (longBox)
            rt.sizeDelta = new Vector2(WonderwallManager.Get.longBoxLength, defaultHeight);

        if(index % 2 == 0)
            answerMesh.text = ans;
        else
            answerMesh.text = ans;
    }

    public void ToggleBoxFade()
    {
        anim.SetTrigger("toggle");
    }
    
    public void OnRevealAnswer(bool correct)
    {
        if (revealed)
            return;

        revealed = true;
        AudioManager.Get.Play(correct ? AudioManager.OneShotClip.Correct : AudioManager.OneShotClip.Incorrect);
        anim.SetTrigger(correct ? "correct" : "incorrect");
        WonderwallManager.Get.wonderwallAnim.SetTrigger(bank == Bank.LeftBank ? "left" : bank == Bank.CentralBank ? "centre" : "right");
    }

    [Button]
    public void OnCorrect()
    {
        OnRevealAnswer(true);
    }

    [Button]
    public void OnIncorrect()
    {
        OnRevealAnswer(false);
    }
}
