using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    public Transform camera1;

    private Rigidbody rb;
    private Vector3 offset;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 12f;
    public float deceleration = 10f;
    public float rotSpeed = 200f;

    private float rotX, rotY;
    private Vector3 currentVelocity;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float stamina;
    public float staminaDrain = 1f;
    public float staminaRegen = 0.5f;
    public float sprintStaminaReq = 0.2f;

    private bool isSprinting;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        offset = camera1.position - transform.position;

        stamina = maxStamina;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Rotate();
        HandleStamina();
        HandleMovement();
    }


    // ------------------ ROTATION ------------------
    void Rotate()
    {
        rotY += Input.GetAxisRaw("Mouse X") * rotSpeed * Time.deltaTime;
        rotX -= Input.GetAxisRaw("Mouse Y") * rotSpeed * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -30, 50f);

        transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        camera1.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }


    // ------------------ STAMINA SYSTEM ------------------
    void HandleStamina()
    {
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        // Only sprint if moving & has stamina
        if (wantsToSprint && stamina > sprintStaminaReq && IsMovingInput())
        {
            isSprinting = true;
            stamina -= staminaDrain * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
        else
        {
            isSprinting = false;
        }

        // Regen stamina when not sprinting
        if (!isSprinting)
        {
            stamina += staminaRegen * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
    }


    // ------------------ MOVEMENT W/ SMOOTH ACCEL ------------------
    void HandleMovement()
    {
        camera1.position = transform.position + offset;

        Vector3 input =
            transform.forward * Input.GetAxisRaw("Vertical") +
            transform.right * Input.GetAxisRaw("Horizontal");

        input = input.normalized;

        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = input * targetSpeed;

        // Smooth acceleration / deceleration
        if (input.magnitude > 0.1f)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }


    bool IsMovingInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f ||
               Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;
    }
}