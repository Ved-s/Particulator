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
    public class ColorRange : UIContainer
    {

        UILabel Header;
        UIInput RMin, RMax, GMin, GMax, BMin, BMax, AMin, AMax;

        public Color Min
        {
            get => _min;
            set 
            {
                _min = value;
                RMin.Text = value.R.ToString();
                GMin.Text = value.G.ToString();
                BMin.Text = value.B.ToString();
                AMin.Text = value.A.ToString();
            }
        }
        public Color Max
        {
            get => _max;
            set
            {
                _max = value;
                RMax.Text = value.R.ToString();
                GMax.Text = value.G.ToString();
                BMax.Text = value.B.ToString();
                AMax.Text = value.A.ToString();
            }
        }

        public override string Text { get => Header.Text; set => Header.Text = value; }

        public bool NoAlpha 
        {
            get => _noAlpha;
            set 
            {
                _noAlpha = value;
                AMin.Visible = !value;
                AMax.Visible = !value;
                GetLocalElement<UILabel>("a").Visible = !value;
            }
        }

        private bool _noAlpha;

        public Action<ColorRange> OnRangeChanged;

        private Color _min, _max;

        public ColorRange()
        {
            Regex regex = new Regex("^[0-9]{0,3}$");

            Size = new PercentPos(160, 160);
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
                    Text = "Color Range"
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
                    Name = "minr",
                    Pos = new PercentPos(10, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex,
                },
                new UIInput()
                {
                    Name = "ming",
                    Pos = new PercentPos(10, 68),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UIInput()
                {
                    Name = "minb",
                    Pos = new PercentPos(10, 96),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex,
                },
                new UIInput()
                {
                    Name = "mina",
                    Pos = new PercentPos(10, 124),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
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
                    Name = "maxr",
                    Pos = new PercentPos(0.5f, 10, 0, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UIInput()
                {
                    Name = "maxg",
                    Pos = new PercentPos(0.5f, 10, 0, 68),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UIInput()
                {
                    Name = "maxb",
                    Pos = new PercentPos(0.5f, 10, 0, 96),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UIInput()
                {
                    Name = "maxa",
                    Pos = new PercentPos(0.5f, 10, 0, 124),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UILabel()
                {
                    Name = "r",
                    Text = "R",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 40),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                },
                new UILabel()
                {
                    Name = "g",
                    Text = "G",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 68),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                },
                new UILabel()
                {
                    Name = "b",
                    Text = "B",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 96),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                },
                new UILabel()
                {
                    Name = "a",
                    Text = "A",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 124),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                }
            };

            Header = GetLocalElement<UILabel>("header");
            RMin = GetLocalElement<UIInput>("minr");
            GMin = GetLocalElement<UIInput>("ming");
            BMin = GetLocalElement<UIInput>("minb");
            AMin = GetLocalElement<UIInput>("mina");
            RMax = GetLocalElement<UIInput>("maxr");
            GMax = GetLocalElement<UIInput>("maxg");
            BMax = GetLocalElement<UIInput>("maxb");
            AMax = GetLocalElement<UIInput>("maxa");

            RMin.OnTextChanged = (i) => ValueChanged(i, true, 0);
            GMin.OnTextChanged = (i) => ValueChanged(i, true, 1);
            BMin.OnTextChanged = (i) => ValueChanged(i, true, 2);
            AMin.OnTextChanged = (i) => ValueChanged(i, true, 3);

            RMax.OnTextChanged = (i) => ValueChanged(i, false, 0);
            GMax.OnTextChanged = (i) => ValueChanged(i, false, 1);
            BMax.OnTextChanged = (i) => ValueChanged(i, false, 2);
            AMax.OnTextChanged = (i) => ValueChanged(i, false, 3);
        }

        private void ValueChanged(UIInput i, bool min, int type) 
        {
            if (!int.TryParse(i.Text, out int v)) return;

            if (v < 0) { v = 0; i.Text = "0"; }
            else if (v > 255) { v = 255; i.Text = "255"; }

            byte b = (byte)v;

            switch (type) 
            {
                case 0 when min: _min.R = b; break;
                case 0:          _max.R = b; break;
                case 1 when min: _min.G = b; break;
                case 1:          _max.G = b; break;
                case 2 when min: _min.B = b; break;
                case 2:          _max.B = b; break;
                case 3 when min: _min.A = b; break;
                case 3:          _max.A = b; break;
            }
            OnRangeChanged?.Invoke(this);
        }
    }
}
