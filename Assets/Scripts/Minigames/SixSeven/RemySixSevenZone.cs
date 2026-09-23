using System;
using UnityEngine;

/// <summary>
/// Zone de détection devant Remy. Déclenche le dialogue Six-Seven quand le joueur entre.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RemySixSevenZone : MonoBehaviour
{
    public event Action PlayerEntered;
    public event Action PlayerExited;

    BoxCollider _box;
    bool _playerInside;

    public bool PlayerInside => _playerInside;

    void Awake()
    {
        _box = GetComponent<BoxCollider>();
        _box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerInside = true;
        PlayerEntered?.Invoke();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerInside = false;
        PlayerExited?.Invoke();
    }

    static bool IsPlayer(Collider other)
    {
        return other.GetComponent<PlayerTriggerProxy>() != null ||
               other.GetComponentInParent<PlayerTriggerProxy>() != null;
    }

    void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(0.1f, 1f, 0.45f, 0.18f);
        Gizmos.DrawCube(col.center, col.size);

        Gizmos.color = new Color(0.1f, 1f, 0.45f, 0.95f);
        Gizmos.DrawWireCube(col.center, col.size);
    }
}
