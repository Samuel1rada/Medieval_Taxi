using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 10f; // Maximum normal speed
    public float acceleration = 20f; // How fast the object accelerates
    public float boostSpeed = 20f; // Maximum speed during boost
    public float boostDuration = 2f; // How long the boost lasts
    public float boostCooldown = 5f; // Cooldown after using boost
    public float brakeForce = 25f; // How fast the object slows down when braking
    public float reverseSpeed = 5f; // Speed when reversing
    public float turnSpeed = 5f; // How fast the object turns

    private float currentSpeed = 0f;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isBoosting = false;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction xAxisAction;
    private InputAction boostAction;
    private InputAction brakeAction;
    private InputAction driveAction;

    private Vector2 movementInput; // Stores the XAxis input (Vector2)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing!");
        }

        // Set up input system
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing!");
        }

        xAxisAction = playerInput.actions["XAxis"];
        boostAction = playerInput.actions["Boost"];
        brakeAction = playerInput.actions["Brake"];
        driveAction = playerInput.actions["Drive"];
    }

    void Update()
    {
        HandleBoost();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleTurning();
        HandleBrake();
    }

    void HandleMovement()
    {
        float driveInput = driveAction.ReadValue<float>(); // Drive input (forward/backward)
        float targetSpeed = isBoosting ? boostSpeed : maxSpeed;

        // Determine the target speed based on drive input
        if (driveInput > 0)
        {
            // Accelerate forward
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed * driveInput, acceleration * Time.fixedDeltaTime);
        }
        else if (driveInput < 0)
        {
            // Accelerate backward
            currentSpeed = Mathf.Lerp(currentSpeed, -reverseSpeed * Mathf.Abs(driveInput), acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // No drive input, slow down naturally
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
        }

        // Move the object forward or backward
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void HandleTurning()
    {
        movementInput = xAxisAction.ReadValue<Vector2>(); // Read Vector2 input for turning

        // Calculate the turn direction based on X-axis input
        float turnInput = movementInput.x;

        // Smoothly rotate the object
        Quaternion deltaRotation = Quaternion.Euler(0f, turnInput * turnSpeed, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    void HandleBoost()
    {
        if (boostAction.triggered && !isBoosting && cooldownTimer <= 0f)
        {
            isBoosting = true;
            boostTimer = boostDuration;
        }

        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                isBoosting = false;
                cooldownTimer = boostCooldown;
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    void HandleBrake()
    {
        float brakeInput = brakeAction.ReadValue<float>(); // Brake input

        if (brakeInput > 0)
        {
            // Apply braking force
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);

            // If fully stopped and brake is still held, start reversing
            if (Mathf.Approximately(currentSpeed, 0f))
            {
                currentSpeed = Mathf.Lerp(currentSpeed, -reverseSpeed * brakeInput, acceleration * Time.fixedDeltaTime);
            }
        }
    }
}