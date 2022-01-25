using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using UIKit.Extensions;

namespace UIKit.UI
{
    public class UIRoot : UIContainer
    {
        public override string TypeName => "root";
        public override string Name => "root";

        public readonly Game Game;

        public KeyboardState CurrentKeys;
        private List<Keys> OldKeys = new List<Keys>();

        public MouseState CurrentMouseKeys;
        private List<MouseKeys> OldMouseKeys = new List<MouseKeys>();

        public int MouseWheel = 0;
        private int OldMouseWheel = 0;

        public Dictionary<string, Texture2D> UITextures { get; } = new Dictionary<string, Texture2D>();

        public override PercentPos Size => new PercentPos(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        public override Rect Bounds => new Rect(Vec2f.Zero, Size.Value);

        public bool Debug = false;
        public bool DebugEnabled = true;

        public new UIElement Hover { get; private set; }
        public new UIElement Active { get; private set; }

        public UIRoot(Game game)
        {
            Platform.Init(game);

            Game = game;
            Colors = new Colors(Color.White, Color.Transparent, Color.Transparent);

            Game.Deactivated += (s, e) => SetActive(null);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Draw(GetGraphics(spriteBatch));

            if (Debug && Hover != null)
                DrawDebug(Hover, new TransformedGraphics(spriteBatch, new Transform() { Scale = 1f }));

            sw.Stop();
            DrawTime = sw.Elapsed;
        }

        internal void DrawDebug(UIElement element, TransformedGraphics graphics)
        {
            element.GetGraphics(graphics.SpriteBatch).DrawRect(element.Bounds, Color.Red);

            Transform t = element.Transform;

            string name = $"{element.Parent?.Name ?? element.Parent?.TypeName}/{element.Name ?? element.TypeName}";

            string data = $"{name}\n" +
                $"TAbs: {element.CalculatedTransform}\n" +
                $"Pos: {APlusBPrc(t.Offset.X, element.Pos.Percent.X)}, {APlusBPrc(t.Offset.Y, element.Pos.Percent.Y)}\n" +
                $"Size: {APlusBPrc(element.Size.Value.X, element.Size.Percent.X)}, {APlusBPrc(element.Size.Value.Y, element.Size.Percent.Y)}\n" +
                $"{Angle} x{Scale.ToString("0.00", NumberFormatInfo.InvariantInfo)}\n" +
                $"Time: " +
                $"U {element.UpdateTime.TotalMilliseconds.ToString("0.00", NumberFormatInfo.InvariantInfo)} " +
                $"D {element.DrawTime.TotalMilliseconds.ToString("0.00", NumberFormatInfo.InvariantInfo)}";

            foreach (UIExtension ext in element.Extensions.AllExtensions)
                data += "\n+ " + ext.ShortId + (ext.Global? " (G)" : "");

            Vec2f pos = MousePosition + new Vec2f(20, 0);
            graphics.DrawText(Font, data, pos, Angle.Zero, Color.Red, Color.Black);

            if (element is UIContainer container)
                foreach (UIElement sub in container.Elements)
                    if (sub is UIModal modal && modal.Visible)
                    {
                        TransformedGraphics modalGraphics = modal.GetGraphics(graphics.SpriteBatch);
                        modalGraphics.DrawRect(modal.Bounds, Color.Yellow);
                        modalGraphics.DrawText(Font, "modal", modal.Bounds, Align.TopLeft, Color.Yellow, Color.Black);
                    }

            if (Active != null) 
            {
                TransformedGraphics activeGraphics = Active.GetGraphics(graphics.SpriteBatch);
                activeGraphics.DrawRect(Active.Bounds, Color.Lime);
                activeGraphics.DrawText(Font, "active", Active.Bounds, Align.Top, Color.Lime, Color.Black);
            }
        }

        private string APlusBPrc(float a, float b) 
        {
            char s = b > 0 ? '+' : '-';
            return $"{a.ToString("0.00", NumberFormatInfo.InvariantInfo)}{s}{(int)(b * 100)}%";
        }

        public override void KeyUpdate(Keys key, EventType type)
        {
            base.KeyUpdate(key, type);
            if (type == EventType.Presssed && key == Keys.F9)
                Debug = !Debug && DebugEnabled;
        }

        public override void UpdateSelf()
        {
            Reset();

            OnUpdate?.Invoke(this);

            CurrentMouseKeys = Mouse.GetState();

            CalculatedTransform = Transform;

            foreach (UIElement element in Elements)
            {
                element.RecalculateTransform();
                element.Update();
            }

            CurrentKeys = Keyboard.GetState();
            HashSet<Keys> changedKeys = new HashSet<Keys>(CurrentKeys.GetPressedKeys());
            changedKeys.UnionWith(OldKeys);
            foreach (Keys key in changedKeys)
            {
                int keyNow = (int)CurrentKeys[key];
                int keyBefore = OldKeys.Contains(key) ? 1 : 0;
                EventType n = (EventType)(keyBefore << 1 | keyNow);
                if (Game.IsActive) KeyUpdate(key, n);
                if (n == EventType.Released) OldKeys.Remove(key);
                else if (n == EventType.Presssed) OldKeys.Add(key);
            }

            bool anyMouseKey = false;

            HashSet<MouseKeys> changedMouseKeys = new HashSet<MouseKeys>(OldMouseKeys);
            List<MouseKeys> mouseKeys = new List<MouseKeys>();
            if (CurrentMouseKeys.LeftButton == ButtonState.Pressed) { mouseKeys.Add(MouseKeys.Left); anyMouseKey = true; }
            if (CurrentMouseKeys.RightButton == ButtonState.Pressed) { mouseKeys.Add(MouseKeys.Right); anyMouseKey = true; }
            if (CurrentMouseKeys.MiddleButton == ButtonState.Pressed) { mouseKeys.Add(MouseKeys.Middle); anyMouseKey = true; }
            if (CurrentMouseKeys.XButton1 == ButtonState.Pressed) { mouseKeys.Add(MouseKeys.XButton1); anyMouseKey = true; }
            if (CurrentMouseKeys.XButton2 == ButtonState.Pressed) { mouseKeys.Add(MouseKeys.XButton2); anyMouseKey = true; }
            changedMouseKeys.UnionWith(mouseKeys);

            foreach (MouseKeys key in changedMouseKeys)
            {
                int keyNow = mouseKeys.Contains(key) ? 1 : 0;
                int keyBefore = OldMouseKeys.Contains(key) ? 1 : 0;
                EventType n = (EventType)(keyBefore << 1 | keyNow);
                if (Game.IsActive) Hover?.MouseKeyUpdate(key, n);
                if (n == EventType.Released) OldMouseKeys.Remove(key);
                else if (n == EventType.Presssed) OldMouseKeys.Add(key);
            }

            MouseWheel = CurrentMouseKeys.ScrollWheelValue / 120;
            if (OldMouseWheel != MouseWheel)
                if (Game.IsActive) Hover?.MouseWheelUpdate(OldMouseWheel - MouseWheel);
            OldMouseWheel = MouseWheel;
            if (!anyMouseKey)
            {
                UIElement prevHover = Hover;
                Hover = Game.IsActive ? GetHover() : null;
                if (prevHover != Hover) OnGlobalHoverChanged?.Invoke(prevHover, Hover);
            }
        }

        internal override UIElement GetHover()
        {
            foreach (UIElement element in Elements)
                if (element.Hover) return element.GetHover();
            return this;
        }

        public void SetActive(UIElement element) 
        {
            if (element is UIRoot) element = null;

            UIElement previousActive = Active;
            Active = element;

            if (previousActive != null) previousActive.OnActiveChanged();
            if (Active != null) Active.OnActiveChanged();
            OnGlobalActiveChanged?.Invoke(previousActive, Active);
        }

        public override UIRoot Root => this;

        public override Vec2f MousePosition => new Vec2f(CurrentMouseKeys.X, CurrentMouseKeys.Y);
        public override Vec2f TransformedMousePosition => new Vec2f(CurrentMouseKeys.X, CurrentMouseKeys.Y).ApplyTransformBack(Transform);

        public Action<UIElement, UIElement> OnGlobalActiveChanged;
        public Action<UIElement, UIElement> OnGlobalHoverChanged;
    }

    public enum EventType
    {
        Presssed = 1,
        Released = 2,
        Hold = 3
    }
    public enum MouseKeys
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
}
