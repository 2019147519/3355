// Assets/Scripts/Rendering/BoardRenderer3D.cs
using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer3D : MonoBehaviour
{
    [Header("Board Mesh")]
    [SerializeField] private Material _boardMaterial;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _boardThickness = 0.15f;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _lineHeight = 0.04f;
    [SerializeField] private float _lineWidth = 0.025f;

    [Header("Star Points (ȭ��)")]
    [SerializeField] private Material _starPointMaterial;
    [SerializeField] private float _starPointRadius = 0.08f;
    [SerializeField] private float _starPointHeight = 0.03f;

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

    // ���� �ٴ� �� ��������������������������������������������������������������������
    private void BuildBoardSurface()
    {
        var go = new GameObject("BoardSurface");
        go.transform.SetParent(_boardRoot.transform);

        float boardWorldSize = (Size - 1) * _cellSize;
        go.transform.localScale = new Vector3(
            boardWorldSize + _cellSize * 1.2f,
            _boardThickness,
            boardWorldSize + _cellSize * 1.2f
        );
        go.transform.localPosition = new Vector3(0f, -_boardThickness * 0.5f, 0f);

        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateBoxMesh();

        var meshRenderer = go.AddComponent<MeshRenderer>();
        if (_boardMaterial != null)
            meshRenderer.material = _boardMaterial;

        go.AddComponent<BoxCollider>();
        go.layer = LayerMask.NameToLayer("Board");
    }

    // ���� ���ڼ� ����������������������������������������������������������������������
    private void BuildGridLines()
    {
        float halfSpan = (Size - 1) * _cellSize * 0.5f;
        var go = new GameObject("GridLines");
        go.transform.SetParent(_boardRoot.transform);

        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateGridMesh(halfSpan, _lineHeight, _lineWidth);

        var meshRenderer = go.AddComponent<MeshRenderer>();
        if (_lineMaterial != null)
            meshRenderer.material = _lineMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    private void CreateLine(GameObject parent, Vector3 start, Vector3 end, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);

        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateLineMesh(start, end, _lineWidth);

        var meshRenderer = go.AddComponent<MeshRenderer>();
        if (_lineMaterial != null)
            meshRenderer.material = _lineMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // ���� ȭ�� (õ���������͸�) ����������������������������������������
    private void BuildStarPoints()
    {
        // ǥ�� ���� ȭ�� ��ǥ (0-indexed)
        int[] pts = { 3, 7, 11 };
        var starRoot = new GameObject("StarPoints");
        starRoot.transform.SetParent(_boardRoot.transform);

        foreach (int r in pts)
            foreach (int c in pts)
            {
                float halfSpan = (Size - 1) * _cellSize * 0.5f;
                var pos = new Vector3(
                    c * _cellSize - halfSpan,
                    _starPointHeight,
                    r * _cellSize - halfSpan
                );
                CreateStarPoint(starRoot, pos, $"Star_{r}_{c}");
            }
    }

    private void CreateStarPoint(GameObject parent, Vector3 pos, string name)
    {
        var go = new GameObject(name);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = pos;

        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateDiskMesh(_starPointRadius, 24);

        var meshRenderer = go.AddComponent<MeshRenderer>();
        if (_starPointMaterial != null)
            meshRenderer.material = _starPointMaterial;
    }

    private static Mesh CreateLineMesh(Vector3 start, Vector3 end, float width)
    {
        var direction = (end - start).normalized;
        var side = Vector3.Cross(Vector3.up, direction).normalized * (width * 0.5f);

        var vertices = new[]
        {
            start - side,
            start + side,
            end + side,
            end - side
        };

        var triangles = new[]
        {
            0, 2, 1,
            0, 3, 2
        };

        var normals = new[]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };

        var mesh = new Mesh { name = "BoardLineMesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateGridMesh(float halfSpan, float y, float width)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        float halfWidth = width * 0.5f;
        float cellSize = (halfSpan * 2f) / (Size - 1);
        float horizontalYOffset = 0.002f;

        for (int i = 0; i < Size; i++)
        {
            float x = i * cellSize - halfSpan;
            AddTopQuad(
                vertices,
                triangles,
                normals,
                new Vector3(x - halfWidth, y, -halfSpan - halfWidth),
                new Vector3(x + halfWidth, y, -halfSpan - halfWidth),
                new Vector3(x + halfWidth, y, halfSpan + halfWidth),
                new Vector3(x - halfWidth, y, halfSpan + halfWidth));
        }

        for (int i = 0; i < Size; i++)
        {
            float z = i * cellSize - halfSpan;
            float horizontalY = y + horizontalYOffset;
            AddTopQuad(
                vertices,
                triangles,
                normals,
                new Vector3(-halfSpan - halfWidth, horizontalY, z - halfWidth),
                new Vector3(halfSpan + halfWidth, horizontalY, z - halfWidth),
                new Vector3(halfSpan + halfWidth, horizontalY, z + halfWidth),
                new Vector3(-halfSpan - halfWidth, horizontalY, z + halfWidth));
        }

        var mesh = new Mesh { name = "BoardGridMesh" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddTopQuad(
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector3> normals,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d)
    {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        triangles.Add(start);
        triangles.Add(start + 3);
        triangles.Add(start + 2);
        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 1);
    }

    private static Mesh CreateDiskMesh(float radius, int segments)
    {
        int sideVertexCount = segments + 1;
        var vertices = new Vector3[sideVertexCount * 2];
        var normals = new Vector3[vertices.Length];
        var triangles = new int[segments * 6];

        vertices[0] = Vector3.zero;
        vertices[sideVertexCount] = Vector3.zero;
        normals[0] = Vector3.up;
        normals[sideVertexCount] = Vector3.down;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            var vertex = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            int top = i + 1;
            int bottom = sideVertexCount + i + 1;

            vertices[top] = vertex;
            vertices[bottom] = vertex;
            normals[top] = Vector3.up;
            normals[bottom] = Vector3.down;
        }

        for (int i = 0; i < segments; i++)
        {
            int tri = i * 6;
            int current = i + 1;
            int next = i == segments - 1 ? 1 : i + 2;
            int bottomCenter = sideVertexCount;
            int bottomCurrent = sideVertexCount + current;
            int bottomNext = sideVertexCount + next;

            triangles[tri] = 0;
            triangles[tri + 1] = next;
            triangles[tri + 2] = current;

            triangles[tri + 3] = bottomCenter;
            triangles[tri + 4] = bottomCurrent;
            triangles[tri + 5] = bottomNext;
        }

        var mesh = new Mesh { name = "BoardDiskMesh" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
    private static Mesh CreateBoxMesh()
    {
        var vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };

        var triangles = new[]
        {
            0, 4, 5, 0, 5, 1,
            1, 5, 6, 1, 6, 2,
            2, 6, 7, 2, 7, 3,
            3, 7, 4, 3, 4, 0,
            4, 7, 6, 4, 6, 5,
            3, 0, 1, 3, 1, 2
        };

        var mesh = new Mesh { name = "BoardSurfaceMesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
