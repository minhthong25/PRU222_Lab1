using UnityEngine;

public class VacuumMovement : MonoBehaviour
{

    private enum RoombaState
    {
        Zigzag,
        Spiral
    }

    [SerializeField] private float _speed = 5f;

    [SerializeField] private LayerMask _obstacleLayerMask;

    private float _rotateSpeed = 60f;

    private RoombaState _currentState = RoombaState.Zigzag;

    private Vector2 _direction = Vector2.down;

    private bool _isOn = true;

    private Rigidbody2D _rb;

    private float _spiralRotationSpeed = 120f;  // Starting rotation speed for spiral
    private float _spiralRotationAcceleration = 10;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!_isOn)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        switch (_currentState)
        {
            case RoombaState.Zigzag:
                ZigzagMovement();
                break;
            case RoombaState.Spiral:
                SpiralMovement();
                break;
        }
    }

    private void Update()
    {
        RotateSprite();
    }

    private void OnMove()
    {
        _isOn = !_isOn;
        Debug.Log($"Roomba is now {(_isOn ? "ON" : "OFF")}");
    }

    private void OnSwitchMode()
    {
        _currentState = _currentState == RoombaState.Zigzag ? RoombaState.Spiral : RoombaState.Zigzag;
        Debug.Log($"Switched to {_currentState} mode");
    }

    private void ZigzagMovement()
    {
        if (IsObstacleAhead())
        {
            float angle = Random.Range(-120f, 120f);

            _direction = Quaternion.Euler(0, 0, angle) * _direction;

            _direction.Normalize();
        }

        _rb.linearVelocity = _direction * _speed;
    }

    private void SpiralMovement()
    {
        if (IsObstacleAhead())
        {
            float deflectionAngle = Random.Range(-45f, 45f);
            _direction = Quaternion.Euler(0, 0, deflectionAngle) * _direction;
            _direction.Normalize();

            // Reverse spiral direction on obstacle
            _spiralRotationSpeed = -_spiralRotationSpeed;
        }

        float deceleration = _spiralRotationAcceleration * Time.fixedDeltaTime;
        float minRotationSpeed = 50f;  // Keep it moderately high for a tight spiral
        float maxRotationSpeed = 150f;

        // Gradually decrease the absolute spiral rotation speed (expand radius)
        if (Mathf.Abs(_spiralRotationSpeed) > minRotationSpeed)
        {
            _spiralRotationSpeed -= Mathf.Sign(_spiralRotationSpeed) * deceleration;
        }
        else
        {
            _spiralRotationSpeed = Mathf.Sign(_spiralRotationSpeed) * minRotationSpeed;
        }

        // Add small oscillation for natural effect
        float oscillation = Mathf.Sin(Time.time * 2f) * 5f;
        _spiralRotationSpeed += oscillation * Time.fixedDeltaTime;

        // Clamp final rotation speed
        _spiralRotationSpeed = Mathf.Clamp(_spiralRotationSpeed, -maxRotationSpeed, maxRotationSpeed);

        float angleDelta = _spiralRotationSpeed * Time.fixedDeltaTime;
        _direction = Quaternion.Euler(0, 0, angleDelta) * _direction;

        _rb.linearVelocity = _direction.normalized * _speed;
    }

    private void RotateSprite()
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool IsObstacleAhead()
    {
        float rayDistance = 1.5f;
        float[] angles = { -30f, 0f, 30f };

        foreach (float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * _direction;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayDistance, _obstacleLayerMask);
            if (hit.collider != null)
                return true;
        }
        return false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float rayDistance = 1.5f;
        float[] angles = { -30f, 0f, 30f };

        foreach (float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * _direction;
            Gizmos.DrawRay(transform.position, dir * rayDistance);
        }
    }

}
    
