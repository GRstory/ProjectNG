using System;
using GRstory.Combat;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Health))]
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] private float _staggerDuration = 0.4f;
        [SerializeField] private float _interactDuration = 0.5f;

        private PlayerInput _input;
        private PlayerMovement _movement;
        private PlayerAim _aim;
        private PlayerInteractor _interactor;
        private Flashlight _flashlight;
        private Health _health;
        private float _stateTimer;

        public EPlayerState State { get; private set; } = EPlayerState.Normal;

        public event Action<EPlayerState> OnStateChanged;

        #region MonoBehaviour
        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _movement = GetComponent<PlayerMovement>();
            _health = GetComponent<Health>();

            TryGetComponent(out _aim);
            TryGetComponent(out _interactor);
            TryGetComponent(out _flashlight);

            PlayerRegistry.RegisterPlayer(this);
        }

        private void OnEnable()
        {
            _health.OnHit += HandleHit;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnHit -= HandleHit;
            _health.OnDied -= HandleDied;
        }

        private void Update()
        {
            switch (State)
            {
                case EPlayerState.Normal:
                    UpdateNormal();
                    break;
                case EPlayerState.Interacting:
                case EPlayerState.Staggered:
                    UpdateTimedState();
                    break;
                case EPlayerState.Dead:
                    break;
            }
        }
        #endregion

        private void UpdateNormal()
        {
            if (_input.FlashlightPressed && _flashlight != null)
                _flashlight.Toggle();

            if (_aim != null && _input.IsAimHeld)
                UpdateAiming();
            else
                UpdateLocomotion();

            if (_input.InteractPressed && _interactor != null && _interactor.TryInteract())
                SetState(EPlayerState.Interacting);
        }

        private void UpdateAiming()
        {
            if (!_aim.IsAiming) _aim.StartAim();

            if (_input.NextTargetPressed) _aim.CycleTarget(1);
            else if (_input.PreviousTargetPressed) _aim.CycleTarget(-1);

            _aim.UpdateAim();

            // 타겟이 있으면 그쪽을 바라본 채 움직이고, 없으면 이동 방향을 바라본다
            if (_aim.HasTarget)
                _movement.Move(_input.MoveInput, EMoveMode.Aim, _aim.AimDirection);
            else
                _movement.Move(_input.MoveInput, EMoveMode.Aim);

            // TODO: 무기 시스템 연결 후 _input.AttackPressed 처리
        }

        private void UpdateLocomotion()
        {
            if (_aim != null && _aim.IsAiming) _aim.StopAim();

            _movement.Move(_input.MoveInput, _input.IsSprintHeld ? EMoveMode.Sprint : EMoveMode.Walk);
        }

        private void UpdateTimedState()
        {
            _movement.Move(Vector2.zero, EMoveMode.Walk); // 몸이 묶여 있는 동안에도 중력은 계속 받는다

            _stateTimer -= Time.deltaTime;
            if (_stateTimer > 0f) return;

            SetState(EPlayerState.Normal);
        }

        // 상태 전이는 전부 이 함수를 거친다. 우선순위: Dead > Staggered > 나머지
        private void SetState(EPlayerState next)
        {
            if (State == EPlayerState.Dead) return;
            if (State == next && next != EPlayerState.Staggered) return; // 연속 피격이면 경직 시간을 처음부터 다시 잰다

            bool changed = State != next;
            if (State == EPlayerState.Normal && _aim != null && _aim.IsAiming)
                _aim.StopAim();

            State = next;
            _stateTimer = next switch
            {
                EPlayerState.Staggered => _staggerDuration,
                EPlayerState.Interacting => _interactDuration,
                _ => 0f,
            };

            if (changed) OnStateChanged?.Invoke(State);
        }

        private void HandleHit(DamageContext context)
        {
            SetState(EPlayerState.Staggered);
        }

        private void HandleDied()
        {
            SetState(EPlayerState.Dead);
        }
    }
}
