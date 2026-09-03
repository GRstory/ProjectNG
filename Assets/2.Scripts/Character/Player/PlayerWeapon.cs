using System;
using GRstory.Combat;
using GRstory.ItemSystem;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(StatusSystem))]
    public class PlayerWeapon : MonoBehaviour, IPlayerData
    {
        [SerializeField, Tooltip("무기 모델을 붙일 손 소켓. 비워두면 모델을 붙이지 않는다")]
        private Transform _handSocket;
        [SerializeField, Tooltip("히트스캔 시작점. 비워두면 플레이어 위치에서 _fireHeight만큼 위")]
        private Transform _fireOrigin;
        [SerializeField] private float _fireHeight = 1.2f;

        private StatusSystem _statusSystem;
        private PlayerAim _aim;
        private Inventory _inventory;
        private GameObject _modelInstance;
        private float _cooldownTimer;

        public WeaponData Equipped { get; private set; }
        public bool IsEquipped => Equipped != null;

        public event Action<WeaponData> OnWeaponChanged;
        public event Action OnAttacked;

        #region MonoBehaviour
        private void Awake()
        {
            _statusSystem = GetComponent<StatusSystem>();
            TryGetComponent(out _aim);
            TryGetComponent(out _inventory);
        }

        private void OnEnable()
        {
            if (_inventory != null) _inventory.OnChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            if (_inventory != null) _inventory.OnChanged -= HandleInventoryChanged;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }
        #endregion

        public void Equip(WeaponData weapon)
        {
            if (weapon == null || weapon == Equipped) return;
            SetWeapon(weapon);
        }

        public void Unequip()
        {
            if (Equipped == null) return;
            SetWeapon(null);
        }

        // 발사 여부를 돌려준다. 빗나가도 발사는 한 것이다
        public bool TryAttack()
        {
            if (Equipped == null || _cooldownTimer > 0f) return false;

            _cooldownTimer = Equipped.Cooldown;
            OnAttacked?.Invoke();

            Vector3 origin = GetFireOrigin();
            Vector3 direction = GetFireDirection(origin);
            if (Physics.Raycast(origin, direction, out RaycastHit hit, Equipped.Range, Equipped.HitMask, QueryTriggerInteraction.Ignore))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.GetDamage(new DamageContext
                {
                    Attacker = gameObject,
                    Damage = Equipped.Damage + _statusSystem.GetValue(EStatusType.Attack),
                    Type = EDamageType.Normal,
                });
            }
            return true;
        }

        // 교체는 항상 이 함수를 거친다. 해제 후 장착을 따로 부르면 이벤트가 두 번 나가므로 한 번에 바꾼다
        private void SetWeapon(WeaponData weapon)
        {
            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
                _modelInstance = null;
            }

            Equipped = weapon;
            _cooldownTimer = 0f;

            if (weapon != null && weapon.ModelPrefab != null && _handSocket != null)
                _modelInstance = Instantiate(weapon.ModelPrefab, _handSocket, false);

            OnWeaponChanged?.Invoke(weapon);
        }

        private Vector3 GetFireOrigin()
        {
            return _fireOrigin != null ? _fireOrigin.position : transform.position + Vector3.up * _fireHeight;
        }

        // 락온 타겟이 있으면 그 몸통 중심으로, 없으면 정면으로 쏜다. 히트스캔이라 중간 장애물이 막으면 빗나가는 게 정상
        private Vector3 GetFireDirection(Vector3 origin)
        {
            if (_aim != null && _aim.HasTarget)
            {
                Vector3 toTarget = _aim.TargetPoint - origin;
                if (toTarget.sqrMagnitude > 0.0001f) return toTarget.normalized;
            }
            return transform.forward;
        }

        // 장착 무기가 인벤토리에서 사라지면(폐기, 로드 복원) 같이 해제한다
        private void HandleInventoryChanged()
        {
            if (Equipped != null && _inventory.CountOf(Equipped) == 0) Unequip();
        }

        #region IPlayerData
        public void CaptureData(PlayerSnapshot snapshot)
        {
            snapshot.EquippedWeapon = Equipped;
        }

        public void RestoreData(PlayerSnapshot snapshot)
        {
            if (snapshot.EquippedWeapon != null) Equip(snapshot.EquippedWeapon);
            else Unequip();
        }
        #endregion
    }
}
