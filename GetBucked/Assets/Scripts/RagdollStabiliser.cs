using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootrb;

    [SerializeField] bool activateForce = true, activateRagdoll = false;
    [SerializeField] float stabiliserForceVal = 2f, slerpDriveMin = 100f, slerpDriveBase = 4000f;

    List<ConfigurableJoint> joints = new();

    bool isGrounded;
    [SerializeField] LayerMask groundLayer;

    private void Start()
    {
        AddAllJoints();
    }

    private void FixedUpdate()
    {
        if (activateForce)
        {
            rootrb.AddForce(Vector3.up * stabiliserForceVal, ForceMode.Force);

        }
        Ragdoll();
    }

    void AddAllJoints()
    {
        ConfigurableJoint[] allJoints = rootrb.GetComponentsInChildren<ConfigurableJoint>();
        foreach (ConfigurableJoint j in allJoints)
        {
            joints.Add(j);
        }
    }

    void Ragdoll()
    {
        if (activateRagdoll)
        {
            activateForce = false;
            foreach (ConfigurableJoint j in joints)
            {
                JointDrive drive = j.slerpDrive;
                drive.positionSpring = slerpDriveMin;

                j.slerpDrive = drive;
            }
        }
        else
        {
            
            foreach (ConfigurableJoint j in joints)
            {
                JointDrive drive = j.slerpDrive;
                drive.positionSpring = slerpDriveBase;

                j.slerpDrive = drive;
            }
        }
    }

    public bool GetRagdollState()
    {
        return activateRagdoll;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((groundLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            isGrounded = true;
            activateForce = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if ((groundLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            isGrounded = false;
            activateForce = false;
        }
    }


}
