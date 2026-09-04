using GRstory.Character;
using GRstory.ItemSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GRstory.UISystem
{
    public class InventoryUI : BaseUI
    {
        private const string UseLabel = "사용";
        private const string EquipLabel = "장착";
        private const string UnequipLabel = "장착 해제";

        [SerializeField] private InventorySlot[] _slotArray;
        [SerializeField] private Image _detailIconImage;
        [SerializeField] private TMP_Text _detailNameText;
        [SerializeField] private TMP_Text _detailDescriptionText;
        [SerializeField] private TMP_Text _detailCountText;
        [SerializeField] private DefaultButton _useButton;
        [SerializeField] private TMP_Text _useButtonText;

        private Inventory _inventory;
        private PlayerWeapon _weapon;
        private int _selectedSlot;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < _slotArray.Length; i++)
            {
                int slot = i;
                _slotArray[i].Button.onClick.AddListener(() => Select(slot));
            }
            _useButton.Button.onClick.AddListener(HandleUseClicked);
        }

        public override void OnUIActive()
        {
            base.OnUIActive();

            PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
            if (player == null || !player.TryGetComponent(out _inventory))
            {
                Debug.LogError("InventoryUI: 플레이어 인벤토리를 찾을 수 없음", this);
                return;
            }

            _inventory.OnChanged += Refresh;

            // 장착은 인벤토리 내용을 바꾸지 않아 OnChanged가 안 오므로 따로 듣는다
            if (player.TryGetComponent(out _weapon))
                _weapon.OnWeaponChanged += HandleWeaponChanged;

            _selectedSlot = 0;
            Refresh();
        }

        public override void OnUIDeactive()
        {
            base.OnUIDeactive();

            if (_inventory != null)
            {
                _inventory.OnChanged -= Refresh;
                _inventory = null;
            }
            if (_weapon != null)
            {
                _weapon.OnWeaponChanged -= HandleWeaponChanged;
                _weapon = null;
            }
        }

        private void Select(int slot)
        {
            _selectedSlot = slot;
            for (int i = 0; i < _slotArray.Length; i++)
            {
                _slotArray[i].SetSelected(i == slot);
            }
            RefreshDetail();
        }

        private void HandleUseClicked()
        {
            if (_inventory == null) return;
            _inventory.TryUse(_selectedSlot);
        }

        private void HandleWeaponChanged(WeaponItemData weapon)
        {
            RefreshDetail();
        }

        private void Refresh()
        {
            for (int i = 0; i < _slotArray.Length; i++)
            {
                _slotArray[i].Bind(i, i < _inventory.Capacity ? _inventory.Slots[i] : null);
            }
            Select(_selectedSlot);
        }

        private void RefreshDetail()
        {
            ItemStack stack = _selectedSlot < _slotArray.Length ? _slotArray[_selectedSlot].Stack : null;
            bool hasItem = stack != null;

            _detailIconImage.enabled = hasItem;
            _detailNameText.text = hasItem ? stack.Item.DisplayName : string.Empty;
            _detailDescriptionText.text = hasItem ? stack.Item.Description : string.Empty;
            _detailCountText.text = hasItem ? stack.Count.ToString() : string.Empty;
            _useButton.Button.interactable = hasItem && stack.Item.IsUsable;
            if (_useButtonText != null) _useButtonText.text = GetUseLabel(stack);

            if (hasItem) _detailIconImage.sprite = stack.Item.Icon;
        }

        // 일반 아이템은 "사용", 무기는 "장착", 장착 중인 무기면 "장착 해제"
        private string GetUseLabel(ItemStack stack)
        {
            if (stack == null || stack.Item is not WeaponItemData weapon) return UseLabel;
            return _weapon != null && _weapon.Equipped == weapon ? UnequipLabel : EquipLabel;
        }
    }
}
