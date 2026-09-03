using System;
using System.Collections.Generic;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.ItemSystem
{
    public class Inventory : MonoBehaviour, IPlayerData
    {
        [SerializeField, Min(1)] private int _capacity = 6;

        private ItemStack[] _slots;   // null = 빈 칸. 슬롯 위치가 UI와 세이브에 그대로 쓰인다

        public int Capacity => _capacity;
        public IReadOnlyList<ItemStack> Slots => _slots;

        public event Action OnChanged;

        #region MonoBehaviour
        private void Awake()
        {
            _slots = new ItemStack[_capacity];
        }
        #endregion

        // 전부 들어가거나 하나도 안 들어간다. 일부만 줍는 상황을 만들지 않기 위해
        public bool TryAdd(ItemData item, int count = 1)
        {
            if (item == null || count <= 0) return false;
            if (GetFreeSpace(item) < count) return false;

            // 이미 있는 스택부터 채우고, 남은 건 빈 칸에 새 스택으로
            for (int i = 0; i < _slots.Length && count > 0; i++)
            {
                ItemStack stack = _slots[i];
                if (stack == null || stack.Item != item) continue;

                int add = Mathf.Min(count, item.MaxStack - stack.Count);
                stack.Count += add;
                count -= add;
            }
            for (int i = 0; i < _slots.Length && count > 0; i++)
            {
                if (_slots[i] != null) continue;

                int add = Mathf.Min(count, item.MaxStack);
                _slots[i] = new ItemStack(item, add);
                count -= add;
            }

            OnChanged?.Invoke();
            return true;
        }

        public bool Remove(ItemData item, int count = 1)
        {
            if (item == null || count <= 0) return false;
            if (CountOf(item) < count) return false;

            // 뒤쪽 스택부터 비워서 앞쪽 슬롯이 먼저 정리되지 않게 한다
            for (int i = _slots.Length - 1; i >= 0 && count > 0; i--)
            {
                ItemStack stack = _slots[i];
                if (stack == null || stack.Item != item) continue;

                int take = Mathf.Min(count, stack.Count);
                count -= take;
                RemoveFromSlot(i, take);
            }

            OnChanged?.Invoke();
            return true;
        }

        public int CountOf(ItemData item)
        {
            int total = 0;
            foreach (ItemStack stack in _slots)
            {
                if (stack != null && stack.Item == item) total += stack.Count;
            }
            return total;
        }

        public bool TryUse(int slot)
        {
            if (slot < 0 || slot >= _slots.Length) return false;

            ItemStack stack = _slots[slot];
            if (stack == null || !stack.Item.IsUsable) return false;

            stack.Item.Use(gameObject);

            if (stack.Item.ConsumeOnUse)
            {
                RemoveFromSlot(slot, 1);
                OnChanged?.Invoke();
            }
            return true;
        }

        // 버리기가 아니라 폐기다. 아이템은 전부 씬에 미리 배치되므로 월드로 되돌아가지 않는다
        public bool Discard(int slot)
        {
            if (slot < 0 || slot >= _slots.Length || _slots[slot] == null) return false;

            _slots[slot] = null;
            OnChanged?.Invoke();
            return true;
        }

        private int GetFreeSpace(ItemData item)
        {
            int space = 0;
            foreach (ItemStack stack in _slots)
            {
                if (stack == null) space += item.MaxStack;
                else if (stack.Item == item) space += item.MaxStack - stack.Count;
            }
            return space;
        }

        private void RemoveFromSlot(int slot, int count)
        {
            ItemStack stack = _slots[slot];
            stack.Count -= count;
            if (stack.Count <= 0) _slots[slot] = null;
        }

        #region IPlayerData
        // 스냅샷이 살아 있는 인벤토리를 참조하면 이후 변경이 스냅샷에 새어 들어가므로 복사한다
        public void CaptureData(PlayerSnapshot snapshot)
        {
            snapshot.Items = new ItemStack[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null)
                    snapshot.Items[i] = new ItemStack(_slots[i].Item, _slots[i].Count);
            }
        }

        public void RestoreData(PlayerSnapshot snapshot)
        {
            Array.Clear(_slots, 0, _slots.Length);

            if (snapshot.Items != null)
            {
                // 용량이 줄어든 프리팹으로 복원하면 넘치는 칸은 버려진다
                int count = Mathf.Min(snapshot.Items.Length, _slots.Length);
                for (int i = 0; i < count; i++)
                {
                    ItemStack saved = snapshot.Items[i];
                    if (saved == null || saved.Item == null) continue;

                    _slots[i] = new ItemStack(saved.Item, saved.Count);
                }
            }

            OnChanged?.Invoke();
        }
        #endregion
    }
}
