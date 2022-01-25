using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UIKit.UI;

namespace UIKit.Modular
{
    public class LabelInput : UIContainer
    {
        public override string TypeName => "labelInput";
        public float InputWidth
        {
            get => Input.Size.Value.X;
            set
            {
                Input.Size = new PercentPos(0, value, 1, -10);
                Input.Pos = new PercentPos(1, -value - 5, 0, 5);
                Label.Size = new PercentPos(1, -value - 15, 1, 0);
            }
        }
        public string InputText { get => Input.Text; set => Input.Text = value; }
        public override string Text { get => Label.Text; set => Label.Text = value; }
        public Action<string> OnInputTextChanged;
        public Regex InputRegex { get => Input.ValidationRegex; set => Input.ValidationRegex = value; }

        public string InputHint { get => Input.HintText; set => Input.HintText = value; }

        public Color InputBackground 
        {
            get => Input.Colors.Background;
            set => Input.Colors = Input.Colors.WithBackground(value);
        }

        public override Colors Colors 
        {
            get => base.Colors;
            set
            {
                base.Colors = value;
                Label.Colors = Label.Colors.WithForeground(value.Foreground);
                Input.Colors = Input.Colors.WithForeground(value.Foreground);
            }
        }

        private UILabel Label;
        private UIInput Input;
        
        public LabelInput() 
        {
            Elements = new ElementCollection(this)
            {
                new UILabel()
                {
                    Name = "label",
                    AutoSize = false,
                    Size = new PercentPos(1, -115, 1, 0),
                    TextAlign = Align.Left,
                    Colors = new Colors(Color.White, Color.Transparent),
                    Pos = new PercentPos(5,0),
                },
                new UIInput()
                {
                    Name = "input",
                    OnTextChanged = (i) => OnInputTextChanged?.Invoke(Input.Text),
                    Size = new PercentPos(0, 100, 1, -10),
                    Pos = new PercentPos(1, -105, 0, 5),
                    TextAlign = Align.Center
                }
            };

            Label = GetLocalElement<UILabel>();
            Input = GetLocalElement<UIInput>();
        }

        public override void UpdateSelf()
        {
            //Label.Size = new(1, -115, 1, 0);
            base.UpdateSelf();
        }
    }
}
