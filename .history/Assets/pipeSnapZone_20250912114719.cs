using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PipeSnapZone : MonoBehaviour
{
    // Faites glisser le bon modèle de tuyau ici dans l'inspecteur Unity
    public GameObject correctPipe; 

    // Référence au gestionnaire principal de l'énigme
    public PipePuzzleManager puzzleManager; 

    private bool isPipePlaced = false;

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet qui entre est le bon tuyau et si la place n'est pas déjà prise
        if (!isPipePlaced && other.gameObject == correctPipe)
        {
            // Le bon tuyau est dans la zone !
            PlacePipe(other.gameObject);
        }
    }

    private void PlacePipe(GameObject pipe)
    {
        isPipePlaced = true;

        // Désactive la possibilité de le saisir à nouveau
        XRGrabInteractable grabInteractable = pipe.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Désactive la physique pour qu'il ne bouge plus
        Rigidbody pipeRigidbody = pipe.GetComponent<Rigidbody>();
        if (pipeRigidbody != null)
        {
            pipeRigidbody.isKinematic = true;
            pipeRigidbody.useGravity = false;
        }

        // "Clipse" le tuyau parfaitement en place (position et rotation)
        pipe.transform.position = transform.position;
        pipe.transform.rotation = transform.rotation;
        
        // Informe le gestionnaire qu'une pièce a été placée
        if (puzzleManager != null)
        {
            puzzleManager.OnPipePlaced();
        }
        
        // Optionnel : Jouer un son de "clic" pour le feedback
        // AudioSource.PlayClipAtPoint(placementSound, transform.position);
    }
}