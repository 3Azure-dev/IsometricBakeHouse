using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _offset = new Vector2(0f, 1.5f);

    [Header("Smoothing")]
    [SerializeField] private float _smoothTime = 0.2f;

    [Header("Level Edges")]
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 100f;
    [SerializeField] private float _minY = 0f;

    private Vector3 _velocity;

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 goal = new Vector3(
            _target.position.x + _offset.x,
            _target.position.y + _offset.y,
            transform.position.z);

        goal.x = Mathf.Clamp(goal.x, _minX, _maxX);
        goal.y = Mathf.Max(goal.y, _minY);

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
    }
}