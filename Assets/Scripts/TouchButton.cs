using UnityEngine;

public class TouchButton : MonoBehaviour
{
    [Tooltip("Glissez ici l'objet AER_SplineMover qui contient le script principal")]
    public DemiTour scriptPrincipal;

    // Unity ne lira QUE ce nom exact pour détecter une collision
    private void OnTriggerEnter(Collider other)
    {
        // On vérifie si l'objet qui touche a le Tag "Gauche" OU (||) le Tag "Droite"
        if (other.CompareTag("Gauche") || other.CompareTag("Droite"))
        {
            if (scriptPrincipal != null)
            {
                // On déclenche le changement d'image
                scriptPrincipal.SwitchToJeuImage();
            }
        }
    }
}