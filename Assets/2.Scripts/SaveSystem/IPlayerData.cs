namespace GRstory.SaveSystem
{
    public interface IPlayerData
    {
        void CaptureData(PlayerSnapshot snapshot);
        void RestoreData(PlayerSnapshot snapshot);
    }
}
