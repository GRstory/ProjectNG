namespace GRstory.Combat
{
    public enum EModifierType
    {
        Flat,           // +n
        PercentAdd,     // 합연산 (0.2 = +20%, 두 개 걸리면 +40%)
        PercentMult,    // 곱연산 (0.2 = x1.2, 두 개 걸리면 x1.44)
    }
}
