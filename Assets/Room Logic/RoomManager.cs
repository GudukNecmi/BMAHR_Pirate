using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomTrigger : MonoBehaviour
{
    public static RoomTrigger current;

    [Header("Combat Settings")]
    public float warningTime = 1f;
    public float aimLockDuration = 0.4f;

    private List<Enemy> enemiesInRoom = new List<Enemy>();
    private int enemiesRemaining;
    private bool isActiveRoom = false;

    void Start()
    {
        current = this;
        GetComponent<Collider>().isTrigger = true;
        FindEnemiesInRoom();
    }

    void FindEnemiesInRoom()
    {
        foreach (Transform child in transform)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemiesInRoom.Add(enemy);
                enemy.DisableEnemy(); // Start with enemies inactive
            }
            enemiesRemaining = enemiesInRoom.Count;
        }
    }
    public void EnemyDied()
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            StartCoroutine(RoomCleared());
        }
    }
    IEnumerator RoomCleared()
    {
        // Wait for any animations
        yield return new WaitForSeconds(1f);

        // Trigger dialogue system
        DialogueSystem.StartDialogue("RoomCleared");
    }

    [System.Obsolete]
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActiveRoom)
        {
            isActiveRoom = true;
            StartCoroutine(RoomSequence());
        }
    }

    [System.Obsolete]
    IEnumerator RoomSequence()
    {
        // Activate enemies
        foreach (Enemy enemy in enemiesInRoom)
        {
            enemy.EnableEnemy();
        }

        // Wait for warning time
        yield return new WaitForSeconds(warningTime);

        // Lock aim to nearest enemy
        Transform nearestEnemy = GetNearestEnemy();
        if (nearestEnemy != null)
        {
            LockPlayerAim(nearestEnemy);

            // Wait before enemies shoot
            yield return new WaitForSeconds(aimLockDuration);

            // Make all enemies shoot
            foreach (Enemy enemy in enemiesInRoom)
            {
                enemy.ShootAtPlayer();
            }
        }
    }

    Transform GetNearestEnemy()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 playerPosition = PlayerInstance.position;

        foreach (Enemy enemy in enemiesInRoom)
        {
            float distance = Vector3.Distance(enemy.transform.position, playerPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    [System.Obsolete]
    void LockPlayerAim(Transform target)
    {
        // Get the player instance
        PlayerInstance player = FindObjectOfType<PlayerInstance>();

        // Get direction to enemy
        Vector3 direction = target.position - player.cameraTransform.position;
        direction.y = 0; // Keep horizontal only for player rotation

        // Rotate player to face enemy
        player.transform.rotation = Quaternion.LookRotation(direction);

        // Calculate vertical aim
        Vector3 fullDirection = target.position - player.cameraTransform.position;
        float angle = Mathf.Atan2(fullDirection.y, fullDirection.magnitude) * Mathf.Rad2Deg;

        // Set camera rotation
        player.cameraTransform.localEulerAngles = new Vector3(-angle, 0, 0);
    }
}