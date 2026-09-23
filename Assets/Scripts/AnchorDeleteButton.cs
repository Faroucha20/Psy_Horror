using UnityEngine;
using TMPro;

/// <summary>
/// Petit cube rouge attaché à un panneau de marqueur.
/// Au contact du doigt index, supprime le marqueur via SpatialAnchorManager.DeleteAnchor().
/// Initialisé par SpatialAnchorManager.RegisterPanel() — pas besoin de configurer manuellement.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class AnchorDeleteButton : MonoBehaviour
{
    private const string LeftFingertipName  = "OVRCameraRig/[BuildingBlock] OVRInteractionComprehensive/LeftInteractions/Interactors/Hand/HandPokeInteractor/HandIndexFingertip";
    private const string RightFingertipName = "OVRCameraRig/[BuildingBlock] OVRInteractionComprehensive/RightInteractions/Interactors/Hand/HandPokeInteractor/HandIndexFingertip";

    private const float ContactRadius   = 0.02f;
    private const float CooldownDuration = 1f;

    private static readonly Color ButtonColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    private static readonly Color HoverColor  = new Color(1f,   0.3f, 0.3f, 1f);

    private SpatialAnchorManager.RoomType _roomType;
    private SpatialAnchorManager          _anchorManager;

    private BoxCollider _collider;
    private Renderer    _renderer;
    private Material    _instanceMaterial;

    private Transform _leftFingertip;
    private Transform _rightFingertip;

    private bool  _isHovered;
    private bool  _triggered;
    private float _cooldown;

    /// <summary>Appelé par SpatialAnchorManager pour lier ce bouton à son marqueur.</summary>
    public void Init(SpatialAnchorManager manager, SpatialAnchorManager.RoomType type)
    {
        _anchorManager = manager;
        _roomType      = type;
    }

    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = false;

        _renderer = GetComponent<Renderer>();
        _instanceMaterial = new Material(_renderer.sharedMaterial);
        _renderer.material = _instanceMaterial;
        _instanceMaterial.color = ButtonColor;

        FindFingertips();
        BuildLabel();
    }

    void OnDestroy()
    {
        if (_instanceMaterial != null)
            Destroy(_instanceMaterial);
    }

    void Update()
    {
        if (_cooldown > 0f)
        {
            _cooldown -= Time.deltaTime;
            return;
        }

        bool contacted = IsFingertipInsideBounds(_leftFingertip)
                      || IsFingertipInsideBounds(_rightFingertip);

        if (contacted && !_isHovered)
            OnPokeEnter();
        else if (!contacted && _isHovered)
            OnPokeExit();
    }

    private bool IsFingertipInsideBounds(Transform fingertip)
    {
        if (fingertip == null) return false;

        Vector3 localPos = transform.InverseTransformPoint(fingertip.position);
        Vector3 halfSize = _collider.size * 0.5f + Vector3.one * ContactRadius;

        return Mathf.Abs(localPos.x) <= halfSize.x
            && Mathf.Abs(localPos.y) <= halfSize.y
            && Mathf.Abs(localPos.z) <= halfSize.z;
    }

    private void OnPokeEnter()
    {
        _isHovered = true;
        _instanceMaterial.color = HoverColor;

        if (_triggered || _anchorManager == null) return;
        _triggered = true;
        _cooldown  = CooldownDuration;

        Debug.Log($">>> Suppression du marqueur {_roomType}");
        _anchorManager.DeleteAnchor(_roomType);
    }

    private void OnPokeExit()
    {
        _isHovered = false;
        _triggered = false;
        _instanceMaterial.color = ButtonColor;
    }

    private void FindFingertips()
    {
        GameObject leftObj  = GameObject.Find(LeftFingertipName);
        GameObject rightObj = GameObject.Find(RightFingertipName);

        if (leftObj  != null) _leftFingertip  = leftObj.transform;
        if (rightObj != null) _rightFingertip = rightObj.transform;
    }

    private void BuildLabel()
    {
        Transform existing = transform.Find("DeleteLabel");
        if (existing != null) return;

        GameObject textObj = new GameObject("DeleteLabel");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.3f);
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale    = Vector3.one * 0.5f;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text      = "✕ Supprimer";
        tmp.fontSize  = 0.4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }
}
