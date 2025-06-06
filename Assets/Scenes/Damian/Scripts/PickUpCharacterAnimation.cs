using UnityEngine;
using System.Collections;
using System;

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

        // Handle smoke and model switching
        if (smokeEffect != null)
        {
            ParticleSystem smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
            smoke.Play();
            Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
            yield return new WaitForSeconds(smokeEffectDuration * 0.5f);

            if (cartPassengerModel != null)
            {
                if (characterModel != null)
                    characterModel.transform.localScale = Vector3.one * 0.001f;

                ParticleSystem cartSmoke = Instantiate(smokeEffect, cartPassengerModel.transform.position, Quaternion.identity);
                cartSmoke.Play();
                Destroy(cartSmoke.gameObject, cartSmoke.main.duration + cartSmoke.main.startLifetime.constantMax);

                yield return new WaitForSeconds(smokeEffectDuration * 0.5f);
                yield return new WaitForSeconds(2f);

                cartPassengerModel.transform.localScale = Vector3.one;
                cartPassengerModel.SetActive(true);
            }
        }
        else
        {
            if (characterModel != null)
                characterModel.transform.localScale = Vector3.one * 0.001f;
            if (cartPassengerModel != null)
            {
                cartPassengerModel.transform.localScale = Vector3.one;
                cartPassengerModel.SetActive(true);
            }
        }

        isAnimating = false;
        pickupCoroutine = null;

        yield return new WaitForSeconds(1f);

        // Reset passenger after animation
        ResetPassenger();

        // Invoke completion event
        if (OnPickupAnimationComplete != null)
            OnPickupAnimationComplete.Invoke();
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
        if (cartPassengerModel != null) cartPassengerModel.SetActive(false);

        isAnimating = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.ResetTrigger("Spin");
            animator.Play("Idle", 0);
        }
    }

    // Coroutine to scale character back after cooldown
    private IEnumerator ScaleBackAfterCooldown()
    {
        yield return new WaitForSeconds(10f);
        if (characterModel != null)
            characterModel.transform.localScale = characterOriginalScale;
    }
}