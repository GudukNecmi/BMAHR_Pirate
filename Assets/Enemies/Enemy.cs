using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Combat")]
    public float shootDelay = 0.2f;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public Animator animator;
    public Collider enemyCollider;

    private bool isDead = false;

    private bool isActive = false;

    public void EnableEnemy()
    {
        isActive = true;
        // Add any activation logic (animations, sounds, etc)
    }

    public void DisableEnemy()
    {
        isActive = false;
        // Add any deactivation logic
    }

    [System.Obsolete]
    public void ShootAtPlayer()
    {
        if (isActive)
        {
            StartCoroutine(ShootSequence());
        }
    }

    [System.Obsolete]
    IEnumerator ShootSequence()
    {
        yield return new WaitForSeconds(shootDelay);

        PlayerInstance playerInstance = FindObjectOfType<PlayerInstance>();
        if (PlayerInstance.current != null)
        {
            Vector3 direction = (PlayerInstance.current.transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    PlayerInstance.current.Die();
                }
            }
        }
    }
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        enemyCollider.enabled = false;

        // Notify room system
        RoomTrigger.current?.EnemyDied();
    }
}