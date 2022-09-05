using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    private GameObject Vehicle;
    private GameObject cameraLookAt;
    private GameObject targetPosition;
    private carController CarC;
    private float speed;
    private float defaltFOV, desiredFOV;

    private void Start() {
        Vehicle = GameObject.FindGameObjectWithTag("Vehicle");
        CarC = Vehicle.GetComponent<carController>();
        targetPosition = Vehicle.transform.Find("cameraPosition").gameObject;
        cameraLookAt = Vehicle.transform.Find("cameraLook").gameObject;
        defaltFOV = Camera.main.fieldOfView;
        desiredFOV = defaltFOV + (CarC.KPH / 30);
    }

    // Update is called once per frame
    private void FixedUpdate() {
        follow();
        cameraFOV();
        speed = (CarC.KPH >= 30) ? 10 : CarC.KPH / 3;
    }

    private void follow() {
        gameObject.transform.position = Vector3.Lerp(transform.position, targetPosition.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(cameraLookAt.gameObject.transform.position);
    }

    private void cameraFOV(){
        Camera.main.fieldOfView = defaltFOV + (CarC.KPH / 10);
    }
}
