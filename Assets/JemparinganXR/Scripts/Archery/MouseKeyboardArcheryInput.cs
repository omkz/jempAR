using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JemparinganXR.Archery
{
    /// <summary>
    /// Temporary mouse & keyboard input provider for prototype validation.
    /// Supports Unity Input System (com.unity.inputsystem) with legacy fallback.
    /// </summary>
    public class MouseKeyboardArcheryInput : MonoBehaviour, IArcheryInput
    {
        [Header("Mouse Aiming")]
        [SerializeField] private float mouseSensitivity = 1.2f;
        [SerializeField] private bool autoLockCursor = true;

        private Vector2 _lookDelta;
        private bool _isDrawHeld;
        private bool _isDrawReleased;
        private bool _isCancelRequested;
        private bool _isContinueRequested;
        private bool _isRestartRequested;

        public Vector2 LookDelta => _lookDelta;
        public bool IsDrawHeld => _isDrawHeld;
        public bool IsDrawReleased => _isDrawReleased;
        public bool IsCancelRequested => _isCancelRequested;
        public bool IsContinueRequested => _isContinueRequested;
        public bool IsRestartRequested => _isRestartRequested;

        private void Start()
        {
            if (autoLockCursor)
            {
                LockCursor();
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
            }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }

            if (mouse != null)
            {
                _lookDelta = mouse.delta.ReadValue() * (mouseSensitivity * 0.1f);
                _isDrawHeld = mouse.leftButton.isPressed;
                _isDrawReleased = mouse.leftButton.wasReleasedThisFrame;
                _isCancelRequested = mouse.rightButton.wasPressedThisFrame;
            }
            else
            {
                _lookDelta = Vector2.zero;
                _isDrawHeld = false;
                _isDrawReleased = false;
                _isCancelRequested = false;
            }

            if (keyboard != null)
            {
                _isContinueRequested = keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
                _isRestartRequested = keyboard.rKey.wasPressedThisFrame;
            }
            else
            {
                _isContinueRequested = false;
                _isRestartRequested = false;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }

            _lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
            _isDrawHeld = Input.GetMouseButton(0);
            _isDrawReleased = Input.GetMouseButtonUp(0);
            _isCancelRequested = Input.GetMouseButtonDown(1);
            _isContinueRequested = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
            _isRestartRequested = Input.GetKeyDown(KeyCode.R);
#endif
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
