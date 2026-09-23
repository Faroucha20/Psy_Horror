using UnityEngine;
using TMPro;

/// <summary>
/// Bouton pokable qui détecte le contact en surveillant directement
/// la position des index fingertips (main gauche et droite) par rapport
/// aux bounds du BoxCollider. Aucune dépendance au système de surface de
/// PokeInteractable — fonctionne nativement avec le HandPokeInteractor du SDK Meta.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PokableRoomButton : MonoBehaviour
{
    [SerializeField] private SpatialAnchorManager anchorManager;
    [SerializeField] private int roomTypeIndex;
    [SerializeField] private string roomLabel;

    // Chemins vers les index fingertips tels qu'ils existent dans la scène
    private const string LeftFingertipName  = "OVRCameraRig/[BuildingBlock] OVRInteractionComprehensive/LeftInteractions/Interactors/Hand/HandPokeInteractor/HandIndexFingertip";
    private const string RightFingertipName = "OVRCameraRig/[BuildingBlock] OVRInteractionComprehensive/RightInteractions/Interactors/Hand/HandPokeInteractor/HandIndexFingertip";

    // Rayon du fingertip (PokeInteractor._radius) + marge de confort
    private const float ContactRadius = 0.02f;

    private static readonly Color NormalColor = new Color(0.1f, 0.3f, 0.8f, 1f);
    private static readonly Color HoverColor  = new Color(0.2f, 0.6f, 1f,  1f);

    private BoxCollider _collider;
    private Renderer    _renderer;
    private Material    _instanceMaterial;

    private Transform _leftFingertip;
    private Transform _rightFingertip;

    private bool  _isHovered;
    private bool  _triggered;
    private float _cooldown;

    private const float CooldownDuration = 1f;

    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = false;

        _renderer = GetComponent<Renderer>();
        _instanceMaterial = new Material(_renderer.sharedMaterial);
        _renderer.material = _instanceMaterial;
        _instanceMaterial.color = NormalColor;

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

    /// <summary>Vérifie si le fingertip est à l'intérieur des bounds locaux du bouton.</summary>
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

        if (_triggered) return;
        _triggered = true;
        _cooldown  = CooldownDuration;

        Debug.Log($">>> Bouton {roomLabel} activé !");
        anchorManager.PlaceAnchor(roomTypeIndex);

        if (transform.parent != null)
            transform.parent.gameObject.SetActive(false);
    }

    private void OnPokeExit()
    {
        _isHovered = false;
        _triggered = false;
        _instanceMaterial.color = NormalColor;
    }

    /// <summary>Localise les index fingertips par leur chemin dans la hiérarchie de scène.</summary>
    private void FindFingertips()
    {
        GameObject leftObj  = GameObject.Find(LeftFingertipName);
        GameObject rightObj = GameObject.Find(RightFingertipName);

        if (leftObj  != null) _leftFingertip  = leftObj.transform;
        if (rightObj != null) _rightFingertip = rightObj.transform;

        if (_leftFingertip  == null)
            Debug.LogWarning($"[PokableRoomButton] Fingertip gauche introuvable : {LeftFingertipName}");
        if (_rightFingertip == null)
            Debug.LogWarning($"[PokableRoomButton] Fingertip droit introuvable : {RightFingertipName}");
    }

    private void BuildLabel()
    {
        Transform existing = transform.Find("Label");
        if (existing != null) return;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.6f);
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale    = Vector3.one;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text      = roomLabel;
        tmp.fontSize  = 0.3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }
}