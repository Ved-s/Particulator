using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIKit.UI
{
    public class UILabel : UIElement
    {
        public override string TypeName => "label";

        public Align TextAlign { get; set; } = Align.TopLeft;
        public bool AutoSize { get; set; } = true;

        public Color? TextShade { get; set; }
        public float TextScale { get; set; } = 1f;

        private int? TextHash;
        private Vec2f TextSize;

        public override void DrawSelf(TransformedGraphics graphics)
        {
            graphics.Transform.Scale *= TextScale;
            Rect bounds = Bounds;
            bounds.Size /= TextScale;
            if (Text != null) 
                graphics.DrawText(Font, Text, bounds, TextAlign, Colors.Foreground, TextShade);
        }

        public override void UpdateSelf()
        {
            int? hash = Text?.GetHashCode();
            if (hash != TextHash && Font != null)
            {
                TextSize = (Text == null) ? Vec2f.Zero : (Vec2f)Font.MeasureString(Text);
                TextHash = hash;
            }

            if (AutoSize)
            {
                SetSize(TextSize);
            }
        }

        
    }
}
