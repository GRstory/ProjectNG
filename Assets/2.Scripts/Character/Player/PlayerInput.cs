using UnityEngine;
using UnityEngine.InputSystem;

namespace GRstory.Character
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actionAsset;

        private InputActionMap _playerMap;
        private InputAction _move;
        private InputAction _sprint;
        private InputAction _aim;
        private InputAction _attack;
        private InputAction _interact;
        private InputAction _flashlight;
        private InputAction _next;
        private InputAction _previous;

        public Vector2 MoveInput => _move.ReadValue<Vector2>();
        public bool IsSprintHeld => _sprint.IsPressed();
        public bool IsAimHeld => _aim.IsPressed();
        public bool AttackPressed => _attack.WasPressedThisFrame();
        public bool InteractPressed => _interact.WasPressedThisFrame();
        public bool FlashlightPressed => _flashlight.WasPressedThisFrame();
        public bool NextTargetPressed => _next.WasPressedThisFrame();
        public bool PreviousTargetPressed => _previous.WasPressedThisFrame();

        #region MonoBehaviour
        private void Awake()
        {
            _playerMap = _actionAsset.FindActionMap("Player", throwIfNotFound: true);
            _move = _playerMap.FindAction("Move", throwIfNotFound: true);
            _sprint = _playerMap.FindAction("Sprint", throwIfNotFound: true);
            _aim = _playerMap.FindAction("Aim", throwIfNotFound: true);
            _attack = _playerMap.FindAction("Attack", throwIfNotFound: true);
            _interact = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _flashlight = _playerMap.FindAction("Flashlight", throwIfNotFound: true);
            _next = _playerMap.FindAction("Next", throwIfNotFound: true);
            _previous = _playerMap.FindAction("Previous", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _playerMap.Enable();
        }

        private void OnDisable()
        {
            _playerMap.Disable();
        }
        #endregion
    }
}
