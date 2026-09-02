using System;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class StatusEffectData
    {
        public string DefinitionId;
        public int StackCount;
        public float RemainingTime;
    }
}
