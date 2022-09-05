using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Камера")]
    public GameObject[] cameras;
    public GameObject[] canvases;
    public int canvasNum = 0;
    public float lerpTime;
    public GameObject mainCamera;

    [Header("generalTopBar")]
    public GameObject generalTopBar;

    [Header("menuCanvas")]
    public GameObject menuCanvas;
    public Text balance;
    public GameObject buyButton;

    [Header("menuCanvas_maps")]
    public GameObject menuCanvas_maps;

    [Header("menuCanvas_garage")]
    public GameObject menuCanvas_garage;
    public GameObject startButton;
    
    [Header("menuCanvas_garage_upgrades")]
    public GameObject menuCanvas_garage_upgrades;

    [Header("Другое")]
    public GameObject toRotate;
    public float rotateSpeed = 5f;
    public int vehiclePointer = 0;
    public VehicleList listOfVehicles;
    // balance = currency  ;  carName + carPrice = carInfo
    public TextMeshProUGUI carName, carPrice;

    private void Awake() {
        balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";

        vehiclePointer = PlayerPrefs.GetInt("pointer");

        GameObject childObject = Instantiate(listOfVehicles.vehicles[vehiclePointer], Vector3.zero, toRotate.transform.rotation) as GameObject;
        childObject.transform.parent = toRotate.transform;
    }

    //перед самым первым кадром
    void Start(){
        generalTopBar.SetActive(true);
    }

    private void FixedUpdate() {
        cameraTranzition();
        toRotate.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    public void garageButton() {
        menuCanvas.SetActive(false);
        menuCanvas_maps.SetActive(false);
        menuCanvas_garage.SetActive(true);
        menuCanvas_garage_upgrades.SetActive(false);
        canvasNum = 1;
        cameraTranzition();
        getCarInfo();
    }
    
    public void upgradesButton() {
        menuCanvas.SetActive(false);
        menuCanvas_maps.SetActive(false);
        menuCanvas_garage.SetActive(false);
        menuCanvas_garage_upgrades.SetActive(true);
        canvasNum = 2;
        cameraTranzition();
        getCarInfo();
    }

    public void homeButton() {
        menuCanvas.SetActive(true);
        menuCanvas_maps.SetActive(false);
        menuCanvas_garage.SetActive(false);
        menuCanvas_garage_upgrades.SetActive(false);
        canvasNum = 0;
        cameraTranzition();
    }

    public void cameraTranzition() {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameras[canvasNum].transform.position, lerpTime * Time.deltaTime);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, cameras[canvasNum].transform.rotation, lerpTime * Time.deltaTime);
    }

    public void rightButton() {
        if(vehiclePointer < listOfVehicles.vehicles.Length - 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Vehicle"));
            vehiclePointer ++;
            PlayerPrefs.SetInt("pointer", vehiclePointer);
            GameObject childObject = Instantiate(listOfVehicles.vehicles[vehiclePointer], Vector3.zero, Quaternion.identity) as GameObject;
            childObject.transform.parent = toRotate.transform;
            getCarInfo();
        }
    }

    public void leftButton()
    {
        if(vehiclePointer > 0)
        {
            Destroy(GameObject.FindGameObjectWithTag("Vehicle"));
            vehiclePointer --;
            PlayerPrefs.SetInt("pointer", vehiclePointer);
            GameObject childObject = Instantiate(listOfVehicles.vehicles[vehiclePointer], Vector3.zero, Quaternion.identity) as GameObject;
            childObject.transform.parent = toRotate.transform;
            getCarInfo();
        }
    }

    public void BuyButton()
    {
        if(PlayerPrefs.GetInt("balance") >= listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carPrice){
            //списание денег с баланса
            PlayerPrefs.SetInt("balance", PlayerPrefs.GetInt("balance") - listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carPrice);
            //куплена машина или нет
            PlayerPrefs.SetString(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString(),
                                listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString());
            getCarInfo();
        }
    }

    public void startGameButton()
    {
        SceneManager.LoadScene("Desert");
    }

    private void getCarInfo()
    {
       if(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() ==
            PlayerPrefs.GetString(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString()))
        {
            carPrice.text = "";
            startButton.SetActive(true);
            buyButton.SetActive(false);
            balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";
            carName.text = listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString();
            return;
        }
        balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";
        carPrice.text = listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carPrice.ToString("") + " $ ";
        carName.text = listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString();
            startButton.SetActive(false);
            buyButton.SetActive(buyButton);
    }

    public void upgrades1()
    {
        if(PlayerPrefs.GetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade") >= 1) return;

        if(PlayerPrefs.GetInt("balance") >= 150)
        {
            PlayerPrefs.SetInt("balance" , PlayerPrefs.GetInt("balance") - 150);

            PlayerPrefs.SetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade" , 1);
        }
        balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";
    }

    public void upgrades2()
    {
        if(PlayerPrefs.GetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade") >= 2) return;
        
        if(PlayerPrefs.GetInt("balance") >= 200)
        {
            PlayerPrefs.SetInt("balance" , PlayerPrefs.GetInt("balance") - 200);

            PlayerPrefs.SetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade" , 1);
        }
        balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";
    }

    public void upgrades3()
    {
        if(PlayerPrefs.GetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade") >= 3) return;
        
        if(PlayerPrefs.GetInt("balance") >= 400)
        {
            PlayerPrefs.SetInt("balance" , PlayerPrefs.GetInt("balance") - 400);

            PlayerPrefs.SetInt(listOfVehicles.vehicles[PlayerPrefs.GetInt("pointer")].GetComponent<carController>().carName.ToString() + "upgrade" , 1);
        }
        balance.text = PlayerPrefs.GetInt("balance").ToString("") + "$";
    }
}