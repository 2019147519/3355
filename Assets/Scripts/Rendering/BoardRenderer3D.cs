// Assets/Scripts/Rendering/BoardRenderer3D.cs
using UnityEngine;

public class BoardRenderer3D : MonoBehaviour
{
    [Header("Board Mesh")]
    [SerializeField] private Material _boardMaterial;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _boardThickness = 0.15f;
    [SerializeField] private float _cellSize = 1f;

    [Header("Star Points (화점)")]
    [SerializeField] private Material _starPointMaterial;
    [SerializeField] private float _starPointRadius = 0.08f;

    private const int Size = BoardManager.Size; // 15
    private GameObject _boardRoot;

    private void Start() => BuildBoard();

    public void BuildBoard()
    {
        if (_boardRoot != null) Destroy(_boardRoot);
        _boardRoot = new GameObject("Board3D");
        _boardRoot.transform.SetParent(transform);

        BuildBoardSurface();
        BuildGridLines();
        BuildStarPoints();
    }

    // ── 바닥 판 ──────────────────────────────────
    private void BuildBoardSurface()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "BoardSurface";
        go.transform.SetParent(_boardRoot.transform);

        float boardWorldSize = (Size - 1) * _cellSize;
        go.transform.localScale = new Vector3(
            boardWorldSize + _cellSize * 1.2f,
            _boardThickness,
            boardWorldSize + _cellSize * 1.2f
        );
        go.transform.localPosition = new Vector3(0f, -_boardThickness * 0.5f, 0f);

        if (_boardMaterial != null)
            go.GetComponent<Renderer>().material = _boardMaterial;

        // 보드 레이어 설정 (InputHandler 레이캐스트용)
        go.layer = LayerMask.NameToLayer("Board");
    }

    // ── 격자선 ───────────────────────────────────
    private void BuildGridLines()
    {
        var lineRoot = new GameObject("GridLines");
        lineRoot.transform.SetParent(_boardRoot.transform);

        float halfSpan = (Size - 1) * _cellSize * 0.5f;

        for (int i = 0; i < Size; i++)
        {
            float pos = i * _cellSize - halfSpan;

            // 가로선
            CreateLine(lineRoot,
                new Vector3(-halfSpan, 0.001f, pos),
                new Vector3(halfSpan, 0.001f, pos),
                $"HLine_{i}");

            // 세로선
            CreateLine(lineRoot,
                new Vector3(pos, 0.001f, -halfSpan),
                new Vector3(pos, 0.001f, halfSpan),
                $"VLine_{i}");
        }
    }

    private void CreateLine(GameObject parent, Vector3 start, Vector3 end, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = _lineMaterial;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // ── 화점 (천원·성·귀목) ────────────────────
    private void BuildStarPoints()
    {
        // 표준 오목 화점 좌표 (0-indexed)
        int[] pts = { 3, 7, 11 };
        var starRoot = new GameObject("StarPoints");
        starRoot.transform.SetParent(_boardRoot.transform);

        foreach (int r in pts)
            foreach (int c in pts)
            {
                float halfSpan = (Size - 1) * _cellSize * 0.5f;
                var pos = new Vector3(
                    c * _cellSize - halfSpan,
                    0.002f,
                    r * _cellSize - halfSpan
                );
                CreateStarPoint(starRoot, pos, $"Star_{r}_{c}");
            }
    }

    private void CreateStarPoint(GameObject parent, Vector3 pos, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(
            _starPointRadius * 2f, 0.002f, _starPointRadius * 2f
        );
        Destroy(go.GetComponent<Collider>());

        if (_starPointMaterial != null)
            go.GetComponent<Renderer>().material = _starPointMaterial;
    }
}