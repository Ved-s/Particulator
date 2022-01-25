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
    public class XYRange : UIContainer
    {

        UILabel Header;
        UIInput XMin, XMax, YMin, YMax;

        public Vec2f Min
        {
            get => _min;
            set 
            {
                _min = value;
                XMin.Text = value.X.ToString("0.00", NumberFormatInfo.InvariantInfo);
                YMin.Text = value.Y.ToString("0.00", NumberFormatInfo.InvariantInfo);
            }
        }
        public Vec2f Max
        {
            get => _max;
            set
            {
                _max = value;
                XMax.Text = value.X.ToString("0.00", NumberFormatInfo.InvariantInfo);
                YMax.Text = value.Y.ToString("0.00", NumberFormatInfo.InvariantInfo);
            }
        }

        public override string Text { get => Header.Text; set => Header.Text = value; }

        public Action<XYRange> OnRangeChanged;

        private Vec2f _min, _max;

        public XYRange() 
        {
            Regex regex = new Regex("^-?[0-9]*(\\.[0-9]*)?$");

            Size = new PercentPos(160, 100);
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
                    Text = "XY Range"
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
                    Name = "minx",
                    Pos = new PercentPos(10, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex,
                },
                new UIInput()
                {
                    Name = "miny",
                    Pos = new PercentPos(10, 68),
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
                    Name = "maxx",
                    Pos = new PercentPos(0.5f, 10, 0, 40),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UIInput()
                {
                    Name = "maxy",
                    Pos = new PercentPos(0.5f, 10, 0, 68),
                    Size = new PercentPos(0.5f, -20, 0, 24),
                    TextAlign = Align.Center,
                    ValidationRegex = regex
                },
                new UILabel()
                {
                    Name = "x",
                    Text = "X",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 40),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                },
                new UILabel()
                {
                    Name = "y",
                    Text = "Y",
                    AutoSize = false,
                    Pos = new PercentPos(0.5f, -10, 0, 68),
                    Size = new PercentPos(20, 24),
                    Colors = new Colors(Color.White, Color.Transparent),
                    TextAlign = Align.Center
                }
            };

            Header = GetLocalElement<UILabel>("header");
            XMin = GetLocalElement<UIInput>("minx");
            XMax = GetLocalElement<UIInput>("maxx");
            YMin = GetLocalElement<UIInput>("miny");
            YMax = GetLocalElement<UIInput>("maxy");

            XMin.OnTextChanged = (i) =>
            {
                if (!float.TryParse(i.Text, NumberStyles.Float, NumberFormatInfo.InvariantInfo,  out float v)) return;
                _min.X = v;
                OnRangeChanged?.Invoke(this);
            };
            YMin.OnTextChanged = (i) =>
            {
                if (!float.TryParse(i.Text, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float v)) return;
                _min.Y = v;
                OnRangeChanged?.Invoke(this);
            };
            XMax.OnTextChanged = (i) =>
            {
                if (!float.TryParse(i.Text, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float v)) return;
                _max.X = v;
                OnRangeChanged?.Invoke(this);
            };
            YMax.OnTextChanged = (i) =>
            {
                if (!float.TryParse(i.Text, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float v)) return;
                _max.Y = v;
                OnRangeChanged?.Invoke(this);
            };
        }
    }
}
