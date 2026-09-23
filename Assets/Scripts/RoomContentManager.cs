using UnityEngine;

public class RoomContentManager : MonoBehaviour
{
    [Header("Dépendances")]
    [Tooltip("Glissez ici le GameObject qui contient le SpatialAnchorManager")]
    [SerializeField] private SpatialAnchorManager anchorManager;

    [Header("Préfabriqués 3D")]
    [SerializeField] private GameObject ClassePrefab;
    [SerializeField] private GameObject BureauPrefab;
    [SerializeField] private GameObject CantinePrefab;
    [SerializeField] private GameObject DMRPrefab;
    [SerializeField] private GameObject AdaPrefab;
    [SerializeField] private GameObject PruhaPrefab;
    [SerializeField] private GameObject GwenPrefab;
    [SerializeField] private GameObject FoyerPrefab;

    // Ajoutez d'autres préfabriqués pour le Foyer, le Bureau, etc.

    // OnEnable est appelé quand le script est activé
    void OnEnable()
    {
        // On s'abonne à l'événement (on branche notre écouteur)
        if (anchorManager != null)
        {
            anchorManager.OnAnchorReady += SpawnRoomContent;
        }
    }

    // OnDisable est appelé quand le script est désactivé ou détruit
    void OnDisable()
    {
        // On se désabonne TOUJOURS pour éviter les erreurs (fuites de mémoire)
        if (anchorManager != null)
        {
            anchorManager.OnAnchorReady -= SpawnRoomContent;
        }
    }

    /// <summary>
    /// Cette fonction est déclenchée automatiquement par le SpatialAnchorManager.
    /// </summary>
    private void SpawnRoomContent(SpatialAnchorManager.RoomType roomType, Vector3 anchorPosition)
    {
        Debug.Log($"[RoomContentManager] Apparition du contenu pour la salle : {roomType}");

        // Selon le type de salle, on fait apparaître des objets différents
        switch (roomType)
        {
            case SpatialAnchorManager.RoomType.Classe:
                SpawnClasse(anchorPosition);
                break;

            case SpatialAnchorManager.RoomType.Cantine:
                SpawnCantine(anchorPosition);
                break;
                
            case SpatialAnchorManager.RoomType.Bureau:
                SpawnBureau(anchorPosition);
                break;

            case SpatialAnchorManager.RoomType.DMR:
                SpawnDMR(anchorPosition);
                break;
            case SpatialAnchorManager.RoomType.Ada:
                SpawnAda(anchorPosition);
                break;
            case SpatialAnchorManager.RoomType.Pruha:
                SpawnPruha(anchorPosition);
                break;
            case SpatialAnchorManager.RoomType.Gwen:
                SpawnGwen(anchorPosition);
                break;
            case SpatialAnchorManager.RoomType.Foyer:
                SpawnFoyer(anchorPosition);
                break;
        }
    }

    // --- Fonctions de placement ---

    private void SpawnClasse(Vector3 center)
    {
        Instantiate(ClassePrefab, center, Quaternion.identity);
    }

    private void SpawnCantine(Vector3 center)
    {
        // Exemple : On place une table exactement sur l'ancre
        Instantiate(CantinePrefab, center, Quaternion.identity);
    }

    private void SpawnDMR(Vector3 center)
    {
        // Exemple : On place le prefab DMR exactement sur l'ancre
        GameObject dmr = Instantiate(DMRPrefab, center, Quaternion.identity);

        // Ajout d'un BoxCollider configuré en Trigger pour détecter le joueur
        BoxCollider box = dmr.AddComponent<BoxCollider>();
        box.isTrigger = true;

        // Configuration du centre et de la taille du collider de la pièce DMR
        box.center = new Vector3(0f, 1f, 0f);
        box.size = new Vector3(6f, 3f, 6f);

        // Ajout du script PieceVisible (targetPrefab à null pour gérer dynamiquement tous les enfants)
        dmr.AddComponent<PieceVisible>();
    }

    private void SpawnAda(Vector3 center)
    {
        Instantiate(AdaPrefab, center, Quaternion.identity);
    }
    private void SpawnFoyer(Vector3 center)
    {
        Instantiate(FoyerPrefab, center, Quaternion.identity);
    }
    private void SpawnPruha(Vector3 center)
    {
        Instantiate(PruhaPrefab, center, Quaternion.identity);
    }
    private void SpawnGwen(Vector3 center)
    {        
        Instantiate(GwenPrefab, center, Quaternion.identity);
    }
    private void SpawnBureau(Vector3 center)
    {
        Instantiate(BureauPrefab, center, Quaternion.identity);
    }
}