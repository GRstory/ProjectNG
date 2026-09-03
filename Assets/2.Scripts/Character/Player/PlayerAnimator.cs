using GRstory.Combat;
using GRstory.ItemSystem;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(PlayerBehaviour))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int StaggerHash = Animator.StringToHash("Stagger");
        private static readonly int InteractHash = Animator.StringToHash("Interact");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField, Tooltip("비워두면 자식에서 찾는다")]
        private Animator _animator;
        [SerializeField] private float _dampTime = 0.1f;
        [SerializeField, Tooltip("조준 상체 레이어 인덱스. 레이어가 없으면 무시된다")]
        private int _upperBodyLayerIndex = 1;
        [SerializeField] private float _upperBodyBlendSpeed = 8f;

        private PlayerBehaviour _behaviour;
        private PlayerMovement _movement;
        private PlayerAim _aim;
        private PlayerWeapon _weapon;
        private Health _health;
        private RuntimeAnimatorController _baseController; // 맨손용. 무기 해제 시 되돌린다
        private float _upperBodyWeight;

        #region MonoBehaviour
        private void Awake()
        {
            _behaviour = GetComponent<PlayerBehaviour>();
            _movement = GetComponent<PlayerMovement>();
            TryGetComponent(out _aim);
            TryGetComponent(out _weapon);
            TryGetComponent(out _health);

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogWarning("Animator를 찾지 못해 PlayerAnimator를 비활성화합니다.", this);
                enabled = false;
                return;
            }

            _baseController = _animator.runtimeAnimatorController;
        }

        private void OnEnable()
        {
            _behaviour.OnStateChanged += HandleStateChanged;
            // 연속 피격은 상태가 그대로라 OnStateChanged가 안 오므로 피격 이벤트로 직접 트리거한다
            if (_health != null) _health.OnHit += HandleHit;
            if (_weapon != null)
            {
                _weapon.OnWeaponChanged += HandleWeaponChanged;
                _weapon.OnAttacked += HandleAttacked;
            }
        }

        private void OnDisable()
        {
            _behaviour.OnStateChanged -= HandleStateChanged;
            if (_health != null) _health.OnHit -= HandleHit;
            if (_weapon != null)
            {
                _weapon.OnWeaponChanged -= HandleWeaponChanged;
                _weapon.OnAttacked -= HandleAttacked;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _animator.SetFloat(SpeedHash, _movement.NormalizedSpeed, _dampTime, deltaTime);

            // 조준 중 옆걸음 블렌드용. 이동 방향을 캐릭터 기준 좌표로 바꾼다
            Vector3 local = transform.InverseTransformDirection(_movement.CurrentVelocity);
            Vector3 localDirection = local.sqrMagnitude > 0.0001f ? local.normalized : Vector3.zero;
            _animator.SetFloat(MoveXHash, localDirection.x, _dampTime, deltaTime);
            _animator.SetFloat(MoveYHash, localDirection.z, _dampTime, deltaTime);

            bool isAiming = _aim != null && _aim.IsAiming;
            _animator.SetBool(IsAimingHash, isAiming);

            UpdateUpperBodyLayer(isAiming, deltaTime);
        }
        #endregion

        private void UpdateUpperBodyLayer(bool isAiming, float deltaTime)
        {
            if (_upperBodyLayerIndex < 0 || _upperBodyLayerIndex >= _animator.layerCount) return;

            _upperBodyWeight = Mathf.MoveTowards(_upperBodyWeight, isAiming ? 1f : 0f, _upperBodyBlendSpeed * deltaTime);
            _animator.SetLayerWeight(_upperBodyLayerIndex, _upperBodyWeight);
        }

        private void HandleStateChanged(EPlayerState state)
        {
            switch (state)
            {
                case EPlayerState.Interacting:
                    _animator.SetTrigger(InteractHash);
                    break;
                case EPlayerState.Dead:
                    _animator.SetBool(IsDeadHash, true);
                    break;
            }
        }

        private void HandleHit(DamageContext context)
        {
            _animator.SetTrigger(StaggerHash);
        }

        // 무기별 파지·조준·발사 클립은 오버라이드 컨트롤러로 바꾼다.
        // 교체 순간 상태머신이 초기화되지만 인벤토리 정지 중에 일어나므로 눈에 띄지 않는다
        private void HandleWeaponChanged(WeaponData weapon)
        {
            RuntimeAnimatorController next = weapon != null && weapon.AnimatorOverride != null
                ? weapon.AnimatorOverride
                : _baseController;

            if (_animator.runtimeAnimatorController != next)
                _animator.runtimeAnimatorController = next;
        }

        private void HandleAttacked()
        {
            _animator.SetTrigger(AttackHash);
        }
    }
}
