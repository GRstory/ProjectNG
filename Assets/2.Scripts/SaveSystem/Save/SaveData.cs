using System;
using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public int Version;
        public DateTime SavedAtUtc;
        public string LastSceneName;
        public PlayerSnapshotData PlayerSnapshot;
        public Dictionary<string, SceneStateData> SceneStateDict = new();
    }
}
