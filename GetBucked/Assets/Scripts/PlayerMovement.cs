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
    [SerializeField] float groundCheckDist;
    bool isGrounded;

    Vector2 moveVect;
    [Header("Locomotion")]
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float jumpForce = 1f;

    [Header("Joints")]
    [SerializeField] bool ragdoll = false;
    [SerializeField] float slerpDriveMax = 4000f, slerpDriveMin = 75f, artificialGravity = -100f;
    ConfigurableJoint[] bodyJoints;


    private void Awake()
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
        GroundCheck();
    }
    private void FixedUpdate()
    {
        Move();

        PlayerRagdoll(ragdoll);
    }

    void Move()
    {
        Vector3 dir = (cam.forward.normalized * moveVect.y) + (cam.right.normalized * moveVect.x);

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
            if (Physics.Raycast(transform.position, Vector3.down, out _, groundCheckDist, groundLayer))
            {
                isGrounded = true;

                stabiliser.SetForceVal(stabiliser.GetInitForce());
            }
            else
            {
                isGrounded = false;

                stabiliser.SetForceVal(artificialGravity);
            }
            Vector3 end = new(transform.position.x, transform.position.y - groundCheckDist, transform.position.z);
            Debug.DrawLine(transform.position, end, Color.red);
        }
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
}
