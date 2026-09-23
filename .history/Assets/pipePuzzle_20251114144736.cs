// 14/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.
// Script modified by Gemini to control Light component color.

using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public GameObject statusLight; // The status light GameObject (your lampBulb)
    public Material redMaterial; // Material for the red light
    public Material greenMaterial; // Material for the green light
    public GameObject door; // The door GameObject
    public ParticleSystem[] steamEffects; // Array of steam particle systems

    // --> AJOUT: Couleurs pour le composant Light
    [Header("Light Colors")]
    public Color redLightColor = Color.red;
    public Color greenLightColor = Color.green;

    [Header("Snap Zones")]
    public PipeSnapZone[] snapZones; // Array of snap zones

    private bool isPuzzleSolved = false;

    // --> AJOUT: Variables pour stocker les composants (meilleure performance)
    private Renderer lightRenderer;
    private Light lightComponent;

    // --> AJOUT: La méthode Start pour initialiser l'état
    private void Start()
    {
        // Récupère les composants une seule fois
        lightRenderer = statusLight.GetComponent<Renderer>();
        lightComponent = statusLight.GetComponent<Light>();

        // Définit l'état initial (lumière rouge)
        if (lightRenderer != null)
        {
            lightRenderer.material = redMaterial;
        }
        if (lightComponent != null)
        {
            lightComponent.color = redLightColor;
        }
    }

    private void Update()
    {
        // Ne vérifie que si le puzzle n'est pas déjà résolu
        if (!isPuzzleSolved)
        {
            CheckPuzzleState();
        }
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

        // Change the status light to green (Material)
        if (lightRenderer != null)
        {
            lightRenderer.material = greenMaterial;
        }

        // --> AJOUT: Change la couleur du composant Light en vert
        if (lightComponent != null)
        {
            lightComponent.color = greenLightColor;
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