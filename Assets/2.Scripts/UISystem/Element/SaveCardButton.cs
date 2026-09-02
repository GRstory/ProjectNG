using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GRstory.UISystem
{
    public class SaveCardButton : DefaultButton
    {
        [SerializeField] private Image _chapterImage;
        [SerializeField] private TMP_Text _slotText;
        [SerializeField] private TMP_Text _chapterNameText;
        [SerializeField] private TMP_Text _saveDateTimeText;

        public int Slot { get; private set; }

        // savedAtUtc가 null이면 빈 슬롯
        public void Bind(int slot, DateTime? savedAtUtc)
        {
            Slot = slot;
            _slotText.text = $"Slot {slot + 1}";
            _saveDateTimeText.text = savedAtUtc.HasValue
                ? savedAtUtc.Value.ToLocalTime().ToString("yyyy.MM.dd HH:mm")
                : "Empty";
        }
    }
}
