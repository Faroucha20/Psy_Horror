using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerZone : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] string tag = "Player";
    [SerializeField] bool triggerOnce = true;

    [Header("Timer UI")]
    [SerializeField] bool showTimerUI;
    [SerializeField] float timerDuration = 10f;
    [SerializeField] string timerLabel = "Temps restant";

    [Header("Message UI")]
    [SerializeField] bool showMessageUI;
    [SerializeField] string message = "Zone declenchee";
    [SerializeField] float messageDuration = 3f;

    [Header("Sound")]
    [SerializeField] bool playSound;
    [SerializeField] AudioClip soundClip;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float pitch = 1f;
    [SerializeField] bool useRandomPitch;
    [SerializeField] float minPitch = 0.9f;
    [SerializeField] float maxPitch = 1.1f;

    [Header("Animation")]
    [SerializeField] bool playAnimation;
    [SerializeField] float animationDelay = 2f; // NOUVEAU : Le délai avant de lancer l'animation
    [SerializeField] RuntimeAnimatorController animationController;
    [SerializeField] float animationDuration = 2f;
    [SerializeField] bool pauseSplineAnimate = true;

    bool hasTriggered;

    void Reset()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = true;
    }

    void Awake()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
            Debug.LogWarning($"{name} : le collider devrait etre en Is Trigger.", this);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{name} : OnTriggerEnter avec {other.name}", this);
        if (!other.CompareTag(tag))
            return;

        if (triggerOnce && hasTriggered)
            return;

        hasTriggered = true;
        ExecuteActions(other.gameObject);
    }

    void ExecuteActions(GameObject otherGo)
    {
        if (showTimerUI)
            TriggerZoneUIOverlay.Instance.ShowTimer(timerDuration, timerLabel);

        if (showMessageUI)
            TriggerZoneUIOverlay.Instance.ShowMessage(message, messageDuration);

        if (playSound)
            PlayConfiguredSound();

        if (playAnimation && otherGo != null)
        {
            StartCoroutine(PlayAnimationOnce(otherGo));
        }
    }

    System.Collections.IEnumerator PlayAnimationOnce(GameObject otherGo)
    {
        // NOUVEAU : On attend le délai défini (ici 2 secondes par défaut) avant de continuer
        if (animationDelay > 0f)
        {
            yield return new WaitForSeconds(animationDelay);
        }

        Animator animator = otherGo.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{name} : Aucun Animator trouve sur {otherGo.name} pour jouer l'animation.", this);
            yield break;
        }

        UnityEngine.Splines.SplineAnimate splineAnimate = null;
        if (pauseSplineAnimate)
        {
            splineAnimate = otherGo.GetComponent<UnityEngine.Splines.SplineAnimate>();
            if (splineAnimate != null)
            {
                splineAnimate.Pause();
            }
        }

        RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
        if (animationController != null)
        {
            animator.runtimeAnimatorController = animationController;
        }

        // On attend la durée de l'animation
        yield return new WaitForSeconds(animationDuration);

        // On remet tout à la normale
        if (animator != null)
        {
            animator.runtimeAnimatorController = originalController;
        }

        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }

    void PlayConfiguredSound()
    {
        if (soundClip == null)
        {
            Debug.LogWarning($"{name} : aucun AudioClip assigne pour le son.", this);
            return;
        }

        float finalPitch = useRandomPitch
            ? Random.Range(minPitch, maxPitch)
            : pitch;

        finalPitch = Mathf.Max(0.01f, finalPitch);

        if (audioSource != null)
        {
            audioSource.pitch = finalPitch;
            audioSource.PlayOneShot(soundClip);
            return;
        }

        GameObject tempAudioObject = new GameObject("TriggerZoneSound");
        tempAudioObject.transform.position = transform.position;

        AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
        tempSource.clip = soundClip;
        tempSource.pitch = finalPitch;
        tempSource.spatialBlend = 0f;
        tempSource.Play();

        Destroy(tempAudioObject, soundClip.length / finalPitch + 0.1f);
    }
}