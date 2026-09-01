using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    public class SceneState
    {
        private readonly HashSet<string> _visitedRoomSet = new();
        private readonly HashSet<string> _clearedRoomSet = new();

        // 오브젝트별 사실 기록. 키는 "{objectId}.{fact}" (예: "3f2a...c1.dead")
        private readonly Dictionary<string, bool> _boolDict = new();
        private readonly Dictionary<string, int> _intDict = new();

        // 미니맵용: 방문한 방 전체 열람
        public IReadOnlyCollection<string> VisitedRooms => _visitedRoomSet;

        public void MarkRoomVisited(string roomId) => _visitedRoomSet.Add(roomId);
        public bool IsRoomVisited(string roomId) => _visitedRoomSet.Contains(roomId);

        public void MarkRoomCleared(string roomId) => _clearedRoomSet.Add(roomId);
        public bool IsRoomCleared(string roomId) => _clearedRoomSet.Contains(roomId);

        public void SetBool(string objectId, string fact, bool value)
            => _boolDict[MakeKey(objectId, fact)] = value;

        public bool GetBool(string objectId, string fact, bool defaultValue = false)
            => _boolDict.TryGetValue(MakeKey(objectId, fact), out bool value) ? value : defaultValue;

        public void SetInt(string objectId, string fact, int value)
            => _intDict[MakeKey(objectId, fact)] = value;

        public int GetInt(string objectId, string fact, int defaultValue = 0)
            => _intDict.TryGetValue(MakeKey(objectId, fact), out int value) ? value : defaultValue;

        private static string MakeKey(string objectId, string fact) => $"{objectId}.{fact}";
    }
}
