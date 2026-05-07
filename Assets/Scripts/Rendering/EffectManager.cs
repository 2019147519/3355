// Assets/Scripts/Rendering/EffectManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("승리 이펙트")]
    [SerializeField] private ParticleSystem _winBlackPrefab; // 흑돌 승리
    [SerializeField] private ParticleSystem _winWhitePrefab; // 백돌 승리

    [Header("참조")]
    [SerializeField] private StoneController _stone;



    private readonly List<ParticleSystem> _winParticles = new();

    private void Awake()
    {
        if (_stone == null) _stone = GetComponent<StoneController>();
    }

    public void ShowWinLine(List<(int row, int col)> cells, Player winner)
    {
        if (cells == null || cells.Count < 2) return;
        ClearWinLine();
        StartCoroutine(AnimateWin(cells, winner));
    }

    private IEnumerator AnimateWin(List<(int row, int col)> cells, Player winner)
    {
        // ── 중앙 위치 ────────────────────────────
        var mid = cells[cells.Count / 2];
        var midPos = _stone.GridToWorld(mid.row, mid.col) + Vector3.up * 0.15f;

        // ── 방향 계산 ────────────────────────────
        var first = _stone.GridToWorld(cells[0].row, cells[0].col);
        var last = _stone.GridToWorld(cells[cells.Count - 1].row, cells[cells.Count - 1].col);
        var dir = (last - first).normalized;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        var rot = Quaternion.Euler(0f, angle, 0f);

        // ── 흑백 프리팹 선택 후 생성 ─────────────
        var prefab = winner == Player.Black ? _winBlackPrefab : _winWhitePrefab;

        if (prefab != null)
        {
            var ps = Instantiate(prefab, midPos, rot, transform);
            ps.Play();
            _winParticles.Add(ps);
        }

        yield return null;
    }

    public void ClearWinLine()
    {
        foreach (var ps in _winParticles)
        {
            if (ps != null)
            {
                ps.Stop();
                Destroy(ps.gameObject);
            }
        }
        _winParticles.Clear();
    }
}