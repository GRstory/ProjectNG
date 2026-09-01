using GRstory.Combat;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StatusSystem))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _sprintMultiplier = 1.6f;
        [SerializeField] private float _aimMultiplier = 0.4f;
        [SerializeField] private float _turnSpeed = 720f;   // deg/s
        [SerializeField] private float _gravity = -20f;

        private CharacterController _controller;
        private StatusSystem _statusSystem;
        private Transform _cameraTransform;
        private float _verticalVelocity;

        public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;
        public Vector3 CurrentVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }

        // 기본 MoveSpeed 대비 배율. 걷기=1, 달리기=_sprintMultiplier, 조준=_aimMultiplier.
        // 슬로우 상태이상이 걸려도 값이 유지되므로 애니메이션 블렌드 기준으로 쓴다.
        public float NormalizedSpeed
        {
            get
            {
                float baseSpeed = _statusSystem.GetValue(EStatusType.MoveSpeed);
                return baseSpeed > 0f ? CurrentSpeed / baseSpeed : 0f;
            }
        }

        #region MonoBehaviour
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _statusSystem = GetComponent<StatusSystem>();
        }

        private void Start()
        {
            _cameraTransform = Camera.main != null ? Camera.main.transform : null;

            if (_statusSystem.GetValue(EStatusType.MoveSpeed) <= 0f)
                Debug.LogWarning("StatusSystem에 MoveSpeed 기본값이 없어 이동 속도가 0입니다.", this);
        }
        #endregion

        // 이동 방향을 바라보면서 움직인다
        public void Move(Vector2 moveInput, EMoveMode mode)
        {
            Vector3 direction = ToWorldDirection(moveInput);
            MoveInternal(direction, mode);
            if (direction.sqrMagnitude > 0.0001f)
                Face(direction);
        }

        // 바라보는 방향을 고정한 채 움직인다. 조준 중 옆걸음용
        public void Move(Vector2 moveInput, EMoveMode mode, Vector3 faceDirection)
        {
            MoveInternal(ToWorldDirection(moveInput), mode);
            Face(faceDirection);
        }

        public void Face(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(worldDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
        }

        // 입력을 카메라가 보는 방향 기준의 월드 방향으로 바꾼다
        private Vector3 ToWorldDirection(Vector2 moveInput)
        {
            Vector3 forward = _cameraTransform != null
                ? Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            return Vector3.ClampMagnitude(forward * moveInput.y + right * moveInput.x, 1f);
        }

        private void MoveInternal(Vector3 direction, EMoveMode mode)
        {
            _verticalVelocity = _controller.isGrounded ? -2f : _verticalVelocity + _gravity * Time.deltaTime;

            Vector3 horizontal = direction * GetSpeed(mode);
            _controller.Move((horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime);

            CurrentVelocity = horizontal;
            CurrentSpeed = horizontal.magnitude;
            if (direction.sqrMagnitude > 0.0001f)
                LastMoveDirection = direction.normalized;
        }

        private float GetSpeed(EMoveMode mode)
        {
            float baseSpeed = _statusSystem.GetValue(EStatusType.MoveSpeed);
            return mode switch
            {
                EMoveMode.Sprint => baseSpeed * _sprintMultiplier,
                EMoveMode.Aim => baseSpeed * _aimMultiplier,
                _ => baseSpeed,
            };
        }
    }
}
