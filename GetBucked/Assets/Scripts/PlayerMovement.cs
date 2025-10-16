using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    PlayerControls playerControls;
    Rigidbody rb;
    Transform cam;
    ConfigurableJoint rootJoint;
    RagdollStabiliser stabiliser;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float bSphereCheckRadius = 2f, bSphereCheckDist = 1f, tSphereCheckRadius, tSphereCheckDist;
    [SerializeField] float stabiliserRestoreDuration = 1f, stabiliserRestoreMoveThreshold = 0.25f;
    bool isGrounded, wasGrounded;
    Coroutine restoreStabiliserRoutine;

    Vector2 moveVect;
    [Header("Locomotion")]
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float jumpForce = 1f;

    [Header("Joints")]
    [SerializeField] bool ragdoll = false;
    [SerializeField] float slerpDriveMax = 4000f, slerpDriveMin = 75f, artificialGravity = -100f;
    ConfigurableJoint[] bodyJoints;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rootJoint = GetComponent<ConfigurableJoint>();
        stabiliser = GetComponentInChildren<RagdollStabiliser>();
        bodyJoints = GetComponentsInChildren<ConfigurableJoint>();

        cam = Camera.main.transform;

        ragdoll = false;
    }

    private void OnEnable()
    {
        playerControls = new();
        playerControls.General.Enable();

        playerControls.General.Move.performed += Move_performed;
        playerControls.General.Jump.performed += Jump_performed;

        //debug
        playerControls.General.Ragdoll.performed += Ragdoll_performed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //
    }

    private void Ragdoll_performed(InputAction.CallbackContext ctx)
    {
        ragdoll = !ragdoll;
        moveVect = Vector2.zero;
    }

    private void Jump_performed(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !ragdoll)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Move_performed(InputAction.CallbackContext ctx)
    {
        if (!ragdoll)
        {
            moveVect = ctx.ReadValue<Vector2>();
        }
    }

    private void Update()
    {
        

    }
    private void FixedUpdate()
    {
        Move();
        GroundCheck();

        ApplyStabiliserForces();
        PlayerRagdoll(ragdoll);
    }

    void Move()
    {
        //makes move direction face in the actual forward direction rather than the camera's forward
        //which points down toward the player
        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 dir = (camForward * moveVect.y) + (camRight * moveVect.x);
        Debug.DrawLine(cam.position, cam.position + dir * 5f, Color.yellow);

        if (moveVect != Vector2.zero)
        {
            Quaternion rot = Quaternion.Euler(0, cam.eulerAngles.y, 0);
            Quaternion inv = Quaternion.Inverse(rot);

            rootJoint.targetRotation = inv;

            rb.AddForce(dir * moveSpeed, ForceMode.Force);

        }
    }

    void GroundCheck()
    {
        if (!ragdoll)
        {
            wasGrounded = isGrounded;

            //uses 2 sphere casts incase the ragdoll leans forward too much
            Vector3 sphereCastTop = new(stabiliser.transform.position.x, stabiliser.transform.position.y - tSphereCheckDist, stabiliser.transform.position.z);
            Vector3 sphereCastBottom = new(stabiliser.transform.position.x, stabiliser.transform.position.y - bSphereCheckDist, stabiliser.transform.position.z);
            if (Physics.CheckSphere(sphereCastTop, tSphereCheckRadius, groundLayer) ||
                Physics.CheckSphere(sphereCastBottom, bSphereCheckRadius, groundLayer))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
          
        }
    }

    void ApplyStabiliserForces()
    {
        if (!isGrounded)
        {
            //when jumping, set artifial gravity and stop the restore stabiliser coroutine
            if(restoreStabiliserRoutine != null)
            {
                StopCoroutine(restoreStabiliserRoutine);
            }
            stabiliser.SetForceVal(artificialGravity);
        }
        else
        {
            if(!wasGrounded && isGrounded)
            {
                //when landing, restore stabiliser graudally if low movement and restore fully if moving fast
                if (moveVect.magnitude < stabiliserRestoreMoveThreshold)
                {
                    restoreStabiliserRoutine = StartCoroutine(RestoreStabiliserForce(stabiliserRestoreDuration));
                }
                else
                {
                    stabiliser.SetForceVal(stabiliser.GetInitForce());
                }

                rb.velocity = new(rb.velocity.x, 0f, rb.velocity.z);
            }
        }
    }
    private IEnumerator RestoreStabiliserForce(float duration)
    {
        float forceVal = stabiliser.GetForceVal();
        float timer = 0f;

        while (timer  < duration)
        {
            timer += Time.fixedDeltaTime;
            stabiliser.SetForceVal(Mathf.Lerp(forceVal, stabiliser.GetInitForce(), timer / duration));
            yield return new WaitForFixedUpdate();
        }

        stabiliser.SetForceVal(stabiliser.GetInitForce());
    }

    void PlayerRagdoll(bool isRagdoll)
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

    public void SetPlayerRagdoll(bool inBool)
    {
        ragdoll = inBool;
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

    private void OnDisable()
    {
        playerControls.General.Disable();
    }

    private void OnDrawGizmos()
    {
        if (stabiliser != null)
        {
            Vector3 sphereCastTop = new(stabiliser.transform.position.x, stabiliser.transform.position.y - tSphereCheckDist, stabiliser.transform.position.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sphereCastTop, tSphereCheckRadius);

            Vector3 sphereCastBottom = new(stabiliser.transform.position.x, stabiliser.transform.position.y - bSphereCheckDist, stabiliser.transform.position.z);
            Gizmos.DrawWireSphere(sphereCastBottom, bSphereCheckRadius);
        }


    }
}
