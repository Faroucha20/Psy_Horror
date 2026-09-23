using UnityEngine;
using Meta.XR.MRUtilityKit;

public class WallPanelPlacer : MonoBehaviour
{
    [SerializeField] private GameObject infoPanelPrefab;
    [SerializeField] private float heightFromFloor = 1.5f;

    void Start()
    {
        Debug.Log(">>> WallPanelPlacer : Start");

        if (MRUK.Instance == null)
        {
            Debug.LogError(">>> MRUK.Instance est NULL !");
            return;
        }

        // LoadSceneFromDevice est géré par ActiveRoomDetector (singleton)
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        Debug.Log(">>> Callback enregistré, en attente du chargement de scène");
    }

    void OnSceneLoaded()
    {
        Debug.Log(">>> OnSceneLoaded déclenché !");

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        if (room == null)
        {
            Debug.LogError(">>> Room est NULL !");
            return;
        }

        Debug.Log(">>> Nombre de murs : " + room.WallAnchors.Count);

        if (room.WallAnchors.Count == 0)
        {
            Debug.LogError(">>> Aucun mur détecté !");
            return;
        }

        MRUKAnchor wall = room.WallAnchors[0];
        Debug.Log(">>> Mur trouvé à : " + wall.transform.position);

        PlacePanelOnWall(wall);

        Debug.Log(">>> Panneau instancié !");
    }

    void PlacePanelOnWall(MRUKAnchor wall)
    {
        Vector3 position = wall.transform.position;
        position.y = heightFromFloor;
        Vector3 offset = wall.transform.forward * 0.05f;

        // Rotation corrigée : 180° sur Y pour éviter l'effet miroir
        Quaternion rotation = wall.transform.rotation * Quaternion.Euler(0, 180, 0);

        GameObject panel = Instantiate(
            infoPanelPrefab,
            position + offset,
            rotation
        );
    }
}