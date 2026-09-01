namespace GRstory.Combat
{
    public class StatusModifier
    {
        public EStatusType Stat { get; }
        public EModifierType Op { get; }
        public float Value { get; }
        public object Source { get; }

        public StatusModifier(EStatusType stat, EModifierType op, float value, object source)
        {
            Stat = stat;
            Op = op;
            Value = value;
            Source = source;
        }
    }
}
