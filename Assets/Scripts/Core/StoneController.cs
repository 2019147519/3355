// Assets/Scripts/Rendering/StoneController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _blackPrefab;
    [SerializeField] private GameObject _whitePrefab;

    [Header("Layout")]
    [SerializeField] private float _cell = 1f;
    [SerializeField] private float _ofsX = -7f;
    [SerializeField] private float _ofsZ = -7f;
    [SerializeField] private float _stoneY = 0.1f;

    [Header("Drop Anim")]
    [SerializeField] private float _dropH = 2.5f;
    [SerializeField] private float _dropT = 0.22f;

    // ★ Inspector에서 직접 연결 (GetComponent 제거)
    [SerializeField] private BoardManager _board;

    private readonly Dictionary<(int, int), GameObject> _stones = new();

    private void OnEnable()
    {
        if (_board == null) _board = GetComponent<BoardManager>(); // 폴백
        _board.OnStonePlaced += Place;
        _board.OnStoneRemoved += Remove;
    }

    private void OnDisable()
    {
        if (_board == null) return;
        _board.OnStonePlaced -= Place;
        _board.OnStoneRemoved -= Remove;
    }

    private void Place(int r, int c, int player)
    {
        var prefab = player == (int)Player.Black ? _blackPrefab : _whitePrefab;
        var world = GridToWorld(r, c);
        var go = Instantiate(prefab, world + Vector3.up * _dropH,
                                 Quaternion.identity, transform);
        _stones[(r, c)] = go;
        StartCoroutine(Drop(go, world));
    }

    private void Remove(int r, int c)
    {
        if (!_stones.TryGetValue((r, c), out var go)) return;
        _stones.Remove((r, c));
        StartCoroutine(FadeOut(go));
    }

    private IEnumerator Drop(GameObject go, Vector3 target)
    {
        var start = go.transform.position;
        for (float t = 0; t < 1f; t += Time.deltaTime / _dropT)
        {
            if (go == null) yield break;
            go.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        if (go != null) go.transform.position = target;
    }

    private IEnumerator FadeOut(GameObject go)
    {
        var rend = go.GetComponent<Renderer>();
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.18f)
        {
            if (go == null) yield break;
            if (rend)
            {
                var col = rend.material.color;
                col.a = 1f - t;
                rend.material.color = col;
            }
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    public void ClearAll()
    {
        foreach (var s in _stones.Values) if (s) Destroy(s);
        _stones.Clear();
    }

    public Vector3 GridToWorld(int r, int c) =>
        new Vector3(c * _cell + _ofsX, _stoneY, r * _cell + _ofsZ);

    public (int r, int c) WorldToGrid(Vector3 w) =>
    (
        Mathf.RoundToInt((w.z - _ofsZ) / _cell),
        Mathf.RoundToInt((w.x - _ofsX) / _cell)
    );
}