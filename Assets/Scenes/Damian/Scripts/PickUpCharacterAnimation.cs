using UnityEngine;
using System.Collections;
using System;

public class PickUpCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private GameObject characterModel;
    [SerializeField] private GameObject cartPassengerModel;
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private float smokeEffectDuration = 0.5f;
    [SerializeField] private Transform rootPosition;
    // Debugging variables
    [SerializeField] private bool debugReset = false;

    private bool isSpinning = false;
    private bool isAnimating = false;
    private Transform cartTarget;
    private Coroutine pickupCoroutine;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 characterOriginalScale;
    [SerializeField] private Transform playerTransform; // Assign this in inspector to the player
    private Coroutine resetScaleCoroutine; // To track cooldown coroutine

    void Update()
    {
        // Always look at player if assigned
        if (playerTransform != null)
        {
            Vector3 lookPos = playerTransform.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookPos);
        }

        if (debugReset == true)
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetPassenger();
        }
    }

    public void StartPickupAnimation(Transform cartTargetPos)
    {
        if (isAnimating || cartTargetPos == null) return;

        Debug.Log("Target Position: " + cartTargetPos.position); // Debug
        cartTarget = cartTargetPos;
        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        pickupCoroutine = StartCoroutine(AnimatePickupSequence());
    }

    private void Awake()
    {
        // Store the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        // Store original scale for character model
        if (characterModel != null)
            characterOriginalScale = characterModel.transform.localScale;
        // Ensure models are in correct initial state
        if (characterModel != null)
        {
            characterModel.SetActive(true);
            characterModel.transform.localScale = characterOriginalScale;
        }
        if (cartPassengerModel != null) cartPassengerModel.SetActive(false);

        // Remove startingRotation logic
    }

    public bool IsAnimating => isAnimating;

    public event Action OnPickupAnimationComplete;

    public void SpawnSmokeAtPassenger()
    {
        if (smokeEffect != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
        }
    }

    private IEnumerator AnimatePickupSequence()
    {
        isAnimating = true;

        // Ensure animator is valid
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            isAnimating = false;
            yield break;
        }

        // Step 1: Wave = Default Idle (no need to trigger anything)

        // Step 2: Walk to cart
        animator.SetBool("IsWalking", true);

        while (cartTarget != null && Vector3.Distance(transform.position, cartTarget.position) > arrivalThreshold)
        {
            // Move towards target
            Vector3 direction = (cartTarget.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face target (only on Y axis)
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

        // Step 3: Spin
        isSpinning = true;
        animator.SetTrigger("Spin");
        Debug.Log("Spin trigger set!");

        // Wait until the animator enters the Spin state
        bool spinStarted = false;
        float spinTimeout = 2f; // Prevent infinite loop
        while (!spinStarted && spinTimeout > 0f)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current animator state: " + state.fullPathHash);
            // Replace "Spin" with the exact state name in your Animator, e.g. "Base Layer.Spin"
            if (state.IsName("Spin") || state.IsTag("Spin"))
            {
                spinStarted = true;
                Debug.Log("Spin state entered!");
            }
            spinTimeout -= Time.deltaTime;
            yield return null;
        }
        // If spin didn't start, force it (for debugging)
        if (!spinStarted)
        {
            Debug.LogWarning("Spin state not entered, forcing with CrossFade.");
            animator.CrossFade("Spin", 0.05f); // Replace "Spin" with your actual state name if needed
            yield return null;
        }

        // Wait until the spin animation is done
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Spin") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Spin"))
        {
            yield return null;
        }
        isSpinning = false;

        // Step 4: Smoke effect before model swap
        if (smokeEffect != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
            yield return new WaitForSeconds(smokeEffectDuration * 0.5f);

            // After passenger smoke, spawn smoke at cart passenger position and scale cart passenger to 1
            if (cartPassengerModel != null)
            {
                // Spawn smoke at cart passenger position
                ParticleSystem cartSmoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
                cartSmoke.Play();
                Destroy(cartSmoke.gameObject, cartSmoke.main.duration + cartSmoke.main.startLifetime.constantMax);

                // Scale cart passenger to 1
                cartPassengerModel.transform.localScale = Vector3.one;
            }
        }

        // Model swap (scale character down instead of disabling)
        if (characterModel != null)
            characterModel.transform.localScale = Vector3.one * 0.001f;
        if (cartPassengerModel != null)
            cartPassengerModel.SetActive(true);

        // Optional: Smoke effect at cart position
        if (smokeEffect != null && cartTarget != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, cartTarget.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
            yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
        }

        isAnimating = false;
        pickupCoroutine = null;

        yield return new WaitForSeconds(1f);

        ResetPassenger();

        // Fire event for external listeners
        if (OnPickupAnimationComplete != null)
            OnPickupAnimationComplete.Invoke();
    }

    public void ResetPassenger()
    {
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }
        isSpinning = false;

        // Use rootPosition if assigned, otherwise fallback to originalPosition
        if (rootPosition != null)
        {
            transform.position = rootPosition.position;
            transform.rotation = rootPosition.rotation;
        }
        else
        {
            transform.position = originalPosition;
            // Look at player if assigned
            if (playerTransform != null)
            {
                Vector3 lookPos = playerTransform.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookPos);
            }
            // else keep current rotation
        }

        if (characterModel != null)
        {
            characterModel.SetActive(true);
            // Start cooldown coroutine to scale back after 10 seconds
            if (resetScaleCoroutine != null)
                StopCoroutine(resetScaleCoroutine);
            resetScaleCoroutine = StartCoroutine(ScaleBackAfterCooldown());
        }
        if (cartPassengerModel != null) cartPassengerModel.SetActive(false);

        isAnimating = false;
        // Reset animator state
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.ResetTrigger("Spin");
            animator.Play("Idle", 0); // Optionally force Idle state
        }
    }

    private IEnumerator ScaleBackAfterCooldown()
    {
        yield return new WaitForSeconds(10f);
        if (characterModel != null)
            characterModel.transform.localScale = characterOriginalScale;
    }
}