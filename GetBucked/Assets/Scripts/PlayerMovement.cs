using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    PlayerControls playerControls;
    Rigidbody rb;

    Vector3 moveDirection;

    [SerializeField] float moveSpeed = 2f;

    private void OnEnable()
    {
        EnableGeneralControls();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
    }

    private void Move_performed(InputAction.CallbackContext ctx)
    {
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        moveDirection = new(moveInput.x, 0, moveInput.y);
    }


    void DisableGeneralControls()
    {
        playerControls.General.Disable();
    }

    private void OnDisable()
    {
        DisableGeneralControls();
    }
}
