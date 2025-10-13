using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint rootJoint;
    RagdollStabiliser stabiliser;

    bool ragdoll = false;
    [SerializeField] float slerpDriveMax = 4000f, slerpDriveMin = 75f;
    ConfigurableJoint[] bodyJoints;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rootJoint = GetComponent<ConfigurableJoint>();
        stabiliser = GetComponentInChildren<RagdollStabiliser>();
        bodyJoints = GetComponentsInChildren<ConfigurableJoint>();
    }
    private void FixedUpdate()
    {
        EnemyRagdoll(ragdoll);
    }

    void EnemyRagdoll(bool isRagdoll)
    {
        if (isRagdoll)
        {
            stabiliser.SetActivateForce(false);
            SetSlerpDrive(slerpDriveMin);

        }
        else
        {
            stabiliser.SetActivateForce(true);
            SetSlerpDrive(slerpDriveMax);

        }

    }
    void SetSlerpDrive(float inVal)
    {
        foreach (ConfigurableJoint cj in bodyJoints)
        {
            JointDrive drive = cj.slerpDrive;
            drive.positionSpring = inVal;
            cj.slerpDrive = drive;

        }

    }

    public void SetRagdoll(bool inBool)
    {
        ragdoll = inBool;
    }

    public RagdollStabiliser GetRagdollStabiliser()
    {
        return stabiliser;
    }
}
