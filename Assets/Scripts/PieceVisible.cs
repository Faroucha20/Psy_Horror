using UnityEngine;
using UnityEngine.Serialization;

public class PieceVisible : MonoBehaviour
{
    [Header("Visibility")]
    [Tooltip("Glissez ici le GameObject présent dans votre scène (ex: Ada ou Foyer)")]
    [FormerlySerializedAs("adaPrefab")]
    public GameObject targetPrefab;


    [Header("Animation AER")]
    [Tooltip("Nom du trigger Animator à déclencher sur l'objet AER")]
    [SerializeField] private string animationTriggerName = "Play";

    private void Start()
    {
        if (targetPrefab != null)
        {
            targetPrefab.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleEnter(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleEnter(collision.gameObject);
    }

    private void HandleEnter(GameObject otherObject)
    {
        if (otherObject.CompareTag("Player"))
        {
            if (targetPrefab != null)
            {
                targetPrefab.SetActive(true);
            }
        }

        if (otherObject.CompareTag("AER"))
        {
            Animator animator = otherObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(animationTriggerName);
            }
            else
            {
                Debug.LogWarning($"Aucun Animator trouvé sur {otherObject.name} pour déclencher l'animation.", this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetPrefab != null)
            {
                targetPrefab.SetActive(false);
            }
        }
    }
}