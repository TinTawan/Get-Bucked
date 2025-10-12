using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    PlayerControls playerControls;

    Vector2 moveInput;

    private void OnEnable()
    {
        EnableGeneralControls();
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void EnableGeneralControls()
    {
        playerControls = new();
        playerControls.General.Enable();

        playerControls.General.Move.performed += Move_performed;
    }

    private void Move_performed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
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
