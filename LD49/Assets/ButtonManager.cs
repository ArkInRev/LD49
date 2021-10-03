using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    private int buttonID;
    [SerializeField]
    private bool buttonState;
    [SerializeField]
    private MeshRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.onButtonToggle += OnButtonChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pressButton()
    {
//        buttonState = !buttonState;
//        GameManager.Instance.onButtonToggle(buttonID,buttonState);
    }

    private void OnButtonChanged(int id, bool btnState)
    {
        if(id == buttonID)
        {
            buttonState = btnState;
            if (buttonState)
            {
                rend.materials[1].SetFloat("Boolean_aba7826d9d9f436d9325e99854b66bca", 1.0f);
            }
            else
            {
                rend.materials[1].SetFloat("Boolean_aba7826d9d9f436d9325e99854b66bca", 0.0f);
            }
            

            // do other things
        }
    }

    public void OnDisable()
    {
        GameManager.Instance.onButtonToggle -= OnButtonChanged;
    }
}
