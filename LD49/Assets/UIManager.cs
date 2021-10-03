using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    public TMP_Text shipPowerText;
    public TMP_Text enginePowerText;
    public TMP_Text lifeSupportPowerText;
    public TMP_Text aiPowerText;
    public TMP_Text coolantPowerText;
    public TMP_Text caPowerText;
    public TMP_Text wastePowerText;


    public TMP_Text coreTempText;
    public TMP_Text coreMeltingTempText;


    public TMP_Text coolantFlowText;
    public TMP_Text coolantReactorFlow;
    public TMP_Text coolantLifeSupportFlow;
    public TMP_Text coolantContaminant;
    public TMP_Text pressureCurrentText;



    public TMP_Text coreWasteText;

    public ParticleSystem o2PS;

    public TMP_Text distanceHomeText;



    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.onPowerChange += OnPowerChange;
        GameManager.Instance.onEnginePowerChange += OnEnginePowerChange;
        GameManager.Instance.onLifeSupportPowerChange += OnLifeSupportPowerChange;
        GameManager.Instance.onAIPowerChange += OnAIPowerChange;
        GameManager.Instance.onCoolantPowerChange += OnCoolantPowerChange;
        GameManager.Instance.onPressureValveChange += OnPressureChange;
        GameManager.Instance.onCoreTempChange += OnCoreTempChange;
        GameManager.Instance.onCoolantFlowChange += OnCoolantFlowChange;
        GameManager.Instance.onWasteChange += OnWasteChange;
        GameManager.Instance.onO2Change += OnO2Change;
        GameManager.Instance.onDistanceChange += OnDistanceChange;

    }

    private void OnEnginePowerChange()
    {
        enginePowerText.text = "Engine: "+GameManager.Instance.GetEnginePower().ToString("F3") +"GWph";
    }

    private void OnLifeSupportPowerChange()
    {
        lifeSupportPowerText.text = "Life: " + GameManager.Instance.GetLifeSupportPower().ToString("F3") + "GWph";
    }
    private void OnPowerChange()
    {
        shipPowerText.text = "Power: " + GameManager.Instance.GetShipPower().ToString("F3") + "GWph";
        caPowerText.text = "Arm: " + GameManager.Instance.GetCAPower().ToString("F3") + "GWph";
        wastePowerText.text = "Loss: " + GameManager.Instance.GetWastePower().ToString("F3") + "GWph";


    }
    private void OnAIPowerChange()
    {
        aiPowerText.text = "AI: " + GameManager.Instance.GetAIPower().ToString("F3") + "GWph";
    }

    private void OnCoolantPowerChange()
    {
        coolantPowerText.text = "Coolant: " + GameManager.Instance.GetCoolantPower().ToString("F3") + "GWph";
    }
    private void OnPressureChange()
    {
        pressureCurrentText.text = "Pressure: " + GameManager.Instance.GetPressure().ToString("F3") + "MPA";
    }

    private void OnCoreTempChange()
    {
        coreTempText.text = "Core Temp: " + GameManager.Instance.GetCoreTemp().ToString("F1") + "°C";
        coreMeltingTempText.text = "Core Melt: " + GameManager.Instance.GetCoreEffectiveMeltTemp().ToString("F1") + "°C";

    }

    private void OnCoolantFlowChange()
    {
        coolantFlowText.text = "Coolant: " + GameManager.Instance.GetCoolantFlow().ToString("F1") + "Lps";
        coolantReactorFlow.text = "Reactor Flow: " + GameManager.Instance.GetReactorFlow().ToString("F1") + "Lps";
        coolantLifeSupportFlow.text = "Life Support: " + GameManager.Instance.GetLifeSupportFlow().ToString("F1") + "Lps";
        coolantContaminant.text = "Contaminated: " + GameManager.Instance.GetContaminant().ToString("F2") + "%";
        pressureCurrentText.text = "Pressure: " + GameManager.Instance.GetPressure().ToString("F3") + "MPA";



    }
    private void OnWasteChange()
    {
        coreWasteText.text = "Waste: " + GameManager.Instance.GetWaste().ToString("F2") + "%";
    }

    private void OnDistanceChange()
    {
        distanceHomeText.text = "Home: " + GameManager.Instance.GetDistanceHome().ToString("F2") + " LY";
    }


    private void OnO2Change()
    {
        var o2emission = o2PS.emission;
        o2emission.rateOverTime = GameManager.Instance.GetLifeSupportPower();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        GameManager.Instance.onPowerChange -= OnPowerChange;
        GameManager.Instance.onEnginePowerChange -= OnEnginePowerChange;
        GameManager.Instance.onLifeSupportPowerChange -= OnLifeSupportPowerChange;
        GameManager.Instance.onAIPowerChange -= OnAIPowerChange;
        GameManager.Instance.onCoolantPowerChange -= OnCoolantPowerChange;
        GameManager.Instance.onPressureValveChange -= OnPressureChange;
        GameManager.Instance.onCoreTempChange -= OnCoreTempChange;
        GameManager.Instance.onCoolantFlowChange -= OnCoolantFlowChange;
        GameManager.Instance.onWasteChange -= OnWasteChange;
        GameManager.Instance.onO2Change -= OnO2Change;
        GameManager.Instance.onDistanceChange -= OnDistanceChange;

    }

}
