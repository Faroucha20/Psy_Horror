using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    // Indiquez le nombre total de tuyaux à placer (2 dans votre cas)
    public int totalPipesRequired = 2; 
    
    // Référence à la porte ou à l'objet à activer une fois l'énigme résolue
    public GameObject objectToActivateOnSolve;
// 14/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public GameObject statusLight; // The status light GameObject
    public Material redMaterial; // Material for the red light
    public Material greenMaterial; // Material for the green light
    public GameObject door; // The door GameObject
    public ParticleSystem[] steamEffects; // Array of steam particle systems

    [Header("Snap Zones")]
    public PipeSnapZone[] snapZones; // Array of snap zones

    private bool isPuzzleSolved = false;

    private void Update()
    {
        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        // Check if all snap zones have the correct pipes
        foreach (PipeSnapZone snapZone in snapZones)
        {
            if (!snapZone.IsPipePlacedCorrectly)
            {
                return; // Puzzle is not solved yet
            }
        }

        // If all pipes are placed correctly, solve the puzzle
        SolvePuzzle();
    }

    private void SolvePuzzle()
    {
        if (isPuzzleSolved) return;

        isPuzzleSolved = true;

        // Change the status light to green
        Renderer lightRenderer = statusLight.GetComponent<Renderer>();
        if (lightRenderer != null)
        {
            lightRenderer.material = greenMaterial;
        }

        // Unlock the door
        Door doorScript = door.GetComponent<Door>();
        if (doorScript != null)
        {
            doorScript.Unlock();
        }

        // Stop steam particle effects
        foreach (ParticleSystem steamEffect in steamEffects)
        {
            steamEffect.Stop();
        }

        Debug.Log("Pipe puzzle solved! Door unlocked.");
    }
}// 14/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public GameObject statusLight; // The status light GameObject
    public Material redMaterial; // Material for the red light
    public Material greenMaterial; // Material for the green light
    public GameObject door; // The door GameObject
    public ParticleSystem[] steamEffects; // Array of steam particle systems

    [Header("Snap Zones")]
    public PipeSnapZone[] snapZones; // Array of snap zones

    private bool isPuzzleSolved = false;

    private void Update()
    {
        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        // Check if all snap zones have the correct pipes
        foreach (PipeSnapZone snapZone in snapZones)
        {
            if (!snapZone.IsPipePlacedCorrectly)
            {
                return; // Puzzle is not solved yet
            }
        }

        // If all pipes are placed correctly, solve the puzzle
        SolvePuzzle();
    }

    private void SolvePuzzle()
    {
        if (isPuzzleSolved) return;

        isPuzzleSolved = true;

        // Change the status light to green
        Renderer lightRenderer = statusLight.GetComponent<Renderer>();
        if (lightRenderer != null)
        {
            lightRenderer.material = greenMaterial;
        }

        // Unlock the door
        Door doorScript = door.GetComponent<Door>();
        if (doorScript != null)
        {
            doorScript.Unlock();
        }

        // Stop steam particle effects
        foreach (ParticleSystem steamEffect in steamEffects)
        {
            steamEffect.Stop();
        }

        Debug.Log("Pipe puzzle solved! Door unlocked.");
    }
}
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