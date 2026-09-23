using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Minigames.Placement
{
    /// <summary>
    /// Emplacement "fantome" (ex: ghost_croissant). Quand l'objet reel correspondant
    /// (un <see cref="PlaceableItem"/> avec le bon Id) est depose dans le collider trigger,
    /// une petite animation se joue puis le fantome est remplace : soit on applique la bonne
    /// texture, soit on verrouille l'objet reel a sa place, soit on masque simplement le fantome.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GhostSlot : MonoBehaviour
    {
        public enum ResolveMode
        {
            SwapGhostMaterial,   // On garde le fantome mais on lui met la vraie texture, l'objet depose est masque.
            KeepPlacedItem,      // On masque le fantome et on garde/verrouille l'objet reel a la place.
            HideGhostOnly        // On masque juste le fantome, sans rien toucher a l'objet depose.
        }

        [Header("Cible acceptee")]
        [SerializeField]
        [Tooltip("Id attendu. Doit correspondre a l'Item Id du PlaceableItem depose.")]
        string acceptedItemId = "croissant";

        [SerializeField]
        [Tooltip("Attendre que l'objet soit lache (plus tenu par la main XR) avant de valider.")]
        bool requireReleased = true;

        [Header("Rendu du fantome")]
        [SerializeField]
        [Tooltip("Renderers du fantome. Rempli automatiquement avec les enfants si laisse vide.")]
        Renderer[] ghostRenderers;

        [Header("Resolution")]
        [SerializeField] ResolveMode resolveMode = ResolveMode.KeepPlacedItem;

        [SerializeField]
        [Tooltip("Materiau applique au fantome en mode SwapGhostMaterial (la bonne texture).")]
        Material solvedMaterial;

        [SerializeField]
        [Tooltip("Aligner l'objet depose sur la position/rotation de ce slot.")]
        bool snapPlacedItem = true;

        [SerializeField]
        [Tooltip("Point de destination du snap. Ce transform par defaut si laisse vide.")]
        Transform snapPoint;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Duree de l'animation de validation (effet de pop + rotation).")]
        float animationDuration = 0.45f;

        [SerializeField]
        [Tooltip("Amplitude du 'pop' de scale pendant l'animation.")]
        float popScale = 1.35f;

        [SerializeField]
        [Tooltip("Animator optionnel : on declenche ce trigger au moment de la validation.")]
        Animator optionalAnimator;

        [SerializeField] string animatorTrigger = "";

        [Header("Son")]
        [SerializeField] AudioClip successClip;
        [SerializeField] AudioSource audioSource;

        [Header("Events")]
        public UnityEvent onPlaced;

        [Header("Manager (optionnel)")]
        [SerializeField] PlacementPuzzleManager manager;

        public bool IsSolved { get; private set; }
        public string AcceptedItemId => acceptedItemId;

        PlaceableItem _candidate;
        bool _resolving;

        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
                Debug.LogWarning($"{name} : le collider du GhostSlot devrait etre en Is Trigger.", this);

            if (ghostRenderers == null || ghostRenderers.Length == 0)
                ghostRenderers = GetComponentsInChildren<Renderer>(true);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (manager == null)
                manager = GetComponentInParent<PlacementPuzzleManager>();
        }

        void OnEnable()
        {
            if (manager != null)
                manager.Register(this);
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsSolved || _resolving)
                return;

            var item = other.GetComponentInParent<PlaceableItem>();
            if (item == null || item.IsPlaced)
                return;

            if (item.ItemId != acceptedItemId)
                return;

            _candidate = item;
        }

        void OnTriggerExit(Collider other)
        {
            var item = other.GetComponentInParent<PlaceableItem>();
            if (item != null && item == _candidate)
                _candidate = null;
        }

        void Update()
        {
            if (IsSolved || _resolving || _candidate == null)
                return;

            if (requireReleased && IsHeld(_candidate))
                return;

            StartCoroutine(Resolve(_candidate));
        }

        static bool IsHeld(PlaceableItem item)
        {
            var grab = item.GetComponentInChildren<XRGrabInteractable>();
            return grab != null && grab.isSelected;
        }

        IEnumerator Resolve(PlaceableItem item)
        {
            _resolving = true;

            if (successClip != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(successClip);
                else
                    AudioSource.PlayClipAtPoint(successClip, transform.position);
            }

            if (optionalAnimator != null && !string.IsNullOrEmpty(animatorTrigger))
                optionalAnimator.SetTrigger(animatorTrigger);

            LockItem(item);

            if (snapPlacedItem && resolveMode != ResolveMode.SwapGhostMaterial)
                SnapItem(item);

            Transform animTarget = ChooseAnimationTarget(item);
            yield return PlayPopAnimation(animTarget);

            ApplyResolution(item);

            IsSolved = true;
            item.MarkPlaced();
            _candidate = null;
            _resolving = false;

            onPlaced?.Invoke();

            if (manager != null)
                manager.NotifySolved(this);
        }

        Transform ChooseAnimationTarget(PlaceableItem item)
        {
            switch (resolveMode)
            {
                case ResolveMode.KeepPlacedItem:
                    return item.transform;
                default:
                    return transform;
            }
        }

        void ApplyResolution(PlaceableItem item)
        {
            switch (resolveMode)
            {
                case ResolveMode.SwapGhostMaterial:
                    ApplySolvedMaterial();
                    SetItemVisible(item, false);
                    break;

                case ResolveMode.KeepPlacedItem:
                    SetGhostVisible(false);
                    break;

                case ResolveMode.HideGhostOnly:
                    SetGhostVisible(false);
                    break;
            }
        }

        void ApplySolvedMaterial()
        {
            if (solvedMaterial == null)
            {
                SetGhostVisible(false);
                return;
            }

            foreach (var r in ghostRenderers)
            {
                if (r != null)
                    r.sharedMaterial = solvedMaterial;
            }
        }

        void SetGhostVisible(bool visible)
        {
            foreach (var r in ghostRenderers)
            {
                if (r != null)
                    r.enabled = visible;
            }
        }

        static void SetItemVisible(PlaceableItem item, bool visible)
        {
            foreach (var r in item.GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;
        }

        void LockItem(PlaceableItem item)
        {
            var grab = item.GetComponentInChildren<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;

            var rb = item.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        void SnapItem(PlaceableItem item)
        {
            Transform target = snapPoint != null ? snapPoint : transform;
            item.transform.SetPositionAndRotation(target.position, target.rotation);
        }

        IEnumerator PlayPopAnimation(Transform target)
        {
            if (target == null || animationDuration <= 0f)
                yield break;

            Vector3 baseScale = target.localScale;
            Quaternion baseRotation = target.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);

                // Pop : le scale monte puis redescend (sin sur une demi-periode).
                float pop = 1f + (popScale - 1f) * Mathf.Sin(t * Mathf.PI);
                target.localScale = baseScale * pop;

                // Petit tour sur l'axe Y pour donner du peps.
                target.localRotation = baseRotation * Quaternion.Euler(0f, 360f * t, 0f);

                yield return null;
            }

            target.localScale = baseScale;
            target.localRotation = baseRotation;
        }

        void OnDrawGizmos()
        {
            var col = GetComponent<Collider>();
            if (col == null)
                return;

            Gizmos.color = IsSolved
                ? new Color(0.2f, 1f, 0.3f, 0.9f)
                : new Color(1f, 0.85f, 0.2f, 0.9f);

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else
            {
                Gizmos.DrawWireSphere(col.bounds.center, col.bounds.extents.magnitude);
            }
        }
    }
}
