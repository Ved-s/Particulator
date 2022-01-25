using System;
using System.Collections.Generic;
using System.Text;
using UIKit.UI;

namespace UIKit.UI
{
    public class UISwitchButton : UIButton
    {
        public List<string> SwitchText { get; set; } = new List<string>();
        public int SwitchState { get; set; }

        public override string Text { get => SwitchText[SwitchState]; }

        public override void MouseKeyUpdate(MouseKeys key, EventType type)
        {
            if (key == MouseKeys.Left && type == EventType.Presssed) 
            {
                SwitchState = (SwitchState + 1) % SwitchText.Count; 
            }
            base.MouseKeyUpdate(key, type);
        }
    }
}
