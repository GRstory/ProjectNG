using GRstory.ItemSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GRstory.UISystem
{
    public class InventorySlot : DefaultButton
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Image _selectedImage;

        public int Slot { get; private set; }
        public ItemStack Stack { get; private set; }

        public void Bind(int slot, ItemStack stack)
        {
            Slot = slot;
            Stack = stack;

            bool hasItem = stack != null;
            _iconImage.enabled = hasItem;
            _countText.enabled = hasItem && stack.Count > 1;

            if (!hasItem) return;
            _iconImage.sprite = stack.Item.Icon;
            _countText.text = stack.Count.ToString();
        }

        public void SetSelected(bool selected)
        {
            _selectedImage.enabled = selected;
        }
    }
}
