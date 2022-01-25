using Microsoft.Xna.Framework;
using System;

namespace UIKit.UI
{
    public class UIButton : UIElement
    {
        public override string TypeName => "button";

        public Action<UIButton> OnClick;

        public Align TextAlign { get; set; } = Align.Center;

        public virtual Colors? HoverColors { get; set; } = null;
        public virtual Colors? ClickColors { get; set; } = null;

        bool Clicked;

        public override void DrawSelf(TransformedGraphics graphics)
        {
            Colors c = Colors;

            if (Clicked)
            {
                c = Colors.FirstColors(ClickColors, Colors);
                DrawBackground(graphics, c.Background);
                DrawOutline(graphics, c.Outline);
            }
            else if (Hover)
            {
                c = Colors.FirstColors(HoverColors, Colors);
                DrawBackground(graphics, c.Background);
                DrawOutline(graphics, c.Outline);
            }
            else 
            {
                DrawBackground(graphics, c.Background);
                DrawOutline(graphics, c.Outline);
            }

            graphics.DrawText(Font, Text, Bounds, TextAlign, c.Foreground);
        }

        public override void MouseKeyUpdate(MouseKeys key, EventType type)
        {
            base.MouseKeyUpdate(key, type);
            if (key == MouseKeys.Left) 
            {
                if (type == EventType.Presssed) 
                {
                    OnClick?.Invoke(this);
                    Clicked = true;
                }
                else if (type == EventType.Released)
                {
                    Clicked = false;
                }
            }
        }
    }
}
