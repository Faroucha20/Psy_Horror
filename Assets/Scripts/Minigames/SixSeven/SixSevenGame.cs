using System;
using UnityEngine;

/// <summary>
/// Détecte les mouvements Six-Seven (alternance haut/bas des mains, paumes vers le ciel) et gère le chrono.
/// </summary>
public class SixSevenGame : MonoBehaviour
{
    public enum Phase { Idle, Playing, Finished }

    [Header("Timing")]
    [SerializeField] private float durationSeconds = 30f;
    [SerializeField] private float minSwapInterval = 0.18f;

    [Header("Detection")]
    [SerializeField] private float minHeightDifference = 0.12f;
    [SerializeField] private float minVerticalTravel = 0.08f;
    [SerializeField] private float palmUpDotThreshold = 0.35f;

    Transform _leftHand;
    Transform _rightHand;

    Phase _phase = Phase.Idle;
    float _timeLeft;
    int _score;
    float _lastCountTime;

    enum HandLayout { Unknown, LeftHigher, RightHigher }
    HandLayout _currentLayout = HandLayout.Unknown;
    HandLayout _previousLayout = HandLayout.Unknown;

    float _leftBaselineY;
    float _rightBaselineY;
    bool _hasBaseline;

    // Fallback éditeur : simule l'alternance avec deux touches
    bool _editorSimLeftUp = true;

    public Phase CurrentPhase => _phase;
    public int Score => _score;
    public float TimeLeft => _timeLeft;

    public event Action<int, float> ScoreUpdated;
    public event Action<int> GameFinished;

    public void BindHands(Transform leftHand, Transform rightHand)
    {
        _leftHand = leftHand;
        _rightHand = rightHand;
    }

    public void StartGame()
    {
        _phase = Phase.Playing;
        _timeLeft = durationSeconds;
        _score = 0;
        _lastCountTime = -minSwapInterval;
        _currentLayout = HandLayout.Unknown;
        _previousLayout = HandLayout.Unknown;
        _hasBaseline = false;
        ScoreUpdated?.Invoke(_score, _timeLeft);
    }

    public void ResetGame()
    {
        _phase = Phase.Idle;
        _timeLeft = durationSeconds;
        _score = 0;
        _currentLayout = HandLayout.Unknown;
        _previousLayout = HandLayout.Unknown;
        _hasBaseline = false;
    }

    void Update()
    {
        if (_phase != Phase.Playing)
            return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            FinishGame();
            return;
        }

        if (TryCountSixSeven())
            ScoreUpdated?.Invoke(_score, _timeLeft);
    }

    void FinishGame()
    {
        _phase = Phase.Finished;
        GameFinished?.Invoke(_score);
    }

    bool TryCountSixSeven()
    {
        if (Time.time - _lastCountTime < minSwapInterval)
            return false;

        if (!TryGetHandHeights(out float leftY, out float rightY, out bool leftPalmUp, out bool rightPalmUp))
            return false;

        if (!leftPalmUp || !rightPalmUp)
            return false;

        var layout = GetLayout(leftY, rightY);
        if (layout == HandLayout.Unknown)
            return false;

        if (!_hasBaseline)
        {
            _leftBaselineY = leftY;
            _rightBaselineY = rightY;
            _hasBaseline = true;
            _previousLayout = layout;
            _currentLayout = layout;
            return false;
        }

        if (Mathf.Abs(leftY - _leftBaselineY) < minVerticalTravel &&
            Mathf.Abs(rightY - _rightBaselineY) < minVerticalTravel)
            return false;

        _currentLayout = layout;

        if (_previousLayout != HandLayout.Unknown &&
            _currentLayout != HandLayout.Unknown &&
            _currentLayout != _previousLayout)
        {
            _score++;
            _lastCountTime = Time.time;
            _previousLayout = _currentLayout;
            _leftBaselineY = leftY;
            _rightBaselineY = rightY;
            return true;
        }

        _previousLayout = _currentLayout;
        return false;
    }

    HandLayout GetLayout(float leftY, float rightY)
    {
        var diff = leftY - rightY;
        if (diff > minHeightDifference)
            return HandLayout.LeftHigher;
        if (diff < -minHeightDifference)
            return HandLayout.RightHigher;
        return HandLayout.Unknown;
    }

    bool TryGetHandHeights(out float leftY, out float rightY, out bool leftPalmUp, out bool rightPalmUp)
    {
        leftY = rightY = 0f;
        leftPalmUp = rightPalmUp = false;

        if (_leftHand != null && _rightHand != null)
        {
            leftY = _leftHand.position.y;
            rightY = _rightHand.position.y;
            leftPalmUp = IsPalmUp(_leftHand);
            rightPalmUp = IsPalmUp(_rightHand);
            return true;
        }

        if (TryOvrControllerFallback(out leftY, out rightY, out leftPalmUp, out rightPalmUp))
            return true;

#if UNITY_EDITOR
        return TryEditorSimulation(out leftY, out rightY, out leftPalmUp, out rightPalmUp);
#else
        return false;
#endif
    }

    bool TryOvrControllerFallback(out float leftY, out float rightY, out bool leftPalmUp, out bool rightPalmUp)
    {
        leftY = rightY = 0f;
        leftPalmUp = rightPalmUp = false;

        var trackingSpace = FindTrackingSpace();
        if (trackingSpace == null)
            return false;

        var leftLocal = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        var rightLocal = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
        var leftRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand);
        var rightRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand);

        leftY = trackingSpace.TransformPoint(leftLocal).y;
        rightY = trackingSpace.TransformPoint(rightLocal).y;
        leftPalmUp = Vector3.Dot(trackingSpace.TransformDirection(leftRot * Vector3.up), Vector3.up) > palmUpDotThreshold;
        rightPalmUp = Vector3.Dot(trackingSpace.TransformDirection(rightRot * Vector3.up), Vector3.up) > palmUpDotThreshold;
        return true;
    }

#if UNITY_EDITOR
    bool TryEditorSimulation(out float leftY, out float rightY, out bool leftPalmUp, out bool rightPalmUp)
    {
        leftPalmUp = rightPalmUp = true;
        var baseHeight = Camera.main != null ? Camera.main.transform.position.y : 1.5f;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            _editorSimLeftUp = true;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            _editorSimLeftUp = false;

        if (_editorSimLeftUp)
        {
            leftY = baseHeight + 0.35f;
            rightY = baseHeight - 0.35f;
        }
        else
        {
            leftY = baseHeight - 0.35f;
            rightY = baseHeight + 0.35f;
        }

        return true;
    }
#endif

    static bool IsPalmUp(Transform hand)
    {
        // Paume vers le ciel : la normale "up" locale du contrôleur/main pointe vers le haut.
        return Vector3.Dot(hand.up, Vector3.up) > 0.35f;
    }

    static Transform FindTrackingSpace()
    {
        var rig = GameObject.Find("OVRCameraRig");
        if (rig == null)
            return null;

        var tracking = rig.transform.Find("TrackingSpace");
        return tracking != null ? tracking : rig.transform;
    }
}
