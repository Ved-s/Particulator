using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIKit.Extensions;

namespace UIKit.UI
{
    public abstract class UIElement
    {
        public abstract string TypeName { get; }
        public virtual string Name { get; set; }

        public Transform Transform = new Transform() { Scale = 1 };

        public float Scale { get => Transform.Scale; set => Transform.Scale = value; }
        public Angle Angle { get => Transform.Angle; set => Transform.Angle = value; }
        public PercentPos Pos { get => _pos; set { _pos = value; Transform.Offset = value.Value; } }
        public virtual PercentPos Size { get; set; }
        public virtual bool NoHover { get; set; }

        public HashSet<Type> IgnoreGlobalExtensions { get; } = new HashSet<Type>();
        public ExtensionManager Extensions { get; }

        public virtual string Text { get; set; }
        public virtual IFont Font { get => _Font ?? Parent?.Font; set => _Font = value; }
        public virtual Colors Colors { get => _Colors ?? Parent.Colors; set => _Colors = value; }

        public virtual bool Visible { get; set; } = true;
        public virtual bool Enabled { get; set; } = true;

        public UIContainer Parent { get; internal set; }
        public virtual UIRoot Root { get; internal set; }

        public Transform CalculatedTransform;
        public Rect AbsoluteBounds;
        public virtual Rect Bounds { get; protected set; } = new Rect(Vec2f.Zero, Vec2f.Zero);

        public virtual Vec2f MousePosition => Parent.MousePosition - Pos.Value;
        public virtual Vec2f TransformedMousePosition => Root.MousePosition.ApplyTransformBack(CalculatedTransform);

        public bool Active => Root?.Active == this;
        public bool Hover { get => !NoHover && Visible && Root.Game.IsActive && Bounds.Contains(TransformedMousePosition); }

        public TimeSpan UpdateTime, DrawTime;

        protected virtual bool CustomBackgroundDraw => false;

        private PercentPos _pos;
        private IFont _Font = null;
        private Colors? _Colors = null;

        public UIElement() 
        {
            Extensions = new ExtensionManager(this);
        }

        public void Reset() 
        {
            Extensions.Reset();
            ResetSelf();
        }
        public void Update()
        {
            if (!Visible || !Enabled) return;

            Stopwatch sw = Stopwatch.StartNew();

            Extensions.Update();
            UpdateSelf();
            OnUpdate?.Invoke(this);

            sw.Stop();
            UpdateTime = sw.Elapsed;
        }
        public void Draw(TransformedGraphics graphics)
        {
            if (!Visible) return;

            Stopwatch watch = Stopwatch.StartNew();

            if (!CustomBackgroundDraw)
            {
                DrawBackground(graphics, Colors.Background);
                DrawOutline(graphics, Colors.Outline);
            }

            DrawSelf(graphics);

            Extensions.Draw(graphics);

            OnDraw?.Invoke(this, graphics);

            watch.Stop();
            DrawTime = watch.Elapsed;
        }

        public void DrawBackground(TransformedGraphics graphics, Color color) 
        {
            if (Extensions.DrawBackground(graphics, color)) return;

            if (color != Color.Transparent)
                graphics.FillRect(Bounds, color);
        }
        public void DrawOutline(TransformedGraphics graphics, Color color)
        {
            if (Extensions.DrawOutline(graphics, color)) return;

            if (color != Color.Transparent)
                graphics.DrawRect(Bounds, color);
        }

        public virtual void KeyUpdate(Keys key, EventType type) 
        {
            Extensions.KeyUpdate(key, type);
        }
        public virtual void MouseKeyUpdate(MouseKeys key, EventType type)
        {
            if (type == EventType.Presssed && !Active && Root != null)
                Root.SetActive(this);

            Extensions.MouseKeyUpdate(key, type);
        }
        public virtual void MouseWheelUpdate(int amount)
        {
            Extensions.MouseWheelUpdate(amount);
        }

        public virtual void OnActiveChanged() { }

        public virtual void ResetSelf() { }
        public virtual void UpdateSelf() { }
        public virtual void DrawSelf(TransformedGraphics graphics) { }

        public void RecalculateTransform()
        {
            //if (Name == "label") Debugger.Break();

            Bounds = new Rect(Vec2f.Zero, Size.Value + Size.Percent * (Parent?.Bounds.Size ?? Vec2f.Zero));
            Transform transform = Transform;
            transform.Offset += Pos.Percent * (Parent?.Bounds.Size ?? Vec2f.Zero);

            CalculatedTransform = Parent.CalculatedTransform.ApplyTransform(transform);

            AbsoluteBounds = Bounds.ApplyTransform(CalculatedTransform);
        }
        public virtual void RecalculateAllTransforms()
        {
            RecalculateTransform();
        }

        internal virtual UIElement GetHover()
        {
            if (!Hover) return null;
            return this;
        }
        public TransformedGraphics GetGraphics(SpriteBatch spriteBatch) 
        {
            return new TransformedGraphics(spriteBatch, CalculatedTransform);
        }
        public virtual UIElement GetElement(string name, int maxSearchParents = int.MaxValue)
        {
            if (maxSearchParents == 0)
            {
                if (name == Name) return this;
                return null;
            }

            string[] path = name.Split(new char[] { '/' }, 2);

            if (path.Length == 2)
            {
                UIElement root = GetElement(path[0], maxSearchParents - 1);
                return root?.GetElement(path[1], maxSearchParents - 2);
            }

            if (name == Name) return this;
            if (name == ".") return this;
            if (name == "..") return Parent;
            if (name == "@") return Root;
            return null;
        }
        public T GetExtension<T>() where T: UIExtension
        {
            foreach (UIExtension ext in Extensions)
                if (ext is T t)
                    return t;
            return null;
        }

        public void SetFont(SpriteFont font)
        {
            Font = new XNASpriteFont(font);
        }
        public void SetPos(Vec2f pos) 
        {
            Pos = Pos.WithValue(pos);
        }
        public void SetSize(Vec2f size)
        {
            Size = Size.WithValue(size);
        }
        public void AddPos(Vec2f pos)
        {
            Pos = Pos.WithValue(pos + Pos.Value);
        }
        public void AddSize(Vec2f size)
        {
            Size = Size.WithValue(size + Size.Value);
        }

        public override string ToString()
        {
            return $"{TypeName} \"{Name}\" @ {Transform}";
        }

        public Action<UIElement> OnUpdate;
        public Action<UIElement, TransformedGraphics> OnDraw;
    }
}
