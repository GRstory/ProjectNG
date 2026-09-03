using GRstory.Character;
using UnityEngine;

namespace GRstory.ItemSystem
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Item/Weapon Data")]
    public class WeaponData : ItemData
    {
        [field: SerializeField, Min(0f)] public float Damage { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float Range { get; private set; } = 15f;

        [field: SerializeField, Min(0f), Tooltip("발사 간격(초)")]
        public float Cooldown { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("히트스캔이 맞을 수 있는 레이어. 벽을 포함해야 엄폐가 동작하고, 플레이어 레이어는 빼야 한다")]
        public LayerMask HitMask { get; private set; }

        [field: SerializeField, Tooltip("AC_Player 기반이어야 한다. 파지·조준·발사 클립을 무기별로 바꾼다")]
        public AnimatorOverrideController AnimatorOverride { get; private set; }

        [field: SerializeField, Tooltip("손 소켓에 붙일 모델. 비워두면 애니메이션만 바뀐다")]
        public GameObject ModelPrefab { get; private set; }

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
