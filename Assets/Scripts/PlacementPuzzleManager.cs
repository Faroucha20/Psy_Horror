using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Minigames.Placement
{
    /// <summary>Event (nb places, nb total) affichable dans l'Inspector.</summary>
    [Serializable]
    public class PlacementProgressEvent : UnityEvent<int, int> { }

    /// <summary>
    /// Suit l'ensemble des <see cref="GhostSlot"/> du mini-jeu "ranger les objets au bon endroit".
    /// Declenche un event quand tous les emplacements ont recu le bon objet.
    /// A placer sur un parent commun des slots (ex: le prefab DMR) ou a assigner a la main.
    /// </summary>
    public class PlacementPuzzleManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Rempli automatiquement au demarrage avec les GhostSlot enfants si laisse vide.")]
        List<GhostSlot> slots = new List<GhostSlot>();

        [Header("Events")]
        [Tooltip("Appele a chaque objet correctement place. (nb places, nb total)")]
        public PlacementProgressEvent onProgress;

        [Tooltip("Appele une fois que tous les objets sont ranges.")]
        public UnityEvent onCompleted;

        public int SolvedCount { get; private set; }
        public int TotalCount => slots.Count;
        public bool IsCompleted => TotalCount > 0 && SolvedCount >= TotalCount;

        void Awake()
        {
            if (slots.Count == 0)
                slots.AddRange(GetComponentsInChildren<GhostSlot>(true));
        }

        public void Register(GhostSlot slot)
        {
            if (slot != null && !slots.Contains(slot))
                slots.Add(slot);
        }

        public void NotifySolved(GhostSlot slot)
        {
            RecountSolved();
            onProgress?.Invoke(SolvedCount, TotalCount);

            if (IsCompleted)
                onCompleted?.Invoke();
        }

        void RecountSolved()
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot != null && slot.IsSolved)
                    count++;
            }
            SolvedCount = count;
        }
    }
}
