using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerZoneUIOverlay : MonoBehaviour
{
    static TriggerZoneUIOverlay instance;

    [SerializeField] TriggerZoneUIPrefabs prefabsConfig;
    [SerializeField] GameObject timerPanelPrefab;
    [SerializeField] GameObject messagePanelPrefab;

    Canvas canvas;
    TextMeshProUGUI timerText;
    GameObject timerPanel;
    TextMeshProUGUI messageText;
    GameObject messagePanel;
    Coroutine timerCoroutine;
    Coroutine messageCoroutine;
    bool uiBuilt;

    public static TriggerZoneUIOverlay Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindFirstObjectByType<TriggerZoneUIOverlay>();
            if (instance != null)
                return instance;

            var overlayObject = new GameObject("TriggerZoneUIOverlay");
            instance = overlayObject.AddComponent<TriggerZoneUIOverlay>();
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ResolvePrefabReferences();
        BuildUI();
    }

    void ResolvePrefabReferences()
    {
        if (prefabsConfig == null)
            prefabsConfig = Resources.Load<TriggerZoneUIPrefabs>("TriggerZoneUIPrefabs");

        if (prefabsConfig == null)
            return;

        if (timerPanelPrefab == null)
            timerPanelPrefab = prefabsConfig.timerPanelPrefab;

        if (messagePanelPrefab == null)
            messagePanelPrefab = prefabsConfig.messagePanelPrefab;
    }

    void BuildUI()
    {
        if (uiBuilt)
            return;

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        if (timerPanelPrefab != null)
        {
            timerPanel = Instantiate(timerPanelPrefab, canvas.transform);
            timerPanel.name = timerPanelPrefab.name;
            timerText = timerPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            timerPanel.SetActive(false);
        }

        if (messagePanelPrefab != null)
        {
            messagePanel = Instantiate(messagePanelPrefab, canvas.transform);
            messagePanel.name = messagePanelPrefab.name;
            messageText = messagePanel.GetComponentInChildren<TextMeshProUGUI>(true);
            messagePanel.SetActive(false);
        }

        uiBuilt = true;
    }

    public void ShowTimer(float duration, string label)
    {
        if (timerPanel == null || timerText == null)
        {
            Debug.LogWarning("TriggerZoneUIOverlay : prefab TimerPanel manquant ou invalide.", this);
            return;
        }

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerPanel.SetActive(true);
        timerCoroutine = StartCoroutine(TimerRoutine(duration, label));
    }

    public void ShowMessage(string message, float duration)
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogWarning("TriggerZoneUIOverlay : prefab ZonePanel manquant ou invalide.", this);
            return;
        }

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageText.text = message;
        messagePanel.SetActive(true);
        messageCoroutine = StartCoroutine(MessageRoutine(duration));
    }

    IEnumerator TimerRoutine(float duration, string label)
    {
        float remaining = Mathf.Max(0f, duration);

        while (remaining > 0f)
        {
            timerText.text = string.IsNullOrWhiteSpace(label)
                ? remaining.ToString("0.0")
                : $"{label} : {remaining:0.0}";

            yield return null;
            remaining -= Time.deltaTime;
        }

        timerText.text = string.IsNullOrWhiteSpace(label)
            ? "0.0"
            : $"{label} : 0.0";

        timerPanel.SetActive(false);
        timerCoroutine = null;
    }

    IEnumerator MessageRoutine(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, duration));
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }
}
