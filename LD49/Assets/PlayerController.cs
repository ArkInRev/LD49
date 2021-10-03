using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Camera mainCamera;

    private float minYRot = -44.0f;
    private float maxYRot = 44.0f;
    [SerializeField]
    private float turnSpeed = 75.0f;
    
    private bool turnLeft;
    private bool turnRight;

    private float mouseLook;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            mouseLook = 0;
        }
        if (Input.GetMouseButton(1))
        {
            mouseLook += (Input.GetAxisRaw("Mouse X") * turnSpeed);
            if (((Input.GetAxisRaw("Mouse X") * turnSpeed)>1)&&(GameManager.Instance.GetScriptStep()==2))
            {
                GameManager.Instance.SetScriptStep(3);
            }
        }
    

        if (Input.GetKey(KeyCode.LeftArrow)) { turnLeft = true; } else { turnLeft = false; }
        if (Input.GetKey(KeyCode.RightArrow)) { turnRight = true; } else { turnRight = false; }
    }

    private void FixedUpdate()
    {
        if (turnLeft)
        {
            mainCamera.transform.Rotate(0, turnSpeed * Time.fixedDeltaTime *-1, 0, Space.Self);
        }
        if (turnRight)
        {
            mainCamera.transform.Rotate(0, turnSpeed * Time.fixedDeltaTime, 0, Space.Self);
        }
        mainCamera.transform.Rotate(0, mouseLook * Time.fixedDeltaTime, 0, Space.Self);
        mouseLook = 0;

       if ((mainCamera.transform.rotation.eulerAngles.y < 360 + minYRot)&& (mainCamera.transform.rotation.eulerAngles.y > 180))mainCamera.transform.rotation = Quaternion.Euler(0,360+minYRot,0);
       if ((mainCamera.transform.rotation.eulerAngles.y > maxYRot)&&(mainCamera.transform.rotation.eulerAngles.y<180)) mainCamera.transform.rotation = Quaternion.Euler(0, maxYRot, 0);

    }

}
