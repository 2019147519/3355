// Assets/Scripts/Rendering/StoneAnimator.cs
using System.Collections;
using UnityEngine;

public class StoneAnimator : MonoBehaviour
{
    [SerializeField] private Material _lastMoveMarkerMat;
    [SerializeField] private float _bounceHeight = 0.15f;
    [SerializeField] private float _bounceDuration = 0.3f;

    private GameObject _lastMarker;

    // 마지막 착수 위치에 마커 표시
    public void MarkLastMove(Vector3 worldPos)
    {
        if (_lastMarker != null) Destroy(_lastMarker);

        _lastMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _lastMarker.transform.position = worldPos + Vector3.up * 0.001f;
        _lastMarker.transform.localScale = new Vector3(0.25f, 0.003f, 0.25f);
        Destroy(_lastMarker.GetComponent<Collider>());

        if (_lastMoveMarkerMat != null)
            _lastMarker.GetComponent<Renderer>().material = _lastMoveMarkerMat;
    }

    // 돌 착수 시 통통 튀는 느낌
    public IEnumerator BounceEffect(Transform stoneTransform)
    {
        var origin = stoneTransform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / _bounceDuration;
            float bounce = Mathf.Sin(t * Mathf.PI) * _bounceHeight;
            stoneTransform.position = origin + Vector3.up * bounce;
            yield return null;
        }
        stoneTransform.position = origin;
    }

    public void ClearMarker()
    {
        if (_lastMarker != null) Destroy(_lastMarker);
    }
}