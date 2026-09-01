using System.Collections.Generic;

namespace GRstory.Combat
{
    public class StatusData
    {
        private readonly List<StatusModifier> _modifierList = new();
        private float _baseValue;
        private float _tempValue;
        private bool _isDirty = true;

        public StatusData(float baseValue) => _baseValue = baseValue;

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _tempValue = GetNewValue();
                    _isDirty = false;
                }
                return _tempValue;
            }
        }

        public void AddModifier(StatusModifier modifier)
        {
            _modifierList.Add(modifier);
            _isDirty = true;
        }

        public bool RemoveModifiersFromSource(object source)
        {
            int removedCount = _modifierList.RemoveAll(m => m.Source == source);
            if (removedCount > 0) _isDirty = true;
            return removedCount > 0;
        }

        private float GetNewValue()
        {
            float flatValue = 0f;
            float percentAddValue = 0f;
            float percentMultValue = 1f;

            foreach (StatusModifier modifier in _modifierList)
            {
                switch (modifier.Op)
                {
                    case EModifierType.Flat:
                        flatValue += modifier.Value;
                        break;
                    case EModifierType.PercentAdd:
                        percentAddValue += modifier.Value;
                        break;
                    case EModifierType.PercentMult:
                        percentMultValue *= 1f + modifier.Value;
                        break;
                }
            }

            return (_baseValue + flatValue) * (1f + percentAddValue) * percentMultValue;
        }
    }
}
