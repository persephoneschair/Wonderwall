using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    public RawImage im;

    public void OnEnable()
    {
        WebCamTexture tx = new WebCamTexture();
        im.texture = tx;
        tx.Play();
    }

    public void OnDisable()
    {
        WebCamTexture tx = (WebCamTexture)im.mainTexture;
        tx.Stop();       
    }
}
