using System;

namespace GRstory.ItemSystem
{
    [Serializable]
    public class ItemStack
    {
        public ItemData Item;
        public int Count;

        public ItemStack(ItemData item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
