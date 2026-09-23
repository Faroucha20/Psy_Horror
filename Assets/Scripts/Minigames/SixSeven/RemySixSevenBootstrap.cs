using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Installe automatiquement la zone Six-Seven devant Remy (visible dans l'éditeur ET en Play).
/// Aucune modification manuelle des assets existants requise.
/// </summary>
[ExecuteAlways]
public class RemySixSevenBootstrap : MonoBehaviour
{
    const string RemyObjectName = "Remy";
    const string SystemObjectName = "RemySixSevenSystem";
    const string ZoneObjectName = "RemySixSevenZone";

    [Header("Zone devant Remy")]
    [SerializeField] private Vector3 zoneLocalPosition = new Vector3(0f, 1.1f, 1.4f);
    [SerializeField] private Vector3 zoneSize = new Vector3(2.2f, 2.4f, 2f);

    RemySixSevenZone _zone;
    SixSevenDialogueUI _ui;
    SixSevenGame _game;

    bool _dialogueVisible;
    bool _awaitingChoice;
    bool _gameRunning;
    bool _gameplayReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void PlayModeAutoInstall() => TryAutoInstall();

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void EditorAutoInstall()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying)
                TryAutoInstall();
        };

        EditorSceneManager.sceneOpened += (_, __) =>
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                    TryAutoInstall();
            };
        };
    }
#endif

    static void TryAutoInstall()
    {
        if (GameObject.Find(RemyObjectName) == null)
            return;

        if (FindFirstObjectByType<RemySixSevenBootstrap>() != null)
            return;

        var system = new GameObject(SystemObjectName);
        system.AddComponent<RemySixSevenBootstrap>();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(system, "Create Remy Six-Seven System");
            EditorSceneManager.MarkSceneDirty(system.scene);
        }
#endif
    }

    void OnEnable()
    {
        EnsureZoneExists();

        if (Application.isPlaying)
            EnsureGameplayReady();
    }

    void OnValidate()
    {
        if (_zone == null)
            _zone = FindZoneUnderRemy();

        if (_zone != null)
            ApplyZoneSettings(_zone.gameObject);
    }

    void Start()
    {
        if (!Application.isPlaying)
            return;

        EnsureGameplayReady();
    }

    void OnDestroy()
    {
        if (_zone != null)
        {
            _zone.PlayerEntered -= OnPlayerEntered;
            _zone.PlayerExited -= OnPlayerExited;
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (_dialogueVisible && (_awaitingChoice || _gameRunning))
            _ui.PlaceInFrontOfPlayer();

        if (!_dialogueVisible || !_awaitingChoice)
            return;

        if (OVRInput.GetDown(OVRInput.Button.One) ||
            OVRInput.GetDown(OVRInput.Button.Three) ||
            Input.GetKeyDown(KeyCode.Y))
        {
            AcceptChallenge();
        }
        else if (OVRInput.GetDown(OVRInput.Button.Two) ||
                 OVRInput.GetDown(OVRInput.Button.Four) ||
                 Input.GetKeyDown(KeyCode.N))
        {
            DeclineChallenge();
        }
    }

    void EnsureZoneExists()
    {
        var remy = GameObject.Find(RemyObjectName);
        if (remy == null)
            return;

        _zone = FindZoneUnderRemy();
        if (_zone == null)
        {
            var zoneGo = new GameObject(ZoneObjectName);
            zoneGo.transform.SetParent(remy.transform, false);
            ApplyZoneSettings(zoneGo);
            _zone = zoneGo.AddComponent<RemySixSevenZone>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(zoneGo, "Create Remy Six-Seven Zone");
                EditorSceneManager.MarkSceneDirty(zoneGo.scene);
            }
#endif
        }
        else
        {
            ApplyZoneSettings(_zone.gameObject);
        }
    }

    void ApplyZoneSettings(GameObject zoneGo)
    {
        zoneGo.transform.localPosition = zoneLocalPosition;
        zoneGo.transform.localRotation = Quaternion.identity;

        var box = zoneGo.GetComponent<BoxCollider>();
        if (box == null)
            box = zoneGo.AddComponent<BoxCollider>();

        box.isTrigger = true;
        box.size = zoneSize;
        box.center = Vector3.zero;
    }

    RemySixSevenZone FindZoneUnderRemy()
    {
        var remy = GameObject.Find(RemyObjectName);
        if (remy == null)
            return null;

        var zoneTransform = remy.transform.Find(ZoneObjectName);
        return zoneTransform != null ? zoneTransform.GetComponent<RemySixSevenZone>() : null;
    }

    void EnsureGameplayReady()
    {
        if (_gameplayReady)
            return;

        var remy = GameObject.Find(RemyObjectName);
        if (remy == null)
        {
            Debug.LogWarning("[SixSeven] Objet 'Remy' introuvable — système désactivé.");
            enabled = false;
            return;
        }

        EnsureZoneExists();
        PlayerTriggerProxy.EnsureOnCamera();
        CreateUiAndGame();

        _zone.PlayerEntered += OnPlayerEntered;
        _zone.PlayerExited += OnPlayerExited;
        _gameplayReady = true;
    }

    void CreateUiAndGame()
    {
        if (_ui == null)
        {
            var uiGo = new GameObject("SixSevenUI");
            uiGo.transform.SetParent(transform, false);
            _ui = uiGo.AddComponent<SixSevenDialogueUI>();
            _ui.Build();
        }

        if (_game == null)
        {
            var gameGo = new GameObject("SixSevenGame");
            gameGo.transform.SetParent(transform, false);
            _game = gameGo.AddComponent<SixSevenGame>();

            var hands = FindHandAnchors();
            _game.BindHands(hands.left, hands.right);

            _game.ScoreUpdated += OnScoreUpdated;
            _game.GameFinished += OnGameFinished;
        }
    }

    (Transform left, Transform right) FindHandAnchors()
    {
        Transform left = null;
        Transform right = null;

        var rig = GameObject.Find("OVRCameraRig");
        if (rig == null)
            return (null, null);

        var all = rig.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            var n = t.name;
            if (left == null && (n == "LeftHandOnControllerAnchor" || n == "LeftHandAnchor" || n == "LeftControllerAnchor"))
                left = t;
            if (right == null && (n == "RightHandOnControllerAnchor" || n == "RightHandAnchor" || n == "RightControllerAnchor"))
                right = t;
        }

        return (left, right);
    }

    void OnPlayerEntered()
    {
        if (_gameRunning)
            return;

        ShowInviteDialogue();
    }

    void OnPlayerExited()
    {
        if (_gameRunning)
            return;

        HideDialogue();
    }

    void ShowInviteDialogue()
    {
        _dialogueVisible = true;
        _awaitingChoice = true;
        _ui.ShowDialogue(
            "Remy — Défi Six-Seven",
            "Yo ! Fais un max de Six-Seven en 30 secondes.\n" +
            "Alterne tes mains : une en haut, une en bas, paumes vers le ciel.\n" +
            "Chaque swap compte !",
            "A / X / Y = Jouer   |   B / N = Refuser"
        );
    }

    void AcceptChallenge()
    {
        _awaitingChoice = false;
        _gameRunning = true;
        _game.StartGame();
        _ui.ShowGameplay(
            "Six-Seven — GO !",
            "Score : 0\nTemps : 30 s",
            "Alterne haut/bas avec les paumes vers le ciel !"
        );
    }

    void DeclineChallenge()
    {
        _awaitingChoice = false;
        _ui.ShowDialogue(
            "Remy",
            "Pas de souci, reviens quand tu veux tenter le défi !",
            "Quitte la zone pour fermer."
        );
    }

    void OnScoreUpdated(int score, float timeLeft)
    {
        _ui.ShowGameplay(
            "Six-Seven — GO !",
            $"Score : {score}\nTemps : {Mathf.CeilToInt(timeLeft)} s",
            "Alterne haut/bas avec les paumes vers le ciel !"
        );
    }

    void OnGameFinished(int finalScore)
    {
        _gameRunning = false;
        _awaitingChoice = false;

        string message = finalScore switch
        {
            0 => "Aïe... zéro Six-Seven. Remy te regarde dubitativement.",
            < 10 => "Pas mal pour débuter ! Remy hoche la tête.",
            < 25 => "Solide ! Remy est impressionné.",
            _ => "INCROYABLE ! Remy te déclare roi du Six-Seven !"
        };

        _ui.ShowDialogue(
            "Temps écoulé !",
            $"Tu as enchaîné {finalScore} Six-Seven.\n{message}",
            "Quitte la zone ou ré-entre pour rejouer."
        );

        _game.ResetGame();
    }

    void HideDialogue()
    {
        _dialogueVisible = false;
        _awaitingChoice = false;
        _ui.Hide();
    }
}
