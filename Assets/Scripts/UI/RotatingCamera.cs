// Assets/Scripts/UI/RotatingCamera.cs
using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _radius = 12f;
    [SerializeField] private float _speed = 8f;   // µµ/√ 
    [SerializeField] private float _height = 8f;
    [SerializeField] private float _tiltAngle = 40f;

    private float _angle;

    private void Update()
    {
        _angle += _speed * Time.deltaTime;
        float rad = _angle * Mathf.Deg2Rad;

        transform.position = new Vector3(
            Mathf.Sin(rad) * _radius,
            _height,
            Mathf.Cos(rad) * _radius
        );
        transform.LookAt(_target != null ? _target.position : Vector3.zero);
    }
}