using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputs : MonoBehaviour{

    [HideInInspector]public carController CarC;
    [HideInInspector]public float vertical;
    [HideInInspector]public float horizontal;
    [HideInInspector]public bool handbrake;
    [HideInInspector]public bool boosting;

    private void Start() {
        CarC = GetComponent<carController>();
    }

    void Update() {
        keyboard();
    }

    public void keyboard () {
        if(CarC.neutralGear == true) {
            vertical = 0;
        } else {
            vertical = Input.GetAxis ("Vertical");
        }
        horizontal = Input.GetAxis ("Horizontal");
        handbrake = (Input.GetAxis ("Jump") != 0) ? true : false;
        boosting = (Input.GetKey (KeyCode.LeftShift)) ? true : false;
        //if (Input.GetKey (KeyCode.LeftShift)) boosting = true;
        //else boosting = false;

    }



}
