using UnityEngine;
using UnityTutorial.Manager;

namespace UnityTutorial.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float MouseSensitivity = 21.9f;

        [Header("Movement")]
        [SerializeField] private float WalkSpeed = 2f;
        [SerializeField] private float RunSpeed = 6f;
        [SerializeField] private float CrouchSpeed = 1.5f;

        [Header("Jump + Ground")]
        [SerializeField, Range(10, 500)] private float JumpFactor = 260f;
        [SerializeField] private float DistanceToGround = 1.1f;
        [SerializeField] private LayerMask GroundMask;  
        [SerializeField] private float AirMovementMultiplier = 0.8f;

        private Rigidbody _rigidbody;
        private InputManager _input;
        private Collider _collider;
        private bool _grounded;

        private float _xRotation;
        private Vector2 _currentVelocity;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<InputManager>();
            _collider = GetComponent<Collider>();

            Debug.Log("<color=yellow>PlayerController initialized.</color>");
        }

        private void FixedUpdate()
        {
            SampleGround();
            Move();
            HandleJump();
            HandleCrouchDebug();
        }

        private void LateUpdate()
        {
            CamMovement();
        }

        private void Move()
        {
            Debug.Log($"Move Input: {_input.Move}, Run: {_input.Run}");

            // get velocity info ONCE here (global to this method)
            Vector3 rbVel = _rigidbody.linearVelocity;
            Vector3 rbVelHorizontal = new Vector3(rbVel.x, 0f, rbVel.z);
            float horizontalSpeed = rbVelHorizontal.magnitude;

            // compute target speed if input exists
            float targetSpeed = _input.Run ? RunSpeed : WalkSpeed;
            if (_input.Crouch) targetSpeed = CrouchSpeed;

            bool noInput = _input.Move == Vector2.zero;

            // ============================================================
            // ======================== GROUNDED ===========================
            // ============================================================
            if (_grounded)
            {
                // ----- NO INPUT → APPLY SPEED-ADAPTIVE FRICTION -----
                if (noInput)
                {
                    // 0 speed = sticky ; 12+ speed = slidy
                    float frictionFactor = Mathf.InverseLerp(0f, 12f, horizontalSpeed);
                    float friction = Mathf.Lerp(10f, 0.05f, frictionFactor);

                    if (horizontalSpeed > 0.01f)
                    {
                        Vector3 frictionForce = -rbVelHorizontal.normalized * friction;
                        _rigidbody.AddForce(frictionForce, ForceMode.Acceleration);
                    }

                    return; // done for grounded no-input case
                }

                // ----- INPUT EXISTS → NORMAL MOVEMENT -----
                _currentVelocity.x = Mathf.Lerp(
                    _currentVelocity.x,
                    _input.Move.x * targetSpeed,
                    1f * Time.fixedDeltaTime
                );

                _currentVelocity.y = Mathf.Lerp(
                    _currentVelocity.y,
                    _input.Move.y * targetSpeed,
                    10f * Time.fixedDeltaTime
                );

                Vector3 diff = new Vector3(
                    _currentVelocity.x - rbVel.x,
                    0f,
                    _currentVelocity.y - rbVel.z
                );

                _rigidbody.AddForce(transform.TransformVector(diff), ForceMode.VelocityChange);
                return;
            }

            // ============================================================
            // ========================== AIR =============================
            // ============================================================
            Vector3 airInput = transform.TransformVector(
                new Vector3(_currentVelocity.x, 0, _currentVelocity.y)
            ) * AirMovementMultiplier;

            _rigidbody.AddForce(airInput, ForceMode.VelocityChange);
        }

        private void CamMovement()
        {
            float mx = _input.Look.x;
            float my = _input.Look.y;

            Debug.Log($"Look Input: {mx}, {my}");

            Camera.position = CameraRoot.position;

            _xRotation -= my * MouseSensitivity * Time.smoothDeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);

            Camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);

            _rigidbody.MoveRotation(
                _rigidbody.rotation *
                Quaternion.Euler(0, mx * MouseSensitivity * Time.smoothDeltaTime, 0)
            );
        }

        private void HandleJump()
        {
            if (!_input.Jump)
            {
                return;
            }

            Debug.Log($"Jump Pressed, Grounded: {_grounded}");

            _input.ConsumeJump();
            
            if (!_grounded)
            {
                Debug.Log("<color=red>Jump failed: NOT GROUNDED.</color>");
                return; // lmnoT t t t t lmnot TTTTTT (the T stand, keep it niche, and keep the change)
            }

            Debug.Log("<color=green>Jumping!</color>");

            _rigidbody.AddForce(-_rigidbody.linearVelocity.y * Vector3.up, ForceMode.VelocityChange);
            _rigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
        }

        private void HandleCrouchDebug()
        {
            if (_input.Crouch)
            {
                Debug.Log("<color=cyan>Crouch pressed. (Note: No crouch logic implemented yet)</color>");
            }
        }

        private void SampleGround()
        {
            Vector3 origin = _rigidbody.worldCenterOfMass;
            float maxDist = DistanceToGround + 0.15f;

            Vector3[] directions =
            {
        Vector3.down,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

            bool foundGround = false;

            foreach (var dir in directions)
            {
                if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, GroundMask))
                {
                    if (hit.collider != _collider)
                    {
                        foundGround = true;
                        break;
                    }
                }
            }

            // Apply final state outside loop
            if (foundGround)
            {
                if (!_grounded) Debug.Log("<color=green>Grounded = TRUE</color>");
                _grounded = true;
            }
            else
            {
                if (_grounded) Debug.Log("<color=red>Grounded = FALSE</color>");
                _grounded = false;
            }

        }
    }
}
