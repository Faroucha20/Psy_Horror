using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panneau world-space devant le joueur pour le dialogue et le score Six-Seven.
/// </summary>
public class SixSevenDialogueUI : MonoBehaviour
{
    const float PanelDistance = 1.4f;

    Canvas _canvas;
    TextMeshProUGUI _titleText;
    TextMeshProUGUI _bodyText;
    TextMeshProUGUI _hintText;

    public bool IsVisible => _canvas != null && _canvas.gameObject.activeSelf;

    public void Build()
    {
        if (_canvas != null)
            return;

        var root = new GameObject("SixSevenDialoguePanel");
        root.transform.SetParent(transform, false);

        _canvas = root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        root.AddComponent<GraphicRaycaster>();

        var canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(900f, 520f);
        canvasRect.localScale = Vector3.one * 0.001f;

        var bg = CreateImage(root.transform, "Background", new Color(0.05f, 0.05f, 0.08f, 0.92f));
        Stretch(bg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        _titleText = CreateText(root.transform, "Title", 42, FontStyles.Bold, TextAlignmentOptions.Center);
        SetRect(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(820f, 70f));

        _bodyText = CreateText(root.transform, "Body", 30, FontStyles.Normal, TextAlignmentOptions.Center);
        SetRect(_bodyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(820f, 280f));

        _hintText = CreateText(root.transform, "Hint", 24, FontStyles.Italic, TextAlignmentOptions.Center);
        _hintText.color = new Color(0.75f, 0.85f, 1f, 1f);
        SetRect(_hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(820f, 80f));

        Hide();
    }

    public void ShowDialogue(string title, string body, string hint)
    {
        EnsureBuilt();
        PlaceInFrontOfPlayer();
        _canvas.gameObject.SetActive(true);
        _titleText.text = title;
        _bodyText.text = body;
        _hintText.text = hint;
    }

    public void ShowGameplay(string title, string body, string hint)
    {
        ShowDialogue(title, body, hint);
    }

    public void Hide()
    {
        if (_canvas != null)
            _canvas.gameObject.SetActive(false);
    }

    public void PlaceInFrontOfPlayer()
    {
        if (_canvas == null)
            return;

        var cam = Camera.main;
        if (cam == null)
            return;

        var forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = cam.transform.forward;
        forward.Normalize();

        _canvas.transform.position = cam.transform.position + cam.transform.forward * PanelDistance;
        _canvas.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    void EnsureBuilt()
    {
        if (_canvas == null)
            Build();
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, float size, FontStyles style, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = align;
        text.color = Color.white;
        text.enableWordWrapping = true;
        return text;
    }

    static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
