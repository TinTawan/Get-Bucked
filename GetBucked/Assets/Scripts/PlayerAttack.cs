using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using UnityEditor;

public class PlayerAttack : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField] float attackKnockback = 5f, attackCooldown = 1f, chargeMult = 2f, maxChargeLevel = 5f;
    [SerializeField] LayerMask playerLayer;

    bool canAttack = false, chargingAttack;
    float attackCooldownTimer = 0f, chargeLevel = 1f;

    TestEnemy enemy;

    public static event Action<PlayerAttack> OnPlayerAttack;

    [SerializeField] GameObject hitEffect, chargeHitEffect;
    [SerializeField] ParticleSystem chargeUpEffect, chargeHeldEffect, inRangeEffect;

    private void OnEnable()
    {
        playerControls = new();
        playerControls.Enable();

        playerControls.General.Attack.performed += Attack_performed;
        playerControls.General.ChargeAttack.started += ctx => chargeLevel = 1f;
        playerControls.General.ChargeAttack.performed += ChargeAttack_performed;
        playerControls.General.ChargeAttack.canceled += ChargeAttack_canceled;

    }


    private void ChargeAttack_canceled(InputAction.CallbackContext ctx)
    {
        if(enemy != null)
        {
            if (chargingAttack)
            {
                PerformChargeAttack(Mathf.Lerp(1, maxChargeLevel, chargeLevel / maxChargeLevel) * chargeMult);
                Debug.Log($"Attack knockback: {Mathf.Lerp(1, maxChargeLevel, chargeLevel / maxChargeLevel) * chargeMult}");
            }
        }
        else
        {
            chargingAttack = false;
            Debug.Log("Charge released");

        }

        if (chargeUpEffect.isPlaying)
        {
            chargeUpEffect.Clear();
            chargeUpEffect.Stop();
        }
        if (chargeHeldEffect.isPlaying)
        {
            chargeHeldEffect.Clear();
            chargeHeldEffect.Stop();
        }

    }

    private void ChargeAttack_performed(InputAction.CallbackContext ctx)
    {
        chargingAttack = true;

        if (!chargeUpEffect.isPlaying) chargeUpEffect.Play();
    }

    void ChargeUpAttack()
    {
        if (chargingAttack)
        {
            if(chargeLevel < maxChargeLevel)
            {
                chargeLevel += Time.deltaTime;

            }
            else
            {
                chargeLevel = maxChargeLevel;

                if (!chargeHeldEffect.isPlaying) chargeHeldEffect.Play();
                
            }
        }
    }
    void PerformChargeAttack(float chargeMult)
    {
        Attack(attackKnockback * chargeMult, 1);

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
                Attack(attackKnockback, 0);
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
                enemy = col.TryGetComponent(out TestEnemy testEnemy) ? testEnemy : null;

                inRangeEffect.Play();
            }

        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            enemy = null;

            inRangeEffect.Stop();
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

    void Attack(float knockback, int type)
    {
        OnPlayerAttack?.Invoke(this);

        enemy.GetComponent<Rigidbody>().AddForce(CalculateAttack(knockback), ForceMode.Impulse);
        if(type == 0)
        {
            Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(chargeHitEffect, chargeUpEffect.transform.position, Quaternion.identity);
        }
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
