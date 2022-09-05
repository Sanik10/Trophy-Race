using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class editorScript : EditorWindow{

    [MenuItem("tools/vehicle")]

    public static void Open(){
        GetWindow<editorScript>();
    }


    public GameObject car;

    //car wheels
    //public GameObject[] wheels = null;
    public bool hasDifferentRearTire = false;
    public GameObject wheelPrefab;
    public GameObject wheelPrefabRear;

    //car components
    public GameObject wheelsContainer;
    public GameObject[] carWheels;
    public string wheelsString = "carWheels";
    public GameObject collidersContainer;
    public Transform[] colliderTransform;
    public WheelCollider[] wheelColliders;
    public string collidersString = "carColliders";
    public GameObject centerOfMass;

    //junk
    private Vector3 wheelPosition;
    private Quaternion wheelRotation;
    public bool test = false;
    public string vehicleStatus = "unchecked";
    static Vector3 frontOffset , rearOffset;
    float frontradius , rearRadius , suspensionDistance , camberValuerFront , camberValuerRear;


    void OnGUI(){

        // car Game Object
        displayValue("car" );



        //display status
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("status: ");
        GUILayout.Label(vehicleStatus);
        GUILayout.EndHorizontal();

        
        wheelsHolderCheck(); // do not delete
        buttos();

    }

    void displayValue(string a ){
        SerializedObject i = new SerializedObject(this);
        EditorGUILayout.PropertyField(i.FindProperty(a));
        i.ApplyModifiedProperties();
    }

    void runChecks(){

        if(car == null){
            vehicleStatus = "car object missing!!";
            return;
        }

        //debug :
        //Debug.Log("wheels Length : " + wheels.Length);

        vehicleStatus = "";

        //if(wheels[0] == null) vehicleStatus = "wheel reference is null!!   ";
        if(wheelPrefab == null)vehicleStatus = "wheel reference is null!!   ";
        
        //check Rigid Body
        Rigidbody A = null;
        try{
            A = car.GetComponent<Rigidbody>();
        }catch{}

        if(A == null){
            A = car.AddComponent<Rigidbody>();
            A.mass = 1200;
        }


        //checks

        foreach (Transform item in car.transform){
            if(item.transform.name == wheelsString)             // check wheels
                wheelsContainer = item.transform.gameObject;
            if(item.transform.name == collidersString)          //check colliders
                collidersContainer = item.transform.gameObject;
            if(item.transform.name == "CenterOfMass")          //check colliders
                centerOfMass = item.transform.gameObject;
        
        }

        if(centerOfMass == null){
            centerOfMass = new GameObject();
            centerOfMass.transform.parent = car.transform;
            centerOfMass.transform.name = "CenterOfMass";
            centerOfMass.transform.position = car.transform.position;
            centerOfMass.transform.localScale = new Vector3(1,1,1);
            centerOfMass.transform.localRotation = car.transform.localRotation;
        }

        if(wheelsContainer != null){
            if(wheelsContainer.transform.childCount == 4){
                carWheels = new GameObject[4];
                int i = 0;
                foreach (Transform item in wheelsContainer.transform){
                    carWheels[i] = item.transform.gameObject; 
                    i++;
                }
            }else{
                vehicleStatus += "wheels Missing!";
            }
        }else{
            wheelsContainer = new GameObject();
            wheelsContainer.transform.parent = car.transform;
            wheelsContainer.transform.name = wheelsString;
            wheelsContainer.transform.position = car.transform.position;
        }


        if(collidersContainer != null){
            if(collidersContainer.transform.childCount == 4){
                colliderTransform = new Transform[4];
                wheelColliders = new WheelCollider[4];

                for (int e = 0; e < collidersContainer.transform.childCount; e++){
                    colliderTransform[e] = collidersContainer.transform.GetChild(e);
                }

                int i = 0;
                foreach (Transform item in collidersContainer.transform){
                    colliderTransform[i] = item.transform; 
                    wheelColliders[i] = collidersContainer.transform.GetChild(i).GetComponent<WheelCollider>();
                    i++;
                }
            }else{
                vehicleStatus += "colliders Missing!";
            }
        }else{
            collidersContainer = new GameObject();
            collidersContainer.transform.parent = car.transform;
            collidersContainer.transform.name = collidersString;
            collidersContainer.transform.position = car.transform.position;
        }

        

        if(vehicleStatus == ""){ 
            vehicleStatus = "Ready!! ";
            //setUpController();
        }
    }

    void wheelsHolderCheck(){
        if(car == null)return;

        displayValue("hasDifferentRearTire" );

        if(hasDifferentRearTire){
            displayValue("wheelPrefab" );
            displayValue("wheelPrefabRear" );
        }else{
            displayValue("wheelPrefab" );

        }



        //if(wheels.Length == 2 || wheels.Length == 1) return;
        //if(wheels == null){
        //    wheels = new GameObject[0];            
        //}
        //if(wheels != null){
        //    if(wheels.Length >= 2){
        //        wheels = new GameObject[2];
        //    }
        //    if(wheels.Length == 0){
        //        wheels = new GameObject[1];
        //    }
        //}
    }

    void buttos(){

        if(GUILayout.Button("Run Checks" , GUILayout.Height(40))){
            runChecks();
        }

        if(car == null)return;

        //wheel prefabs
        if(collidersContainer != null)
        if(collidersContainer.transform.childCount > 0){
            if(hasDifferentRearTire){
                if(wheelPrefab != null && wheelPrefabRear != null)
                    if(wheelsContainer != null)
                    if(wheelsContainer.transform.childCount == 0)
                    if(GUILayout.Button("create wheels" , GUILayout.Height(40))){
                        createWheels();
                    }
            }else{
                if(wheelPrefab != null )
                    if(wheelsContainer != null)
                    if(wheelsContainer.transform.childCount == 0)
                    if(GUILayout.Button("create wheels" , GUILayout.Height(40))){
                        createWheels();
                    }

            }
            if(wheelsContainer != null)
            if(wheelsContainer.transform.childCount > 0){
                    updateWheels();
                    wheelsProperties();
                if(GUILayout.Button("delete wheels" , GUILayout.Height(40))){
                    deleteWheels();
                }
            }
        }

        // create collider > colliders
        if(collidersContainer != null)
        if(collidersContainer.transform.childCount == 0)
        if(GUILayout.Button("create collideres" , GUILayout.Height(40))){
            createColliders();
        }
        
    }

    void createWheels(){

        if(wheelPrefab != null){
            carWheels = new GameObject[4];
            if(hasDifferentRearTire){
                if(wheelPrefabRear == null){
                    vehicleStatus += " rear wheel prefab missing!!  ";
                    return;
                }
                for (int i = 0; i < 4; i++){
                    if(i > 1)
                        carWheels[i] = Instantiate(wheelPrefabRear);
                    else
                        carWheels[i] = Instantiate(wheelPrefab);
                        carWheels[i].transform.parent = wheelsContainer.transform;
                        carWheels[i].transform.position = Vector3.zero;
                        carWheels[i].transform.localPosition = Vector3.zero;
                        carWheels[i].transform.localScale = new Vector3(1,1,1);
                        carWheels[i].transform.name = "wheel" + i;
                        if(i % 2 != 0)
                        carWheels[i].transform.GetChild(0).Rotate(new Vector3(0,180,0));
                }
            }else{
                if(wheelPrefab == null){
                    vehicleStatus += " wheel prefab missing!!  ";
                    return;
                }
                for (int i = 0; i < 4; i++){
                        carWheels[i] = Instantiate(wheelPrefab);
                        carWheels[i].transform.parent = wheelsContainer.transform;
                        carWheels[i].transform.position = Vector3.zero;
                        carWheels[i].transform.localPosition = Vector3.zero;
                        carWheels[i].transform.localScale = new Vector3(1,1,1);
                        carWheels[i].transform.name = "wheel" + i;
                        if(i % 2 != 0)
                        carWheels[i].transform.GetChild(0).Rotate(new Vector3(0,180,0));
                }
            }

        }




//        if(wheels.Length > 1)
//        if(wheels[1] != null){
//            carWheels = new GameObject[4];
//            for (int i = 0; i < 4; i++){
//                if(i > 1)
//                    carWheels[i] = Instantiate(wheels[1]);
//                else
//                    carWheels[i] = Instantiate(wheels[0]);
//                carWheels[i].transform.parent = wheelsContainer.transform;
//                carWheels[i].transform.position = Vector3.zero;
//                carWheels[i].transform.localPosition = Vector3.zero;
//                carWheels[i].transform.localScale = new Vector3(1,1,1);
//                carWheels[i].transform.name = "wheel" + i;
//                Debug.Log("spawned: " + carWheels[i].transform.position);
//            }
//        }        
//        if(wheels[0] != null){
//            carWheels = new GameObject[4];
//            for (int i = 0; i < 4; i++){
//                carWheels[i] = Instantiate(wheels[0]);
//                carWheels[i].transform.parent = wheelsContainer.transform;
//                carWheels[i].transform.position = Vector3.zero;
//                carWheels[i].transform.localPosition = Vector3.zero;
//                carWheels[i].transform.localScale = new Vector3(1,1,1);
//                carWheels[i].transform.name = "wheel" + i;
//                Debug.Log("spawned: " + carWheels[i].transform.position);
//            }
//        }        

    }

    void createColliders(){

        if(collidersContainer == null){
            vehicleStatus += "collider container missing!  ";
            return;
        }
        
        colliderTransform = new Transform[4];
        wheelColliders = new WheelCollider[4];

        for (int i = 0; i < wheelColliders.Length; i++){
            colliderTransform[i] = new GameObject().transform;
            colliderTransform[i].transform.parent = collidersContainer.transform ;
            colliderTransform[i].name = "collider" + i;
            colliderTransform[i].transform.position = Vector3.zero;
            wheelColliders[i] = colliderTransform[i].gameObject.AddComponent<WheelCollider>();

        }

    }

    void deleteWheels(){
        DestroyImmediate(wheelsContainer);

        wheelsContainer = new GameObject();
        wheelsContainer.transform.parent = car.transform;
        wheelsContainer.transform.name = wheelsString;
        wheelsContainer.transform.position = car.transform.position;

        carWheels = null;
    }

    void updateWheels(){

		for (int i = 0; i < wheelColliders.Length; i++) {
			wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            carWheels[i].transform.rotation = wheelRotation;
            carWheels[i].transform.position = wheelPosition;

        }
    }

    void wheelsProperties(){
        checkExistingPosition();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("suspension distance" ,  GUILayout.Height(30));
        suspensionDistance = EditorGUILayout.Slider(suspensionDistance,.05f, .6f);
        EditorGUILayout.EndHorizontal();


        //front

        GUILayout.Label("Front" ,  GUILayout.Height(40));

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front wheels position Z" ,  GUILayout.Height(30));
        frontOffset.z = EditorGUILayout.Slider(frontOffset.z,.8f, 4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front wheels position Y" ,  GUILayout.Height(30));
        frontOffset.y = EditorGUILayout.Slider(frontOffset.y,0, 3);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front wheels position x" ,  GUILayout.Height(30));
        frontOffset.x = EditorGUILayout.Slider(frontOffset.x,0.5f, 3);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front wheels radius" ,  GUILayout.Height(30));
        frontradius = EditorGUILayout.Slider(frontradius,0.2f, 1);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < 2 ; i++){
            wheelColliders[i].radius = frontradius;
            wheelColliders[i].suspensionDistance = suspensionDistance;
            if(i % 2 == 0)
            colliderTransform[i].transform.localPosition = new Vector3(-frontOffset.x,frontOffset.y,frontOffset.z);
            else
            colliderTransform[i].transform.localPosition = new Vector3(frontOffset.x,frontOffset.y,frontOffset.z);

        }
        
        GUILayout.Label("Rear" ,  GUILayout.Height(40));

        //rear
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("rear wheels position Z" ,  GUILayout.Height(30));
        rearOffset.z = EditorGUILayout.Slider(rearOffset.z,.8f, 4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("rear wheels position Y" ,  GUILayout.Height(30));
        rearOffset.y = EditorGUILayout.Slider(rearOffset.y,0, 3);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("rear wheels position x" ,  GUILayout.Height(30));
        rearOffset.x = EditorGUILayout.Slider(rearOffset.x,0.5f, 3);
        EditorGUILayout.EndHorizontal();
                
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front wheels radius" ,  GUILayout.Height(30));
        rearRadius = EditorGUILayout.Slider(rearRadius,0.2f, 1);
        EditorGUILayout.EndHorizontal();


        for (int i = 2; i < wheelColliders.Length ; i++){
            wheelColliders[i].suspensionDistance = suspensionDistance;
            wheelColliders[i].radius = rearRadius;
            if(i % 2 == 0)
            colliderTransform[i].transform.localPosition = new Vector3(-rearOffset.x,rearOffset.y,-rearOffset.z);
            else
            colliderTransform[i].transform.localPosition = new Vector3(rearOffset.x,rearOffset.y,-rearOffset.z);
            
        }

        GUILayout.Label(" " ,  GUILayout.Height(40));

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("front camber" ,  GUILayout.Height(30));
        camberValuerFront = EditorGUILayout.Slider(camberValuerFront,0 , -8);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("rear camber" ,  GUILayout.Height(30));
        camberValuerRear = EditorGUILayout.Slider(camberValuerRear,0 , -8);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < 4; i++){



            if(i % 2 != 0){
                if(i < 2)
                carWheels[i].transform.GetChild(0).transform.localRotation = Quaternion.Euler(0 , 180 , camberValuerFront);
                else
                carWheels[i].transform.GetChild(0).transform.localRotation = Quaternion.Euler(0 , 180 , camberValuerRear);
            }else{
                if(i < 2)
                carWheels[i].transform.GetChild(0).transform.localRotation = Quaternion.Euler(0 , 0 , camberValuerFront);
                else
                carWheels[i].transform.GetChild(0).transform.localRotation = Quaternion.Euler(0 , 0 , camberValuerRear);

            }
        }

    }

    void checkExistingPosition(){    

        suspensionDistance = wheelColliders[0].GetComponent<WheelCollider>().suspensionDistance;


        for (int i = 0; i < wheelColliders.Length; i++){
            if(i  < 2){
                frontOffset = new Vector3(Mathf.Abs(colliderTransform[i].transform.localPosition.x),Mathf.Abs(colliderTransform[i].transform.localPosition.y),Mathf.Abs(colliderTransform[i].transform.localPosition.z));
                frontradius =   colliderTransform[i].GetComponent<WheelCollider>().radius;    
            }
        }

        for (int i = 2; i < wheelColliders.Length; i++){
                rearOffset = new Vector3(Mathf.Abs(colliderTransform[i].transform.localPosition.x),Mathf.Abs(colliderTransform[i].transform.localPosition.y),Mathf.Abs(colliderTransform[i].transform.localPosition.z));
                rearRadius =  colliderTransform[i].GetComponent<WheelCollider>().radius;
            
        }




    }

    void setUpController(){
        carController c = car.GetComponent<carController>();
        c.gears = new float[7];

        c.maxRPM = 8000;
        c.minRPM = 4000;

        c.DownForceValue = 5;
        c.dragAmount = 0.015f;
        c.EngineSmoothTime = 0.5f;
        c.finalDrive = 3.71f;

        c.gears[0] = 2.89f;
        c.gears[1] = 1.99f;
        c.gears[2] = 1.49f;
        c.gears[3] = 1.16f;
        c.gears[4] = 0.94f;
        c.gears[5] = 0.78f;
        c.gears[6] = 0.58f;

        if( car.tag != "Player" )car.tag = "Player";

    }


}