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

        _lastMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        var markerPos = new Vector3(worldPos.x, worldPos.y + 0.15f, worldPos.z);
        _lastMarker.transform.position = markerPos;
        _lastMarker.transform.localScale = new Vector3(_markerSize * 2f, 0.002f, _markerSize * 2f);

        Destroy(_lastMarker.GetComponent<Collider>());

        if (_markerMat != null)
            _lastMarker.GetComponent<Renderer>().material = _markerMat;
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