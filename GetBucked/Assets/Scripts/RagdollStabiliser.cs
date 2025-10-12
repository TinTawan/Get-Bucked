using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollStabiliser : MonoBehaviour
{
    [SerializeField] private Rigidbody rootrb;

    [SerializeField] bool activateForce = true, activateRagdoll = false;
    [SerializeField] float forceVal = 2f, slerpDriveMin = 100f;

    List<ConfigurableJoint> joints = new();
    float slerpDriveVal;

    private void Start()
    {
        AddAllJoints();
        slerpDriveVal = joints[0].slerpDrive.positionSpring;
    }

    private void FixedUpdate()
    {
        if (activateForce)
        {
            rootrb.AddForce(Vector3.up * forceVal * Time.deltaTime);

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
                drive.positionSpring = slerpDriveVal;

                j.slerpDrive = drive;
            }
        }
    }
}
