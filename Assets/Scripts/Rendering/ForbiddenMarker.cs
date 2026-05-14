// Assets/Scripts/Rendering/ForbiddenMarker.cs
using System.Collections.Generic;
using UnityEngine;

public class ForbiddenMarker : MonoBehaviour
{
    [SerializeField] private Material _markerMat;
    [SerializeField] private StoneController _stone;

    private readonly List<GameObject> _markers = new();

    private void OnEnable()
    {
        var board = GetComponent<BoardManager>();
        if (board) board.OnForbiddenMove += ShowMarker;
    }

    private void OnDisable()
    {
        var board = GetComponent<BoardManager>();
        if (board) board.OnForbiddenMove -= ShowMarker;
    }

    private void ShowMarker(int row, int col, ForbiddenType type)
    {
        ClearMarkers();

        // X 표시 — 두 개의 얇은 Cube를 45도로 교차
        var pos = _stone.GridToWorld(row, col) + Vector3.up * 0.15f;

        CreateBar(pos, 45f);
        CreateBar(pos, -45f);
    }

    private void CreateBar(Vector3 pos, float angle)
    {
        var go = new GameObject("ForbiddenMarkerBar");
        var line = go.AddComponent<LineRenderer>();

        if (_markerMat != null)
            line.material = _markerMat;

        line.positionCount = 2;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
        float halfLength = 0.3f;
        line.SetPosition(0, pos - direction * halfLength);
        line.SetPosition(1, pos + direction * halfLength);

        _markers.Add(go);

        // 1.5초 후 자동 제거
        Destroy(go, 1.5f);
    }

    public void ClearMarkers()
    {
        foreach (var m in _markers) if (m) Destroy(m);
        _markers.Clear();
    }
}
