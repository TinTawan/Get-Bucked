using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class PlayerAttack : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField] float attackKnockback = 5f, attackCooldown = 1f, chargeMult = 2f, maxChargeLevel = 5f;
    [SerializeField] LayerMask playerLayer;

    bool canAttack = false, /*canChargeAttack,*/ chargingAttack;
    float attackCooldownTimer = 0f, chargeLevel = 1f;

    TestEnemy enemy;

    public static event Action<PlayerAttack> OnPlayerAttack;

    private void OnEnable()
    {
        playerControls = new();
        playerControls.Enable();

        playerControls.General.Attack.performed += Attack_performed;
        playerControls.General.ChargeAttack.performed += ChargeAttack_performed;
        playerControls.General.ChargeAttack.canceled += ChargeAttack_canceled;

    }

    private void ChargeAttack_canceled(InputAction.CallbackContext ctx)
    {
        //chargingAttack = false;
        if(enemy != null)
        {
            if (chargingAttack)
            {
                PerformChargeAttack(Mathf.Lerp(1, maxChargeLevel, chargeLevel) * chargeMult);
                Debug.Log($"Attack knockback: {Mathf.Lerp(1, maxChargeLevel, chargeLevel) * chargeMult}");
            }
        }
        else
        {
            chargeLevel = 1f;
            chargingAttack = false;
            Debug.Log("Charge released");

        }

    }

    private void ChargeAttack_performed(InputAction.CallbackContext ctx)
    {
        chargingAttack = true;
        Debug.Log("Hold charge");
    }

    void ChargeUpAttack()
    {
        if (chargingAttack)
        {
            if(chargeLevel < maxChargeLevel)
            {
                chargeLevel += Time.deltaTime;
                Debug.Log("Charging");

            }
            else
            {
                chargeLevel = maxChargeLevel;
                Debug.Log("Max Charge");

            }
        }
    }
    void PerformChargeAttack(float chargeMult)
    {
        Attack(attackKnockback * chargeMult);

        chargeLevel = 1;
        chargingAttack = false;
    }

    private void Attack_performed(InputAction.CallbackContext ctx)
    {
        if (canAttack)
        {
            canAttack = false;
            Debug.Log("attack");

            if (enemy != null)
            {
                Attack(attackKnockback);
            }
        }       
    }

    private void Update()
    {
        AttackCooldown();
        ChargeUpAttack();
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            if(enemy == null)
            {
                Debug.Log("Enemy in range");
                enemy = col.TryGetComponent(out TestEnemy testEnemy) ? testEnemy : null;
            }

        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            enemy = null;
        }
    }

    void AttackCooldown()
    {
        if (!canAttack)
        {
            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
                canAttack = false;
            }
            else
            {
                canAttack = true;
                attackCooldownTimer = attackCooldown;
            }
        }
    }

    void Attack(float knockback)
    {
        OnPlayerAttack?.Invoke(this);

        enemy.GetComponent<Rigidbody>().AddForce(CalculateAttack(knockback), ForceMode.Impulse);
    }

    Vector3 CalculateAttack(float damage)
    {
        return -transform.forward * damage + transform.up * damage;
    }

    /*IEnumerator Attack()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }*/

    /*private void OnDrawGizmos()
    {
        Vector3 castCenter = (attackOrigin.forward + attackOrigin.position) * attackRange;
        Vector3 size = attackRange * Vector3.one;
        Gizmos.DrawCube(castCenter, size);
    }*/

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
