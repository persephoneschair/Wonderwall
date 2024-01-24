using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    void Start()
    {
        SwitchCamOn();
    }

    public void SwitchCamOn()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        GetComponent<RawImage>().texture = webcamTexture;
        webcamTexture.Play();
    }
}
