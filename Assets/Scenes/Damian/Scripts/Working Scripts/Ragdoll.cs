using UnityEngine;
using UnityEngine.AI;

public class Ragdoll : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private WaypintNavigator waypoint;
    public Collider specificTrigger;
    public string targetTag = "Player";
    public bool DebugRagdoll = false; 
    public float DestroyAfter = 100f;
    [Tooltip("Randomized between min/max values")] 
    public float minImpactForce = 20f;
    public float maxImpactForce = 100f;
    [Range(0, 1)] public float UpwardsForceRatio = 1f;
    
    // Audio variables
    public enum Gender { Male, Female }
    public Gender characterGender = Gender.Male;
    [Range(0f, 1f)] public float rareSoundChance = 0.1f;
    
    [Header("Male Sounds")]
    public AudioClip[] maleDefaultSounds;
    public AudioClip[] maleRareSounds;
    
    [Header("Female Sounds")]
    public AudioClip[] femaleDefaultSounds;
    public AudioClip[] femaleRareSounds;
    
    public AudioSource audioSource;
    [Range(0, 1)] public float volume = 1f;
    public bool playRandomPitch = false;
    [Range(0.1f, 3f)] public float minPitch = 0.9f;
    [Range(0.1f, 3f)] public float maxPitch = 1.1f;
    
    // Audio fading
    public bool enableDistanceFading = true;
    public float maxHearingDistance = 30f;
    public float minVolume = 0.2f;
    private Transform playerTransform;

    void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        waypoint = GetComponent<WaypintNavigator>();

        // Audio source setup
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Find player for distance fading
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null) playerTransform = player.transform;
        
        DisableRagdoll();
    }

    private void Start()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (DebugRagdoll && Input.GetKeyDown(KeyCode.Space))
        {
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(transform.position, (Vector3.up + Vector3.forward).normalized, randomForce);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Only ragdoll if the player is moving
            Rigidbody playerRb = other.attachedRigidbody;
            if (playerRb != null && playerRb.linearVelocity.magnitude < 0.2f)
                return;

            Vector3 impactDirection = (transform.position - other.transform.position).normalized;
            impactDirection = AddUpwardForce(impactDirection);
            
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(other.ClosestPoint(transform.position), impactDirection, randomForce);
        }
    }

    private Vector3 AddUpwardForce(Vector3 originalDirection)
    {
        return (originalDirection + Vector3.up * UpwardsForceRatio).normalized;
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    public void EnableRagdoll(Vector3 impactPoint, Vector3 direction, float force)
    {
        if (animator != null)
        {
            animator.enabled = false; 
        }
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        if (waypoint != null)
        {
            waypoint.enabled = false;
        }

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
            
            float distance = Vector3.Distance(impactPoint, rigidbody.position);
            float forceMultiplier = 1f / (distance + 1f);

            rigidbody.AddForce(direction * force * forceMultiplier, ForceMode.Impulse);
            
            if (rigidbody.transform == transform || rigidbody.transform.parent == transform)
            {
                rigidbody.AddForce(Vector3.up * force * 0.5f, ForceMode.Impulse);
            }
        }

        Debug.Log("RandollEnabeld");
        PlayGenderBasedSound();
        Destroy(gameObject, DestroyAfter);
    }

    private void PlayGenderBasedSound()
    {
        if (audioSource == null) return;

        // Determine if we should play rare sound
        bool playRareSound = Random.value < rareSoundChance;
        AudioClip clipToPlay = null;

        // Select appropriate sound based on gender and rarity
        switch (characterGender)
        {
            case Gender.Male:
                if (playRareSound && maleRareSounds.Length > 0)
                {
                    clipToPlay = maleRareSounds[Random.Range(0, maleRareSounds.Length)];
                }
                else if (maleDefaultSounds.Length > 0)
                {
                    clipToPlay = maleDefaultSounds[Random.Range(0, maleDefaultSounds.Length)];
                }
                break;
                
            case Gender.Female:
                if (playRareSound && femaleRareSounds.Length > 0)
                {
                    clipToPlay = femaleRareSounds[Random.Range(0, femaleRareSounds.Length)];
                }
                else if (femaleDefaultSounds.Length > 0)
                {
                    clipToPlay = femaleDefaultSounds[Random.Range(0, femaleDefaultSounds.Length)];
                }
                break;
        }

        if (clipToPlay != null)
        {
            // Calculate volume based on distance if enabled
            float finalVolume = volume;
            if (enableDistanceFading && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                finalVolume = Mathf.Lerp(minVolume, volume, 1 - Mathf.Clamp01(distance / maxHearingDistance));
            }

            // Set random pitch if enabled
            if (playRandomPitch)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
            else
            {
                audioSource.pitch = 1f;
            }

            audioSource.PlayOneShot(clipToPlay, finalVolume);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            // Only ragdoll if the player is moving
            Rigidbody playerRb = collision.rigidbody;
            if (playerRb != null && playerRb.linearVelocity.magnitude < 0.2f)
                return;

            ContactPoint contact = collision.contacts[0];
            Vector3 impactDirection = (contact.normal + Vector3.up * UpwardsForceRatio).normalized;
            float randomForce = Random.Range(minImpactForce, maxImpactForce);
            EnableRagdoll(contact.point, -impactDirection, randomForce);
        }
    }
}