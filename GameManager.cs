using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public VehicleList list;
    public carController CarC;
    private GameObject Vehicle;
    public GameObject needle;
    public GameObject startPosition;
    public Text KPH;
    public Text gearText;
    private float desiredPosition;

    private void Awake() {
        Instantiate(list.vehicles[PlayerPrefs.GetInt("pointer")], startPosition.transform.position, startPosition.transform.rotation);
        Vehicle = GameObject.FindGameObjectWithTag("Vehicle");
        CarC = Vehicle.GetComponent<carController>();
    }

    private void FixedUpdate() {
        KPH.text = CarC.KPH.ToString("0");
        updateNeedle();
        updateGear();
    }

    public void updateNeedle() {
        needle.transform.eulerAngles = new Vector3(0, 0, (CarC.engineRPM / -100));
    }
    
    public void updateGear() {
        gearText.text = (!CarC.reverse) ? (CarC.neutralGear == true) ? "N" : (CarC.currentGear + 1).ToString("") : "R";
    }
}
