using System;
using System.Collections.Generic;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.Combat
{
    public class StatusEffectController : MonoBehaviour, IPlayerData
    {
        private readonly List<StatusEffectInstance> _activeEffectList = new();

        public IReadOnlyList<StatusEffectInstance> ActiveEffects => _activeEffectList;

        public event Action<StatusEffectInstance> OnEffectApplied;
        public event Action<StatusEffectInstance> OnEffectStackChanged;
        public event Action<StatusEffectInstance> OnEffectRemoved;

        #region MonoBehaviour
        private void Update()
        {
            for (int i = _activeEffectList.Count - 1; i >= 0; i--)
            {
                StatusEffectInstance instance = _activeEffectList[i];
                StatusEffectDefinition definition = instance.Definition;

                if (definition.TickInterval > 0f)
                {
                    instance.TickTimer += Time.deltaTime;
                    while (instance.TickTimer >= definition.TickInterval)
                    {
                        instance.TickTimer -= definition.TickInterval;
                        foreach (StatusEffectModule module in definition.Modules)
                        {
                            module.OnTick(instance);
                        }
                    }
                }

                if (definition.Duration > 0f)
                {
                    instance.RemainingTime -= Time.deltaTime;
                    if (instance.RemainingTime <= 0f)
                    {
                        RemoveAt(i);
                    }
                }
            }
        }
        #endregion

        public StatusEffectInstance Apply(StatusEffectDefinition definition, GameObject caster)
        {
            if (definition.StackPolicy != EStackPolicy.Independent)
            {
                StatusEffectInstance existing = _activeEffectList.Find(e => e.Definition == definition);
                if (existing != null)
                {
                    existing.RemainingTime = definition.Duration;

                    if (definition.StackPolicy == EStackPolicy.AddStack && existing.StackCount < definition.MaxStacks)
                    {
                        existing.StackCount++;
                        foreach (StatusEffectModule module in definition.Modules)
                        {
                            module.OnStackChanged(existing);
                        }
                        OnEffectStackChanged?.Invoke(existing);
                    }
                    return existing;
                }
            }

            StatusEffectInstance instance = new StatusEffectInstance(definition, gameObject, caster);
            _activeEffectList.Add(instance);
            foreach (StatusEffectModule module in definition.Modules)
            {
                module.OnApply(instance);
            }
            OnEffectApplied?.Invoke(instance);
            return instance;
        }

        // 디스펠용: 해당 효과 전부 제거
        public void Remove(StatusEffectDefinition definition)
        {
            for (int i = _activeEffectList.Count - 1; i >= 0; i--)
            {
                if (_activeEffectList[i].Definition == definition)
                {
                    RemoveAt(i);
                }
            }
        }

        public void RemoveAll()
        {
            for (int i = _activeEffectList.Count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        private void RemoveAt(int index)
        {
            StatusEffectInstance instance = _activeEffectList[index];
            _activeEffectList.RemoveAt(index);
            foreach (StatusEffectModule module in instance.Definition.Modules)
            {
                module.OnRemove(instance);
            }
            OnEffectRemoved?.Invoke(instance);
        }

        public void CaptureData(PlayerSnapshot snapshot)
        {
            snapshot.StatusEffects.Clear();
            foreach (StatusEffectInstance instance in _activeEffectList)
            {
                snapshot.StatusEffects.Add(new StatusEffectSaveData
                {
                    Definition = instance.Definition,
                    StackCount = instance.StackCount,
                    RemainingTime = instance.RemainingTime,
                });
            }
        }

        public void RestoreData(PlayerSnapshot snapshot)
        {
            RemoveAll();
            foreach (StatusEffectSaveData saveData in snapshot.StatusEffects)
            {
                if (saveData.Definition == null) continue;

                // StackCount를 직접 대입하면 StatModifierModule의 스택별 모디파이어가 누락되므로
                // 반드시 Apply를 스택 수만큼 거쳐 모듈 콜백(OnApply/OnStackChanged)을 태운다.
                // 원래 시전자는 이전 씬과 함께 사라졌으므로 자기 자신을 시전자로 둔다.
                StatusEffectInstance instance = Apply(saveData.Definition, gameObject);
                for (int i = 1; i < saveData.StackCount; i++)
                {
                    Apply(saveData.Definition, gameObject);
                }
                instance.RemainingTime = saveData.RemainingTime;
            }
        }
    }
}
