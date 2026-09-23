using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RoomDetector : MonoBehaviour
{
    void Start()
    {
        // LoadSceneFromDevice est géré par ActiveRoomDetector (singleton)
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        // Récupère le sol
        MRUKAnchor floor = room.FloorAnchor;
        Debug.Log("Sol trouvé à : " + floor.transform.position);

        // Récupère les murs
        foreach (MRUKAnchor wall in room.WallAnchors)
        {
            Debug.Log("Mur trouvé à : " + wall.transform.position);
        }

        // Récupère le plafond
        MRUKAnchor ceiling = room.CeilingAnchor;
        Debug.Log("Plafond trouvé à : " + ceiling.transform.position);
    }
}