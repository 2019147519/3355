// Assets/Scripts/Rendering/EffectManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _winLineMat;

    // ★ Inspector 직접 연결
    [SerializeField] private StoneController _stone;

    private readonly List<GameObject> _winObjs = new();

    private void Awake()
    {
        // 폴백 — 같은 오브젝트에 있을 경우
        if (_stone == null) _stone = GetComponent<StoneController>();
    }

    public void ShowWinLine(List<(int row, int col)> cells)
    {
        if (cells == null) return;
        ClearWinLine();
        StartCoroutine(AnimateWin(cells));
    }

    private IEnumerator AnimateWin(List<(int, int)> cells)
    {
        foreach (var (r, c) in cells)
        {
            var pos = _stone.GridToWorld(r, c) + Vector3.up * 0.05f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.28f;

            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            if (_winLineMat != null)
                go.GetComponent<Renderer>().material = _winLineMat;

            _winObjs.Add(go);
            yield return new WaitForSeconds(0.07f);
        }
    }

    public void ClearWinLine()
    {
        foreach (var go in _winObjs) if (go) Destroy(go);
        _winObjs.Clear();
    }
}