using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class UICursor : MonoBehaviour
{
    VirtualMouseInput vMouse;

    [SerializeField] float rayMaxDist = 5f;
    [SerializeField] LayerMask diceLayer;

    void Start()
    {
        vMouse = GetComponentInParent<VirtualMouseInput>();
    }

    void Update()
    {
        if (vMouse.virtualMouse.leftButton.wasPressedThisFrame)
        {
            Select();
        }
    }

    void Select()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, rayMaxDist, diceLayer))
        {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green);
            Debug.Log("Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * rayMaxDist, Color.red);
            Debug.Log("No Hit");
        }
    }
}
