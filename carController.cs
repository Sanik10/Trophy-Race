using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class carController : MonoBehaviour{
    //components:
    [HideInInspector]public inputs input;
    [HideInInspector]public Rigidbody rgdbody;
    [HideInInspector]public audio audioSource;

    internal enum driveType{
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }

    [Header("Двигатель")]
    public float torque = 100;
    public float maxRPM, minRPM;
    public float idleRPM = 1000;
    public float MPS;
    public float KPH;
    public float MPH;
    public float FPS;
    public AnimationCurve enginePower;
    public float engineRPM;
    public float engineLoad;
    [Range(10, 50)]public float cutOffTime;
    public float changeGearSpeed;
    [SerializeField]private driveType drive;
    [Range(0.2f,0.8f)]public float EngineSmoothTime;
    private float acceleration, totalPower;

    [Header("КПП")]
    public float[] gears;
    public float[] speedSwitch;
    public float finalDrive = 3.4f;
    public int currentGear;
    private float gearChangeRate;
    [Range(0.1f, 3)]public float shiftTime;

    [Header("Колёса")]
    [HideInInspector]public WheelCollider[] wheelColliders;
    public float ForwardStifness;
    public float SidewaysStifness;
    [HideInInspector]public Transform[] wheelTransforms;
    private Vector3 wheelPosition;
    private Quaternion wheelRotation;
    public float wheelsRPM;
    public float wheelBase;
    public float rearTrack;

    [Header("Управление")]
    private float vertical , horizontal;
    private float finalTurnAngle;
    [Range(1000, 10000)]public float brakePowerVar;
    public float radius;

    [Header("Для отладки")]
    [HideInInspector]public bool vehicleChecked = false;
    private float engineLerpValue, brakePower;
	public WheelFrictionCurve forwardFriction, sidewaysFriction;
    public float[] wheelSlip;
    public bool engineLerp;
    public bool reverse;
    public bool grounded;
    public bool neutralGear;

    [Header("Важные значения")]
    public float DownForceValue, dragAmount;
    public float currentVelocity, lastFrameVelocity, Gforce;
    private float KPHconverter = 3.6f;
    private float MPHconverter = 2.237f;
    private float FPSconverter = 3.281f;

    [Header("Информация о машине")]
    public string carName;
    public int carPrice;
    public int upgradeLevel = 0;

    [Header("Звуки")]
    [HideInInspector]public bool test;
    [HideInInspector]public bool nitroFlag = false;
    public float lastValue;
    public bool activateBurbleSound = false;


    private void Start() {
        findValues();
    }
  
    private void Update() {
        currentVelocity = rgdbody.velocity.magnitude;
        lastValue = engineRPM;

        moveCar();
        updateWheels();
    }

    private void FixedUpdate() {
        shifter();
        currentVelocity = rgdbody.velocity.magnitude;
        Gforce = (currentVelocity - lastFrameVelocity) / (Time.deltaTime * Physics.gravity.magnitude);
        lastFrameVelocity = currentVelocity;
    }

    private void findValues(){
        foreach (Transform i in gameObject.transform) {
            if(i.transform.name == "carColliders") {
                wheelColliders = new WheelCollider[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++) {
                    wheelColliders[q] = i.transform.GetChild(q).GetComponent<WheelCollider>();
                }    
            }
            if(i.transform.name == "carWheels") {
                wheelTransforms = new Transform[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++) {
                    wheelTransforms[q] = i.transform.GetChild(q);
                }    
            }
        }
        //get components:
        input = GetComponent<inputs>();
        rgdbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<audio>();
        wheelSlip = new float[wheelColliders.Length];
        vehicleChecked = true;
        print("Values complete"); 
    }

    private void moveCar( ){
        runEngine();
        steerVehicle();
    }

    private void runEngine() {
        lerpEngine();
        wheelRPM();

        vertical = input.vertical;
        acceleration = vertical > 0 ? vertical : wheelsRPM <= 1 ? vertical : 0;
        test = (lastValue > engineRPM) ? true : false;

        if(engineRPM >= maxRPM) {
            setEngineLerp(maxRPM - 1000);
        } else if(KPH > 30 && reverse) {
            setEngineLerp(maxRPM - 250);
        }

        if(!engineLerp && engineRPM < maxRPM && !reverse) {
            engineRPM = Mathf.Lerp(engineRPM, idleRPM + Mathf.Abs(wheelsRPM) * finalDrive * (gears[currentGear]), (EngineSmoothTime * 10) * Time.deltaTime);
            totalPower = enginePower.Evaluate(engineRPM) * (gears[currentGear] * finalDrive) * acceleration;
        } else if(!engineLerp && engineRPM < maxRPM && reverse) {
            engineRPM = Mathf.Lerp(engineRPM, idleRPM + Mathf.Abs(wheelsRPM) * finalDrive * ((gears[currentGear]) * 3), (EngineSmoothTime * 10) * Time.deltaTime);
            totalPower = enginePower.Evaluate(engineRPM) * (gears[currentGear] * finalDrive) * acceleration;
        }

        if(engineRPM > 4500 && vertical == 0) {
            activateBurbleSound = true;
        } else {
            activateBurbleSound = false;
        }
        
        engineLoad = Mathf.Lerp(engineLoad, vertical - ((engineRPM - 1000) / maxRPM ), (EngineSmoothTime * 10) * Time.deltaTime);
        runCar();
    }

    private void runCar() {
        if(drive == driveType.rearWheelDrive) {
            for (int i = 2; i < wheelColliders.Length; i++) {
                wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2) ;
            }
        } else if(drive == driveType.frontWheelDrive) {
            for (int i = 0; i < wheelColliders.Length - 2; i++) {
                wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2) ;
            }
        } else {
            for (int i = 0; i < wheelColliders.Length; i++) {
                wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / wheelColliders.Length;
            }
        }


        for (int i = 0; i < wheelColliders.Length; i++) {
                if (vertical < 0 && !reverse) {
                    brakePower = (wheelSlip[i] <= 0.35f) ? brakePowerVar : (wheelSlip[i] <= 0.55f) ? (brakePowerVar / 10) : 0;
                } else if(!reverse && input.handbrake) {
                    wheelColliders[2].brakeTorque = 10000;
                    wheelColliders[3].brakeTorque = 10000;
                } else if(reverse && input.handbrake) {
                    brakePower = (wheelSlip[i] <= 0.35f) ? brakePowerVar : (wheelSlip[i] <= 0.55f) ? (brakePowerVar / 10) : 0;
                } else if(vertical == 0 && KPH == 0) {
                    brakePower = 10;
                } else {
                    brakePower = 0;
                }
            wheelColliders[i].brakeTorque = brakePower;
        }

        //rgdbody.angularDrag = (KPH > 100)? KPH / 100 : 0;
        if (KPH <= 5 && KPH >= -5) {
            GetComponent<Rigidbody>().drag = 0;
        }
        rgdbody.drag = dragAmount + (KPH / 47500);

        MPS = GetComponent<Rigidbody>().velocity.magnitude;
        KPH = MPS * KPHconverter;
        MPH = MPS * MPHconverter;
        FPS = MPS * FPSconverter;
        friction();
    }

    private void steerVehicle() {
        horizontal = Mathf.Lerp(horizontal, input.horizontal, (input.horizontal != 0) ? 2 * Time.deltaTime : 3 * 2 * Time.deltaTime);
        finalTurnAngle = (KPH < 20) ? radius : (KPH < 100) ? (radius + (KPH / 4)) : (radius + (KPH / 2)); // : (KPH < 150) ? (radius + (KPH / 7.5f)) : (radius + (KPH / 6));

        if(horizontal > 0) {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (finalTurnAngle - (rearTrack / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (finalTurnAngle + (rearTrack / 2))) * horizontal;
        } else if(horizontal < 0) {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (finalTurnAngle + (rearTrack / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (finalTurnAngle - (rearTrack / 2))) * horizontal;
        } else if(horizontal == 0) {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
    }

    private bool checkSpeed() {
        if(KPH >= speedSwitch[currentGear]) {
            return true;
        } else {
            return false;
        }
    }

    private void shifter() {
        if(!allWheelsGrounded())return;
        if(currentGear < gears.Length-1 && Time.time >= gearChangeRate && !reverse && checkSpeed()){
            currentGear ++;
            neutralGear = true;
            Invoke("neutralGearV", shiftTime);
            audioSource.gearUpAudio();
            audioSource.backFireAudio();
            setEngineLerp(engineRPM - (engineRPM / 3));
            gearChangeRate = Time.time + 1.4f/1f;
        }
        
        if(engineRPM < minRPM && currentGear > 0 && Time.time >= gearChangeRate && !reverse){
            gearChangeRate = Time.time + 2;
            setEngineLerp(engineRPM + (engineRPM / 2));
            currentGear --;
            neutralGear = true;
            Invoke("neutralGearV", shiftTime);
            audioSource.gearDownAudio();
        }
    }


    private void updateWheels(){
        for (int i = 0; i < wheelColliders.Length; i++) {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelTransforms[i].transform.localRotation = Quaternion.Euler(0,wheelColliders[i].steerAngle,0);                                    //steer rotation
            if(i % 2 != 0) {
                wheelTransforms[i].transform.GetChild(0).transform.Rotate( wheelColliders[i].rpm * -6.6f * Time.deltaTime, 0 ,0,Space.Self);    //engine rotation
            } else {
                wheelTransforms[i].transform.GetChild(0).transform.Rotate( wheelColliders[i].rpm * 6.6f * Time.deltaTime, 0 ,0,Space.Self);     //engine rotation
            }
            wheelTransforms[i].transform.position = wheelPosition;
        }
    }

    private void wheelRPM() {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++) {
            sum += wheelColliders[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
 
        if(wheelsRPM < -1 && !reverse ) {
            reverse = true;
            //if (gameObject.tag != "AI") manager.changeGear();
        } else if(wheelsRPM > 0 && reverse) {
            reverse = false;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
    }

    private void setEngineLerp(float num) {
        engineLerp = true;
        engineLerpValue = num;
    }

    private void lerpEngine() {
        if(engineLerp){
            totalPower = 0;
            engineRPM = Mathf.Lerp(engineRPM, engineLerpValue, cutOffTime * Time.deltaTime );
            engineLerp = engineRPM <= engineLerpValue + 100 ? false : true;
        }
    }

    private void neutralGearV() {
        neutralGear = false;
    }

    private bool allWheelsGrounded() {
        if(wheelColliders[0].isGrounded && wheelColliders[1].isGrounded && wheelColliders[2].isGrounded && wheelColliders[3].isGrounded){
            return true;
        } else {
            return false;
        }
    }

    private void friction() {
        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheelColliders.Length];
        for (int i = 0; i < wheelColliders.Length; i++){
            if(wheelColliders[i].GetGroundHit(out hit) && i >= 2) {
                grounded = true;
                sum += Mathf.Abs(hit.sidewaysSlip);
            } else {
                grounded = false;
            }
            wheelSlip[i] = Mathf.Abs(hit.forwardSlip) + Mathf.Abs(hit.sidewaysSlip);
            sidewaysSlip[i] = Mathf.Abs(hit.sidewaysSlip);
        }
        sum /= wheelColliders.Length - 2;
    }

    // void OnGUI(){
    //     float pos = 50;

    //     GUI.Label(new Rect(20, pos, 200, 20),"currentGear: " + currentGear.ToString("0"));
    //     pos+=25f;
    //     GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineRPM,0,maxRPM);
    //     pos+=25f;

    //     GUI.Label(new Rect(20, pos, 200, 20),"Torque: " + totalPower.ToString("0"));
    //     pos+=25f;
    //     GUI.Label(new Rect(20, pos, 200, 20),"KPH: " + KPH.ToString("0"));
    //     pos+=25f;
    //     GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineLoad, 0.1F, 1.0F);
    //     pos+=25f;
    //     GUI.Label(new Rect(20, pos, 200, 20),"brakes: " + brakePower.ToString("0"));
    //     pos+=25f;
    //     GUI.Label(new Rect(20, pos, 200, 20),"currentVelocity: " + currentVelocity.ToString("0"));
    //     pos+=25f;
    //     GUI.Label(new Rect(20, pos, 200, 20),"lastFrameVelocity: " + lastFrameVelocity.ToString("0"));
    //     pos+=25f;
    //     GUI.Label(new Rect(20, pos, 200, 20),"Gforce: " + Gforce.ToString("0"));
    //     pos+=25f;
        
    // }
}
