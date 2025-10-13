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
    [SerializeField] float slerpDriveMax = 4000f, slerpDriveMin = 75f, wakeUpTimeMin = 2f, wakeUpTimeMax = 5f;
    [SerializeField] int baseHealth = 3;
    ConfigurableJoint[] bodyJoints;

    int health;
     
    private void OnEnable()
    {
        PlayerAttack.OnPlayerAttack += PlayerAttack_OnPlayerAttack;
    }

    private void PlayerAttack_OnPlayerAttack(PlayerAttack ctx)
    {
        if(health > 1)
        {
            health--;
            Debug.Log($"Enemy has {health} health left");
        }
        else
        {
            Debug.Log("Enemy Ragdoll");
            SetRagdoll(true);
            stabiliser.SetActivateForce(false);

            StartCoroutine(ResetRagdoll());
        }
                
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rootJoint = GetComponent<ConfigurableJoint>();
        stabiliser = GetComponentInChildren<RagdollStabiliser>();
        bodyJoints = GetComponentsInChildren<ConfigurableJoint>();

        health = baseHealth;
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

    IEnumerator ResetRagdoll()
    {
        yield return new WaitForSeconds(Random.Range(wakeUpTimeMin, wakeUpTimeMax));
        SetRagdoll(false);
        Debug.Log("Enemy roke up");

        yield return new WaitForSeconds(1f);
        stabiliser.SetActivateForce(true);
        Debug.Log("Enemy stabiliser on");

        health = baseHealth;

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
