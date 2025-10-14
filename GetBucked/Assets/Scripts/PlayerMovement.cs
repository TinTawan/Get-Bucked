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
    [SerializeField] float groundCheckDist, sphereCheckRadius = 2f, sphereCheckDist = 1f;
    bool isGrounded;

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
        GroundCheck();
    }
    private void FixedUpdate()
    {
        Move();

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
            Vector3 sphereCastPos = new(stabiliser.transform.position.x, stabiliser.transform.position.y - sphereCheckDist, stabiliser.transform.position.z);
            if (Physics.CheckSphere(sphereCastPos, sphereCheckRadius, groundLayer))
            {
                isGrounded = true;

                stabiliser.SetForceVal(stabiliser.GetInitForce());
            }
            else
            {
                isGrounded = false;

                stabiliser.SetForceVal(artificialGravity);
            }
          
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

    /*private void OnDrawGizmos()
    {
        Vector3 castEnd = new(stabiliser.transform.position.x, stabiliser.transform.position.y - sphereCheckDist, stabiliser.transform.position.z);

        //Gizmos.DrawWireSphere(stabiliser.transform.position, sphereCheckRadius);
        Gizmos.DrawWireSphere(castEnd, sphereCheckRadius);

    }*/
}
