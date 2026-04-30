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
}