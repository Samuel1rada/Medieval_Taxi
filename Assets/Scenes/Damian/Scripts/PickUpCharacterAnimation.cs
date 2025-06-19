using UnityEngine;
using System.Collections;
using System;
using Unity.Cinemachine;

public class PickUpCharacterAnimation : MonoBehaviour
{
    // Serialized fields for Unity Inspector
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private GameObject characterModel;
    [SerializeField] private GameObject cartPassengerModel;
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private float smokeEffectDuration = 0.5f;
    [SerializeField] private Transform rootPosition;
    [SerializeField] private bool debugReset = false;
    [SerializeField] private string idleStateName = "Idle"; // <-- Add this line


    // Private state variables
    private bool isSpinning = false;
    private bool isAnimating = false;
    private Transform cartTarget;
    private Coroutine pickupCoroutine;

    // Original transform data for reset
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 characterOriginalScale;
    [SerializeField] private Transform playerTransform;
    private Coroutine resetScaleCoroutine;

    [Header("Cinemachine Camera Settings")]
    [SerializeField] private CinemachineCamera mainCinemachineCamera;
    [SerializeField] private Transform characterLookTarget; // usually characterModel.transform
    [SerializeField] private float characterFOV = 40f;
    [SerializeField] private float originalFOVValue = 60f; // <-- Add this line
    private Transform originalLookAtTarget;
    private float originalFOV;

    [Header("Camera Return Target")]
    [SerializeField] private Transform cameraReturnTarget; // <-- Add this line

    // Add a coroutine reference for camera return
    private Coroutine cameraReturnCoroutine;

    // Unity Update loop
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

        // Debug reset functionality
        if (debugReset == true)
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetPassenger();
        }
    }

    // Public method to start pickup animation
    public void StartPickupAnimation(Transform cartTargetPos)
    {
        if (isAnimating || cartTargetPos == null) return;
        cartTarget = cartTargetPos;

        // Store original LookAt and FOV, then switch to character (do NOT change Follow)
        if (mainCinemachineCamera != null)
        {
            originalLookAtTarget = mainCinemachineCamera.LookAt;
            // Use the field value as fallback if not set
            originalFOV = mainCinemachineCamera.Lens.FieldOfView != 0 ? mainCinemachineCamera.Lens.FieldOfView : originalFOVValue;
            mainCinemachineCamera.LookAt = characterLookTarget != null ? characterLookTarget : this.transform;
            mainCinemachineCamera.Lens.FieldOfView = characterFOV;
        }

        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        pickupCoroutine = StartCoroutine(AnimatePickupSequence());
    }

    // Initialization
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

    // Property to check if animation is running
    public bool IsAnimating => isAnimating;

    // Event for animation completion
    public event Action OnPickupAnimationComplete;

    // Spawn smoke effect at passenger location
    public void SpawnSmokeAtPassenger()
    {
        if (smokeEffect != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
        }
    }

    // Main pickup animation coroutine
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

        // === Only after spin is finished, handle smoke, model switching, and camera ===
        if (cartPassengerModel != null)
        {
            if (characterModel != null)
                characterModel.transform.localScale = Vector3.one * 0.001f;

            // Spawn smoke at NPC position
            if (smokeEffect != null)
            {
                ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
                smoke.Play();
                Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
                yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
            }

            // Camera change to character (optional, can be removed if not needed)
            if (mainCinemachineCamera != null)
            {
                originalLookAtTarget = mainCinemachineCamera.LookAt;
                originalFOV = mainCinemachineCamera.Lens.FieldOfView != 0 ? mainCinemachineCamera.Lens.FieldOfView : originalFOVValue;
                mainCinemachineCamera.LookAt = characterLookTarget != null ? characterLookTarget : this.transform;
                mainCinemachineCamera.Lens.FieldOfView = characterFOV;
            }

            cartPassengerModel.transform.localScale = Vector3.one;
            cartPassengerModel.SetActive(true); // <-- Move this line down

            // Immediately set camera to track cameraReturnTarget and FOV to originalFOV
            if (mainCinemachineCamera != null && cameraReturnTarget != null)
            {
                mainCinemachineCamera.LookAt = cameraReturnTarget;
                mainCinemachineCamera.Lens.FieldOfView = originalFOV;
            }

            // Spawn smoke at cart passenger
            if (smokeEffect != null)
            {
                ParticleSystem cartSmoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
                cartSmoke.Play();
                Destroy(cartSmoke.gameObject, cartSmoke.main.duration + cartSmoke.main.startLifetime.constantMax);
                yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
            }

            // Now activate the cart passenger model after all effects and transitions
            cartPassengerModel.SetActive(true);

            yield return new WaitForSeconds(2f);
        }

        isAnimating = false;
        pickupCoroutine = null;

        yield return new WaitForSeconds(1f);

        // Reset passenger after animation
        ResetPassenger();

        // Restore camera's LookAt and FOV smoothly to cameraReturnTarget (do NOT change Follow)
        if (mainCinemachineCamera != null && cameraReturnTarget != null)
        {
            if (cameraReturnCoroutine != null)
                StopCoroutine(cameraReturnCoroutine);
            cameraReturnCoroutine = StartCoroutine(SmoothReturnCamera(mainCinemachineCamera, cameraReturnTarget, originalFOV, 1f));
        }
    }

    // Smoothly return the camera to its original LookAt and FOV
    private IEnumerator SmoothReturnCamera(CinemachineCamera cam, Transform target, float targetFOV, float duration)
    {
        if (cam == null) yield break;

        Transform startLookAt = cam.LookAt;
        float startFOV = cam.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Only interpolate FOV, as LookAt is a Transform reference (snap at end)
            cam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.Lens.FieldOfView = targetFOV;
        cam.LookAt = target;
    }

    // Reset passenger to original state
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
        // if (cartPassengerModel != null) cartPassengerModel.SetActive(false); // <-- Remove or comment out this line

        isAnimating = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.ResetTrigger("Spin");
            if (!string.IsNullOrEmpty(idleStateName) && AnimatorHasState(animator, 0, idleStateName))
                animator.Play(idleStateName, 0);
            // else: do not call Play if state doesn't exist
        }
    }

    // Coroutine to scale character back after cooldown
    private IEnumerator ScaleBackAfterCooldown()
    {
        yield return new WaitForSeconds(10f);
        if (characterModel != null)
            characterModel.transform.localScale = characterOriginalScale;
    }

    // Utility to check if animator has a state in a given layer
    private bool AnimatorHasState(Animator anim, int layer, string stateName)
    {
        return anim.HasState(layer, Animator.StringToHash(stateName));
    }

    // Add this method at the end of the class
    public void SetCartPassengerInactive()
    {
        if (cartPassengerModel != null)
            cartPassengerModel.SetActive(false);
    }

    // Add this method at the end of the class
    public void SpawnSmokeAtCartPassenger()
    {
        if (smokeEffect != null && cartPassengerModel != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
        }
    }
}