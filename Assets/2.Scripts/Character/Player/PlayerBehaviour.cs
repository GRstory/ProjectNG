using System;
using GRstory.Combat;
using GRstory.UISystem;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerAim))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(Flashlight))]
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] private float _staggerDuration = 0.4f;
        [SerializeField] private float _interactDuration = 0.5f;

        private PlayerInput _input;
        private PlayerMovement _movement;
        private PlayerAim _aim;
        private PlayerInteractor _interactor;
        private Flashlight _flashlight;
        private PlayerWeapon _weapon;
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
            _aim = GetComponent<PlayerAim>();
            _interactor = GetComponent<PlayerInteractor>();
            _flashlight = GetComponent<Flashlight>();
            _weapon = GetComponent<PlayerWeapon>();

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
            // 정지 중엔 인벤토리 닫기만 받고 나머지 입력은 전부 막는다
            if (UIManager.Instance.IsPaused)
            {
                if (_input.InventoryPressed && UIManager.Instance.IsTop<InventoryUI>())
                    UIManager.Instance.DeactiveUI<InventoryUI>();
                return;
            }

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
            if (_input.InventoryPressed)
            {
                UIManager.Instance.ActiveUI<InventoryUI>();
                return;
            }

            if (_input.FlashlightPressed)
                _flashlight.Toggle();

            // 무기가 있을때만 조준
            if (_input.IsAimHeld && _weapon.IsEquipped)
                UpdateAiming();
            else
                UpdateLocomotion();

            if (_input.InteractPressed && _interactor.TryInteract())
                SetState(EPlayerState.Interacting);
        }

        private void UpdateAiming()
        {
            if (!_aim.IsAiming) _aim.StartAim();
            _aim.UpdateAim(_input.PointerPosition);

            // 몸은 조준 방향을 보고 다리는 입력대로 움직인다. 옆걸음은 애니메이터가 속도 방향으로 섞는다
            _movement.Move(_input.MoveInput, EMoveMode.Aim, _aim.AimDirection);

            // 발사는 조준 중에만. 이동과 병렬이라 상태를 바꾸지 않는다.
            // 연사 무기는 누르고 있는 동안 쿨다운 간격으로, 단발은 누를 때마다 한 발
            bool triggerPulled = _weapon.Equipped.IsAutomatic ? _input.IsAttackHeld : _input.AttackPressed;
            if (triggerPulled)
                _weapon.TryAttack();
        }

        private void UpdateLocomotion()
        {
            if (_aim.IsAiming) _aim.StopAim();

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
            if (State == EPlayerState.Normal && _aim.IsAiming)
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
