using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit.UI;

namespace UIKit
{
    public static class Ext
    {
        public static Vec2f CalculateAlign(this Align align, Vec2f containerSize, Vec2f objectSize) 
        {
            Vec2f sdiff = containerSize - objectSize;

            switch (align)
            {
                case Align.TopLeft:  return Vec2f.Zero;
                case Align.Top:      return new Vec2f(sdiff.X / 2, 0);
                case Align.TopRight: return new Vec2f(sdiff.X, 0);

                case Align.Left   : return new Vec2f(0, sdiff.Y / 2);
                case Align.Center : return sdiff / 2;
                case Align.Right  : return new Vec2f(sdiff.X, sdiff.Y / 2);

                case Align.BottomLeft  : return new Vec2f(0, sdiff.Y);
                case Align.Bottom      : return new Vec2f(sdiff.X / 2, sdiff.Y);
                case Align.BottomRight : return sdiff;
            }
            return Vec2f.Zero;
        }

        public static bool Shift(this KeyboardState state) => state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
        public static bool Ctrl(this KeyboardState state) => state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl);
        public static bool Alt(this KeyboardState state) => state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt);

        public static bool isNullEmptyOrWhitespace(this string str) => string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }
}
