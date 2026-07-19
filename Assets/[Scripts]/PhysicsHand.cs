using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    [Header("PID Translation")]
    [SerializeField] float frequency = 50f;
    [SerializeField] float damping = 1f;
    
    [Header("PID Rotation")]
    [SerializeField] float rotFrequency = 100f;
    [SerializeField] float rotDamping = 0.9f;
    
    [Header("References")]
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] Transform target; // Your VR Controller/Hand Anchor

    [Space]
    [Header("Springs'")]
    [SerializeField] float climbForce = 1000f;
    [SerializeField] float climbDrag = 500f;
    
    Vector3 _previousPosition;
    Rigidbody _rigidbody;

    bool _isColliding;
    

    void Start()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
        _rigidbody = GetComponent<Rigidbody>();
        // Prevents Unity from capping how fast the hand can spin to catch up to the controller
        _rigidbody.maxAngularVelocity = float.PositiveInfinity;
        _previousPosition = transform.position;
    }

    // FIX: Changed from Update to FixedUpdate for physical consistency
    void FixedUpdate()
    {
        if (target == null || playerRigidbody == null) return;

        PIDMovement();
        PIDRotation();
        if (_isColliding) HookesLaw();
    }

    void PIDMovement()
    {
        float kp = (6f * frequency) * (6f * frequency) * 0.25f;
        float kd = 4.5f * frequency * damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        
        // Hand chases target, taking the player's moving velocity into account
        Vector3 force = (target.position - transform.position) * ksg + (playerRigidbody.velocity - _rigidbody.velocity) * kdg;
        _rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void PIDRotation()
    {
        float kp = (6f * rotFrequency) * (6f * rotFrequency) * 0.25f;
        float kd = 4.5f * rotFrequency * rotDamping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        
        Quaternion q = target.rotation * Quaternion.Inverse(transform.rotation);
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        
        q.ToAngleAxis(out float angle, out Vector3 axis); 
        axis.Normalize();
        axis *= Mathf.Deg2Rad;
        
        // Hand matches controller rotation, adjusting for the player's body angular velocity if it exists
        Vector3 torque = ksg * axis * angle + (playerRigidbody.angularVelocity - _rigidbody.angularVelocity) * kdg;
        _rigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    void HookesLaw()
    {
        Vector3 displacementFromResting = transform.position - target.position;
        Vector3 force = displacementFromResting * climbForce;
        float drag = GetDrag();

        playerRigidbody.AddForce(force, ForceMode.Acceleration);
        playerRigidbody.AddForce(drag * -playerRigidbody.velocity * climbDrag, ForceMode.Acceleration);
    }

    float GetDrag()
    {
        Vector3 handVelocity = (target.localPosition - _previousPosition) / Time.fixedDeltaTime;
        float drag = 1 / handVelocity.magnitude + 0.01f;
        drag = drag > 1 ? 1 : drag;
        drag = drag < 0.03f ? 0.03f : drag;
        _previousPosition = transform.position;
        return drag;
    }

    void OnCollisionEnter(Collision Collision)
    {
        _isColliding = true;
    }

    void OnCollisionExit(Collision other)
    {
        _isColliding = false;
    }
}