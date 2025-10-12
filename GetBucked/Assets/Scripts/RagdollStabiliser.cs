using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootRB;

    [SerializeField] bool activateForce;
    [SerializeField] float forceVal = 2f;

    private void Start()
    {
        activateForce = true;
    }

    private void FixedUpdate()
    {
        if (activateForce)
        {
            rootRB.AddForce(Vector3.up * forceVal, ForceMode.Force);

        }
    }

    public void SetActivateForce(bool inBool)
    {
        activateForce = inBool;
    }

    /*public void SetForceVal(float inVal)
    {
        forceVal = inVal;
    }*/

}
