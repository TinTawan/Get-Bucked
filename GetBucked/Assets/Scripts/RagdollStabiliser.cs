using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootRB;

    [SerializeField] bool activateForce;
    [SerializeField] float artificalDrag = 10f, upwardForce = 450f;
    float stabilisingForce, initDrag;

    private void Start()
    {
        activateForce = true;
        stabilisingForce = upwardForce;
        initDrag = artificalDrag;
    }

    private void FixedUpdate()
    {
        if (activateForce)
        {
            //upwards stabilising force
            rootRB.AddForce(Vector3.up * upwardForce, ForceMode.Force);

            //horizontal drag to reduce sway 
            Vector3 swayDrag = new(rootRB.velocity.x, 0f, rootRB.velocity.z);
            rootRB.AddForce(-swayDrag * artificalDrag, ForceMode.Force);
        }
    }

    public void SetActivateForce(bool inBool)
    {
        activateForce = inBool;
    }

    public void SetForceVal(float inVal)
    {
        upwardForce = inVal;
    }

    public void SetArtficialDrag(float inVal)
    {
        artificalDrag = inVal;
    }

    public float GetForceVal()
    {
        return upwardForce;
    }

    public float GetInitForce()
    {
        return stabilisingForce;
    }
    public float GetInitDrag()
    {
        return initDrag;
    }
}
