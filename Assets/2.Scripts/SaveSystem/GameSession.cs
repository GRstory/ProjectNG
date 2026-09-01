using System.Collections.Generic;
using UnityEngine;

namespace GRstory.SaveSystem
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }
        public PlayerSnapshot PlayerSnapshot { get; private set; }

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
        public SceneState GetSceneState(string sceneName)
        {
            if (!_sceneStateDict.TryGetValue(sceneName, out SceneState state))
            {
                state = new SceneState();
                _sceneStateDict[sceneName] = state;
            }
            return state;
        }

        public void ClearSession()
        {
            PlayerSnapshot = null;
            _sceneStateDict.Clear();
        }
        #endregion

    }
}
