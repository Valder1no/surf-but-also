using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityTutorial.Manager
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput PlayerInput;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Run { get; private set; }
        public bool Jump { get; private set; }
        public bool Crouch { get; private set; }

        private InputActionMap _currentMap;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _runAction;
        private InputAction _jumpAction;
        private InputAction _crouchAction;

        private void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _currentMap = PlayerInput.currentActionMap;

            _moveAction = _currentMap.FindAction("Move");
            _lookAction = _currentMap.FindAction("Look");
            _runAction = _currentMap.FindAction("Run");
            _jumpAction = _currentMap.FindAction("Jump");
            _crouchAction = _currentMap.FindAction("Crouch");

            _moveAction.performed += ctx => Move = ctx.ReadValue<Vector2>();
            _moveAction.canceled += ctx => Move = Vector2.zero;

            _lookAction.performed += ctx => Look = ctx.ReadValue<Vector2>();
            _lookAction.canceled += ctx => Look = Vector2.zero;

            _runAction.performed += ctx => Run = ctx.ReadValueAsButton();
            _runAction.canceled += ctx => Run = false;

            _jumpAction.started += ctx => Jump = true;
            _jumpAction.canceled += ctx => Jump = false;

            _crouchAction.performed += ctx => Crouch = ctx.ReadValueAsButton();
            _crouchAction.canceled += ctx => Crouch = false;

            _jumpAction.performed += ctx =>
            {
                Jump = ctx.ReadValueAsButton();
                Jump = true;
                Debug.Log("<color=green>Jump performed event detected!</color>");
            };

            _jumpAction.canceled += ctx =>
            {
                Jump = false; 
                Debug.Log("<color=yellow>Jump canceled.</color>");
            };
        }
        

        private void OnEnable() => _currentMap.Enable();
        private void OnDisable() => _currentMap.Disable();

        public void ConsumeJump()
        {
            Jump = false;
        }

        
    }


}

