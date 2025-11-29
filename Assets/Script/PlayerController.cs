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
                return;
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
            float maxDist = DistanceToGround + 0.5f;

            Debug.DrawRay(origin, Vector3.down * maxDist, Color.red);

            Debug.Log($"Raycasting from {origin}, distance {maxDist}");

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, GroundMask))
            {
                Debug.Log($"Ray hit: {hit.collider.name} (layer: {hit.collider.gameObject.layer})");

                if (hit.collider != _collider)
                {
                    if (!_grounded)
                        Debug.Log("<color=green>Grounded = TRUE</color>");

                    _grounded = true;
                    return;
                }
                else
                {
                    Debug.Log("<color=red>Ray hit the PLAYER's own collider — ignoring!</color>");
                }
            }
            else
            {
                Debug.Log("<color=orange>No ground detected</color>");
            }

            if (_grounded)
                Debug.Log("<color=orange>Grounded = FALSE</color>");

            _grounded = false;
        }
    }
}
