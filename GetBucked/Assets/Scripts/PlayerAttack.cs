using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class PlayerAttack : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField] float attackDmg = 5f, attackCooldown = 1f;
    [SerializeField] LayerMask playerLayer;

    bool canAttack = false;
    float attackTimer = 0f;

    TestEnemy enemy;

    public static event Action<PlayerAttack> OnPlayerAttack;

    private void OnEnable()
    {
        playerControls = new();
        playerControls.Enable();

        playerControls.General.Attack.performed += Attack_performed;
    }

    private void Attack_performed(InputAction.CallbackContext ctx)
    {
        if (canAttack)
        {
            canAttack = false;
            Debug.Log("attack");

            if (enemy != null)
            {
                Attack();
            }
        }       
    }

    private void Update()
    {
        AttackCooldown();
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
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                canAttack = false;
            }
            else
            {
                canAttack = true;
                attackTimer = attackCooldown;
            }
        }
    }

    void Attack()
    {
        OnPlayerAttack?.Invoke(this);

        enemy.GetComponent<Rigidbody>().AddForce(CalculateAttack(attackDmg), ForceMode.Impulse);
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
}
