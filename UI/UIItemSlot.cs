using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit.UI;
using UIKit;
using Microsoft.Xna.Framework;

namespace Particulator.UI
{
    public class UIItemSlot : UIElement
    {
        public override string TypeName => "itemslot";

        public Item Item { get => item; set { item = value; fakeInv[slot] = value; } }
        public int Slot  
        {
            get => slot; 
            set 
            {
                slot = value;
                if (slot >= fakeInv.Length) slot = fakeInv.Length - 1;
                fakeInv[slot] = item;
            } 
        }
        public int Context { get; set; } = 0;
        public bool TakeItemFromCursor { get; set; } = true;

        private Item[] fakeInv = new Item[11];
        private int slot;
        private Item item = new Item();

        public override PercentPos Size => new PercentPos(52, 52);
        public Func<Item, bool> ItemFilter;
        public Action<UIItemSlot> OnItemChanged;

        public UIItemSlot() 
        {
            for (int i = 0; i < 11; i++)
                fakeInv[i] = new Item();
            Slot = 10;
            Context = Terraria.UI.ItemSlot.Context.InventoryItem;
            Colors = new Colors(Color.Transparent, Color.Transparent);
        }

        public override void DrawSelf(TransformedGraphics graphics)
        {
            float scale = Main.inventoryScale;
            Main.inventoryScale = graphics.Transform.Scale;
            Terraria.UI.ItemSlot.Draw(graphics.SpriteBatch, fakeInv, Context, slot, AbsoluteBounds.Position);
            Main.inventoryScale = scale;
            if (Hover && !item.IsAir)
            {
                Terraria.UI.ItemSlot.MouseHover(fakeInv, Context, slot);
            }

        }

        public override void UpdateSelf()
        {
            
        }

        public override void MouseKeyUpdate(MouseKeys key, EventType type)
        {
            base.MouseKeyUpdate(key, type);
            if (key == MouseKeys.Left && type == EventType.Presssed) 
            {
                Item cursor = Main.mouseItem.Clone();
                if (cursor.IsAir && Item.IsAir) return;

                if (cursor.IsAir && !Item.IsAir) 
                {
                    Item = new Item();
                    Main.PlaySound(Terraria.ID.SoundID.Grab);
                    OnItemChanged?.Invoke(this);
                    return;
                }
                if (ItemFilter != null && !ItemFilter(cursor)) return;

                if (TakeItemFromCursor) 
                {
                    Main.mouseItem = Item.Clone();
                    cursor.favorited = false;
                    Item = cursor;
                }
                else
                {
                    cursor.stack = 1;
                    cursor.favorited = false;
                    Item = cursor;
                }
                Main.PlaySound(Terraria.ID.SoundID.Grab);
                OnItemChanged?.Invoke(this);
            }
        }
    }
}
