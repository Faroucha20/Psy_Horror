using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Assurez-vous d'avoir ce using

public class PipeSnapZone : MonoBehaviour
{
    [Header("Snap Zone Settings")]
    public GameObject correctPipe; // Le tuyau parent (avec le Rigidbody)
    public Transform snapPosition; // L'endroit où il doit se fixer

    public bool IsPipePlacedCorrectly { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        // --- CORRECTION ICI ---
        // On cherche le Rigidbody de l'objet qui entre. 
        // Cela permet de trouver le parent même si c'est un collider enfant qui touche.
        GameObject incomingObject = other.gameObject;
        if (other.attachedRigidbody != null)
        {
            incomingObject = other.attachedRigidbody.gameObject;
        }

        // Vérification
        if (incomingObject == correctPipe)
        {
            PlacePipe(incomingObject);
        }
    }

    private void PlacePipe(GameObject pipeObject)
    {
        IsPipePlacedCorrectly = true;

        // 1. Couper la physique et l'attraper en VR
        var rb = pipeObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Rend l'objet insensible à la gravité/physique
            rb.velocity = Vector3.zero;
        }

        // Désactiver le grab pour qu'on ne puisse plus l'enlever (optionnel)
        // Note: Si vous avez une erreur ici, remplacez 'XRGrabInteractable' par le bon nom selon votre version
        var grabInteractable = pipeObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        // Si la ligne du dessus est rouge (erreur), essayez celle-ci (pour les anciennes versions Unity) :
        // var grabInteractable = pipeObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // 2. Placer parfaitement
        pipeObject.transform.position = snapPosition.position;
        pipeObject.transform.rotation = snapPosition.rotation;

        Debug.Log($"Succès ! {pipeObject.name} placé dans la zone {name}.");
    }

    private void OnTriggerExit(Collider other)
    {
        // Même logique pour la sortie (au cas où on permettrait de l'enlever)
        GameObject outgoingObject = other.gameObject;
        if (other.attachedRigidbody != null)
        {
            outgoingObject = other.attachedRigidbody.gameObject;
        }

        if (outgoingObject == correctPipe)
        {
            // Si on veut gérer le retrait, on mettrait IsPipePlacedCorrectly = false ici
            // Mais souvent dans un puzzle, une fois placé, c'est fini.
        }
    }
}