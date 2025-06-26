using UnityEngine;
using System.Collections;
using System;
using Unity.Cinemachine;

/// <summary>
/// Handles the pickup animation, model switching, and camera logic for a passenger character.
/// </summary>
public class PickUpCharacterAnimation : MonoBehaviour
{
    // === Inspector Fields ===

    [SerializeField] private Animator animator;                  // Animator for character
    [SerializeField] private float moveSpeed = 2f;               // Speed to move to cart
    [SerializeField] private float arrivalThreshold = 0.5f;      // Distance threshold for arrival
    [SerializeField] private GameObject characterModel;          // Reference to the character model
    [SerializeField] private GameObject cartPassengerModel;      // Reference to the cart passenger model
    [SerializeField] private ParticleSystem smokeEffect;         // Particle effect for smoke
    [SerializeField] private float smokeEffectDuration = 0.5f;   // Duration for smoke effect
    [SerializeField] private Transform rootPosition;             // Transform for reset position
    [SerializeField] private bool debugReset = false;            // Enable debug reset
    [SerializeField] private string idleStateName = "Idle";      // Animator idle state name

    // === State Variables ===

    private bool isSpinning = false;                             // True if spinning animation is active
    private bool isAnimating = false;                            // True if pickup animation is running
    private Transform cartTarget;                                // Target transform for cart
    private Coroutine pickupCoroutine;                           // Reference to running coroutine

    private Vector3 originalPosition;                            // Original position for reset
    private Quaternion originalRotation;                         // Original rotation for reset
    private Vector3 characterOriginalScale;                      // Original scale for reset
    [SerializeField] private Transform playerTransform;          // Reference to player transform
    private Coroutine resetScaleCoroutine;                       // Coroutine for scaling back

    [Header("Cinemachine Camera Settings")]
    [SerializeField] private CinemachineCamera mainCinemachineCamera; // Cinemachine camera reference
    [SerializeField] private Transform characterLookTarget;           // Camera look target
    [SerializeField] private float characterFOV = 40f;                // Camera FOV during animation
    [SerializeField] private float originalFOVValue = 60f;            // Default FOV value
    private Transform originalLookAtTarget;                           // Camera's original LookAt
    private float originalFOV;                                        // Camera's original FOV

    [Header("Camera Return Target")]
    [SerializeField] private Transform cameraReturnTarget;            // Camera target after animation

    private Coroutine cameraReturnCoroutine;                          // Coroutine for camera return

    // === Unity Methods ===

    void Update()
    {
        // Rotate to face player if not walking or spinning
        if (playerTransform != null && animator != null && !animator.GetBool("IsWalking") && !isSpinning)
        {
            Vector3 lookPos = playerTransform.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookPos);
        }

        // Debug: Reset passenger with R key if enabled
        if (debugReset == true)
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetPassenger();
        }
    }

    /// <summary>
    /// Starts the pickup animation sequence towards the cart.
    /// </summary>
    public void StartPickupAnimation(Transform cartTargetPos)
    {
        if (isAnimating || cartTargetPos == null) return;
        cartTarget = cartTargetPos;

        // Store camera state and focus on character
        if (mainCinemachineCamera != null)
        {
            originalLookAtTarget = mainCinemachineCamera.LookAt;
            originalFOV = mainCinemachineCamera.Lens.FieldOfView != 0 ? mainCinemachineCamera.Lens.FieldOfView : originalFOVValue;
            mainCinemachineCamera.LookAt = characterLookTarget != null ? characterLookTarget : this.transform;
            mainCinemachineCamera.Lens.FieldOfView = characterFOV;
        }

        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        pickupCoroutine = StartCoroutine(AnimatePickupSequence());
    }

    /// <summary>
    /// Initializes original transform and model state.
    /// </summary>
    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        if (characterModel != null)
            characterOriginalScale = characterModel.transform.localScale;
        if (characterModel != null)
        {
            characterModel.SetActive(true);
            characterModel.transform.localScale = characterOriginalScale;
        }
        if (cartPassengerModel != null) cartPassengerModel.SetActive(false);
    }

    /// <summary>
    /// Returns true if the pickup animation is running.
    /// </summary>
    public bool IsAnimating => isAnimating;

    /// <summary>
    /// Event triggered when pickup animation is complete.
    /// </summary>
    public event Action OnPickupAnimationComplete;

    /// <summary>
    /// Spawns smoke effect at the passenger's current position.
    /// </summary>
    public void SpawnSmokeAtPassenger()
    {
        if (smokeEffect != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
        }
    }

    /// <summary>
    /// Main coroutine for the pickup animation sequence.
    /// Handles walking, spinning, model switching, and camera logic.
    /// </summary>
    private IEnumerator AnimatePickupSequence()
    {
        isAnimating = true;
        if (animator == null)
        {
            isAnimating = false;
            yield break;
        }

        animator.SetBool("IsWalking", true);

        // Move towards cart target
        while (cartTarget != null && Vector3.Distance(transform.position, cartTarget.position) > arrivalThreshold)
        {
            Vector3 direction = (cartTarget.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            Vector3 lookPos = cartTarget.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            yield return null;
        }

        animator.SetBool("IsWalking", false);

        // Trigger spin animation
        isSpinning = true;
        animator.SetTrigger("Spin");

        // Wait for spin animation to start
        bool spinStarted = false;
        float spinTimeout = 2f;
        while (!spinStarted && spinTimeout > 0f)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Spin") || state.IsTag("Spin"))
            {
                spinStarted = true;
            }
            spinTimeout -= Time.deltaTime;
            yield return null;
        }
        if (!spinStarted)
        {
            animator.CrossFade("Spin", 0.05f);
            yield return null;
        }

        // Wait for spin animation to finish
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Spin") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Spin"))
        {
            yield return null;
        }
        isSpinning = false;

        // After spin, handle smoke, model switching, and camera
        if (cartPassengerModel != null)
        {
            if (characterModel != null)
                characterModel.transform.localScale = Vector3.one * 0.001f;

            // Smoke at NPC position
            if (smokeEffect != null)
            {
                ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
                smoke.Play();
                Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
                yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
            }

            // Camera focus on character
            if (mainCinemachineCamera != null)
            {
                originalLookAtTarget = mainCinemachineCamera.LookAt;
                originalFOV = mainCinemachineCamera.Lens.FieldOfView != 0 ? mainCinemachineCamera.Lens.FieldOfView : originalFOVValue;
                mainCinemachineCamera.LookAt = characterLookTarget != null ? characterLookTarget : this.transform;
                mainCinemachineCamera.Lens.FieldOfView = characterFOV;
            }

            cartPassengerModel.transform.localScale = Vector3.one;
            cartPassengerModel.SetActive(true);

            // Smoothly transition camera to cart passenger model
            if (mainCinemachineCamera != null && cartPassengerModel != null)
            {
                yield return StartCoroutine(SmoothLookAtTransition(mainCinemachineCamera, cartPassengerModel.transform, 0.5f));
                yield return new WaitForSeconds(1); // Hold for a short moment
                if (cameraReturnTarget != null)
                {
                    yield return StartCoroutine(SmoothLookAtTransition(mainCinemachineCamera, cameraReturnTarget, 0.5f));
                    mainCinemachineCamera.Lens.FieldOfView = originalFOV;
                }
            }

            // Smoke at cart passenger
            if (smokeEffect != null)
            {
                ParticleSystem cartSmoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
                cartSmoke.Play();
                Destroy(cartSmoke.gameObject, cartSmoke.main.duration + cartSmoke.main.startLifetime.constantMax);
                yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
            }

            cartPassengerModel.SetActive(true);

            yield return new WaitForSeconds(2f);
        }

        isAnimating = false;
        pickupCoroutine = null;

        yield return new WaitForSeconds(1f);

        // Reset passenger after animation
        ResetPassenger();

        // Smoothly restore camera's LookAt and FOV
        if (mainCinemachineCamera != null && cameraReturnTarget != null)
        {
            if (cameraReturnCoroutine != null)
                StopCoroutine(cameraReturnCoroutine);
            cameraReturnCoroutine = StartCoroutine(SmoothReturnCamera(mainCinemachineCamera, cameraReturnTarget, originalFOV, 1f));
        }
    }

    /// <summary>
    /// Smoothly transitions the camera's LookAt to the target transform over the given duration.
    /// </summary>
    private IEnumerator SmoothLookAtTransition(CinemachineCamera cam, Transform target, float duration)
    {
        if (cam == null || target == null) yield break;

        Transform startLookAt = cam.LookAt;
        Vector3 startPos = startLookAt != null ? startLookAt.position : cam.transform.forward * 10f;
        Vector3 endPos = target.position;
        float elapsed = 0f;

        // Use a temporary GameObject as an intermediate LookAt target
        GameObject tempLookAt = new GameObject("TempCameraLookAt");
        tempLookAt.transform.position = startPos;
        cam.LookAt = tempLookAt.transform;

        while (elapsed < duration)
        {
            tempLookAt.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        tempLookAt.transform.position = endPos;
        cam.LookAt = target;
        Destroy(tempLookAt);
    }

    /// <summary>
    /// Smoothly returns the camera to its original LookAt and FOV.
    /// </summary>
    private IEnumerator SmoothReturnCamera(CinemachineCamera cam, Transform target, float targetFOV, float duration)
    {
        if (cam == null) yield break;

        Transform startLookAt = cam.LookAt;
        float startFOV = cam.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.Lens.FieldOfView = targetFOV;
        cam.LookAt = target;
    }

    /// <summary>
    /// Resets the passenger to its original state and position.
    /// </summary>
    public void ResetPassenger()
    {
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }
        isSpinning = false;

        // Reset position and rotation
        if (rootPosition != null)
        {
            transform.position = rootPosition.position;
            transform.rotation = rootPosition.rotation;
        }
        else
        {
            transform.position = originalPosition;
            if (playerTransform != null)
            {
                Vector3 lookPos = playerTransform.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookPos);
            }
        }

        // Reset models and scale
        if (characterModel != null)
        {
            characterModel.SetActive(true);
            if (resetScaleCoroutine != null)
                StopCoroutine(resetScaleCoroutine);
            resetScaleCoroutine = StartCoroutine(ScaleBackAfterCooldown());
        }

        isAnimating = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.ResetTrigger("Spin");
            if (!string.IsNullOrEmpty(idleStateName) && AnimatorHasState(animator, 0, idleStateName))
                animator.Play(idleStateName, 0);
        }
    }

    /// <summary>
    /// Coroutine to scale the character back to original size after a cooldown.
    /// </summary>
    private IEnumerator ScaleBackAfterCooldown()
    {
        yield return new WaitForSeconds(10f);
        if (characterModel != null)
            characterModel.transform.localScale = characterOriginalScale;
    }

    /// <summary>
    /// Checks if the animator has a state with the given name in the given layer.
    /// </summary>
    private bool AnimatorHasState(Animator anim, int layer, string stateName)
    {
        return anim.HasState(layer, Animator.StringToHash(stateName));
    }

    /// <summary>
    /// Sets the cart passenger model inactive (used after job completion).
    /// </summary>
    public void SetCartPassengerInactive()
    {
        if (cartPassengerModel != null)
            cartPassengerModel.SetActive(false);
    }

    /// <summary>
    /// Spawns smoke effect at the cart passenger's position (used after job completion).
    /// </summary>
    public void SpawnSmokeAtCartPassenger()
    {
        if (smokeEffect != null && cartPassengerModel != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
        }
    }

    /// <summary>
    /// Call this when input is enabled to make the camera look at the cameraReturnTarget.
    /// </summary>
    public void CameraLookAtReturnTarget()
    {
        if (mainCinemachineCamera != null && cameraReturnTarget != null)
        {
            mainCinemachineCamera.LookAt = cameraReturnTarget;
            mainCinemachineCamera.Lens.FieldOfView = originalFOV;
        }
    }

    // Optional: helper to create an offset transform for the camera to look at (uncomment if you want to use it)
    // private Transform GetOffsetTransform(Transform target, Vector3 offset)
    // {
    //     GameObject temp = new GameObject("TempLookAt");
    //     temp.transform.position = target.position + offset;
    //     Destroy(temp, 2f); // Clean up after a short time
    //     return temp.transform;
    // }
}


//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡴⠶⠶⠶⠶⠶⠤⠤⠤⢤⣤⣠⡶⠻⠉⢹⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⣷⠀⢀⣀⡠⠤⠤⠤⠤⢄⣴⠟⠀⠀⠀⢀⣿⣿⣖⠶⠤⠤⣄⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⣿⡄⠀⣀⣀⣀⣀⣀⣀⣴⣿⠏⠀⠀⠀⡇⢘⣿⣿⣝⣧⣐⣒⣤⣬⠭⣉⣛⠒⠦⢤⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡀⠈⡍⠁⠀⠀⡾⢱⡟⠀⠀⠀⠀⡗⠈⢿⡿⠙⠚⢿⡄⠈⠉⠉⠓⠚⠿⢵⣖⣪⣭⣓⠢⣄⡀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡜⠀⣷⠀⠀⢸⠃⣿⣇⠀⢀⢀⣈⣀⣀⡈⢷⣶⣷⣶⣿⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠑⠶⠤⣉⡳⢦⡀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣧⡄⠸⡄⠀⢸⢠⣟⣧⣶⣿⣛⢿⣿⣿⣿⢷⣿⣿⡿⠿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠑⠺⢷⣄⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢹⡍⠀⣇⠀⣿⠻⣿⣿⣿⣿⣷⣦⡙⣿⣿⣿⣿⣯⣡⣤⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⡇⠀⢸⠀⣿⢼⣿⣿⠷⠋⡙⣟⣿⣉⠉⠹⢿⣿⣏⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢳⡀⠘⣆⣿⢸⣻⣿⣾⡏⣡⡄⣀⣉⣹⣶⣾⣿⡏⠟⣽⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⣿⡀⢻⡿⣌⣿⣿⣿⣿⠟⢛⣉⠁⠈⣿⣿⡟⠁⢠⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣯⠁⠘⣧⣿⣿⣿⣿⣿⣶⣶⣶⣶⣶⣿⡟⣤⣴⣿⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⡤⣿⣀⠀⢿⡏⢧⡈⣿⣿⣿⣿⣿⣿⣿⢿⠟⠛⠻⣿⠿⠙⣗⣦⣄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣶⠏⠁⢀⣿⡏⡀⢸⣷⣸⣿⡙⠿⣿⣿⣿⣿⣟⢮⡞⠀⠀⠋⠀⢘⣿⠟⠈⠉⠳⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⢰⣿⠁⣀⣀⣸⣿⣷⢷⠀⣿⣷⡱⣝⠒⣿⣿⢿⡿⣷⣿⠆⠀⣠⠂⢠⡟⡁⠀⠀⠈⠙⢿⣷⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⣠⣿⣩⠠⠤⣀⣭⣿⣿⣞⡆⢹⣷⡿⣍⠓⠦⣼⣿⣿⡋⢀⣤⡞⣁⠴⠿⢋⣕⠀⡀⠀⠀⠀⢻⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⢀⣼⠿⠏⢀⠀⠀⢸⣿⣿⣿⣿⢃⠀⣿⣿⠈⠣⡀⠨⣿⣿⣾⣛⣽⡟⠁⠛⣿⡿⠿⠀⡇⠀⠀⠀⠘⣿⡀                      ⠀⠀⠀⠀⠀⠀     ⠀⣠⠶⣶⠁⢶⡒⢶⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⢠⠟⣿⠤⣄⠈⣳⣼⣿⣿⠝⠃⣿⣾⡀⢹⣌⡓⠦⠬⠿⠿⢿⣿⣯⣭⡔⠒⡛⠁⠀⡤⣠⣿⠀⡄⠀⢠⠈⣷⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀        ⠀⠀⠀⠀⠀⣤⣾⣿⣦⢿⡷⠖⣳⢤⣿⢻⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⢰⣿⠁⣀⣀⣸⣿⣷⢷⡿⠏⠀⠀⠘⣷⣧⣀⣏⣛⡲⠤⠿⣿⣿⣯⣭⣽⣶⠞⠁⣠⢞⣽⣿⡟⢨⠁⠀⢸⡆⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀             ⠀⠀⠀⠀⢸⠏⢼⣿⠟⢉⣴⡿⢋⣠⡈⠻⣿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⣿⠁⣸⠃⠙⠛⣿⢦⣀⣀⣤⢶⣶⣿⣿⣿⣶⣶⡏⠀⢀⠉⢻⣟⣫⠿⠊⢀⡴⠋⠀⠀⠀⠀⠐⣻⣿⣧⣾⣏⠀⢸⡇⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀             ⠀⠀⠀⠀⠀⠈⣷⣿⡆⠞⣻⡿⠆⠸⡟⠋⠁⠀⢿⣿⠀⠀⠀⠀⢳⡛⢦⠀⢷⣼⡄⠀⢸⠛⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⢠⡟⢀⣴⣶⣾⣿⣿⡏⣰⢸⠇⣇⡏⠀⠀⠙⣿⣿⣟⡄⢹⡿⠿⠛⠛⣿⣶⣶⡦⠤⢔⣂⣀⠀⣾⡟⠻⡿⠁⢌⡿⠀⢀⡇⠀⠀⠀⠀⠀⠀⠀⠀             ⠀⠀⠀⠀⠀⠀⠀⠙⣿⡆⠌⠛⠀⣀⣹⡶⠤⣄⠈⣇⠀⢰⣶⣄⠀⠻⣉⣷⣸⡤⢷⠀⡏⢸⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⢸⠀⣸⠿⠟⠟⠙⡿⠀⡇⡾⢠⣏⢣⣄⢲⣄⡘⣿⣿⣷⠘⣇⢀⣒⡯⠉⠉⠁⠈⠓⠿⣿⠟⢰⣿⡇⠴⠁⠀⣼⡽⠁⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀             ⠀⠀⠀⠀⠀⠀⠀⠈⣷⣿⡆⠞⣻⡿⠆⠸⡟⠋⠁⠀⢿⣿⠀⠀⠀⠀⢳⡛⢦⠀⢷⣼⡄⠀⢸⠛⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⢘⣇⠉⠀⠀⣀⡼⠁⢸⣱⠇⢠⢻⡄⣿⣿⣿⣿⣿⣿⣏⠀⢻⣿⣷⣄⡄⠀⠀⢀⡤⠞⠁⢠⣿⣿⡀⠀⢀⣾⣵⡗⠀⣾⡇⠀⠀⠀⠀⠀⠀⠀⠀             ⢀⣶⣄⠀⠘⣿⣿⡄⠀⠀⠈⣷⣄⠛⠿⠟⣠⣾⣀⣀⢸⡀⠙⠋⠹⣌⠛⠛⠻⣿⢠⡆⣿⠙⠒⢶⡿⠿⣿⣷⣦⣄⡀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⢹⣟⣷⣦⣾⣷⠀⣿⣿⣧⣿⢿⣇⠀⣿⣿⠛⠀⡀⢀⠀⣠⣿⣫⣾⡾⠞⠛⠛⠾⠁⣸⢁⡿⠀⠀⠀⠀⠀⠀⠀⠀⠀             ⠈⠻⣿⣷⣄⠹⣟⣿⡄⠀⠀⣸⣿⠛⣶⣿⣿⣿⢿⡏⣩⢁⡄⠀⢠⡿⡰⢾⣷⣄⣤⢀⣜⣀⣀⡀⠁⠀⠈⠛⠿⠓⢿⣦⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⢸⡟⣿⡿⣿⣷⢿⣿⣿⣿⡜⣷⠄⢸⡘⣊⣭⡾⢋⡾⣿⣿⣿⣵⠶⠚⠁⠀⠀⠀⣿⣸⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀              ⢀⣿⣬⣿⠿⠿⡇⣾⣧⣾⢿⠀⠀⡸⠀⣿⣻⢽⣿⣿⠿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣏⢹⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀