using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    public class SceneState
    {
        private readonly HashSet<string> _visitedRoomSet = new();

        // 오브젝트별 사실 기록. 키는 "{objectId}.{fact}" (예: "3f2a...c1.dead")
        private readonly Dictionary<string, bool> _boolDict = new();
        private readonly Dictionary<string, int> _intDict = new();

        // 미니맵용: 방문한 방 전체 열람
        public IReadOnlyCollection<string> VisitedRooms => _visitedRoomSet;

        public void MarkRoomVisited(string roomId) => _visitedRoomSet.Add(roomId);
        public bool IsRoomVisited(string roomId) => _visitedRoomSet.Contains(roomId);

        public void SetBool(string objectId, string fact, bool value)
            => _boolDict[MakeKey(objectId, fact)] = value;

        public bool GetBool(string objectId, string fact, bool defaultValue = false)
            => _boolDict.TryGetValue(MakeKey(objectId, fact), out bool value) ? value : defaultValue;

        public void SetInt(string objectId, string fact, int value)
            => _intDict[MakeKey(objectId, fact)] = value;

        public int GetInt(string objectId, string fact, int defaultValue = 0)
            => _intDict.TryGetValue(MakeKey(objectId, fact), out int value) ? value : defaultValue;

        private static string MakeKey(string objectId, string fact) => $"{objectId}.{fact}";

        #region 저장용 변환
        public SceneStateData ToData() => new SceneStateData
        {
            VisitedRoomList = new List<string>(_visitedRoomSet),
            BoolFactDict = new Dictionary<string, bool>(_boolDict),
            IntFactDict = new Dictionary<string, int>(_intDict),
        };

        public static SceneState FromData(SceneStateData data)
        {
            SceneState state = new();
            if (data.VisitedRoomList != null)
                state._visitedRoomSet.UnionWith(data.VisitedRoomList);
            if (data.BoolFactDict != null)
                foreach (KeyValuePair<string, bool> pair in data.BoolFactDict)
                    state._boolDict[pair.Key] = pair.Value;
            if (data.IntFactDict != null)
                foreach (KeyValuePair<string, int> pair in data.IntFactDict)
                    state._intDict[pair.Key] = pair.Value;
            return state;
        }
        #endregion
    }
}
