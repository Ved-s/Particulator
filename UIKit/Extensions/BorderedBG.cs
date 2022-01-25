using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public class BorderedBG : UIExtension
    {
        public override string ShortId => "borderBG";

        public int CornerSize { get; set; } = 12;

        public UITexture Background { get; set; }
        public UITexture Border     { get; set; }

        public BorderedBG(UITexture background = null, UITexture border = null) 
        {
            Background = background;
            Border = border;
        }

        public override bool DrawBackground(UIElement element, TransformedGraphics graphics, Color color)
        {
            Texture2D tex = Background?.GetValue(element.Root);

            if (tex == null)
                return false;
            Draw(element, tex, graphics, color);

            return true;
        }

        public override bool DrawOutline(UIElement element, TransformedGraphics graphics, Color color)
        {
            Texture2D tex = Border?.GetValue(element.Root);

            if (tex == null)
                return false;
            Draw(element, tex, graphics, color);

            return true;
        }

        private void Draw(UIElement element, Texture2D tex, TransformedGraphics graphics, Color color)
        {
            Vec2f corner = new Vec2f(CornerSize);

            Vec2f size = element.Bounds.Size;
            Vec2f bend = size - corner;
            Vec2f bsize = bend - corner;

            Vec2f tsize = new Vec2f(tex.Bounds.Width, tex.Bounds.Height);
            Vec2f tbend = tsize - corner;
            Vec2f tbsize = tbend - corner;

            graphics.SetBatch(samplerState: SamplerState.PointWrap);

            bool scaling = true;

            graphics.DrawTexture(tex, new Rect(Vec2f.Zero, corner), color,     new Rect(Vec2f.Zero, corner), scaling);
            graphics.DrawTexture(tex, new Rect(bend.X, 0, corner), color,      new Rect(tbend.X, 0, corner), scaling);
            graphics.DrawTexture(tex, new Rect(0, bend.Y, corner), color,      new Rect(0, tbend.Y, corner), scaling);
            graphics.DrawTexture(tex, new Rect(bend.X, bend.Y, corner), color, new Rect(tbend.X, tbend.Y, corner), scaling);
            
            graphics.DrawTexture(tex, new Rect(corner.X, 0, bsize.X, corner.Y), color,      new Rect(corner.X, 0, tbsize.X, corner.Y), scaling);
            graphics.DrawTexture(tex, new Rect(0, corner.Y, corner.X, bsize.Y), color,      new Rect(0, corner.Y, corner.X, tbsize.Y), scaling);
            graphics.DrawTexture(tex, new Rect(corner.X, bend.Y, bsize.X, corner.Y), color, new Rect(corner.X, tbend.Y, tbsize.X, corner.Y), scaling);
            graphics.DrawTexture(tex, new Rect(bend.X, corner.Y, corner.X, bsize.Y), color, new Rect(tbend.X, corner.Y, corner.X, tbsize.Y), scaling);
            
            graphics.DrawTexture(tex, new Rect(corner, bsize), color, new Rect(corner, tbsize), scaling);

            graphics.SpriteBatch.End();
            graphics.SpriteBatch.Begin();
        }
    }
}
