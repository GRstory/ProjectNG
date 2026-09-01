namespace GRstory.Combat
{
    public enum EStackPolicy
    {
        RefreshDuration,    // 재적용 시 지속시간만 갱신
        AddStack,           // 재적용 시 스택 증가 (MaxStacks까지) + 지속시간 갱신
        Independent,        // 재적용 시 별개 인스턴스로 공존
    }
}
