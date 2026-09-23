using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    // Indiquez le nombre total de tuyaux à placer (2 dans votre cas)
    public int totalPipesRequired = 2; 
    
    // Référence à la porte ou à l'objet à activer une fois l'énigme résolue
    public GameObject objectToActivateOnSolve;

    private int pipesPlacedCount = 0;

    // Cette méthode sera appelée par chaque PipeSnapZone
    public void OnPipePlaced()
    {
        pipesPlacedCount++;

        // Vérifie si toutes les pièces sont en place
        if (pipesPlacedCount >= totalPipesRequired)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        Debug.Log("Énigme des tuyaux résolue !");

        // Action à réaliser : par exemple, ouvrir la porte
        if (objectToActivateOnSolve != null)
        {
            // Vous pouvez jouer une animation, désactiver l'objet, etc.
            // Ici, nous allons simplement l'activer (s'il était désactivé)
            // ou vous pourriez appeler une fonction spécifique sur un script de porte.
            objectToActivateOnSolve.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);
        }
    }
}