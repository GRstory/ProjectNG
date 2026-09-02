using System;
using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class PlayerSnapshotData
    {
        public float MaxHealth;
        public float CurrentHealth;
        public List<StatusEffectData> StatusEffectList = new();
    }
}
