using UnityEngine;

/// <summary>
/// Ajoute un collider trigger sur la tête du joueur pour la détection de zone (VR).
/// Créé automatiquement par RemySixSevenBootstrap — ne pas placer manuellement sur Remy.
/// </summary>
[DisallowMultipleComponent]
public class PlayerTriggerProxy : MonoBehaviour
{
    [SerializeField] private float radius = 0.35f;

    public static PlayerTriggerProxy EnsureOnCamera()
    {
        var existing = FindFirstObjectByType<PlayerTriggerProxy>();
        if (existing != null)
            return existing;

        Transform head = null;
        if (Camera.main != null)
            head = Camera.main.transform;

        if (head == null)
        {
            var rig = GameObject.Find("OVRCameraRig");
            if (rig != null)
            {
                var centerEye = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
                head = centerEye != null ? centerEye : rig.transform;
            }
        }

        if (head == null)
            return null;

        var proxyGo = new GameObject("SixSevenPlayerTrigger");
        proxyGo.transform.SetParent(head, false);
        var sphere = proxyGo.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 0.35f;

        var rb = proxyGo.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var proxy = proxyGo.AddComponent<PlayerTriggerProxy>();
        proxy.radius = 0.35f;
        return proxy;
    }

    void OnValidate()
    {
        var sphere = GetComponent<SphereCollider>();
        if (sphere != null)
            sphere.radius = radius;
    }
}
