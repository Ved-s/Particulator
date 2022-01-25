using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public abstract class UIExtension
    {
        public abstract string ShortId { get; }
        public virtual bool Global { get; set; } = false;

        public virtual void Draw(UIElement element, TransformedGraphics graphics) { }
        public virtual void Update(UIElement element) { }

        public virtual void KeyUpdate(UIElement element, Keys key, EventType type) { }
        public virtual void MouseKeyUpdate(UIElement element, MouseKeys key, EventType type) { }
        public virtual void MouseWheelUpdate(UIElement element, int amount) { }

        public virtual bool DrawBackground(UIElement element, TransformedGraphics graphics, Color color) => false;
        public virtual bool DrawOutline(UIElement element, TransformedGraphics graphics, Color color) => false;
    }

    public class ExtensionManager : IEnumerable<UIExtension>
    {
        public HashSet<UIExtension> Extensions = new HashSet<UIExtension>();

        internal UIElement Element;

        internal HashSet<UIExtension> AllExtensions = new HashSet<UIExtension>();

        public ExtensionManager(UIElement element) 
        {
            Element = element;
        }

        public void Reset() 
        {
            AllExtensions.Clear();
        }

        internal void SetGlobalExtensions(HashSet<UIExtension> exts)
        {
            AllExtensions.UnionWith(exts.Where(ext => !Element.IgnoreGlobalExtensions.Contains(ext.GetType())));
        }

        public void Update() 
        {
            AllExtensions.UnionWith(Extensions.Where(ext => ext.Global));

            if (Element is UIContainer container)
                foreach (UIElement element in container.Elements)
                    element.Extensions.SetGlobalExtensions(AllExtensions);

            AllExtensions.UnionWith(Extensions.Where(ext => !ext.Global));

            foreach (UIExtension ext in AllExtensions)
                ext.Update(Element);
        }

        public void Draw(TransformedGraphics graphics) 
        {
            foreach (UIExtension ext in AllExtensions)
                ext.Draw(Element, graphics);
        }

        public void KeyUpdate(Keys key, EventType type) 
        {
            foreach (UIExtension ext in AllExtensions)
                ext.KeyUpdate(Element, key, type);
        }
        public void MouseKeyUpdate(MouseKeys key, EventType type) 
        {
            foreach (UIExtension ext in AllExtensions)
                ext.MouseKeyUpdate(Element, key, type);
        }
        public void MouseWheelUpdate(int amount) 
        {
            foreach (UIExtension ext in AllExtensions)
                ext.MouseWheelUpdate(Element, amount);
        }

        public bool DrawBackground(TransformedGraphics graphics, Color color) 
        {
            foreach (UIExtension ext in AllExtensions)
                if (ext.DrawBackground(Element, graphics, color))
                    return true;
            return false;
        }
        public bool DrawOutline(TransformedGraphics graphics, Color color) 
        {
            foreach (UIExtension ext in AllExtensions)
                if (ext.DrawOutline(Element, graphics, color))
                    return true;

            return false;
        }

        public IEnumerator<UIExtension> GetEnumerator()
        {
            return ((IEnumerable<UIExtension>)Extensions).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Extensions).GetEnumerator();
        }
        public void Add(UIExtension ext) 
        {
            Extensions.Add(ext);
        }
    }
}
