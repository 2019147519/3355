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
        // 1. 시작점과 끝점 좌표 가져오기
        Vector3 firstPos = _stone.GridToWorld(cells[0].row, cells[0].col);
        Vector3 lastPos = _stone.GridToWorld(cells[cells.Count - 1].row, cells[cells.Count - 1].col);

        // 2. 중심점 계산 (Y축 살짝 올림)
        Vector3 midPos = Vector3.Lerp(firstPos, lastPos, 0.5f) + Vector3.up * 0.15f;

        // 3. 방향 벡터 계산
        Vector3 dir = (lastPos - firstPos).normalized;

        // 4. 회전값 계산 (LookRotation 사용)
        // dir 방향을 앞(Forward)으로 보고, 위쪽을 Vector3.up으로 설정
        Quaternion rot = Quaternion.LookRotation(dir);

        // 5. 프리팹 생성
        var prefab = winner == Player.Black ? _winBlackPrefab : _winWhitePrefab;

        if (prefab != null)
        {
            var ps = Instantiate(prefab, midPos, rot, transform);
            // 생성 시 rot을 넣었으므로 추가적인 rotation 대입은 필요 없으나, 확실히 하기 위해:
            ps.transform.forward = dir;
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