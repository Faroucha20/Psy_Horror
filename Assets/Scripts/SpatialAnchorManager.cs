using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Meta.XR.MRUtilityKit;

public class SpatialAnchorManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject infoPanelPrefab;

    [Header("UI Sélection")]
    [SerializeField] private GameObject roomSelectionUI;

    public enum RoomType { Classe, Cantine, Bureau, DMR, Ada, Pruha, Gwen, Foyer }

    /// <summary>Fired when an anchor has been placed or restored. Args: room type and its world position.</summary>
    public event Action<RoomType, Vector3> OnAnchorReady;

    // Maps each instantiated panel to its assigned room type.
    private readonly Dictionary<GameObject, RoomType> _anchorPanels = new();

    void Start()
    {
        LoadSavedAnchors();
    }

    /// <summary>Called by UI buttons to place a new spatial anchor in front of the user.</summary>
    public void PlaceAnchor(int roomTypeIndex)
    {
        RoomType type = (RoomType)roomTypeIndex;

        Vector3 position = Camera.main.transform.position
                         + Camera.main.transform.forward * 1f;
        Quaternion rotation = Quaternion.LookRotation(
            Camera.main.transform.forward, Vector3.up
        );

        GameObject panel = Instantiate(infoPanelPrefab, position, rotation);
        OVRSpatialAnchor anchor = panel.AddComponent<OVRSpatialAnchor>();

        SetPanelLabel(panel, type);
        RegisterPanel(panel, type);
        SaveAnchor(anchor, type);

        if (roomSelectionUI) roomSelectionUI.SetActive(false);
    }

    /// <summary>
    /// Returns the RoomType of the anchor whose panel is inside the given MRUK room,
    /// or null if no anchor has been placed in that room yet.
    /// </summary>
    public RoomType? GetRoomTypeForRoom(MRUKRoom room)
    {
        foreach (var kvp in _anchorPanels)
        {
            if (kvp.Key == null) continue;
            if (room.IsPositionInRoom(kvp.Key.transform.position))
                return kvp.Value;
        }
        return null;
    }

    /// <summary>
    /// Supprime le panneau et l'ancre spatiale correspondant au type de salle donné.
    /// Efface également la sauvegarde dans PlayerPrefs.
    /// </summary>
    public async void DeleteAnchor(RoomType type)
    {
        GameObject panelToDelete = null;
        foreach (var kvp in _anchorPanels)
        {
            if (kvp.Value == type && kvp.Key != null)
            {
                panelToDelete = kvp.Key;
                break;
            }
        }

        if (panelToDelete == null)
        {
            Debug.LogWarning($"[SpatialAnchorManager] Aucun panneau trouvé pour {type}.");
            return;
        }

        // Suppression de l'ancre sur le device
        OVRSpatialAnchor anchor = panelToDelete.GetComponent<OVRSpatialAnchor>();
        if (anchor != null)
        {
            var result = await anchor.EraseAnchorAsync();
            if (result.Success)
                Debug.Log($">>> Ancre {type} effacée du device.");
            else
                Debug.LogWarning($">>> Échec suppression ancre {type} : {result.Status}");
        }

        // Suppression de la sauvegarde PlayerPrefs
        string key = "Anchor_" + type;
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        _anchorPanels.Remove(panelToDelete);
        Destroy(panelToDelete);

        Debug.Log($">>> Marqueur {type} supprimé.");
    }

    private void RegisterPanel(GameObject panel, RoomType type)
    {
        _anchorPanels[panel] = type;

        // Câble le bouton de suppression présent sur le panneau, s'il existe
        AnchorDeleteButton deleteBtn = panel.GetComponentInChildren<AnchorDeleteButton>();
        if (deleteBtn != null)
            deleteBtn.Init(this, type);

        OnAnchorReady?.Invoke(type, panel.transform.position);
    }

    private static void SetPanelLabel(GameObject panel, RoomType type)
    {
        TextMeshProUGUI text = panel.GetComponentInChildren<TextMeshProUGUI>();
        if (text) text.text = type.ToString();
    }

    private async void SaveAnchor(OVRSpatialAnchor anchor, RoomType type)
    {
        while (!anchor.Created) await System.Threading.Tasks.Task.Yield();

        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            Debug.Log($">>> Ancre {type} sauvegardée : {anchor.Uuid}");
            PlayerPrefs.SetString("Anchor_" + type, anchor.Uuid.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError($">>> Échec sauvegarde {type}");
        }
    }

    private void LoadSavedAnchors()
    {
        foreach (RoomType type in Enum.GetValues(typeof(RoomType)))
        {
            string key = "Anchor_" + type;
            if (!PlayerPrefs.HasKey(key)) continue;

            Guid uuid = new Guid(PlayerPrefs.GetString(key));
            Debug.Log($">>> Chargement ancre {type} : {uuid}");
            LoadAnchorAsync(uuid, type);
        }
    }

    private async void LoadAnchorAsync(Guid uuid, RoomType type)
    {
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(
            new List<Guid> { uuid }, unboundAnchors
        );

        if (!result.Success || unboundAnchors.Count == 0)
        {
            Debug.LogError($">>> Impossible de charger {type}");
            return;
        }

        foreach (var unbound in unboundAnchors)
            RestoreAnchor(unbound, type);
    }

    private async void RestoreAnchor(OVRSpatialAnchor.UnboundAnchor unbound, RoomType type)
    {
        bool success = await unbound.LocalizeAsync();
        if (!success)
        {
            Debug.LogError($">>> Impossible de localiser {type}");
            return;
        }

        GameObject panel = Instantiate(infoPanelPrefab);
        OVRSpatialAnchor anchor = panel.AddComponent<OVRSpatialAnchor>();
        unbound.BindTo(anchor);

        SetPanelLabel(panel, type);
        RegisterPanel(panel, type);
        Debug.Log($">>> Ancre {type} restaurée !");
    }
}