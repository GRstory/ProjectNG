using GRstory.Character;
using GRstory.SaveSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    public static SceneManagerEx Instance { get; private set; }
    public bool IsLoading { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance != null) return;

        GameObject managerObject = new GameObject(nameof(SceneManagerEx));
        Instance = managerObject.AddComponent<SceneManagerEx>();
        DontDestroyOnLoad(managerObject);
    }

    public void SceneTravel(string targetScene, string spawnPointId)
    {
        if (IsLoading) return;

        GameSession session = GameSession.Instance;
        PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
        if (player != null)
        {
            session.CapturePlayer(player.gameObject);
        }
        session.NextSpawnPointId = spawnPointId;

        StartCoroutine(LoadRoutine(targetScene));
    }

    public void LoadScene(string sceneName)
    {
        if (IsLoading) return;

        GameSession.Instance.NextSpawnPointId = null;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        yield return SceneManager.LoadSceneAsync(sceneName);
        IsLoading = false;
    }
}
