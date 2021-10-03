using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }



    private float _tickElapsed = 0f;
    private float _ticksPerSecond = 1f;
    private float _distanceHome = 1000000f;
    [SerializeField]   
    private float _shipSpeedFactor = 100f;


    private float _coreTemp = 1400f;
    private float _coreMeltTemp = 2800f;
    private float _coreMaxTemp = 3800f;
    private float _coreEffectiveMeltTemp = 2800f;
    private float _coreMinimumTemp = 1000f;
    private float _coreMass = 100000f;
    [SerializeField]
    private float _coreWasteMass = 0f;
    private float _coreBaseWaste = 1f;
    private float _coreWasteCleanup = 5f;


    private float _coolantFlow = 160;
    private float _coolantReactorFlow;
    private float _coolantLifeSupportFlow;
    [SerializeField]
    private float _coolantLifeSupportFactor;
    [SerializeField]
    private float _coolantMultiplier = 3f;
    private float _coolantCistern = 100000f;
    private float _coolantContaminant = 0f;
    [SerializeField]
    private float _coolantContaminationFactor = 10f;

    private float _pressureStandard = 1000;
    [SerializeField]
    private float _pressureCurrent = 1000;
    [SerializeField]
    private float _pressureValveMax = 100;


    public bool _desklampState = false;
    public bool _controlLampState = false;
    public bool _reactorLampState = false;


    private float _enginePower;
    private float _lifeSupportPower;
    private float _aiPower;
    private float _coolantPower;
    private float _wastePower;

    private float o2 = 100f;
    private float o2Max = 100f;
    private float o2perTick = 10f;
    private float o2MultiplierPerTick = 0.5f;


    [SerializeField]
    private Slider engineSlider;
    private float _engineSliderAmount;
    private float _engineIdlePower = 20f;
    private float _enginePowerRange = 180f;

    [SerializeField]
    private Slider lifeSupportSlider;
    private float _lifeSupportSliderAmount;
    private float _lifeSupportIdlePower = 10f;
    private float _lifeSupportPowerRange = 40f;

    [SerializeField]
    private Slider aiSlider;
    private float _aiSliderAmount;
    private float _aiIdlePower = 5f;
    private float _aiPowerRange = 195f;

    [SerializeField]
    private Slider coolantSlider;
    private float _coolantSliderAmount;
    private float _coolantIdlePower = 5f;
    private float _coolantPowerRange = 45f;

    [SerializeField]
    private Slider pressureSlider;

    [SerializeField]
    private int[] controlArmButtonIDs;
    [SerializeField]
    private bool[] controlArmButtonStatuses;


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


    // Start is called before the first frame update asdas
    void Start()
    {
        enginePowerChange();
        lifeSupportPowerChange();
        aiPowerChange();
        coolantPowerChange();
        powerChange();
        coreTempChange();
        tempChange();
        wasteChange();
        o2Change();
        coolantFlowChange();
        distanceChange();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _tickElapsed += Time.fixedDeltaTime;
        if (_tickElapsed >= (1 / _ticksPerSecond))
        {
            //Debug.Log("Tick");
            o2Handler();
            fuelHandler(); // calculate waste and efficiency in the core
            coolantHandler(); // coolant flow handling
            heatHandler(); // heating and cooling calculated.
            distanceHandler();




            _tickElapsed = 0;
        }
    }

    private void distanceHandler()
    {
        _distanceHome = Mathf.Clamp(_distanceHome - (GetEnginePower() * _shipSpeedFactor),0,1000000);
        distanceChange();
    }

    private void o2Handler()
    {
        o2 = Mathf.Clamp(o2 + (GetLifeSupportPower() * o2MultiplierPerTick) - o2perTick,0,o2Max);
        o2Change();
    }

    private void fuelHandler()
    {
        float fuelConsumed = GetShipPower();
        float fuelWaste = fuelConsumed * _coreBaseWaste *_coreMass / (_coreMass - _coreWasteMass);
        _coreWasteMass = _coreWasteMass + fuelWaste + (fuelWaste*pressureSlider.value);
        _wastePower = (GetEnginePower()+GetLifeSupportPower()+GetAIPower()+GetCoolantPower())*(_coreWasteMass / _coreMass);
        wasteChange();
    }

    private void coolantHandler()
    {
        _coolantFlow = 50 + (GetCoolantPower() * _coolantMultiplier);
        _coolantLifeSupportFlow = _coolantLifeSupportFactor * GetLifeSupportPower();
        _coolantReactorFlow = (_coolantFlow - _coolantLifeSupportFlow) *(1-(GetContaminant()/100));
        float pctCoreWaste = (_coreWasteMass / _coreMass);
        float contaminating = _coolantReactorFlow * pctCoreWaste * _coolantContaminationFactor;

        float pctContaminated = _coolantContaminant / _coolantCistern;
        float dirtyAmount = contaminating * pctContaminated;
        _pressureCurrent = Mathf.Clamp(_pressureCurrent + dirtyAmount, _pressureStandard, 3 * _pressureStandard);

        _coolantContaminant += (contaminating-dirtyAmount);

        _pressureCurrent = Mathf.Clamp(_pressureCurrent - (_pressureValveMax * pressureSlider.value), _pressureStandard, 3 * _pressureStandard);

        coolantFlowChange();


        
    }

    private void heatHandler()
    {
        _coreEffectiveMeltTemp = _coreMeltTemp * (_pressureStandard / _pressureCurrent); // adjust based on pressure, 1000 MPa base
        _coreTemp = Mathf.Clamp( _coreTemp + GetShipPower() - _coolantReactorFlow,_coreMinimumTemp,_coreMaxTemp);
        tempChange();
        coreTempChange();
    }

    public event Action<int,bool> onButtonToggle;
    public void ButtonToggle(int buttonID, bool buttonState)
    {
        if(onButtonToggle != null)
        {
            onButtonToggle(buttonID, buttonState);
        }
    }


    public void pressDeskLampButton()
    {
        _desklampState = !_desklampState;     
        ButtonToggle(1, _desklampState);
//        Debug.Log("Desklamp toggled " + _desklampState.ToString());
    }

    public void pressControlRoomLightsButton()
    {
        _controlLampState = !_controlLampState;
        ButtonToggle(2, _controlLampState);
//        Debug.Log("Control Room Lights toggled " + _controlLampState.ToString());
    }

    public void pressReactorRoomLightsButton()
    {
        _reactorLampState = !_reactorLampState;
        ButtonToggle(3, _reactorLampState);
//        Debug.Log("Reactor Room Lights toggled " + _reactorLampState.ToString());
    }


    public event Action<int, float> onSliderChange;
    public void SliderChange(int sliderID, float amount)
    {
        if (onSliderChange != null)
        {
            onSliderChange(sliderID, amount);
        }
    }

    public event Action onEnginePowerChange;
    public void enginePowerChange()
    {
        int id = 1;
        _enginePower = _engineIdlePower + (engineSlider.value * _enginePowerRange);
        SliderChange(id, engineSlider.value);
        powerChange();
        if (onEnginePowerChange != null)
        {
            onEnginePowerChange();
        }

    }

    public float GetEnginePower()
    {
        return _enginePower;
    }

    public event Action onLifeSupportPowerChange;
    public void lifeSupportPowerChange()
    {
        int id = 2;
        _lifeSupportPower = _lifeSupportIdlePower + (lifeSupportSlider.value * _lifeSupportPowerRange);
        SliderChange(id, lifeSupportSlider.value);
        powerChange();
        if (onLifeSupportPowerChange != null)
        {
            onLifeSupportPowerChange();
        }

    }

    public float GetLifeSupportPower()
    {
        return _lifeSupportPower;
    }

    public float GetShipPower()
    {
        return _enginePower + _lifeSupportPower + _aiPower + _coolantPower +_wastePower;
    }
    public event Action onPowerChange;
    public void powerChange()
    {
        if (onPowerChange != null)
        {
            onPowerChange();
        }

    }

    public event Action onAIPowerChange;
    public void aiPowerChange()
    {
        int id = 3;
        _aiPower = _aiIdlePower + (aiSlider.value * _aiPowerRange);
        SliderChange(id, aiSlider.value);
        powerChange();
        if (onAIPowerChange != null)
        {
            onAIPowerChange();
        }

    }

    public float GetAIPower()
    {
        return _aiPower;
    }

    public event Action onCoolantPowerChange;
    public void coolantPowerChange()
    {
        int id = 4;
        _coolantPower = _coolantIdlePower + (coolantSlider.value * _coolantPowerRange);
        SliderChange(id, coolantSlider.value);
        coolantFlowChange();
        powerChange();
        if (onCoolantPowerChange != null)
        {
            onCoolantPowerChange();
        }

    }

    public float GetCoolantPower()
    {
        return _coolantPower;
    }


    public event Action onCoreTempChange;
    public void coreTempChange()
    {

        tempChange();
        if (onCoreTempChange != null)
        {
            onCoreTempChange();
        }

    }

    public float GetCoreTemp()
    {
        return _coreTemp;
    }

    public float GetCoreEffectiveMeltTemp()
    {
        return _coreEffectiveMeltTemp;
    }

    public event Action onTempChange;
    public void tempChange()
    {
        if (onTempChange != null)
        {
            onTempChange();
        }

    }

    public event Action onCoolantFlowChange;
    public void coolantFlowChange()
    {

        if (onCoolantFlowChange != null)
        {
            onCoolantFlowChange();
        }

    }

    public float GetCoolantFlow()
    {
        return _coolantFlow;
    }

    public event Action onWasteChange;
    public void wasteChange()
    {

        if (onWasteChange != null)
        {
            onWasteChange();
        }

    }

    public float GetWaste()
    {
        return (_coreWasteMass/_coreMass)*100;
    }

    public float GetContaminant()
    {
        return (_coolantContaminant / _coolantCistern) * 100;
    }

    public event Action onO2Change;
    public void o2Change()
    {

        if (onO2Change != null)
        {
            onO2Change();
        }

    }

    public float GetO2()
    {
        return (o2 / o2Max);
    }

    public float GetReactorFlow()
    {
        return _coolantReactorFlow;
    }

    public float GetLifeSupportFlow()
    {
        return _coolantLifeSupportFlow;
    }

    public float GetWastePower()
    {
        return _wastePower;
    }

    public event Action onDistanceChange;
    public void distanceChange()
    {

        if (onDistanceChange != null)
        {
            onDistanceChange();
        }

    }

    public float GetDistanceHome()
    {
        return _distanceHome;
    }

    public float GetPressure()
    {
        return _pressureCurrent;
    }

    public event Action onPressureValveChange;
    public void pressureValveChange()
    {
        int id = 5;
        //_aiPower = _aiIdlePower + (aiSlider.value * _aiPowerRange);
        SliderChange(id, pressureSlider.value);
        coolantFlowChange();
        if (onPressureValveChange != null)
        {
            onPressureValveChange();
        }

    }

    public float GetPressureValve()
    {
        return pressureSlider.value;
    }

    public event Action<int> onControlArmChange;
    public void controlArmChange(int controlButtonID)
    {
        int thisID = 0;
        //find the selected button in the array    
        foreach(int i in controlArmButtonIDs)
        {
            if (controlArmButtonIDs[i] == controlButtonID)
            {
                //handle this button being pressed
                thisID = i;
            }
        }
        // toggle it now
        
        controlArmButtonStatuses[thisID] = !controlArmButtonStatuses[thisID];
        //loop again to set properly

        foreach (int i in controlArmButtonIDs)
        {
            if (thisID == i)
            {
                //handle this toggle
            } else
            {
                //handle all other buttons
                controlArmButtonStatuses[i] = !controlArmButtonStatuses[thisID];
            }
            ButtonToggle(controlArmButtonIDs[i], controlArmButtonStatuses[i]);

        }



        if (onControlArmChange != null)
        {
            onControlArmChange(controlButtonID);
        }




    }

}



