using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject roomSelectionUI;

    void Update()
    {
        // Ouvre/ferme le menu avec le bouton Menu (≡) du contrôleur gauche
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            bool isActive = roomSelectionUI.activeSelf;
            roomSelectionUI.SetActive(!isActive);

            // Place le menu devant l'utilisateur
            if (!isActive)
            {
                roomSelectionUI.transform.position =
                    Camera.main.transform.position +
                    Camera.main.transform.forward * 0.5f;

                roomSelectionUI.transform.rotation =
                    Quaternion.LookRotation(Camera.main.transform.forward);
            }
        }
    }
}
