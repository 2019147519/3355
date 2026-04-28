// Assets/Scripts/Rendering/CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Position Presets")]
    [SerializeField] private Vector3 _defaultPosition = new(0f, 14f, -6f);
    [SerializeField] private Vector3 _defaultRotation = new(62f, 0f, 0f);

    [Header("Pinch Zoom")]
    [SerializeField] private float _minFOV = 30f;
    [SerializeField] private float _maxFOV = 70f;
    [SerializeField] private float _zoomSpeed = 0.08f;

    [Header("Pan")]
    [SerializeField] private float _panSpeed = 0.015f;
    [SerializeField] private float _panLimit = 5f;

    private Camera _cam;
    private float _prevPinchDist;
    private bool _isPinching;
    private Vector2 _lastPanPos;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        ResetCamera();
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        transform.eulerAngles = _defaultRotation;
        _cam.fieldOfView = 20f;
    }

    private void Update()
    {
        HandlePinchZoom();
        HandlePan();
    }

    // 式式 б纂 邀 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    private void HandlePinchZoom()
    {
        if (Input.touchCount != 2) { _isPinching = false; return; }

        var t0 = Input.GetTouch(0);
        var t1 = Input.GetTouch(1);
        float dist = Vector2.Distance(t0.position, t1.position);

        if (!_isPinching) { _prevPinchDist = dist; _isPinching = true; return; }

        float delta = _prevPinchDist - dist;
        _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView + delta * _zoomSpeed,
                                        _minFOV, _maxFOV);
        _prevPinchDist = dist;
    }

    // 式式 и 槳陛塊 の 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    private void HandlePan()
    {
        if (Input.touchCount != 1) return;

        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            _lastPanPos = touch.position;
            return;
        }

        if (touch.phase != TouchPhase.Moved) return;

        Vector2 delta = touch.position - _lastPanPos;
        _lastPanPos = touch.position;

        var move = new Vector3(-delta.x * _panSpeed, 0f, -delta.y * _panSpeed);
        var next = transform.position + move;

        next.x = Mathf.Clamp(next.x, -_panLimit, _panLimit);
        next.z = Mathf.Clamp(next.z, -_panLimit, _panLimit);
        transform.position = next;
    }
}