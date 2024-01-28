using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PrizeField : MonoBehaviour
{
    public GameObject obj;
    public TMP_InputField field;
    public TextMeshProUGUI label;
    public int index;

    public void OnChangeValue(string s)
    {
        (MainMenuManager.Get.GetGameplayConfig() as GameplayConfigManager).OnChangePrizeValue(index, s);
    }
}
