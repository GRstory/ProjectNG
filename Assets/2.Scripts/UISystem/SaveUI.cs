using GRstory.Character;
using GRstory.SaveSystem;
using System;
using UnityEngine;

namespace GRstory.UISystem
{
    public class SaveUI : BaseUI
    {
        [SerializeField] private SaveCardButton[] _cardArray;   // 인덱스 = 슬롯 번호

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < _cardArray.Length; i++)
            {
                int slot = i;
                _cardArray[i].Button.onClick.AddListener(() => HandleCardClicked(slot));
            }
        }

        public override void OnUIActive()
        {
            base.OnUIActive();

            RefreshAllCards();
        }

        private void RefreshAllCards()
        {
            for (int slot = 0; slot < _cardArray.Length; slot++)
            {
                DateTime? savedAt = SaveManager.TryRead(slot, out SaveData data) ? data.SavedAtUtc : null;
                _cardArray[slot].Bind(slot, savedAt);
            }
        }

        private void HandleCardClicked(int slot)
        {
            PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
            if (player == null)
            {
                Debug.LogError("SaveUI: 플레이어가 없어 저장할 수 없음", this);
                return;
            }

            SaveManager.Save(player.gameObject, slot);
            UIManager.Instance.DeactiveUI<SaveUI>();
        }
    }
}
