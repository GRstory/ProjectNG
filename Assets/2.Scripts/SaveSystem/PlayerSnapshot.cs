using System;
using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class PlayerSnapshot
    {
        public float MaxHealth;
        public float CurrentHealth;
        public List<StatusEffectSaveData> StatusEffects = new();
    }
}
