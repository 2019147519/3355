using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Board Framing")]
    [SerializeField] private Vector3 _defaultPosition = new(0f, 60f, -55f);
    [SerializeField] private Vector3 _defaultRotation = new(50f, 0f, 0f);
    [SerializeField] private float _boardHalfSize = 7.6f;
    [SerializeField] private float _viewPadding = 0f;
    [SerializeField] private float _minAutoFOV = 20f;
    [SerializeField] private float _maxAutoFOV = 48f;

    [Header("Touch Camera Control")]
    [SerializeField] private bool _enableTouchCameraControl = false;

    [Header("Pinch Zoom")]
    [SerializeField] private float _minFOV = 20f;
    [SerializeField] private float _maxFOV = 70f;
    [SerializeField] private float _zoomSpeed = 0.08f;

    [Header("Pan")]
    [SerializeField] private float _panSpeed = 0.015f;
    [SerializeField] private float _panLimit = 5f;

    private Camera _cam;
    private float _prevPinchDist;
    private bool _isPinching;
    private Vector2 _lastPanPos;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        ResetCamera();
    }

    private void Start()
    {
        ResetCamera();
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            ResetCamera();

        if (!ShouldHandleTouchCameraInput())
            return;

        HandlePinchZoom();
        HandlePan();
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        transform.eulerAngles = _defaultRotation;
        _cam.fieldOfView = CalculateFOVForBoard();
        CacheScreenSize();
    }

    private float CalculateFOVForBoard()
    {
        float aspect = _cam.aspect > 0f
            ? _cam.aspect
            : Screen.width / Mathf.Max(1f, (float)Screen.height);
        float paddingScale = Mathf.Clamp01(1f - _viewPadding);
        float requiredTan = 0f;

        for (int ix = -1; ix <= 1; ix += 2)
        {
            for (int iz = -1; iz <= 1; iz += 2)
            {
                var point = new Vector3(ix * _boardHalfSize, 0f, iz * _boardHalfSize);
                var local = transform.InverseTransformPoint(point);
                if (local.z <= 0.01f) continue;

                requiredTan = Mathf.Max(requiredTan, Mathf.Abs(local.y / local.z));
                requiredTan = Mathf.Max(requiredTan, Mathf.Abs(local.x / local.z) / Mathf.Max(0.01f, aspect));
            }
        }

        requiredTan /= Mathf.Max(0.01f, paddingScale);
        float fov = Mathf.Atan(requiredTan) * Mathf.Rad2Deg * 2f;
        return Mathf.Clamp(fov, _minAutoFOV, _maxAutoFOV);
    }

    private void CacheScreenSize()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    private bool ShouldHandleTouchCameraInput()
    {
        return _enableTouchCameraControl && !Application.isMobilePlatform;
    }

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
        next.z = Mathf.Clamp(next.z, _defaultPosition.z - _panLimit, _defaultPosition.z + _panLimit);
        transform.position = next;
    }
}
