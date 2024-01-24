using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LatticeManager : SingletonMonoBehaviour<LatticeManager>
{
    public Material latticeMat;
    public PostProcessVolume latticePP;
    private Bloom bloomLayer;

    void Start()
    {
        if (latticePP != null)
            if (latticePP.profile.TryGetSettings(out bloomLayer))
                SetMenu();
    }

    [Button]
    public void SetMenu()
    {
        SetBloom(3f, 0.5f);
        SetShader(new Color(0.525f, 0.537f, 0.576f, 1.0f), 0.1f, 0.05f, new Vector4(0.5f, 0.5f, 0.5f, 1f), 0.25f);
    }

    [Button]
    public void SetDefault()
    {
        SetBloom(1f, 0.5f);
        SetShader(new Color(0.090f, 0.133f, 0.435f, 1.0f), 0.5f, 0.25f, new Vector4(2.5f, 0.5f, 0f, 1f), 0.5f);
    }

    [Button]
    public void SetCountdown()
    {
        SetBloom(10f, 0.5f);
        SetShader(new Color(0.525f, 0.537f, 0.576f, 1.0f), 0.5f, 0.25f, new Vector4(0.25f, 0.25f, 0.15f, 1f), 1.0f);
    }

    [Button]
    public void SetPit()
    {
        SetBloom(5f, 0.5f);
        SetShader(new Color(0.706f, 0.620f, 0.224f, 1.0f), 0.01f, 0.01f, new Vector4(0.15f, 0.15f, 0.15f, 1f), 1f);
    }

    [Button]
    public void SetBailoutActive()
    {
        SetBloom(1f, 0.5f);
        SetShader(new Color(0.616f, 0.110f, 0.204f, 1.0f), 0.5f, 0.25f, new Vector4(4.5f, 0.5f, 0f, 1f), 1f);
    }

    [Button]
    public void SetBail()
    {
        SetBloom(3f, 0.75f);
        SetShader(new Color(0.706f, 0.773f, 0.098f, 1.0f), 0.1f, 0.1f, new Vector4(1.0f, 1.0f, 1.0f, 1f), 0.75f);
    }

    [Button]
    public void SetLose()
    {
        SetBloom(1.5f, 0.5f);
        SetShader(new Color(0.773f, 0.412f, 0.098f, 1.0f), 0.05f, 0.05f, new Vector4(0.5f, 0.5f, 0.5f, 1f), 1f);
    }

    [Button]
    public void SetJackpot()
    {
        SetBloom(1.5f, 0.5f);
        SetShader(new Color(0.329f, 0.773f, 0.098f, 1.0f), 0.15f, 0.15f, new Vector4(1.5f, 1.5f, 1.5f, 1f), 1f);
    }

    private void SetBloom(float intensity, float threshold)
    {
        bloomLayer.intensity.value = intensity;
        bloomLayer.threshold.value = threshold;
    }

    private void SetShader(Color col, float rotX, float rotY, Vector4 moveSpeed, float moveMultiplier )
    {
        latticeMat.SetColor("_LineColor", col); // Change the color as needed

        latticeMat.SetFloat("_RotationX", rotX); // Set the rotation value between 0 and 1
        latticeMat.SetFloat("_RotationY", rotY); // Set the rotation value between 0 and 1

        latticeMat.SetVector("_MoveSpeed", moveSpeed); // Change the vector as needed

        latticeMat.SetFloat("_MoveSpeedMultiplier", moveMultiplier); // Set the multiplier value between 0 and 1
    }
}
