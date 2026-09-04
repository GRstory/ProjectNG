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
        private InputAction _point;
        private InputAction _inventory;

        public Vector2 MoveInput => _move.ReadValue<Vector2>();
        public Vector2 PointerPosition => _point.ReadValue<Vector2>();   // 화면 픽셀 좌표. 조준 방향의 근원
        public bool IsSprintHeld => _sprint.IsPressed();
        public bool IsAimHeld => _aim.IsPressed();
        public bool AttackPressed => _attack.WasPressedThisFrame();
        public bool IsAttackHeld => _attack.IsPressed();
        public bool InteractPressed => _interact.WasPressedThisFrame();
        public bool FlashlightPressed => _flashlight.WasPressedThisFrame();
        public bool InventoryPressed => _inventory.WasPressedThisFrame();

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
            _point = _playerMap.FindAction("Point", throwIfNotFound: true);
            _inventory = _playerMap.FindAction("Inventory", throwIfNotFound: true);
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
