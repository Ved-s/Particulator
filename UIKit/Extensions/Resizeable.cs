using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public class Resizeable : UIExtension
    {
        public override string ShortId => "resize";

        public int GrabRange { get; set; } = 10;
        public MouseKeys Key { get; set; } = MouseKeys.Left;

        public bool AllowTopResize { get; set; }
        public bool AllowLeftResize { get; set; }
        public bool AllowBottomResize { get; set; } = true;
        public bool AllowRightResize { get; set; } = true;

        public bool Dragging;

        bool top, right, bottom, left;
        float fadeTop, fadeRight, fadeBottom, fadeLeft;
        Vec2f mouse;

        const float fade = 0.1f;

        public override void MouseKeyUpdate(UIElement element, MouseKeys key, EventType type)
        {
            if (key != Key) return;

            Vec2f m = element.TransformedMousePosition;
            Vec2f s = element.Bounds.Size;

            if (type == EventType.Presssed)
            {
                mouse = m;

                top    = AllowTopResize && m.Y > 0 && m.Y < GrabRange;
                right  = AllowRightResize && m.X > s.X - GrabRange && m.X < s.X;
                bottom = AllowBottomResize && m.Y > s.Y - GrabRange && m.Y < s.Y;
                left   = AllowLeftResize && m.X < GrabRange && m.X > 0;

                if (top && bottom) top = false;
                if (right && left) left = false;

                Dragging = top || right || bottom || left;
            }
            else if (type == EventType.Hold)
            {
                Vec2f diff = m - mouse;

                Vec2f size = new Vec2f();
                Vec2f pos = new Vec2f();

                if (top)    { fadeTop = 1;    pos.Y += diff.Y; size.Y -= diff.Y; }
                if (right)  { fadeRight = 1;  size.X += diff.X; }
                if (bottom) { fadeBottom = 1; size.Y += diff.Y; }
                if (left)   { fadeLeft = 1;   pos.X += diff.X; size.X -= diff.X; }

                element.AddSize(size);
                element.AddPos(pos * element.Scale);
                element.RecalculateAllTransforms();
                mouse = element.TransformedMousePosition;
            }
            else 
            {
                Dragging = top = right = bottom = left = false;
            }
        }

        public override void Draw(UIElement element, TransformedGraphics graphics)
        {
            Vec2f s = element.Bounds.Size;

            if (fadeTop > 0)
            {
                fadeTop -= fade;
                graphics.FillRect(new Rect(0, 0, s.X, GrabRange), Color.White * 0.5f * fadeTop);  
            }
            if (fadeRight > 0) 
            { 
                fadeRight -= fade;
                graphics.FillRect(new Rect(s.X - GrabRange, 0, GrabRange, s.Y), Color.White * 0.5f * fadeRight);  
            }
            if (fadeBottom > 0) 
            {
                fadeBottom -= fade;
                graphics.FillRect(new Rect(0, s.Y - GrabRange, s.X, GrabRange), Color.White * 0.5f * fadeBottom); 
            }
            if (fadeLeft > 0) 
            {
                fadeLeft -= fade;
                graphics.FillRect(new Rect(0, 0, GrabRange, s.Y), Color.White * 0.5f * fadeLeft);
            }
        }
    }
}
