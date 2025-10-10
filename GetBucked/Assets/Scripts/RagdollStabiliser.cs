using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootrb;

    [SerializeField] bool activateForce = true;
    [SerializeField] float forceVal = 2f;


    private void FixedUpdate()
    {
        if (activateForce)
        {
            rootrb.AddForce(Vector3.up * forceVal * Time.deltaTime);

        }
    }
}
