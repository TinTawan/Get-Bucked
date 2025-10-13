using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField] float attackDmg = 5f/*, attackRange = 2f, attackDuration = 1f*/;
    [SerializeField] LayerMask playerLayer;

    //bool isAttacking = false;

    TestEnemy enemy;

    private void OnEnable()
    {
        playerControls = new();
        playerControls.Enable();

        playerControls.General.Attack.performed += Attack_performed;
    }

    private void Attack_performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("attack");
        //isAttacking = true;

        if(enemy != null)
        {
            Attack();
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            Debug.Log("Enemy in range");
            /*if(col.TryGetComponent(out TestEnemy enemy))
            {

                Debug.Log("Hit enemy");
                enemy.SetRagdoll(true);
                Rigidbody enemyRB = enemy.GetComponent<Rigidbody>();
                enemyRB.AddForce(CalculateAttack(attackDmg));
            }*/

            enemy = col.TryGetComponent(out TestEnemy testEnemy) ? testEnemy : null;

        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            enemy = null;
        }
    }

    void Attack()
    {
        enemy.SetRagdoll(true);
        enemy.GetRagdollStabiliser().SetActivateForce(false);

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
