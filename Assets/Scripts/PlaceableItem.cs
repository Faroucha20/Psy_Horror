using UnityEngine;

namespace Minigames.Placement
{
    /// <summary>
    /// A placer sur chaque objet "reel" (non ghost) que le joueur doit ranger,
    /// par exemple le vrai_croissant. L'identifiant permet a une <see cref="GhostSlot"/>
    /// de savoir si l'objet depose est le bon.
    /// </summary>
    public class PlaceableItem : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Identifiant de l'objet (ex: croissant). Doit correspondre a l'Accepted Item Id d'une GhostSlot.")]
        string itemId = "croissant";

        public string ItemId => itemId;

        /// <summary>
        /// True une fois l'objet correctement place dans sa zone fantome.
        /// Evite qu'il soit compte plusieurs fois par plusieurs slots.
        /// </summary>
        public bool IsPlaced { get; private set; }

        public void MarkPlaced()
        {
            IsPlaced = true;
        }
    }
}
