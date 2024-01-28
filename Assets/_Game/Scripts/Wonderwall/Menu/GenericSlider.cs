using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueMesh;
    public bool appendSeconds;
    public bool formatAsTime;

    public void OnChangeValue()
    {
        if (formatAsTime)
            valueMesh.text = GetFormattedTime();
        else
            valueMesh.text = slider.value.ToString() + (appendSeconds ? "s" : "");
    }

    private string GetFormattedTime()
    {
        float f = slider.value;

        int minutes = Mathf.FloorToInt(f / 60f);
        int seconds = Mathf.RoundToInt(f % 60f);
        return $"{minutes}:{seconds.ToString("00")}";
    }
}
