using GRstory.Character;
using UnityEngine;

namespace GRstory.ItemSystem
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Item/Weapon Item Data")]
    public class WeaponItemData : ItemData
    {
        [field: SerializeField, Min(0f), Tooltip("펠릿 하나당 데미지. 샷건 총량은 Damage × PelletCount")]
        public float Damage { get; private set; } = 10f;

        [field: SerializeField, Min(0f)] public float Range { get; private set; } = 15f;

        [field: SerializeField, Min(0f), Tooltip("발사 간격(초). 연사 무기는 이 간격으로 계속 나간다")]
        public float Cooldown { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("누르고 있는 동안 계속 발사. 끄면 누를 때마다 한 발")]
        public bool IsAutomatic { get; private set; }

        [field: SerializeField, Min(1), Tooltip("한 발에 나가는 레이 수. 샷건은 여러 개")]
        public int PelletCount { get; private set; } = 1;

        [field: SerializeField, Min(0f), Tooltip("펠릿이 흩어지는 원뿔 반각(도). 0이면 정확히 조준점으로")]
        public float SpreadAngle { get; private set; }

        [field: SerializeField, Tooltip("히트스캔이 맞을 수 있는 레이어. 벽을 포함해야 엄폐가 동작하고, 플레이어 레이어는 빼야 한다")]
        public LayerMask HitMask { get; private set; }

        [field: SerializeField, Tooltip("AC_Player 기반이어야 한다. 파지·조준·발사 클립을 무기별로 바꾼다")]
        public AnimatorOverrideController AnimatorOverride { get; private set; }

        [field: SerializeField, Tooltip("손 소켓에 붙일 무기 액터 프리팹. 총구 위치와 발사를 담당한다")]
        public WeaponActor ActorPrefab { get; private set; }

        public override bool IsUsable => true;

        // 무기 사용 = 장착 토글. 이미 장착 중이면 해제한다
        public override void Use(GameObject user)
        {
            if (!user.TryGetComponent(out PlayerWeapon weapon))
            {
                Debug.LogError($"무기 '{name}' 장착 실패: '{user.name}'에 PlayerWeapon이 없음", this);
                return;
            }

            if (weapon.Equipped == this) weapon.Unequip();
            else weapon.Equip(this);
        }

        // 무기는 겹치지 않고, 사용(장착)해도 사라지지 않는다
        private void OnValidate()
        {
            MaxStack = 1;
            ConsumeOnUse = false;
        }
    }
}
