// Assets/Scripts/Core/InputHandler.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _boardMask;
    [SerializeField] private GameManager _gm;
    [SerializeField] private StoneController _stone;

    private bool _enabled;
    private bool _waitingForPointerRelease;
    private int _enabledFrame = -1;

    public void SetEnabled(bool on)
    {
        if (!on)
        {
            _enabled = false;
            _waitingForPointerRelease = false;
            Debug.Log($"[InputHandler] SetEnabled({on})");
            return;
        }

        _enabled = on;
        _enabledFrame = Time.frameCount;
        _waitingForPointerRelease = HasActivePointer();
        Debug.Log($"[InputHandler] SetEnabled({on})");
    }

    private void Update()
    {
        if (!_enabled) return;
        if (Time.frameCount <= _enabledFrame) return;

        if (_waitingForPointerRelease)
        {
            if (HasActivePointer()) return;
            _waitingForPointerRelease = false;
        }

        Vector2? screen = null;

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended) return;
            if (IsPointerOverUI(touch.fingerId)) return;

            screen = touch.position;
        }
        else if (!Application.isMobilePlatform && Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            screen = (Vector2)Input.mousePosition;
        }

        if (screen == null) return;


        var ray = _cam.ScreenPointToRay(screen.Value);

        if (!Physics.Raycast(ray, out var hit, 100f, _boardMask))
        {
            return;
        }


        var (row, col) = _stone.WorldToGrid(hit.point);

        if (row < 0 || row >= BoardManager.Size || col < 0 || col >= BoardManager.Size)
        {
            return;
        }

        _gm.OnBoardTapped(row, col);
    }

    private static bool HasActivePointer()
    {
        return Input.touchCount > 0 || Input.GetMouseButton(0);
    }

    private static bool IsPointerOverUI(int pointerId = -1)
    {
        if (EventSystem.current == null) return false;

        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
