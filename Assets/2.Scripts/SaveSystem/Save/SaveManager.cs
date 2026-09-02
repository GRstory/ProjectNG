using System;
using System.Collections.Generic;
using System.IO;
using GRstory.Combat;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GRstory.SaveSystem
{
    public static class SaveManager
    {
        private const int CurrentVersion = 1;

        private static StatusEffectDatabase _statusEffectDatabase;

        public static bool HasSave(int slot = 0)
        {
            return File.Exists(GetPath(slot));
        }

        public static void Delete(int slot = 0)
        {
            string path = GetPath(slot);
            if (File.Exists(path)) File.Delete(path);
        }

        public static void Save(GameObject player, int slot = 0)
        {
            GameSession session = GameSession.Instance;
            session.CapturePlayer(player);

            SaveData saveData = new SaveData
            {
                Version = CurrentVersion,
                LastSceneName = SceneManager.GetActiveScene().name,
                PlayerSnapshot = ToData(session.PlayerSnapshot),
            };
            foreach (KeyValuePair<string, SceneState> pair in session.SceneStateDict)
            {
                saveData.SceneStateDict[pair.Key] = pair.Value.ToData();
            }

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string path = GetPath(slot);
            string tempPath = path + ".tmp";

            // 쓰다 만 파일이 기존 세이브를 덮지 않도록 임시 파일에 완성한 뒤 교체
            File.WriteAllText(tempPath, json);
            if (File.Exists(path)) File.Replace(tempPath, path, null);
            else File.Move(tempPath, path);
        }

        public static bool TryLoad(out string lastSceneName, int slot = 0)
        {
            lastSceneName = null;

            string path = GetPath(slot);
            if (!File.Exists(path)) return false;

            SaveData saveData;
            try
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogError($"세이브 파일 읽기 실패: {path}\n{e}");
                return false;
            }

            if (saveData == null) return false;
            if (saveData.Version != CurrentVersion) return false; // 포맷이 바뀌면 여기서 구버전 마이그레이션

            Dictionary<string, SceneState> sceneStateDict = new();
            if (saveData.SceneStateDict != null)
            {
                foreach (KeyValuePair<string, SceneStateData> pair in saveData.SceneStateDict)
                {
                    sceneStateDict[pair.Key] = SceneState.FromData(pair.Value);
                }
            }

            GameSession.Instance.RestoreSession(FromData(saveData.PlayerSnapshot), sceneStateDict);
            lastSceneName = saveData.LastSceneName;
            return true;
        }

        private static string GetPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
        }

        private static StatusEffectDatabase GetDatabase()
        {
            if (_statusEffectDatabase == null)
            {
                _statusEffectDatabase = Resources.Load<StatusEffectDatabase>("StatusEffectDatabase");
                if (_statusEffectDatabase == null)
                {
                    Debug.LogError("Resources 폴더에서 StatusEffectDatabase 에셋을 찾지 못함");
                }
            }
            return _statusEffectDatabase;
        }

        #region Convert
        private static PlayerSnapshotData ToData(PlayerSnapshot snapshot)
        {
            PlayerSnapshotData data = new PlayerSnapshotData
            {
                MaxHealth = snapshot.MaxHealth,
                CurrentHealth = snapshot.CurrentHealth,
            };
            foreach (StatusEffectSaveData effect in snapshot.StatusEffects)
            {
                if (effect.Definition == null) continue;
                data.StatusEffectList.Add(new StatusEffectData
                {
                    DefinitionId = effect.Definition.Id,
                    StackCount = effect.StackCount,
                    RemainingTime = effect.RemainingTime,
                });
            }
            return data;
        }

        private static PlayerSnapshot FromData(PlayerSnapshotData data)
        {
            if (data == null) return null;

            PlayerSnapshot snapshot = new PlayerSnapshot
            {
                MaxHealth = data.MaxHealth,
                CurrentHealth = data.CurrentHealth,
            };
            if (data.StatusEffectList == null) return snapshot;

            StatusEffectDatabase database = GetDatabase();
            foreach (StatusEffectData effect in data.StatusEffectList)
            {
                StatusEffectDefinition definition = database != null
                    ? database.GetById(effect.DefinitionId)
                    : null;
                if (definition == null)
                {
                    Debug.LogWarning($"세이브의 상태이상 Id '{effect.DefinitionId}'를 데이터베이스에서 찾지 못해 건너뜀");
                    continue;
                }
                snapshot.StatusEffects.Add(new StatusEffectSaveData
                {
                    Definition = definition,
                    StackCount = effect.StackCount,
                    RemainingTime = effect.RemainingTime,
                });
            }
            return snapshot;
        }
        #endregion
    }
}
