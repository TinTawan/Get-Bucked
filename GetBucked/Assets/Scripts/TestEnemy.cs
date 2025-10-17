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

    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDist = 2f;

    int health;
     
    private void OnEnable()
    {
        PlayerAttack.OnPlayerAttack += PlayerAttack_OnPlayerAttack;
    }

    private void PlayerAttack_OnPlayerAttack(PlayerAttack ctx)
    {
        //only apply if player isnt already knocked out
        if (!ragdoll)
        {
            //decrement health
            health--;
            Debug.Log($"Enemy has {health} health left");

            //if 0, ragdoll for x seconds
            if (health <= 0)
            {
                SetRagdoll(true);
                stabiliser.SetActivateForce(false);

                StartCoroutine(ResetRagdoll());
            }
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
    private void Update()
    {
        GroundCheck();
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

    void GroundCheck()
    {
        Debug.DrawLine(stabiliser.transform.position, 
            new(stabiliser.transform.position.x, stabiliser.transform.position.y - groundCheckDist, stabiliser.transform.position.z), 
            Color.yellow);
        if (!ragdoll)
        {
            if (Physics.Raycast(stabiliser.transform.position, Vector3.down, out _, groundCheckDist, groundLayer))
            {
                stabiliser.SetActivateForce(true);
            }
            else
            {
                stabiliser.SetActivateForce(false);
            }
        }
    }

    IEnumerator ResetRagdoll()
    {
        yield return new WaitForSeconds(Random.Range(wakeUpTimeMin, wakeUpTimeMax));
        SetRagdoll(false);
        health = baseHealth;
        //Debug.Log("Enemy roke up");

        yield return new WaitForSeconds(1f);
        stabiliser.SetActivateForce(true);
        //Debug.Log("Enemy stabiliser on");


    }

    public void SetRagdoll(bool inBool)
    {
        ragdoll = inBool;
    }

    public RagdollStabiliser GetRagdollStabiliser()
    {
        return stabiliser;
    }

    private void OnDisable()
    {
        PlayerAttack.OnPlayerAttack -= PlayerAttack_OnPlayerAttack;
    }
}
