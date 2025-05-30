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
    [SerializeField] private bool debugReset = false;

    private bool isSpinning = false;
    private bool isAnimating = false;
    private Transform cartTarget;
    private Coroutine pickupCoroutine;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 characterOriginalScale;
    [SerializeField] private Transform playerTransform;
    private Coroutine resetScaleCoroutine;

    void Update()
    {
        if (playerTransform != null && animator != null && !animator.GetBool("IsWalking") && !isSpinning)
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
        cartTarget = cartTargetPos;
        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        pickupCoroutine = StartCoroutine(AnimatePickupSequence());
    }

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
        if (animator == null)
        {
            isAnimating = false;
            yield break;
        }

        animator.SetBool("IsWalking", true);

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

        isSpinning = true;
        animator.SetTrigger("Spin");

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

        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Spin") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Spin"))
        {
            yield return null;
        }
        isSpinning = false;

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

        ResetPassenger();

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

    private IEnumerator ScaleBackAfterCooldown()
    {
        yield return new WaitForSeconds(10f);
        if (characterModel != null)
            characterModel.transform.localScale = characterOriginalScale;
    }
}