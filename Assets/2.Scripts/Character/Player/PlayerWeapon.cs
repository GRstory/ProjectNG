using System;
using GRstory.Combat;
using GRstory.ItemSystem;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.Character
{
    [RequireComponent(typeof(StatusSystem))]
    [RequireComponent(typeof(PlayerAim))]
    [RequireComponent(typeof(Inventory))]
    public class PlayerWeapon : MonoBehaviour, IPlayerData
    {
        [SerializeField, Tooltip("무기 액터를 붙일 손 소켓. 총구 위치가 여기서 나오므로 필수")]
        private Transform _handSocket;

        private StatusSystem _statusSystem;
        private PlayerAim _aim;
        private Inventory _inventory;
        private WeaponActor _actor;

        public WeaponItemData Equipped { get; private set; }
        public bool IsEquipped => Equipped != null;

        public event Action<WeaponItemData> OnWeaponChanged;
        public event Action OnAttacked;

        #region MonoBehaviour
        private void Awake()
        {
            _statusSystem = GetComponent<StatusSystem>();
            _aim = GetComponent<PlayerAim>();
            _inventory = GetComponent<Inventory>();
        }

        private void OnEnable()
        {
            _inventory.OnChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            _inventory.OnChanged -= HandleInventoryChanged;
        }
        #endregion

        public void Equip(WeaponItemData weapon)
        {
            if (weapon == null || weapon == Equipped) return;
            if (weapon.ActorPrefab == null)
            {
                Debug.LogError($"무기 '{weapon.name}' 장착 실패: ActorPrefab이 비어 있음", weapon);
                return;
            }
            SetWeapon(weapon);
        }

        public void Unequip()
        {
            if (Equipped == null) return;
            SetWeapon(null);
        }

        // 발사 여부를 돌려준다. 방향은 플레이어 조준에서, 원점과 수치는 액터에서 나온다
        public bool TryAttack()
        {
            if (_actor == null) return false;

            Vector3 direction = _aim.GetFireDirection(_actor.Muzzle.position);
            float bonusDamage = _statusSystem.GetValue(EStatusType.Attack);
            if (!_actor.TryFire(gameObject, direction, bonusDamage)) return false;

            OnAttacked?.Invoke();
            return true;
        }

        // 교체는 항상 이 함수를 거친다. 해제 후 장착을 따로 부르면 이벤트가 두 번 나가므로 한 번에 바꾼다
        private void SetWeapon(WeaponItemData weapon)
        {
            if (_actor != null)
            {
                Destroy(_actor.gameObject);
                _actor = null;
            }

            Equipped = weapon;

            if (weapon != null)
            {
                _actor = Instantiate(weapon.ActorPrefab, _handSocket, false);
                _actor.Initialize(weapon);
            }

            OnWeaponChanged?.Invoke(weapon);
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
