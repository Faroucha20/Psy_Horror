using System;
using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Singleton that continuously monitors the player's position and fires events
/// when they enter or exit an MRUK room.
/// </summary>
public class ActiveRoomDetector : MonoBehaviour
{
    public static ActiveRoomDetector Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private Transform playerHead;
    [SerializeField, Range(0.1f, 2f)] private float checkInterval = 0.5f;

    /// <summary>Fired when the player enters a new MRUK room. Arg: the entered room.</summary>
    public event Action<MRUKRoom> OnRoomEntered;

    /// <summary>Fired when the player leaves their current MRUK room. Arg: the exited room.</summary>
    public event Action<MRUKRoom> OnRoomExited;

    private MRUKRoom _currentRoom;
    private float _timer;
    private bool _sceneLoaded;

    /// <summary>The MRUK room the player is currently inside, or null if outside any room.</summary>
    public MRUKRoom CurrentRoom => _currentRoom;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (playerHead == null)
            playerHead = Camera.main.transform;

        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        MRUK.Instance.LoadSceneFromDevice();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded()
    {
        _sceneLoaded = true;
        Debug.Log($">>> ActiveRoomDetector : {MRUK.Instance.Rooms.Count} pièce(s) chargée(s)");
        CheckCurrentRoom();
    }

    void Update()
    {
        if (!_sceneLoaded) return;

        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        CheckCurrentRoom();
    }

    private void CheckCurrentRoom()
    {
        MRUKRoom detected = null;

        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            if (room.IsPositionInRoom(playerHead.position))
            {
                detected = room;
                break;
            }
        }

        if (detected == _currentRoom) return;

        if (_currentRoom != null)
        {
            Debug.Log($">>> Sortie de salle : {_currentRoom.name}");
            OnRoomExited?.Invoke(_currentRoom);
        }

        _currentRoom = detected;

        if (_currentRoom != null)
        {
            Debug.Log($">>> Entrée dans salle : {_currentRoom.name}");
            OnRoomEntered?.Invoke(_currentRoom);
        }
    }
}
