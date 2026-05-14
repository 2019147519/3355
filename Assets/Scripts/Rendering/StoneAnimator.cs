// StoneAnimator.cs — 딜레이 추가
using System.Collections;
using UnityEngine;

public class StoneAnimator : MonoBehaviour
{
    [Header("마지막 착수 마커")]
    [SerializeField] private Material _markerMat;
    [SerializeField] private float _markerSize = 0.18f;
    [SerializeField] private float _markerDelay = 0.25f; // ★ 돌 드롭 후 딜레이

    private GameObject _lastMarker;
    private Coroutine _markerCo;

    public void MarkLastMove(Vector3 worldPos)
    {
        // 이전 코루틴 취소 + 마커 제거
        if (_markerCo != null) StopCoroutine(_markerCo);
        ClearMarker();

        _markerCo = StartCoroutine(ShowMarkerDelayed(worldPos));
    }

    private IEnumerator ShowMarkerDelayed(Vector3 worldPos)
    {
        // ★ 돌이 착지할 때까지 대기
        yield return new WaitForSeconds(_markerDelay);

        _lastMarker = new GameObject("LastMoveMarker");
        var markerPos = new Vector3(worldPos.x, worldPos.y + 0.15f, worldPos.z);
        _lastMarker.transform.position = markerPos;

        var meshFilter = _lastMarker.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateDiskMesh(_markerSize, 24);

        var meshRenderer = _lastMarker.AddComponent<MeshRenderer>();
        if (_markerMat != null)
            meshRenderer.material = _markerMat;
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

        var mesh = new Mesh { name = "DiskMarkerMesh" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    public void ClearMarker()
    {
        if (_lastMarker != null)
        {
            Destroy(_lastMarker);
            _lastMarker = null;
        }
    }
}
