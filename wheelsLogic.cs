using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelsLogic : MonoBehaviour
{
    private WheelCollider[] wheelColliders;

    [Header("Настройка колёс")]
    [Range(0.7f, 1.3f)]public float tireGrip = 1;
    [Range(1, 2)]public float forwardValue = 1;
    [Range(1, 2)]public float sidewaysValue = 2;
    [Range(0.5f, 2f)]public float asymptoteValue = 1f;
    [Range(0.1f, 0.6f)]public float extremumSlip = 0.4f;
    [Range(0.4f, 1.6f)]public float asymptoteSlip = 0.8f;

    private WheelFrictionCurve forwardFriction, sidewaysFriction;

    private float[] forwardSlip;
    private float[] sidewaysSlip;
    private float[] overallSlip;

    // Start is called before the first frame update
    void Start()
    {
        findValues();
        setUpWheels();
    }

    private void setUpWheels()
    {
        forwardSlip = new float[4];
        sidewaysSlip = new float[4];
        overallSlip = new float[4];
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            forwardFriction = wheelColliders[i].forwardFriction;
            forwardFriction.asymptoteValue = asymptoteValue;
            forwardFriction.extremumSlip = extremumSlip;
            forwardFriction.asymptoteSlip = asymptoteSlip;

            wheelColliders[i].forwardFriction = forwardFriction;

            sidewaysFriction = wheelColliders[i].sidewaysFriction;
            sidewaysFriction.asymptoteValue = asymptoteValue;
            sidewaysFriction.extremumSlip = extremumSlip;
            sidewaysFriction.asymptoteSlip = asymptoteSlip;

            wheelColliders[i].sidewaysFriction = sidewaysFriction;
        }
    }

    // Update is called once per frame
    void Update()
    {
        friction();
    }

    private void findValues()
    {
        foreach (Transform i in gameObject.transform)
        {
            if(i.transform.name == "carColliders")
            {
                wheelColliders = new WheelCollider[i.transform.childCount];
                for(int q = 0; q < i.transform.childCount; q++)
                {
                    wheelColliders[q] = i.transform.GetChild(q).GetComponent<WheelCollider>();
                }
            }
        }
    }

    private void friction()
    {
        WheelHit hit;
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if(wheelColliders[i].GetGroundHit(out hit))
            {
                overallSlip[i] = Mathf.Abs(hit.forwardSlip + hit.sidewaysSlip);

                forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = tireGrip - (overallSlip[i] / 2) / forwardValue;
                wheelColliders[i].forwardFriction = sidewaysFriction;

                sidewaysFriction = wheelColliders[i].sidewaysFriction;
                sidewaysFriction.stiffness = tireGrip - overallSlip[i] / sidewaysValue;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;

                forwardSlip[i] = hit.forwardSlip;
                sidewaysSlip[i] = hit.sidewaysSlip;
            }
        }
    }
}
