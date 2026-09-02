using System.Collections.Generic;
using UnityEngine;

namespace GRstory.SaveSystem
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }
        public PlayerSnapshot PlayerSnapshot { get; private set; }

        public string LastSceneName { get; set; }
        public string NextSpawnPointId { get; set; }

        private readonly Dictionary<string, SceneState> _sceneStateDict = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance != null) return;

            GameObject sessionObject = new GameObject(nameof(GameSession));
            Instance = sessionObject.AddComponent<GameSession>();
            DontDestroyOnLoad(sessionObject);
        }

        #region Player
        public void CapturePlayer(GameObject player)
        {
            PlayerSnapshot = new PlayerSnapshot();
            foreach (IPlayerData data in player.GetComponentsInChildren<IPlayerData>(true))
            {
                data.CaptureData(PlayerSnapshot);
            }
        }

        public void RestorePlayer(GameObject player)
        {
            if (PlayerSnapshot == null) return;

            foreach (IPlayerData data in player.GetComponentsInChildren<IPlayerData>(true))
            {
                data.RestoreData(PlayerSnapshot);
            }
        }
        #endregion

        #region Scene
        // 저장 시 전체 씬 상태 열람용
        public IReadOnlyDictionary<string, SceneState> SceneStateDict => _sceneStateDict;

        public SceneState GetSceneState(string sceneName)
        {
            if (!_sceneStateDict.TryGetValue(sceneName, out SceneState state))
            {
                state = new SceneState();
                _sceneStateDict[sceneName] = state;
            }
            return state;
        }

        // 세이브 파일 로드 시 세션 전체를 교체한다
        public void RestoreSession(PlayerSnapshot playerSnapshot, Dictionary<string, SceneState> sceneStateDict)
        {
            PlayerSnapshot = playerSnapshot;
            _sceneStateDict.Clear();
            foreach (KeyValuePair<string, SceneState> pair in sceneStateDict)
            {
                _sceneStateDict[pair.Key] = pair.Value;
            }
        }

        public void ClearSession()
        {
            PlayerSnapshot = null;
            _sceneStateDict.Clear();
        }
        #endregion

    }
}
