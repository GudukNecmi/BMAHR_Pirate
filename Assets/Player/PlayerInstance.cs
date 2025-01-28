using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance current;
    public Transform cameraTransform;
    public Transform playerTransform { get; private set; }
    public static Vector3 position => current.transform.position;

    void Awake()
    {
        current = this;
        playerTransform = base.transform;
    }
    public void Die()
    {
        // Handle player death
        Debug.Log("Player Died!");
        // Add game over logic
    }
}