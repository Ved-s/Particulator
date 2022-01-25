using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UIKit
{
    public abstract class Platform
    {
        public enum PlatformType { XNA, FNA, MonoGame }

        public static PlatformType CurrentPlatformType { get; private set; }
        public static Platform Current { get; private set; }

        public static void Init(Game game) 
        {
            if (Current != null) return;

            string windowType = game.Window.GetType().Name;

            switch (windowType) 
            {
                case "WindowsGameWindow": Current = new XNA(); CurrentPlatformType = PlatformType.XNA; break;
                case "FNAWindow":         Current = new FNA(); CurrentPlatformType = PlatformType.FNA; break;
                case "SdlGameWindow":     Current = new MG();  CurrentPlatformType = PlatformType.MonoGame; break;
            }

            if (Current != null) 
            {
                Current.InitPlatform(game);
            }
        }

        public TextInputDelegate TextInput { get; set; }
        public virtual string Clipboard { get; set; }

        protected abstract void InitPlatform(Game game);

        internal class XNA : Platform 
        {
            protected override void InitPlatform(Game game)
            {
                GameWindow window = game.Window;
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)window.GetType().GetField("mainForm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(window);

                form.KeyPreview = true;
                form.PreviewKeyDown += (s, e) =>
                {
                    TextInput?.Invoke((Keys)e.KeyCode, (char)e.KeyValue);
                };
            }

            public override string Clipboard 
            { 
                get => System.Windows.Forms.Clipboard.GetText(); 
                set => System.Windows.Forms.Clipboard.SetText(value);
            }
        }

        internal class FNA : Platform
        {
            protected override void InitPlatform(Game game)
            { 
                Type textInputExt = game.Window.GetType().Assembly.GetTypes().First(t => t.Name == "TextInputEXT");
                EventInfo eventInfo = textInputExt.GetEvent("TextInput", BindingFlags.Public | BindingFlags.Static);
                eventInfo.AddEventHandler(null, new Action<char>((c) =>
                {
                    TextInput(Keys.None, c);
                }));

            }
        }

        internal class MG : Platform
        {
            protected override void InitPlatform(Game game)
            {
                //game.Window.TextInput += (s, e) =>
                //{
                //    TextInput(e.Key, e.Character);
                //};
            }

            public override string Clipboard 
            {
                get 
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        return System.Windows.Forms.Clipboard.GetText();
                    throw new NotSupportedException();
                }
                set 
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        System.Windows.Forms.Clipboard.SetText(value);
                    else throw new NotSupportedException();
                } 
            }
        }

        public delegate void TextInputDelegate(Microsoft.Xna.Framework.Input.Keys key, char chr);
    }
}
