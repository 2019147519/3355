// Assets/Scripts/Core/InputHandler.cs
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _boardMask;
    [SerializeField] private GameManager _gm;
    [SerializeField] private StoneController _stone;

    private bool _enabled;

    public void SetEnabled(bool on)
    {
        _enabled = on;
        Debug.Log($"[InputHandler] SetEnabled({on})");
    }

    private void Update()
    {
        if (!_enabled) return;

        Vector2? screen = null;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            screen = Input.GetTouch(0).position;
        else if (Input.GetMouseButtonDown(0))
            screen = (Vector2)Input.mousePosition;

        if (screen == null) return;

        Debug.Log($"[InputHandler] 클릭 감지 — screen={screen}");

        var ray = _cam.ScreenPointToRay(screen.Value);

        if (!Physics.Raycast(ray, out var hit, 100f, _boardMask))
        {
            Debug.Log("[InputHandler] Raycast 실패 — 보드 못 맞춤");
            return;
        }

        Debug.Log($"[InputHandler] Raycast 성공 — hit={hit.point}");

        var (row, col) = _stone.WorldToGrid(hit.point);
        Debug.Log($"[InputHandler] 그리드 변환 — row={row}, col={col}");

        if (row < 0 || row >= BoardManager.Size || col < 0 || col >= BoardManager.Size)
        {
            Debug.Log("[InputHandler] 범위 벗어남");
            return;
        }

        _gm.OnBoardTapped(row, col);
    }
}