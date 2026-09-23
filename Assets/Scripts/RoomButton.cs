using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private SpatialAnchorManager anchorManager;
    [SerializeField] private int roomTypeIndex;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            anchorManager.PlaceAnchor(roomTypeIndex);
        });
    }
}