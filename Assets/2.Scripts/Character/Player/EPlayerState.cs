namespace GRstory.Character
{
    public enum EPlayerState
    {
        Normal,      // 이동/조준/사격이 동시 실행되는 기본 상태
        Interacting,
        Staggered,
        Dead,
    }
}
