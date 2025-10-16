using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    PlayerControls playerControls;
    Rigidbody rb;
    Transform cam;
    ConfigurableJoint rootJoint;
    RagdollStabiliser stabiliser;
    CinemachineFreeLook cineCam;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float bSphereCheckRadius = 2f, bSphereCheckDist = 1f, tSphereCheckRadius, tSphereCheckDist;
    [SerializeField] float stabiliserRestoreDuration = 1f, stabiliserRestoreMoveThreshold = 0.25f;
    bool isGrounded, wasGrounded;
    Coroutine restoreStabiliserRoutine;

    Vector2 moveVect;
    [Header("Locomotion")]
    [SerializeField] float jumpForce = 1f;
    [SerializeField] float moveSpeed = 1f, runSpeedMult = 0.5f, fovDeltaSpeed = 1f;
    float initMoveSpeed, initFov, runFov;
    bool holdingRun, canRun, isRunning;
    Coroutine moveFOVCoroutine, runFOVCoroutine;
    [SerializeField] float maxStamina = 5f;
    float stamina;

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

        cineCam = FindObjectOfType<CinemachineFreeLook>();
        cam = Camera.main.transform;

        ragdoll = false;
        initMoveSpeed = moveSpeed;
        initFov = cineCam.m_Lens.FieldOfView;
        runFov = initFov + 10;
        stamina = maxStamina;
    }

    private void OnEnable()
    {
        playerControls = new();
        playerControls.General.Enable();

        playerControls.General.Move.performed += Move_performed;
        playerControls.General.Run.performed += Run_performed;
        playerControls.General.Run.canceled += Run_cancelled;
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
    private void Run_performed(InputAction.CallbackContext ctx)
    {
        holdingRun = true;
    }
    private void Run_cancelled(InputAction.CallbackContext ctx)
    {
        holdingRun = false;
    }

    void FOVChange()
    {
        if (holdingRun && canRun)
        {
            if (moveVect != Vector2.zero)
            {
                runFOVCoroutine = StartCoroutine(RunFOV(runFov));
                if (moveFOVCoroutine != null)
                {
                    StopCoroutine(moveFOVCoroutine);
                }
            }
            
        }
        if(!holdingRun || !canRun || moveVect == Vector2.zero)
        {
            moveFOVCoroutine = StartCoroutine(WalkFOV(initFov));
            if (runFOVCoroutine != null)
            {
                StopCoroutine(runFOVCoroutine);
            }
        }

    }
    IEnumerator RunFOV(float newFOV)
    {
        while (cineCam.m_Lens.FieldOfView < newFOV)
        {
            cineCam.m_Lens.FieldOfView = Mathf.Lerp(cineCam.m_Lens.FieldOfView, newFOV, fovDeltaSpeed);
            yield return new WaitForEndOfFrame();
        }

        cineCam.m_Lens.FieldOfView = newFOV;

    }
    IEnumerator WalkFOV(float newFOV)
    {
        while (cineCam.m_Lens.FieldOfView > newFOV)
        {
            cineCam.m_Lens.FieldOfView = Mathf.Lerp(cineCam.m_Lens.FieldOfView, newFOV, fovDeltaSpeed);
            yield return new WaitForEndOfFrame();
        }

        cineCam.m_Lens.FieldOfView = newFOV;
    }

    void Stamina()
    {
        //if player holds sprint and has enough stamina (plus isnt standing still)
        if(holdingRun && canRun && moveVect != Vector2.zero)
        {
            //then they are running if they have enough stamina
            isRunning = true;
            if(stamina < 0)
            {
                //when stamina runs out, they cant run anymore
                canRun = false;
            }
            else
            {
                //use sprint up
                stamina -= Time.deltaTime;
            }
        }
        else
        {
            isRunning = false;
        }
        //if player isnt holding sprint or is out of stamina (or is standing still)
        if (!holdingRun || !canRun || moveVect == Vector2.zero)
        {
            if (stamina < maxStamina)
            {
                //regen sprint faster than it is used up
                stamina += Time.deltaTime * 1.25f;
            }
            else
            {
                //stamina has regened so player can sprint again
                canRun = true;
            }
        }

    }

    private void Update()
    {
        FOVChange();
        Stamina();
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

            float speed = isRunning ? moveSpeed * runSpeedMult : moveSpeed;
            rb.AddForce(dir * speed, ForceMode.Force);

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
