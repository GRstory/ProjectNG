using System;
using System.Collections.Generic;
using GRstory.ItemSystem;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class PlayerSnapshot
    {
        public float MaxHealth;
        public float CurrentHealth;
        public List<StatusEffectSaveData> StatusEffects = new();
        public ItemStack[] Items;   // 인덱스 = 슬롯. null이면 빈 칸
        public WeaponItemData EquippedWeapon;   // null이면 맨손. 항상 Items 안에 있는 무기여야 한다
    }
}
