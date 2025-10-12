using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootRB;

    [SerializeField] bool activateForce;
    [SerializeField] float dragStrength = 10f;
    float forceVal = 2f;

    private void Start()
    {
        activateForce = true;
    }

    private void FixedUpdate()
    {
        if (activateForce)
        {
            //upwards stabilising force
            rootRB.AddForce(Vector3.up * forceVal, ForceMode.Force);

            //horizontal drag to reduce sway 
            Vector3 swayDrag = new(rootRB.velocity.x, 0f, rootRB.velocity.z);
            rootRB.AddForce(-swayDrag * dragStrength, ForceMode.Force);
        }
    }

    public void SetActivateForce(bool inBool)
    {
        activateForce = inBool;
    }

    public void SetForceVal(float inVal)
    {
        forceVal = inVal;
    }

}
