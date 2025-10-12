using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    PlayerControls playerControls;
    Rigidbody rb;
    RagdollStabiliser stabiliser;

    Vector3 moveDirection;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpForce = 2f;
    float initDrag;
    [SerializeField] float minDrag = 0f;

    [Header("Ground Check")]
    [SerializeField] bool isGrounded;
    /*[SerializeField] float groundCheckRadius = 2f;
    [SerializeField] float groundCheckMaxLength = 2f;
    [SerializeField] LayerMask groundLayer;*/

    private void OnEnable()
    {
        EnableGeneralControls();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stabiliser = GetComponentInChildren<RagdollStabiliser>();

        initDrag = rb.drag;
    }

    private void Update()
    {
        //GroundedCheck();
        isGrounded = stabiliser.IsGrounded();
    }

    void FixedUpdate()
    {
        if(moveDirection != Vector3.zero)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    void EnableGeneralControls()
    {
        playerControls = new();
        playerControls.General.Enable();

        playerControls.General.Move.performed += Move_performed;
        playerControls.General.Jump.performed += Jump_performed;
    }

    private void Jump_performed(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !stabiliser.GetRagdollState())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        }
    }

    private void Move_performed(InputAction.CallbackContext ctx)
    {
        if (!stabiliser.GetRagdollState())
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            moveDirection = new(moveInput.x, 0, moveInput.y);
        }
        
    }



    /*void GroundedCheck()
    {
        Vector3 groundCheckPos = new(transform.position.x, transform.position.y + groundCheckMaxLength, transform.position.z);
        isGrounded = !Physics.SphereCast(groundCheckPos, groundCheckRadius, Vector3.down, out RaycastHit hit, 2f, groundLayer);

        if (isGrounded)
        {
            stabiliser.SetStabiliserForce(true);
            rb.drag = initDrag;
        }
        else
        {
            stabiliser.SetStabiliserForce(false);
            rb.drag = minDrag;

        }
    }*/


    void DisableGeneralControls()
    {
        playerControls.General.Disable();
    }

    private void OnDisable()
    {
        DisableGeneralControls();
    }

    private void OnDrawGizmos()
    {
        //Vector3 groundCheckPos = new(transform.position.x, transform.position.y + groundCheckMaxLength, transform.position.z);
        //Gizmos.DrawWireSphere(groundCheckPos, groundCheckRadius);
    }
}
