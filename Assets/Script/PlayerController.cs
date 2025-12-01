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
        private bool _canWallJump;

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
            SampleWallJump();
            ApplyAntiStick();
            Move();
            HandleJump();
            HandleCrouchDebug();}

            private void ApplyAntiStick()
            {
                if (_grounded) return; // don't push away when on the ground

                Vector3 origin = _rigidbody.worldCenterOfMass;
                float dist = 0.9f;  // small distance to detect touching walls

                Vector3[] dirs = {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left,
                    Vector3.right
                };

                foreach (var d in dirs)
                {
                    if (Physics.Raycast(origin, d, dist, GroundMask))
                    {
                        // Push opposite of the wall normal
                        _rigidbody.AddForce(-d * 100f, ForceMode.Acceleration);
                        return;
                    }
                }
                
            }

        private void LateUpdate()
        {
            CamMovement();
        }

        // ---------------- MOVEMENT ----------------

        private void Move()
        {
            Debug.Log($"Move Input: {_input.Move}, Run: {_input.Run}");

            float targetSpeed = _input.Run ? RunSpeed : WalkSpeed;

            if (_input.Crouch) targetSpeed = CrouchSpeed;
            if (_input.Move == Vector2.zero) targetSpeed = 0;

            Debug.Log($"TargetSpeed = {targetSpeed}");

            if (_grounded)
            {
                _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _input.Move.x * targetSpeed, 10f * Time.fixedDeltaTime);
                _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _input.Move.y * targetSpeed, 10f * Time.fixedDeltaTime);

                Vector3 diff = new Vector3(
                    _currentVelocity.x - _rigidbody.linearVelocity.x,
                    0,
                    _currentVelocity.y - _rigidbody.linearVelocity.z
                );

                Debug.Log($"Applying Ground Movement Force: {diff}");
                _rigidbody.AddForce(transform.TransformVector(diff), ForceMode.VelocityChange);
            }
            else
            {
                Vector3 air = transform.TransformVector(new Vector3(_currentVelocity.x, 0, _currentVelocity.y))
                              * AirMovementMultiplier;

                Debug.Log($"Applying Air Force: {air}");
                _rigidbody.AddForce(air, ForceMode.VelocityChange);
            }
        }

        // ---------------- CAMERA ----------------

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
                _rigidbody.rotation * Quaternion.Euler(0, mx * MouseSensitivity * Time.smoothDeltaTime, 0)
            );
        }

        // ---------------- JUMP ----------------

        private void HandleJump()
        {
            if (!_input.Jump) return;

            Debug.Log($"Jump Pressed. Grounded: {_grounded}, WallTouch: {_canWallJump}");
            _input.ConsumeJump();

            if (!_canWallJump)
            {
                Debug.Log("<color=red>Jump failed: no wall to jump from.</color>");
                return;
            }

            Debug.Log("<color=green>WALL JUMP!</color>");

            // Remove downward velocity
            _rigidbody.AddForce(-_rigidbody.linearVelocity.y * Vector3.up, ForceMode.VelocityChange);

            // Jump upward
            _rigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
        }

        private void HandleCrouchDebug()
        {
            if (_input.Crouch)
            {
                Debug.Log("<color=cyan>Crouch pressed. (Note: No crouch logic implemented yet)</color>");
            }
        }

        // ---------------- GROUND CHECK (DOWN ONLY) ----------------

        private void SampleGround()
        {
            Vector3 origin = _rigidbody.worldCenterOfMass;
            float maxDist = DistanceToGround + 0.5f;

            Debug.DrawRay(origin, Vector3.down * maxDist, Color.red);

            // ONLY DOWN determines grounded state
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, GroundMask)
                && hit.collider != _collider)
            {
                if (!_grounded)
                    Debug.Log("<color=green>Grounded = TRUE</color>");

                _grounded = true;
            }
            else
            {
                if (_grounded)
                    Debug.Log("<color=red>Grounded = FALSE</color>");

                _grounded = false;
            }
        }

        // ---------------- WALL JUMP CHECK ----------------

        private void SampleWallJump()
        {
            Vector3 origin = _rigidbody.worldCenterOfMass;
            float dist = 1.1f;

            Vector3[] directions =
            {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right
            };

            _canWallJump = false;

            foreach (var dir in directions)
            {
                if (Physics.Raycast(origin, dir, dist, GroundMask))
                {
                    _canWallJump = true;
                    return;
                }
            }
        }
    }
}
