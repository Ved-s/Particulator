using System;
using System.Collections.Generic;
using System.Text;

namespace UIKit.UI
{
    public class UIRadioButton : UIButton
    {
        public override Colors Colors { get => Checked ? CheckedColors ?? base.Colors.SwapBF() : base.Colors; set => base.Colors = value; }
        public override Colors? HoverColors { get => Checked ? CheckedColors ?? base.HoverColors?.SwapBF() : base.HoverColors; set => base.HoverColors = value; }

        public Colors? CheckedColors { get; set; }
        public bool SuppressChangeEvent { get; set; }

        private bool CheckedInternal;
        public bool Checked 
        {
            get => CheckedInternal;
            set 
            {
                if (value == CheckedInternal) return;
                if (value) 
                {
                    CheckedInternal = true;
                    SetChecked();
                }
            } 
        }

        public string RadioGroupName { get; set; } = "radio";
        public object RadioState { get; set; }

        public RadioCheckedChangedDelegate OnRadioCheckedChanged;


        public override void MouseKeyUpdate(MouseKeys key, EventType @event)
        {
            base.MouseKeyUpdate(key, @event);
            if (key == MouseKeys.Left && @event == EventType.Presssed)
            {
                Checked = true;
            }
        }

        private void SetChecked() 
        {
            UIRadioButton oldRadio = null;

            foreach (UIElement element in Parent.Elements)
                if (element is UIRadioButton radio
                    && radio.RadioGroupName == RadioGroupName
                    && radio != this
                    && radio.Checked) 
                {
                    oldRadio = radio;
                }

            if (oldRadio is null) return;

            oldRadio.CheckedInternal = false;

            if (SuppressChangeEvent) return;

            foreach (UIElement element in Parent.Elements)
                if (element is UIRadioButton radio
                    && radio.RadioGroupName == RadioGroupName) 
                {
                    radio.OnRadioCheckedChanged?.Invoke(RadioState, oldRadio, this);
                }
        }

        public delegate void RadioCheckedChangedDelegate(object state, UIRadioButton old, UIRadioButton @new);
    }
}
