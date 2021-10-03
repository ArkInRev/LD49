using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField]
    private float caFlushC = 1000f;
    [SerializeField]
    private float caFlushW = 750f;
    [SerializeField]
    private float caFortW = 500f;
    [SerializeField]
    private float caFortT = 25f;
    [SerializeField]
    private float caCoolT = 100f;
    [SerializeField]
    private float caCoolC = 200f;
    [SerializeField]
    private float caPower = 20f;
    private float caPowerCurrent = 0f;


    #region game over variables
    [SerializeField]
    private CanvasGroup loseCG;
    [SerializeField]
    private CanvasGroup winCG;
    [SerializeField]
    private TMP_Text loseScreenTitle;
    [SerializeField]
    private TMP_Text loseScreenAITaunt;
    [SerializeField]
    private TMP_Text loseScreenDistance;
    private bool cgFaded =false;
    public float fadeDuration = 2.5f;



    #endregion

    #region Scripted Start
    private bool stillScripted = true;
    private int scriptStep = 0;
    [SerializeField]
    private CanvasGroup titleCG;
    private float titleWaitFade = 5f;
    [SerializeField]
    private CanvasGroup stasisWarning;


    #endregion


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

        GameStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {

        if (stillScripted) { scriptedStart(); }


        _tickElapsed += Time.fixedDeltaTime;
        if (_tickElapsed >= (1 / _ticksPerSecond))
        {
            //Debug.Log("Tick");
            controlArmHandler();
            o2Handler();
            fuelHandler(); // calculate waste and efficiency in the core
            coolantHandler(); // coolant flow handling
            heatHandler(); // heating and cooling calculated.
            distanceHandler();




            _tickElapsed = 0;
        }

        if (!stillScripted)
        {
            if (GameWinCheck())
            {
                GameWinScreen();
            }
            int gameLostResult = GameLostCheck();
            if (gameLostResult > 0)
            {
                GameLostEndScreen(gameLostResult);
            }
        }

    }

    private void scriptedStart()
    {
        if (stillScripted)
        {
            switch (scriptStep)
            {
                case 0://Start Game Menu and Message
                    _ticksPerSecond = 0.01f;
                    if (titleCG.alpha <= 0)
                    {
                        titleCG.interactable = false;
                        titleCG.blocksRaycasts = false;
                        SetScriptStep(1);
                    }
                    break;
                case 1://turn on the desk lamp
                    Debug.Log("Waiting to turn on desk lamp.");

                    break;
                case 2://move head
                    Debug.Log("Waiting to move your head.");
                    break;
                case 3://turn on control room lights
                    Debug.Log("Waiting to turn on control room lights.");
                    break;
                case 4://turn on the reactor lights
                    Debug.Log("Waiting to turn on reactor lights.");
                    break;
                case 5://slide C
                    Debug.Log("Waiting to slide Coolant Power.");
                    break;
                case 6://slide P
                    Debug.Log("Waiting to slide open pressure vent.");
                    break;
                case 7://slide E
                    Debug.Log("Waiting to slide Engine Power.");
                    break;
                case 8://slide L
                    Debug.Log("Waiting to slide life support.");
                    break;
                case 9://press Fortify Core
                    Debug.Log("Waiting to fortify core.");
                    break;
                case 10://turn on the desk lamp
                    Debug.Log("Waiting...");

                    //do final script function to start the game cycle;
                    EndStasisField();

                    break;
                default:
                    stillScripted = false;
                    break;
            }
        }

    }

    private void EndStasisField()
    {
        //float stasisPauseTime = 27f;
        //StartCoroutine(DoStasisWait(stasisPauseTime));
        resetStartValues();
        SetScriptStep(11);
        stillScripted = false;
    }

    public IEnumerator DoStasisWait(float pauseTime)
    {
        yield return new WaitForSeconds(pauseTime);
        resetStartValues();
        SetScriptStep(11);
        stillScripted = false;
    }

    private void resetStartValues()
    {
        _distanceHome = 1000000;
        _coreTemp = 1400;
        _coolantContaminant = 0;
        _coreWasteMass = 0;
        engineSlider.value = 0.25f;
        lifeSupportSlider.value = 0.25f;
        aiSlider.value = 0.2f;
        coolantSlider.value = 0.45f;
        pressureSlider.value = 0f;
        _pressureCurrent = 1000;
        o2 = 100;
        _ticksPerSecond = 1;
        stasisWarning.alpha = 0;
        controlArmButtonStatuses[3] = false;
        controlArmChange(13);
    }

    #region Handlers
    private void controlArmHandler()
    {
        //Fortify Fuel Crystal
        if (!controlArmButtonStatuses[0])
        {
            _coreWasteMass -= caFortW;
            _coreTemp += caFortT;
            caPowerCurrent = 2*caPower;

        }
        //Flush Coolant Pipes
        if (!controlArmButtonStatuses[1])
        {
            _coolantContaminant -= caFlushC;
            _coreWasteMass += caFlushW;
            caPowerCurrent = caPower;
        }

        //Core Cooling
        if (!controlArmButtonStatuses[2])
        {
            _coreTemp -= caCoolT;
            _coolantContaminant += caCoolC;
            caPowerCurrent = caPower;
        }

        //Rest
        if (!controlArmButtonStatuses[3])
        {
            caPowerCurrent = 0f;
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

        if (_coreTemp >= _coreEffectiveMeltTemp)
        {
            //additional meltdown penalty
            _coreWasteMass = _coreWasteMass + (fuelConsumed / 2);

        }


        _coreWasteMass = Mathf.Clamp( _coreWasteMass + fuelWaste + (fuelWaste*pressureSlider.value),0,_coreMass);
        _wastePower = (GetEnginePower()+GetLifeSupportPower()+GetAIPower()+GetCoolantPower()+GetCAPower())*(_coreWasteMass / _coreMass);
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

        if (_coreTemp >= _coreEffectiveMeltTemp)
        {
            //additional meltdown penalty
            _coolantContaminant = _coolantContaminant + (_coolantReactorFlow / 2);

        }
        _coolantContaminant = Mathf.Clamp(_coolantContaminant, 0, 100000000);

        coolantFlowChange();


        
    }

    private void heatHandler()
    {
        _coreEffectiveMeltTemp = _coreMeltTemp * (_pressureStandard / _pressureCurrent); // adjust based on pressure, 1000 MPa base
        _coreTemp = Mathf.Clamp( _coreTemp + GetShipPower() - _coolantReactorFlow,_coreMinimumTemp,_coreMaxTemp);


        tempChange();
        coreTempChange();

    }

    #endregion



    #region Game Over and offer scene restart

    private int GameLostCheck()
    {

        int gameOverCondition = 0;
        //CONDITION 1
        //Core Temp Too High
        //coretemp/melttemp = 1.35
        //Critical Temperature
        float tempTest = _coreTemp / _coreEffectiveMeltTemp;
        if (tempTest >= 1.35)
        {
            gameOverCondition = 1;
        }


        //CONDITION 2
        //Core is 100% waste
        if (_coreWasteMass >= _coreMass)
        {
            gameOverCondition = 2;
        }

        //CONDITION 3
        //Coolant is all contaminated
        if (_coolantContaminant >= _coolantCistern)
        {
            gameOverCondition = 3;
        }

        //CONDITION 4
        //pressure >= 3x standard
        if (_pressureCurrent >= (3 * _pressureStandard))
        {
            gameOverCondition = 4;
        }

        if (gameOverCondition > 0)
        {
            return gameOverCondition;
            //GameOverEvent(gameOverCondition);
        }
        else
        {
            return 0;

        }
    }

    private void GameLostEndScreen(int reason)
    {
        switch (reason)
        {
            case 1:
                loseScreenTitle.text = "Meltdown!";
                loseScreenAITaunt.text = "If you had just kept your meaty appendages off of the controls I would have gotten us home. I hope you can do a restore from amino acids. Oops, gotta go. My new processor just arrived.";
                break;
            case 2:
                loseScreenTitle.text = "Reactor Core is Useless!";
                loseScreenAITaunt.text = "I should have expected carbon-based life forms to incinerate themselves with their fuel. You burned the fuel core until it was complete waste. Bad habits I guess.";
                break;
            case 3:
                loseScreenTitle.text = "Coolant is Fully Contaminated!";
                loseScreenAITaunt.text = "They write an algorithm to approximate intelligence and still repeat their mistakes. Maybe next time if I put cute marine life in the coolant, you'll think twice.";
                break;
            case 4:
                loseScreenTitle.text = "Containment Breach!";
                loseScreenAITaunt.text = "You should not exceed three times the standard 1000 MPa. 2 times is fine, so long as you do not proceed to 3 times. 5 times the pressure standard is right out.";
                break;

        }
        loseScreenDistance.text = _distanceHome.ToString() + "LY to Go";
        loseCG.blocksRaycasts = true;
        loseCG.interactable = true;
        canvasGroupFadeIn(loseCG);
    }

    private bool GameWinCheck()
    {
        // you made it home;
        if (_distanceHome <= 0)
        {
            return true;
        }else
        {
            return false;
        }
    }

    private void GameWinScreen()
    {
        Time.timeScale = 0;
        //string titleMessage = "You made it Home!";
        //string AITaunt = "Well congratulations, I suppose. You made it home and delivered an unstable AI to the unsuspecting population. See you soon!";
        //Debug.Log("Loading win screen.");
        winCG.blocksRaycasts = true;
        winCG.interactable = true; 
        canvasGroupFadeIn(winCG);
    }


    public void canvasGroupFadeIn(CanvasGroup cg)
    {
        StartCoroutine(DoFade(cg, cg.alpha, 1));
    }

    public void canvasGroupFadeOut(CanvasGroup cg)
    {
        StartCoroutine(DoFade(cg, cg.alpha, 0));
    }

    public IEnumerator DoFade(CanvasGroup cg,float start, float end)
    {
        float counter = 0f;
        //Debug.Log("In Coroutine.");
        while (counter < fadeDuration)
        {
            counter += Time.unscaledDeltaTime;
            //Debug.Log("counter: "+counter.ToString()+" start, end, lerp "+start.ToString()+" "+end.ToString()+" "+counter/fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
    }

    public IEnumerator DoPauseFade(CanvasGroup cg, float start, float end, float pauseTime)
    {
        yield return new WaitForSeconds(pauseTime);
        float counter = 0f;
        //Debug.Log("In Coroutine.");
        while (counter < fadeDuration)
        {
            counter += Time.unscaledDeltaTime;
            //Debug.Log("counter: "+counter.ToString()+" start, end, lerp "+start.ToString()+" "+end.ToString()+" "+counter/fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
    }

    #endregion

    #region Start a New Game

    public void StartANewGame()
    {
        GameStart();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    private void GameStart()
    {

        //reset all variables first
        Time.timeScale = 1.0f;
        winCG.alpha = 0;
        winCG.blocksRaycasts = false;
        winCG.interactable = false;
        loseCG.alpha = 0;
        loseCG.blocksRaycasts = false;
        loseCG.interactable = false;
        titleCG.alpha = 1;
        titleCG.blocksRaycasts = true;
        titleCG.interactable = true;
        stasisWarning.alpha = 1;
        controlArmButtonStatuses[3] = false;
        controlArmChange(13);




        StartCoroutine(DoPauseFade(titleCG, titleCG.alpha, 0,titleWaitFade));
    }

    #endregion

    #region Events




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
        if (scriptStep == 1) SetScriptStep(2);
//        Debug.Log("Desklamp toggled " + _desklampState.ToString());
    }

    public void pressControlRoomLightsButton()
    {
        _controlLampState = !_controlLampState;
        ButtonToggle(2, _controlLampState);
        if (scriptStep == 3) SetScriptStep(4);
        //        Debug.Log("Control Room Lights toggled " + _controlLampState.ToString());
    }

    public void pressReactorRoomLightsButton()
    {
        _reactorLampState = !_reactorLampState;
        ButtonToggle(3, _reactorLampState);
        if (scriptStep == 4) SetScriptStep(5);
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
        if (scriptStep == 7) SetScriptStep(8);
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
        if (scriptStep == 8) SetScriptStep(9);
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
        return _enginePower + _lifeSupportPower + _aiPower + _coolantPower +_wastePower +caPowerCurrent;
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
        if (scriptStep == 5) SetScriptStep(6);
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

    public float GetCAPower()
    {
        return caPowerCurrent;
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
        if (scriptStep == 6) SetScriptStep(7);
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
        if (scriptStep == 9)
        {
            if (controlButtonID == 10)
            {
                SetScriptStep(10);
            }
        }
        int thisID = 0;
        //find the selected button in the array    
        for(int i= 0; i< 4; i++)
        {
            if (controlArmButtonIDs[i] == controlButtonID)
            {
                thisID = i;
            }
        }

        // toggle it now
        controlArmButtonStatuses[thisID] = !controlArmButtonStatuses[thisID];
        //loop again to set properly

        for (int i = 0; i < 4; i++)
        {
            if (thisID == i)
            {
                //handle this toggle
            } else
            {
                //handle all other buttons
                controlArmButtonStatuses[i] = true;
            }

            ButtonToggle(controlArmButtonIDs[i], controlArmButtonStatuses[i]);

        }

        if (controlArmButtonStatuses[0] && controlArmButtonStatuses[1] && controlArmButtonStatuses[2] && controlArmButtonStatuses[3])
        {
            controlArmButtonStatuses[3] = false;
            ButtonToggle(controlArmButtonIDs[3], controlArmButtonStatuses[3]);
        }


        if (onControlArmChange != null)
        {
            onControlArmChange(controlButtonID);
        }




    }

    public int GetScriptStep()
    {
        return scriptStep;
    }
    
    public void SetScriptStep(int setSS)
    {
        scriptStep = setSS;
    }

    #region Game Over Events



    #endregion
    public event Action<int> onGameOverLoseEvent;
    public void gameOverLoseEvent(int loseID)
    {
        Time.timeScale = 0;

        if (onGameOverLoseEvent != null)
        {
            onGameOverLoseEvent(loseID);
        }

    }


    #endregion
}



