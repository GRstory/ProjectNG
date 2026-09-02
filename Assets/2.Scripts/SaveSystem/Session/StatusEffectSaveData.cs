using System;
using GRstory.Combat;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class StatusEffectSaveData
    {
        public StatusEffectDefinition Definition;
        public int StackCount;
        public float RemainingTime;
    }
}
