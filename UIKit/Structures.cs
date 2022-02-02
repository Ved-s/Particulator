using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using System.Text;
using UIKit.UI;

namespace UIKit
{
    public struct Angle
    {
        public float Radians { get; set; }
        public float Degrees
        {
            get => Radians / (float)Math.PI * 180f;
            set => Radians = value / 180f * (float)Math.PI;
        }

        public static Angle Zero { get => new Angle { Radians = 0f }; }

        public static Angle FromRadians(float rad) => new Angle() { Radians = rad };
        public static Angle FromDegrees(float deg) => new Angle() { Degrees = deg };

        public static Angle operator +(Angle a, Angle b) => new Angle() { Radians = a.Radians + b.Radians };
        public static Angle operator -(Angle a, Angle b) => new Angle() { Radians = a.Radians - b.Radians };

        public static Angle operator -(Angle a) => new Angle() { Radians = -a.Radians };

        public override string ToString()
        {
            return $"{Degrees.ToString("0.00", NumberFormatInfo.InvariantInfo)}°";
        }
    }
    public struct Vec2f
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2f(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vec2f(float xy)
        {
            X = xy;
            Y = xy;
        }
        public Vec2f(Vec2f vec, float addX = 0, float addY = 0)
        {
            X = vec.X + addX;
            Y = vec.Y + addY;
        }
        public Vec2f(float length, Angle angle)
        {
            X = length * (float)Math.Cos(angle.Radians);
            Y = length * (float)Math.Sin(angle.Radians);
        }

        public float Length
        {
            get => (float)Math.Sqrt(X * X + Y * Y);
            set
            {
                float angle = Angle.Radians;
                X = value * (float)Math.Cos(angle);
                Y = value * (float)Math.Sin(angle);
            }
        }
        public Angle Angle
        {
            get => Angle.FromRadians((float)Math.Atan2(Y, X));
            set
            {
                float length = Length;
                X = length * (float)Math.Cos(value.Radians);
                Y = length * (float)Math.Sin(value.Radians);
            }
        }

        public static Vec2f Zero { get => new Vec2f(0, 0); }

        public static Vec2f operator /(Vec2f a, float v) => new Vec2f(a.X / v, a.Y / v);
        public static Vec2f operator *(Vec2f a, float v) => new Vec2f(a.X * v, a.Y * v);

        public static Vec2f operator /(Vec2f a, Vec2f b) => new Vec2f(a.X / b.X, a.Y / b.Y);
        public static Vec2f operator *(Vec2f a, Vec2f b) => new Vec2f(a.X * b.X, a.Y * b.Y);

        public static Vec2f operator +(Vec2f a, Vec2f b) => new Vec2f(a.X + b.X, a.Y + b.Y);
        public static Vec2f operator -(Vec2f a, Vec2f b) => new Vec2f(a.X - b.X, a.Y - b.Y);

        public static Vec2f operator +(Vec2f a, Vector2 b) => new Vec2f(a.X + b.X, a.Y + b.Y);
        public static Vec2f operator -(Vec2f a, Vector2 b) => new Vec2f(a.X - b.X, a.Y - b.Y);

        public static Vec2f operator +(Vec2f a, Point b) => new Vec2f(a.X + b.X, a.Y + b.Y);
        public static Vec2f operator -(Vec2f a, Point b) => new Vec2f(a.X - b.X, a.Y - b.Y);

        public static Vec2f operator -(Vec2f a) => new Vec2f(-a.X, -a.Y);

        public static implicit operator Vector2(Vec2f v) => new Vector2(v.X, v.Y);
        public static explicit operator Point(Vec2f v) => new Point((int)v.X, (int)v.Y);

        public static implicit operator Vec2f(Point v) => new Vec2f(v.X, v.Y);
        public static explicit operator Vec2f(Vector2 v) => new Vec2f(v.X, v.Y);

        public Vec2f ApplyTransform(Transform transform)
        {
            Vec2f v = new Vec2f(X, Y);
            v.Angle += transform.Angle;
            v *= transform.Scale;
            v += transform.Offset;
            return v;
        }

        public Vec2f ApplyTransformBack(Transform transform)
        {
            Vec2f v = new Vec2f(X, Y);

            v -= transform.Offset;
            v /= transform.Scale;
            v.Angle -= transform.Angle;
            return v;
        }

        public Vec2f Rotate(Angle angle, Vec2f origin)
        {
            Vec2f v = this - origin;
            v.Angle += angle;
            return v + origin;
        }

        public override string ToString()
        {
            return $"{X.ToString(NumberFormatInfo.InvariantInfo)}, {Y.ToString(NumberFormatInfo.InvariantInfo)}";
        }
    }
    public struct Rect
    {
        static NumberFormatInfo dotnfi = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public Angle Angle;
        public float X, Y, Width, Height;

        public Rect(float x, float y, float width, float height, Angle angle = default)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Angle = angle;
        }
        public Rect(float x, float y, Vec2f size, Angle angle = default)
        {
            X = x;
            Y = y;
            Width = size.X;
            Height = size.Y;
            Angle = angle;
        }
        public Rect(Vec2f pos, float width, float height, Angle angle = default)
        {
            X = pos.X;
            Y = pos.Y;
            Width = width;
            Height = height;
            Angle = angle;
        }
        public Rect(Vec2f pos, Vec2f size, Angle angle = default)
        {
            X = pos.X;
            Y = pos.Y;
            Width = size.X;
            Height = size.Y;
            Angle = angle;
        }

        public Vec2f Position { get => new Vec2f(X, Y); set { X = value.X; Y = value.Y; } }
        public Vec2f Size { get => new Vec2f(Width, Height); set { Width = value.X; Height = value.Y; } }

        public float Left { get => X; }
        public float Top { get => Y; }
        public float Right { get => X + Width; }
        public float Bottom { get => Y + Height; }

        public static explicit operator Rectangle(Rect rect) => new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

        public Rect ApplyTransform(Transform transform)
        {
            Rect rect = new Rect();
            rect.Position = Position.ApplyTransform(transform);
            rect.Angle = Angle + transform.Angle;
            rect.Size = Size * transform.Scale;
            return rect;
        }

        public bool Contains(Vec2f pos)
        {
            pos -= Position;
            pos.Angle -= Angle;

            return pos.X >= 0 && pos.Y >= 0 && pos.Y < Height && pos.X < Width;
        }

        public override string ToString()
        {
            return $"({Position}) {Width.ToString(dotnfi)}x{Height.ToString(dotnfi)} {Angle}";
        }
    }
    public struct Transform
    {
        public Vec2f Offset;
        public Angle Angle;
        public float Scale;

        public Transform ApplyTransform(Transform transform)
        {
            return new Transform
            {
                Angle = Angle + transform.Angle,
                Offset = transform.Offset.ApplyTransform(this),
                Scale = Scale * transform.Scale
            };
        }

        public override string ToString()
        {
            return $"(" +
                $"{Offset.X.ToString("0.00", NumberFormatInfo.InvariantInfo)}," +
                $" {Offset.Y.ToString("0.00", NumberFormatInfo.InvariantInfo)}" +
                $") {Angle} x{Scale.ToString("0.00", NumberFormatInfo.InvariantInfo)}";
        }

        public void IntClamp() 
        {
            Offset.X = (float)Math.Floor(Offset.X);
            Offset.Y = (float)Math.Floor(Offset.Y);
        }
    }
    public struct TransformedGraphics
    {

        private static Texture2D Pixel;

        public Transform Transform;

        public SpriteBatch SpriteBatch { get; private set; }

        public TransformedGraphics(SpriteBatch spriteBatch, Transform transform)
        {
            if (Pixel == null)
            {
                Pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                Pixel.SetData(new Color[] { Color.White });
            }

            SpriteBatch = spriteBatch;
            Transform = transform;
        }

        public void DrawLine(Vec2f a, Vec2f b, Color color)
        {
            a = a.ApplyTransform(Transform);
            b = b.ApplyTransform(Transform);

            Vec2f vec = b - a;

            SpriteBatch.Draw(Pixel, a, null, color, vec.Angle.Radians, Vector2.Zero, new Vector2(vec.Length, 1), SpriteEffects.None, 0);
        }
        public void DrawRect(Rect rect, Color color)
        {
            Vec2f[] points = new Vec2f[]
            {
                rect.Position,
                new Vec2f(rect.Width, rect.Angle) + rect.Position,
                new Vec2f(rect.Height, rect.Angle + Angle.FromDegrees(90)) + new Vec2f(rect.Width, rect.Angle) + rect.Position,
                new Vec2f(rect.Height, rect.Angle + Angle.FromDegrees(90)) + rect.Position,
            };
            DrawLine(points[0], points[1], color);
            DrawLine(points[1], points[2], color);
            DrawLine(points[2], points[3], color);
            DrawLine(points[3], points[0], color);
        }
        public void FillRect(Rect rect, Color color)
        {
            rect = rect.ApplyTransform(Transform);
            SpriteBatch.Draw(Pixel, rect.Position, null, color, rect.Angle.Radians, Vector2.Zero, rect.Size, SpriteEffects.None, 0);
        }
        public void DrawTexture(Texture2D texture, Rect rect, Color color, Rect? origin = null, bool useScaling = true)
        {
            rect = rect.ApplyTransform(Transform);

            if (useScaling)
            {
                Vec2f size = origin.HasValue ? origin.Value.Size : new Vec2f(texture.Bounds.Width, texture.Bounds.Height);
                SpriteBatch.Draw(texture, rect.Position, (Rectangle?)origin, color, rect.Angle.Radians, Vector2.Zero, rect.Size / size, SpriteEffects.None, 0);
            }
            else 
            {
                SpriteBatch.Draw(texture, (Rectangle)rect, (Rectangle?)origin, color, rect.Angle.Radians, Vector2.Zero, SpriteEffects.None, 0);
            }
        }

        public void DrawText(IFont font, string text, Vec2f pos, Angle angle, Color color, Color? shade = null)
        {
            if (text is null) return;
            pos = pos.ApplyTransform(Transform) + new Vec2f(0, 4); 
            float rad = (angle + Transform.Angle).Radians;

            Vec2f scale = new Vec2f(Transform.Scale);

            if (shade != null)
            {
                float off = Transform.Scale;

                font.Draw(SpriteBatch, text, pos + new Vec2f(off, off),   shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(-off, off),  shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(off, -off),  shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(-off, -off), shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
            }

            font.Draw(SpriteBatch, text, pos, color, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
        }
        public void DrawText(IFont font, string text, Rect rect, Align align, Color color, Color? shade = null)
        {
            if (text is null) return;
            Vec2f pos = rect.Position;
            pos += align.CalculateAlign(rect.Size, font.MeasureString(text));
            DrawText(font, text, pos, Angle.Zero, color, shade);
        }
        public void DrawText(IFont font, StringBuilder text, Vec2f pos, Angle angle, Color color, Color? shade = null)
        {
            if (text is null) return;
            pos = pos.ApplyTransform(Transform) + font.DrawOffset;
            float rad = (angle + Transform.Angle).Radians;

            Vec2f scale = new Vec2f(Transform.Scale);

            if (shade != null)
            {
                float off = Transform.Scale;

                font.Draw(SpriteBatch, text, pos + new Vec2f(off, off), shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(-off, off), shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(off, -off), shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
                font.Draw(SpriteBatch, text, pos + new Vec2f(-off, -off), shade.Value, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
            }

            font.Draw(SpriteBatch, text, pos, color, rad, Vec2f.Zero, scale, SpriteEffects.None, 0f);
        }
        public void DrawText(IFont font, StringBuilder text, Rect rect, Align align, Color color, Color? shade = null)
        {
            if (text is null) return;
            Vec2f pos = rect.Position;
            pos += align.CalculateAlign(rect.Size, font.MeasureString(text));
            DrawText(font, text, pos, Angle.Zero, color, shade);
        }

        public void SetBatch(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null
            ) 
        {
            SpriteBatch.End();
            if (transformMatrix.HasValue)
                SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix.Value);
            else
                SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect);
        }
    }
    public struct Colors
    {
        public Color Foreground;
        public Color Background;
        public Color Outline;

        public Colors(byte fgr, byte fgg, byte fgb, byte bgr, byte bgg, byte bgb, byte olr, byte olg, byte olb)
        { Foreground = new Color(fgr, fgg, fgb); Background = new Color(bgr, bgg, bgb); Outline = new Color(olr, olg, olb); }

        public Colors(Color fg, byte bgr, byte bgg, byte bgb, byte olr, byte olg, byte olb)
        { Foreground = fg; Background = new Color(bgr, bgg, bgb); Outline = new Color(olr, olg, olb); }

        public Colors(byte fgr, byte fgg, byte fgb, Color bg, byte olr, byte olg, byte olb)
        { Foreground = new Color(fgr, fgg, fgb); Background = bg; Outline = new Color(olr, olg, olb); }

        public Colors(Color fg, Color bg, byte olr, byte olg, byte olb)
        { Foreground = fg; Background = bg; Outline = new Color(olr, olg, olb); }

        public Colors(byte fgr, byte fgg, byte fgb, byte bgr, byte bgg, byte bgb, Color? ol = null)
        { Foreground = new Color(fgr, fgg, fgb); Background = new Color(bgr, bgg, bgb); Outline = ol ?? Color.Transparent; }

        public Colors(Color fg, byte bgr, byte bgg, byte bgb, Color? ol = null)
        { Foreground = fg; Background = new Color(bgr, bgg, bgb); Outline = ol ?? Color.Transparent; }

        public Colors(byte fgr, byte fgg, byte fgb, Color bg, Color? ol = null)
        { Foreground = new Color(fgr, fgg, fgb); Background = bg; Outline = ol ?? Color.Transparent; }

        public Colors(Color fg, Color bg, Color? ol = null) { Foreground = fg; Background = bg; Outline = ol ?? Color.Transparent; }

        public static Colors FirstColors(Colors? a, Colors b)
        {
            return new Colors()
            {
                Foreground = (a?.Foreground == Color.Transparent ? b.Foreground : a?.Foreground) ?? b.Foreground,
                Background = (a?.Background == Color.Transparent ? b.Background : a?.Background) ?? b.Background,
                Outline = (a?.Outline == Color.Transparent ? b.Outline : a?.Outline) ?? b.Outline
            };
        }

        public Colors SwapBF()
        {
            return new Colors()
            {
                Foreground = Background,
                Background = Foreground,
                Outline = Outline
            };
        }

        public Colors WithForeground(Color fg) => new Colors(fg, Background, Outline);
        public Colors WithForeground(byte r, byte g, byte b) => new Colors(new Color(r,g,b), Background, Outline);
        public Colors WithBackground(Color bg) => new Colors(Foreground, bg, Outline);
        public Colors WithBackground(byte r, byte g, byte b) => new Colors(Foreground, new Color(r, g, b), Outline);
        public Colors WithOutline(Color ol) => new Colors(Foreground, Background, ol);
        public Colors WithOutline(byte r, byte g, byte b) => new Colors(Foreground, Background, new Color(r, g, b));

    }
    public class UITexture 
    {
        public string TextureName { get; set; } = null;
        public Texture2D Texture { get; set; } = null;

        public UITexture(string name) { TextureName = name; }
        public UITexture(Texture2D texture) { Texture = texture; }

        public static implicit operator UITexture(string name) => new UITexture(name);
        public static implicit operator UITexture(Texture2D texture) => new UITexture(texture);

        public Texture2D GetValue(UIRoot root) 
        {
            if (root == null) return null;
            if (Texture != null) return Texture;
            if (TextureName != null) 
                if (root.UITextures.TryGetValue(TextureName, out Texture2D tex))
                    return tex;
            
            return null;
        }
    }
    public enum Align
    {
        TopLeft, Top, TopRight,
        Left, Center, Right,
        BottomLeft, Bottom, BottomRight
    }

    public interface IFont 
    {
        int LineSpacing { get; }
        float CharacterSpacing { get; }
        Vec2f DrawOffset { get; }

        Vec2f MeasureString(string text);
        Vec2f MeasureString(StringBuilder text);

        void Draw(SpriteBatch spriteBatch, string text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth);
        void Draw(SpriteBatch spriteBatch, StringBuilder text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth);
    }

    public class XNASpriteFont : IFont 
    {
        public SpriteFont Font { get; set; }

        public int LineSpacing => Font.LineSpacing;
        public float CharacterSpacing => Font.Spacing;
        public float DefaultScale => 1f;
        public Vec2f DrawOffset => new Vec2f(0, 2);

        public XNASpriteFont(SpriteFont font) 
        {
            Font = font;
        }

        public Vec2f MeasureString(string text)
        {
            return (Vec2f)Font.MeasureString(text);
        }

        public Vec2f MeasureString(StringBuilder text)
        {
            return (Vec2f)Font.MeasureString(text);
        }

        public void Draw(SpriteBatch spriteBatch, string text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth)
        {
            spriteBatch.DrawString(Font, text, pos, color, angle, origin, scale, spriteEffects, layerDepth);
        }

        public void Draw(SpriteBatch spriteBatch, StringBuilder text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth)
        {
            spriteBatch.DrawString(Font, text, pos, color, angle, origin, scale, spriteEffects, layerDepth);
        }

        
    }

    public struct PercentPos 
    {
        public Vec2f Value;
        public Vec2f Percent;

        public PercentPos(Vec2f value, Vec2f percent)
        {
            Value = value;
            Percent = percent;
        }

        public PercentPos(float xmul, float x, float ymul, float y) 
        {
            Value = new Vec2f(x, y);
            Percent = new Vec2f(xmul, ymul);
        }

        public PercentPos(float x,  float y)
        {
            Value = new Vec2f(x, y);
            Percent = Vec2f.Zero;
        }

        public PercentPos WithValue(Vec2f vec) => new PercentPos(vec, Percent);
        public PercentPos WithAddedValue(Vec2f vec) => new PercentPos(vec + Value, Percent);
        public PercentPos WithPercent(Vec2f vec) => new PercentPos(Value, vec);
    }
}
