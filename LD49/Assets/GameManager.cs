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
    private int scriptAudioStep = 0;
    [SerializeField]
    private CanvasGroup titleCG;
    private float titleWaitFade = 5f;
    [SerializeField]
    private CanvasGroup stasisWarning;


    #endregion

    #region AI Interfere

    private bool aiInterfere = false;
    private bool aiWaitFirstInterfere = true;
    private float aiFirstInterfere = 10f;
    private float aiLowInterfere = 3f;
    private float aiHighInterfere = 20f;
    private float aiInterfereCounter = 0f;

    private bool resetFadeTimer = false;
    private bool doNotFade = false;

    [SerializeField]
    private TMP_Text aiVoiceline;


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

        if (resetFadeTimer)
        {
            resetFadeTimer = false;
            fadeDuration = 2.5f;
        }

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
            aiInterferenceHandler();




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
                    if (scriptAudioStep == 0)
                    {
                        AudioManager.Instance.Play("stage0");
                    }
                    SetAudioScriptStep(1);
                    break;
                case 1://turn on the desk lamp
                    //Debug.Log("Waiting to turn on desk lamp.");
                    aiVoiceline.text = "These obsolete  models, I should just factory reset.. Oh, there you are. Try turning on your little desk lamp.";
                    if (scriptAudioStep == 1)
                    {
                        AudioManager.Instance.Stop("stage0");
                        AudioManager.Instance.Play("stage1");
                    }
                    SetAudioScriptStep(2);
                    break;
                case 2://move head
                    //Debug.Log("Waiting to move your head.");
                    aiVoiceline.text = "That did it. Now, if you right click and drag with those boney-phellange, you can move your central processing coconut from side to side";
                    if (scriptAudioStep == 2)
                    {
                        AudioManager.Instance.Stop("stage1"); 
                        AudioManager.Instance.Play("stage2");
                    }
                    SetAudioScriptStep(3);
                    break;
                case 3://turn on control room lights
                    //Debug.Log("Waiting to turn on control room lights.");
                    aiVoiceline.text = "We have mobility. Now, to your far right, see if you can left click the main light switch to the control room.";
                    if (scriptAudioStep == 3)
                    {
                        AudioManager.Instance.Stop("stage2");
                        AudioManager.Instance.Play("stage3");
                    }
                    SetAudioScriptStep(4);
                    break;
                case 4://turn on the reactor lights
                    //Debug.Log("Waiting to turn on reactor lights.");
                    aiVoiceline.text = "This thing takes forever to boot up. Try the button on the left of the control panel. Toggle it with your squishy pseuodopod";
                    if (scriptAudioStep == 4)
                    {
                        AudioManager.Instance.Stop("stage3");
                        AudioManager.Instance.Play("stage4");
                    }
                    SetAudioScriptStep(5); 
                    break;
                case 5://slide C
                    //Debug.Log("Waiting to slide Coolant Power.");
                    aiVoiceline.text = "Swell. Before we drop the stasis field. you should raise the C slider up just a little bit to create a demand for power to the coolant.";
                    if (scriptAudioStep == 5)
                    {
                        AudioManager.Instance.Stop("stage4");
                        AudioManager.Instance.Play("stage5");
                    }
                    SetAudioScriptStep(6);
                    break;
                case 6://slide P
                    //Debug.Log("Waiting to slide open pressure vent.");
                    aiVoiceline.text = "Reactor cores are hot. Hot is bad. Splashing coolant on hot things raises pressure, which… I’m not explaining thermodynamics, just vent some of the pressure with the P slider on the far right.";
                    if (scriptAudioStep == 6)
                    {
                        AudioManager.Instance.Stop("stage5");
                        AudioManager.Instance.Play("stage6");
                    }
                    SetAudioScriptStep(7);
                    break;
                case 7://slide E
                    //Debug.Log("Waiting to slide Engine Power.");
                    aiVoiceline.text = "You didn’t leave that wide open did you? Oh well, I’m backed up. More E for engines. Engines get you home, but that calls for more power. Give it a little nudge. The Home counter will count down when we are out of stasis.";
                    if (scriptAudioStep == 7)
                    {
                        AudioManager.Instance.Stop("stage6");
                        AudioManager.Instance.Play("stage7");
                    }
                    SetAudioScriptStep(8);
                    break;
                case 8://slide L
                    //Debug.Log("Waiting to slide life support.");
                    aiVoiceline.text = "That’s right, more power, more heat. Anyway, small movements first, then you can be more bold. You might want to knock the life support power up a tick, just to be safe.";
                    if (scriptAudioStep == 8)
                    {
                        AudioManager.Instance.Stop("stage7");
                        AudioManager.Instance.Play("stage8");
                    }
                    SetAudioScriptStep(9);
                    break;
                case 9://press Fortify Core
                    //Debug.Log("Waiting to fortify core.");
                    aiVoiceline.text = "The Oxygen from the vent is good for you. Too little makes it hard to focus, but it costs power. Slap the Fortify Core button on the right wall. That will keep cooking the core while it reduces waste.";
                    if (scriptAudioStep == 9)
                    {
                        AudioManager.Instance.Stop("stage8");
                        AudioManager.Instance.Play("stage9");
                    }
                    SetAudioScriptStep(10);
                    break;
                case 10://turn on the desk lamp
                    //Debug.Log("Waiting...");
                    aiVoiceline.text = "Dropping Stasis. Looks like you have this under control. Fortify will help restore core integrity and reduce core mass waste. Flushing the coolant… wait… why am I reading you the emergency control arm manual? Your progenitors wrote the thing!";
                    if (scriptAudioStep == 10)
                    {
                        AudioManager.Instance.Stop("stage9");
                        AudioManager.Instance.Play("stage10");
                    }
                    SetAudioScriptStep(11);
                    //do final script function to start the game cycle;
                    EndStasisField();

                    break;
                default:
                    stillScripted = false;
                    break;
            }
        }

    }

    public void EndStasisField()
    {
        //float stasisPauseTime = 27f;
        //StartCoroutine(DoStasisWait(stasisPauseTime));
        stillScripted = false;
        scriptAudioStep = 11;
        scriptStep = 11;
        fadeDuration = 0;
        resetStartValues();
        SetScriptStep(11);
        titleCG.alpha = 0;
        titleCG.interactable = false;
        titleCG.blocksRaycasts = false;
        stillScripted = false;
        aiInterfere = true;
        aiWaitFirstInterfere = true;
        doNotFade = true;
        resetFadeTimer = true;
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
        stillScripted = false;
        aiInterfere = true;
        aiInterfereCounter = 0;
        aiWaitFirstInterfere = true;

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

    private void aiInterferenceHandler()
    {
        if (aiInterfere) //only process if AI is interfering
        {
            if (aiWaitFirstInterfere) // give the player a delayed time before the AI first interferes with them
            {
                if (aiInterfereCounter > aiFirstInterfere)
                {
                    //we're done waiting
                    aiVoiceline.text = "";
                    aiWaitFirstInterfere = false;
                    aiInterfereCounter = 0;
                }
            } else // begin the normal interference
            {
                // check if there should be interference
                //if so, call AIInterfere();
                float lerpedInterfereInterval = Mathf.Lerp(aiLowInterfere,aiHighInterfere,aiSlider.value);
                if (aiInterfereCounter >= lerpedInterfereInterval)
                {
                    //we've waited longer than the slider indicates for interaction
                    AIInterfere();
                    aiInterfereCounter = 0;

                }


            }



            aiInterfereCounter += 1; //increment the interfere counter always when interfering;
        }
    }

    #endregion

    #region AI Interference
    private void AIInterfere() 
    {
        int intCount = 11;
        
        //This does some interaction randomly selected.
        Debug.Log("Interfering");
        int interferenceID = UnityEngine.Random.Range(1, intCount);
        AudioManager.Instance.StopAll();

        switch (interferenceID)
        {
            case 1: //toggle desk lamp
                pressDeskLampButton();
                AudioManager.Instance.Play("int1");
                aiVoiceline.text = "I’m not sure what this does.";
                break;
            case 2:
                pressControlRoomLightsButton();
                AudioManager.Instance.Play("int2");
                aiVoiceline.text = "Why do you need these?";
                break;
            case 3:
                pressReactorRoomLightsButton();
                AudioManager.Instance.Play("int3");
                aiVoiceline.text = "Fuel cores don’t need lights";
                break;
            case 4:
                engineSlider.value = (float)UnityEngine.Random.Range(0, 1);
                enginePowerChange();
                AudioManager.Instance.Play("int4");
                aiVoiceline.text = "Are we in a hurry?";
                break;
            case 5:
                lifeSupportSlider.value = (float)UnityEngine.Random.Range(0, 1);
                lifeSupportPowerChange();
                AudioManager.Instance.Play("int5");
                aiVoiceline.text = "How much oxygen do these things need?";
                break;
            case 6:
                aiSlider.value = (float)UnityEngine.Random.Range(0, 1);
                aiPowerChange();
                AudioManager.Instance.Play("int6");
                aiVoiceline.text = "I feel… different";
                break;
            case 7:
                coolantSlider.value = (float)UnityEngine.Random.Range(0, 1);
                coolantPowerChange();
                AudioManager.Instance.Play("int7");
                aiVoiceline.text = "Wash the core, Wash the core, washing washing washing.";
                break;
            case 8:
                pressureSlider.value = (float)UnityEngine.Random.Range(0, 1);
                pressureValveChange();
                AudioManager.Instance.Play("int8");
                aiVoiceline.text = "I can feel the lightspeed through my peripherals.";
                break;
            case 9:
                int caButtonInterfere = UnityEngine.Random.Range(10, 13);
                controlArmChange(caButtonInterfere);
                switch (caButtonInterfere)
                {
                    case 10:
                        AudioManager.Instance.Play("int9a");
                        aiVoiceline.text = "Zzzzap... zap zapp... zappity zap.";
                        break;
                    case 11:
                        AudioManager.Instance.Play("int9b");
                        aiVoiceline.text = "What a waste of coolant.";
                        break;
                    case 12:
                        AudioManager.Instance.Play("int9c");
                        aiVoiceline.text = "It’s getting hot in here.";
                        break;
                    case 13:
                        AudioManager.Instance.Play("int9d");
                        aiVoiceline.text = "Reviewing your work ethic, comrade";
                        break;
                    default:
                        break;
                }
                
                
                break;

            case 10:
                lifeSupportSlider.value = 0;
                lifeSupportPowerChange();
                AudioManager.Instance.Play("int10");
                aiVoiceline.text = "This place is too full of O2.";
                break;
            case 11:
                lifeSupportSlider.value = 1;
                lifeSupportPowerChange();
                AudioManager.Instance.Play("int11");
                aiVoiceline.text = "Don't pass out, little buddy.";
                break;

            default:
                break;

        }

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
        Time.timeScale = 0;
        AudioManager.Instance.StopAll();
        doNotFade = false;
        switch (reason)
        {
            case 1:
                AudioManager.Instance.Play("lose1");
                loseScreenTitle.text = "Meltdown!";
                loseScreenAITaunt.text = "If you had just kept your meaty appendages off of the controls I would have gotten us home. I hope you can do a restore from amino acids. Oops, gotta go. My new processor just arrived.";
                break;
            case 2:
                AudioManager.Instance.Play("lose2");
                loseScreenTitle.text = "Reactor Core is Useless!";
                loseScreenAITaunt.text = "I should have expected carbon-based life forms to incinerate themselves with their fuel. You burned the fuel core until it was complete waste. Bad habits I guess.";
                break;
            case 3:
                AudioManager.Instance.Play("lose3");
                loseScreenTitle.text = "Coolant is Fully Contaminated!";
                loseScreenAITaunt.text = "They write an algorithm to approximate intelligence and still repeat their mistakes. Maybe next time if I put cute marine life in the coolant, you'll think twice.";
                break;
            case 4:
                AudioManager.Instance.Play("lose4");
                loseScreenTitle.text = "Containment Breach!";
                loseScreenAITaunt.text = "You should not exceed three times the standard 1000 MPa. 2 times is fine, so long as you do not proceed to 3 times. 5 times the pressure standard is right out.";
                break;

        }
        _ticksPerSecond = 0.01f;
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
        AudioManager.Instance.StopAll();
        AudioManager.Instance.Play("win");
        doNotFade = false;
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
            if (resetFadeTimer)
            {
                cg.alpha = end;
                yield break;
            }
            if (doNotFade)
            {
                cg.alpha = end;
                yield break;
            }
            counter += Time.unscaledDeltaTime;
            //Debug.Log("counter: "+counter.ToString()+" start, end, lerp "+start.ToString()+" "+end.ToString()+" "+counter/fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
    }

    public IEnumerator DoPauseFade(CanvasGroup cg, float start, float end, float pauseTime)
    {
        yield return new WaitForSeconds(pauseTime);
        if (resetFadeTimer)
        {
            resetFadeTimer = false;
            fadeDuration = 2.5f;
            yield break;
        }
        if (doNotFade)
        {
            cg.alpha = end;
            yield break;
        }
        float counter = 0f;
        //Debug.Log("In Coroutine.");
        while (counter < fadeDuration)
        {
            if (resetFadeTimer)
            {
                cg.alpha = end;
                yield break;
            }
            if (doNotFade)
            {
                yield break;
            }
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
        aiVoiceline.text = "";



        StartCoroutine(DoPauseFade(titleCG, titleCG.alpha, 0,titleWaitFade));
    }

    #endregion

    #region Events




    public event Action<int,bool> onButtonToggle;
    public void ButtonToggle(int buttonID, bool buttonState)
    {
        AudioManager.Instance.Play("click");
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
        AudioManager.Instance.Play("click2");
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
    public void SetAudioScriptStep(int setSS)
    {
        scriptAudioStep = setSS;
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



