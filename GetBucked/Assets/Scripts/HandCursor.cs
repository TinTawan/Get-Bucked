using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandCursor : MonoBehaviour
{
    PlayerControls playerControls;

    Vector2 mousePos;
    Vector3 handPos;

    bool isClicking;

    void OnEnable()
    {
        playerControls = new();
        playerControls.UI.Enable();

        playerControls.UI.Point.performed += Point_performed;
        playerControls.UI.Click.performed += ctx => isClicking = true;
        playerControls.UI.Click.canceled += ctx => isClicking = false;


        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Point_performed(InputAction.CallbackContext ctx)
    {
        mousePos = ctx.ReadValue<Vector2>();
        Vector3 worldMousePos = new(mousePos.x, mousePos.y, 0.5f);

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(worldMousePos);

        handPos = new(worldPoint.x, worldPoint.y, Camera.main.transform.position.z + 0.5f);
        transform.position = handPos;
    }

    public bool GetIsClicking()
    {
        return isClicking;
    }

    public Vector2 GetMousePos()
    {
        return mousePos;
    }

    private void OnDisable()
    {
        playerControls.UI.Point.performed -= Point_performed;
        playerControls.UI.Click.performed -= ctx => isClicking = true;
        playerControls.UI.Click.canceled -= ctx => isClicking = false;
        playerControls.UI.Disable();
    }
}
