using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenuManager : MonoBehaviour
{
    public MenuButton parentButton;

    public virtual void Start()
    {
        parentButton.GetComponent<Button>().interactable = false;
    }

    public virtual void OnRoomConnected()
    {
        parentButton.GetComponent<Button>().interactable = true;
    }
}
