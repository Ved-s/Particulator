using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace UIKit.UI
{
    public class UIList : UIElement
    {
        public override string TypeName => "list";

        public List<string> Items { get; } = new List<string>();

        public int Scroll { get; set; } = 0;
        public int Selected { get => _Selected; set { _Selected = value; OnSelectedChanged?.Invoke(this); } }
        public int ItemHeight { get; set; } = 20;

        public Colors ItemHover { get; set; } = new Colors(Color.White, Color.White * 0.1f);
        public Colors ItemSelected { get; set; } = new Colors(Color.White, Color.White * 0.2f);

        public Align TextAlign { get; set; } = Align.TopLeft;

        public Action<UIList> OnSelectedChanged;

        private int MaxVisibleItems => (int)Bounds.Height / ItemHeight;
        private int MaxScroll => Math.Max(0, Items.Count - MaxVisibleItems);
        private int _Selected = -1;

        public override void DrawSelf(TransformedGraphics graphics)
        {
            int ypos = 0;
            for (int i = Scroll; i < Math.Min(Items.Count, Scroll + MaxVisibleItems); i++) 
            {
                Rect rect = new Rect(0, ypos, Bounds.Width - 5, ItemHeight);
                Color tc = Colors.Foreground;

                if (Selected == i)
                {
                    tc = ItemSelected.Foreground;
                    graphics.FillRect(rect, ItemSelected.Background);
                    graphics.DrawRect(rect, ItemSelected.Outline);
                }
                else if (rect.Contains(TransformedMousePosition)) 
                {
                    tc = ItemHover.Foreground;
                    graphics.FillRect(rect, ItemHover.Background);
                    graphics.DrawRect(rect, ItemHover.Outline);
                }
                graphics.DrawText(Font, Items[i], rect, TextAlign, tc, Color.Black);
                ypos += ItemHeight;
            }

            if (Items.Count > MaxVisibleItems)
            {
                graphics.FillRect(new Rect(Bounds.Width - 5, 0, 5, Bounds.Height), Color.Gray * 0.5f);

                float scroll = (float)Scroll / MaxScroll;
                float height = (float)MaxVisibleItems / Items.Count * Bounds.Height;

                graphics.FillRect(new Rect(Bounds.Width - 5, scroll * (Bounds.Height - height), 5, height), Color.LightGray * 0.5f);
            }
        }

        public override void MouseKeyUpdate(MouseKeys key, EventType type)
        {
            base.MouseKeyUpdate(key, type);
            if (key == MouseKeys.Left && type == EventType.Presssed) 
            {
                int index = Scroll + ((int)TransformedMousePosition.Y / ItemHeight);
                if (index >= Items.Count) return;

                Selected = index;
            }
        }

        public override void KeyUpdate(Keys key, EventType type)
        {
            base.KeyUpdate(key, type);
            if (!Active) return;
            if (type == EventType.Presssed) 
            {
                if (key == Keys.Up) 
                {
                    Selected--;
                    if (Selected < 0)
                    {
                        Selected = Items.Count - 1;
                        Scroll = MaxScroll;
                    }
                    else if (Selected == Scroll - 1) 
                    {
                        Scroll--;
                        FixScroll();
                    }
                }
                else if (key == Keys.Down)
                {
                    Selected++;
                    if (Selected >= Items.Count)
                    {
                        Selected = 0;
                        Scroll = 0;
                    }
                    else if (Selected == Scroll + MaxVisibleItems)
                    {
                        Scroll++;
                        FixScroll();
                    }
                }
            }
        }

        public override void MouseWheelUpdate(int amount)
        {
            Scroll += amount;
            FixScroll();
        }

        public override void UpdateSelf()
        {
            FixScroll();
        }

        private void FixScroll()
        {
            if (Scroll < 0) Scroll = 0;
            else if (Scroll > MaxScroll) Scroll = MaxScroll;
        }
    }
}
