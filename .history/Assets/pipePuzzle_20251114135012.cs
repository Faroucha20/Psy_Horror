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
}