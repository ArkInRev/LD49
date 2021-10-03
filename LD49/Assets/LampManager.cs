using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LampManager : MonoBehaviour
{

    [SerializeField]
    private bool invertLamp = false;
    private bool lampState = false;
    [SerializeField]
    private Light lamp;
    [SerializeField]
    private int lampID;


/*    [SerializeField]
    private Material matOff;
    [SerializeField]
    private Material matOn;
*/
    [SerializeField]
    private float lampOnIntensity = 1.28f;


    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.onButtonToggle += OnLampChanged;
        if (!invertLamp) { lamp.intensity = 0; } else { lamp.intensity = lampOnIntensity; }
    }

    public void OnLampChanged(int id, bool thisState)
    {
        if(lampID == id) //this lamp has been adjusted
        {
            lampState = thisState;
            if (lampState)
            {

                if (invertLamp)
                {
                    lamp.intensity = 0;
                }else
                {
                    lamp.intensity = lampOnIntensity;
                }
                
            } else
            {

                if (invertLamp)
                {
                    lamp.intensity = lampOnIntensity; 
                    
                }
                else
                {
                    lamp.intensity = 0;
                }
            }

        }
    }


    public void OnDisable()
    {
        GameManager.Instance.onButtonToggle -= OnLampChanged;
    }


}
