using System;
using System.Collections.Generic;

namespace GRstory.SaveSystem
{
    [Serializable]
    public class SceneStateData
    {
        public List<string> VisitedRoomList;
        public Dictionary<string, bool> BoolFactDict;
        public Dictionary<string, int> IntFactDict;
    }
}
