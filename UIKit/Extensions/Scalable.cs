using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public class Scalable : UIExtension
    {
        public override string ShortId => "scale";

        public Keys ScaleKey { get; set; } = Keys.LeftControl;
        public float ScaleValue { get; set; } = 1.1f;

        public override void MouseWheelUpdate(UIElement element, int amount) 
        {
            if (element.Root.CurrentKeys.IsKeyUp(ScaleKey)) return;

            float scdiff = amount * ScaleValue;

            if (scdiff >= 0)
                element.Scale /= scdiff;
            else 
                element.Scale *= -scdiff;
        }
    }
}
