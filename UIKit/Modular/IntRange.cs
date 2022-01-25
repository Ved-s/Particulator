using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UIKit.UI;

namespace UIKit.Modular
{
    public class IntRange : UIContainer
    {

        UILabel Header;
        UIInput VMin, VMax;

        public int Min
        {
            get => _min;
            set 
            {
                _min = value;
                VMin.Text = value.ToString();
            }
        }
        public int Max
        {
            get => _max;
            set
            {
                _max = value;
                VMax.Text = value.ToString();
            }
        }

        public override string Text { get => Header.Text; set => Header.Text = value; }

        public Action<IntRange> OnRangeChanged;

        public bool AllowNegative 
        {
            get => _allowNegative;
            set 
            {
                _allowNegative = value;
                Regex r = _allowNegative ? new Regex(@"^-?[0-9]*$", RegexOptions.Compiled) : new Regex("^[0-9]*$");

                VMin.ValidationRegex = r;
                VMax.ValidationRegex = r;
            }
        }
        
        private bool _allowNegative;
        private int _min, _max;

        public IntRange() 
        {
            Regex regex = new Regex("^[0-9]*$");

            Size = new PercentPos(160, 70);
            Elements = new ElementCollection(this)
            {
                new UILabel()
                {
                    AutoSize = false,
                    Name = "header",
                    Pos = new PercentPos(0,3),
                    Size = new PercentPos(1, 0, 0, 20),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center,
                    Text = "Float Range"
                },
                new UILabel()
                {
                    Name = "min",
                    Text = "Min",
                    AutoSize = false,
                    Pos = new PercentPos(10, 25),
                    Size = new PercentPos(0.5f, -20, 0, 15),
                    TextAlign = Align.Center,
                    Colors = new Colors(Color.White, Color.Transparent),
                },
                new UIInput()
                {
                    Name = "minv",
                    Pos = new PercentPos(10, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex,
                },
                new UILabel()
                {
                    Name = "max",
                    Text = "Max",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, 10, 0, 25),
                    Size = new PercentPos(0.5f, -20, 0, 15),
                    TextAlign = Align.Center,
                    Colors = new Colors(Color.White, Color.Transparent),
                },
                new UIInput()
                {
                    Name = "maxv",
                    Pos = new PercentPos(0.5f, 10, 0, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                }
            };

            Header = GetLocalElement<UILabel>("header");
            VMin = GetLocalElement<UIInput>("minv");
            VMax = GetLocalElement<UIInput>("maxv");

            VMin.OnTextChanged = (i) =>
            {
                if (!int.TryParse(i.Text, out int v)) return;
                _min = v;
                OnRangeChanged?.Invoke(this);
            };
            VMax.OnTextChanged = (i) =>
            {
                if (!int.TryParse(i.Text, out int v)) return;
                _max = v;
                OnRangeChanged?.Invoke(this);
            };
        }
    }
}
