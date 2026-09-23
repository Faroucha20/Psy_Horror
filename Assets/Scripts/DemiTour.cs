using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class DemiTour : MonoBehaviour
{
    [Header("Configuration Mouvement")]
    [SerializeField] Transform modelToRotate;

    [Header("Événement 3D du 2ème Tour")]
    [SerializeField] GameObject imagesContainer;
    [SerializeField] float distanceFromPlayer = 0.5f;
    [SerializeField] float heightOffset = 1.2f;

    [Header("Gestion des Images Spécifiques")]
    [SerializeField] GameObject imageTab;
    [SerializeField] GameObject imageAlt;
    [SerializeField] GameObject imageCode;
    [SerializeField] GameObject imageJeu;
    [SerializeField] float endDisplayDuration = 3f;
    
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioClip warningAudioClip;
    [SerializeField] AudioClip explanationAudioClip;

    private SplineAnimate splineAnimate;
    private Transform playerTransform;
    private bool isMovingForward = true;
    private float previousTime = 0f;
    
    private int lapCount = 1; 
    private bool eventTriggered = false;

    private bool hasSucceeded = false;
    private bool explanationPlayed = false;
    private Coroutine timerCoroutine = null;

    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        if (splineAnimate != null) previousTime = splineAnimate.NormalizedTime;

        // NOUVELLE MÉTHODE : On utilise directement la caméra VR (les yeux du joueur)
        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Attention : Aucune MainCamera trouvée dans la scène !");
        }

        if (imagesContainer != null) imagesContainer.SetActive(false);
    }

    void Update()
    {
        if (splineAnimate == null || modelToRotate == null || !splineAnimate.IsPlaying) 
            return;

        float currentTime = splineAnimate.NormalizedTime;
        float timeDiff = currentTime - previousTime;

        if (timeDiff < -0.0001f && isMovingForward) 
        {
            isMovingForward = false;
            modelToRotate.localRotation = Quaternion.Euler(0, 180, 0); 
        }
        else if (timeDiff > 0.0001f && !isMovingForward) 
        {
            isMovingForward = true;
            modelToRotate.localRotation = Quaternion.Euler(0, 0, 0); 
            
            lapCount++; 
            if (lapCount == 2 && !eventTriggered)
            {
                eventTriggered = true;
                TriggerSecondLapEvent();
            }
        }
        previousTime = currentTime;
    }

    void TriggerSecondLapEvent()
    {
        if (imagesContainer == null) return;

        if (playerTransform != null)
        {
            Vector3 spawnPosition = playerTransform.position + (playerTransform.forward * distanceFromPlayer);
            spawnPosition.y += heightOffset;
            imagesContainer.transform.position = spawnPosition;
            imagesContainer.transform.rotation = playerTransform.rotation;
        }

        imagesContainer.SetActive(true);

        if (imageTab != null) imageTab.SetActive(true);
        if (imageAlt != null) imageAlt.SetActive(true);
        if (imageCode != null) imageCode.SetActive(true);
        if (imageJeu != null) imageJeu.SetActive(false);

        PlayAudio(audioClip);

        hasSucceeded = false;
        explanationPlayed = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(AltTabTimerCoroutine());
    }

    // NOUVEAU : Cette fonction est maintenant "public" pour que le bouton puisse l'appeler
    public void SwitchToJeuImage()
    {
        if (hasSucceeded) return;
        hasSucceeded = true;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (imageTab != null) imageTab.SetActive(false);
        if (imageAlt != null) imageAlt.SetActive(false);
        if (imageJeu != null) imageJeu.SetActive(false);
        
        if (imageCode != null) imageCode.SetActive(true);

        if (endDisplayDuration > 0)
        {
            StartCoroutine(HideAllImagesAfterDelay());
        }

        if (!explanationPlayed)
        {
            StartCoroutine(PlayExplanationAfterDelay(3f));
        }
    }

    System.Collections.IEnumerator HideAllImagesAfterDelay()
    {
        yield return new WaitForSeconds(endDisplayDuration);
        if (imagesContainer != null) imagesContainer.SetActive(false);
    }

    void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }

    System.Collections.IEnumerator AltTabTimerCoroutine()
    {
        yield return new WaitForSeconds(7f);

        if (!hasSucceeded)
        {
            // Le joueur n'a pas appuyé sur Alt & Tab dans les 5 secondes
            PlayAudio(warningAudioClip);

            if (warningAudioClip != null)
            {
                yield return new WaitForSeconds(warningAudioClip.length);
            }

            if (!explanationPlayed)
            {
                explanationPlayed = true;
                PlayAudio(explanationAudioClip);
            }
        }
        timerCoroutine = null;
    }

    System.Collections.IEnumerator PlayExplanationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!explanationPlayed)
        {
            explanationPlayed = true;
            PlayAudio(explanationAudioClip);
        }
    }
}