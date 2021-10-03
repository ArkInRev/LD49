using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class o2Manager : MonoBehaviour
{
    public Volume vol;
    public VolumeProfile volPro;
    public Vignette o2Vig;

    private void Start()
    {
        volPro = vol.profile;
        GameManager.Instance.onO2Change += OnO2Change;
        volPro.TryGet<Vignette>(out o2Vig);

    }

    private void OnO2Change()
    {
        float targetIntensity = Mathf.Clamp(1-GameManager.Instance.GetO2(), 0.2f, 1);
        //Debug.Log("O2 = " + GameManager.Instance.GetO2()+ "  Inverted = " + 1/GameManager.Instance.GetO2());
        o2Vig.intensity.value = targetIntensity;
    }

    private void OnDisable()
    {
        GameManager.Instance.onO2Change -= OnO2Change;
    }
}
