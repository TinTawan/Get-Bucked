using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class UICursor : MonoBehaviour
{
    VirtualMouseInput virtualMouseInput;
    Canvas canvas;
    RectTransform canvasTransform;

    [SerializeField] float rayMaxDist = 5f;
    [SerializeField] LayerMask diceLayer;

    void Start()
    {
        virtualMouseInput = GetComponent<VirtualMouseInput>();
        canvas = GetComponentInParent<Canvas>();
        canvasTransform = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (virtualMouseInput.virtualMouse.leftButton.wasPressedThisFrame)
        {
            Select();

        }
    }

    private void LateUpdate()
    {
        //stop vMouse position from going past the bounds of the screen
        Vector2 vMousePos = virtualMouseInput.virtualMouse.position.value;
        vMousePos.x = Mathf.Clamp(vMousePos.x, 0f, canvasTransform.rect.width);
        vMousePos.y = Mathf.Clamp(vMousePos.y, 0f, canvasTransform.rect.height);
        InputState.Change(virtualMouseInput.virtualMouse.position, vMousePos);

    }

    void Select()
    {
        RaycastHit hit;
        if(Physics.Raycast(virtualMouseInput.cursorTransform.position, transform.forward, out hit, rayMaxDist, diceLayer))
        {
            Debug.DrawRay(virtualMouseInput.cursorTransform.position, transform.forward * hit.distance, Color.green);
            Debug.Log("Hit");
        }
        else
        {
            Debug.DrawRay(virtualMouseInput.cursorTransform.position, transform.forward * rayMaxDist, Color.red);
            Debug.Log("No Hit");
        }
    }
}
