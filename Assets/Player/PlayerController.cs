using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    public float crouchHeight = 0.5f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 5f;

    [Header("Hit Marker")]
    public Image hitMarker;
    public float hitMarkerDuration = 0.1f;


    [Header("Combat")]
    public int maxAmmo = 6;
    public float reloadTime = 2f;
    public Transform gunPivot;
    public Camera fpsCamera;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public float range = 300f;


    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isCrouching = false;
    private int currentAmmo;
    private bool isReloading = false;
    private float originalCameraY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentAmmo = maxAmmo;
        originalCameraY = playerCamera.localPosition.y;
        LockCursor(true);
        hitMarker.enabled = false;
    }

    void Update()
    {
        Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward * 10, Color.blue);
        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(false);
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            LockCursor(true);

        if (!isReloading)
        {
            HandleMovement();
            HandleLook();
            HandleShoot();
        }

        HandleReload();
        HandleCrouch();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = isCrouching ? crouchSpeed : Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        Vector3 targetCameraPos = playerCamera.localPosition;
        targetCameraPos.y = isCrouching ? originalCameraY - (standHeight - crouchHeight) : originalCameraY;
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCameraPos, crouchTransitionSpeed * Time.deltaTime);
    }

    void HandleShoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                StartCoroutine(Reload());
            }
        }
    }

    void Shoot()
    {
        currentAmmo--;
        muzzleFlash.Play();
        Debug.Log("Ammo: " + currentAmmo + "/" + maxAmmo);

        Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward * range, Color.red, 1f);
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            // Check if we hit an enemy
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log(enemy.name + " was hit!");
                enemy.Die();
                StartCoroutine(ShowHitMarker());
            }

            // Create hit effect
            GameObject hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(hitEffect, 1f);
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reloaded!");
    }

    void LockCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    IEnumerator ShowHitMarker()
    {
        if (hitMarker != null)
        {
            hitMarker.enabled = true;
            yield return new WaitForSeconds(hitMarkerDuration);
            hitMarker.enabled = false;
        }
    }

}