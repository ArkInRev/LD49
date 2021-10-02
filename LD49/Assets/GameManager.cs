using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }




    public bool _desklampState = false;
    public bool _controlLampState = false;
    public bool _reactorLampState = false;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        Debug.Log("Desklamp toggled " + _desklampState.ToString());
    }

    public void pressControlRoomLightsButton()
    {
        _controlLampState = !_controlLampState;
        ButtonToggle(2, _controlLampState);
        Debug.Log("Control Room Lights toggled " + _controlLampState.ToString());
    }

    public void pressReactorRoomLightsButton()
    {
        _reactorLampState = !_reactorLampState;
        ButtonToggle(3, _reactorLampState);
        Debug.Log("Reactor Room Lights toggled " + _reactorLampState.ToString());
    }


}



