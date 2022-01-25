using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIKit.UI
{
    public class UICheckButton : UIButton
    {
        public override Colors Colors { get => Checked ? CheckedColors ?? base.Colors.SwapBF() : base.Colors; set => base.Colors = value; }
        public override Colors? HoverColors { get => Checked ? CheckedColors ?? base.HoverColors?.SwapBF() : base.HoverColors; set => base.HoverColors = value; }

        public Colors? CheckedColors { get; set; }

        public virtual bool Checked { get; set; }

        public Action<UICheckButton> OnCheckedChanged;

        public override void MouseKeyUpdate(MouseKeys key, EventType @event)
        {
            if (key == MouseKeys.Left && @event == EventType.Presssed)
            {
                Checked = !Checked;
                OnCheckedChanged?.Invoke(this);
            }
            base.MouseKeyUpdate(key, @event);
        }
    }
}
