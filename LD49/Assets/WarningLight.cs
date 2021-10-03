using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLight : MonoBehaviour
{

    [SerializeField]
    private int buttonID;
    [SerializeField]
    private MeshRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
//        GameManager.Instance.onWarningLightChange += OnWarningLightChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDisable()
    {
 //       GameManager.Instance.onWarningLightChange -= OnWarningLightChange;
    }

    private void OnWarningLightChange()
    {

    }
}
