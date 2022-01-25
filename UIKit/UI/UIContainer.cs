using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UIKit.UI
{
    public class UIContainer : UIElement
    {
        public override string TypeName => "container";

        public ElementCollection Elements { get; protected set; }

        public bool HoverTransparentBackground { get; set; } = false;

        public UIContainer()
        {
            Elements = new ElementCollection(this);
        }

        public override void ResetSelf()
        {
            foreach (UIElement element in Elements)
                element.Reset();
        }

        public override void DrawSelf(TransformedGraphics graphics)
        {
            bool anyModals = false, modalTransparentBkg = true;

            foreach (UIElement element in Elements)
            {
                if (element is UIModal modal && element.Visible)
                {
                    anyModals = true;
                    if (modal.ModalBackgroundColor.A == 255) 
                        modalTransparentBkg = false;
                }
            }

            if (modalTransparentBkg)
                foreach (UIElement element in Elements)
                {
                    if (!(element is UIModal))
                        element.Draw(element.GetGraphics(graphics.SpriteBatch));
                }

            if (anyModals)
            {
                foreach (UIElement element in Elements)
                {
                    if (element is UIModal modal && element.Visible)
                    {
                        graphics.FillRect(Bounds, modal.ModalBackgroundColor);
                        graphics.DrawRect(Bounds, Colors.Outline);

                        element.Draw(new TransformedGraphics(graphics.SpriteBatch, element.CalculatedTransform));
                    }
                }
            }
        }

        public override void UpdateSelf()
        {
            foreach (UIElement element in Elements)
            {
                element.RecalculateTransform();
                element.Update();
            }
        }

        public override void RecalculateAllTransforms() 
        {
            RecalculateTransform();
            foreach (UIElement element in Elements)
            {
                if (element is UIContainer container) container.RecalculateAllTransforms();
                else element.RecalculateTransform();
            }
        }

        public T GetLocalElement<T>(string name = null) where T : UIElement
        {
            foreach (UIElement e in Elements)
                if (e is T && (name == null || e.Name == name))
                    return e as T;

            foreach (UIElement e in Elements)
                if (e is UIContainer c) 
                {
                    T element = c.GetLocalElement<T>(name);
                    if (element != null)
                        return element;
                }

            return null;
        }

        public T GetElement<T>(string name, int maxSearchParents = int.MaxValue) where T : UIElement
        {
            return GetElement(name, maxSearchParents) as T;
        }

        public override UIElement GetElement(string name, int maxSearchParents = int.MaxValue)
        {
            string[] path = name.Split(new char[] { '/' }, 2);

            if (path.Length == 2)
            {
                UIElement root = GetElement(path[0], 0);
                return root?.GetElement(path[1], 0);
            }

            if (name == Name) return this;
            if (name == ".") return this;
            if (name == "..") return Parent;
            if (name == "@") return Root;

            if (name.StartsWith(".")) { maxSearchParents = 0; name = name.Substring(1); }

            if (name.StartsWith("@"))
            {
                name = name.Substring(1);
                return Root.GetElement(name, 0);
            }
            foreach (UIElement searchelement in Elements)
                if (searchelement.Name == name)
                    return searchelement;

            if (maxSearchParents > 0)
                return Parent?.GetElement(name, maxSearchParents - 1);

            return null;
        }

        public override void KeyUpdate(Keys key, EventType type)
        {
            base.KeyUpdate(key, type);
            foreach (UIElement element in Elements) element.KeyUpdate(key, type);
        }

        internal override UIElement GetHover()
        {
            if (!Hover) return null;

            foreach (UIElement possibleModal in Elements)
                if (possibleModal is UIModal && possibleModal.Visible)
                    return possibleModal.GetHover() ?? this;

            foreach (UIElement element in Elements)
                if (element.Hover) return element.GetHover();

            return this;
        }

        public override UIRoot Root
        {
            get => base.Root;
            internal set
            {
                base.Root = value;
                foreach (UIElement element in Elements)
                    element.Root = value;
            }
        }
    }

    public class ElementCollection : IEnumerable<UIElement>
    {
        List<UIElement> ElementList = new List<UIElement>();
        UIContainer Parent;

        public ElementCollection(UIContainer parent)
        {
            Parent = parent;
        }

        public void Add(UIElement element)
        {
            ElementList.Add(element);
            element.Parent = Parent;
            element.Root = Parent.Root;
        }

        public void Remove(string name) 
        {
            UIElement el = ElementList.FirstOrDefault(e => e.Name == name);
            if (el != null) 
            {
                el.Parent = null;
                ElementList.Remove(el);
            }
        }
        public void Remove<T>(string name = null)
        {
            UIElement el = ElementList.FirstOrDefault(e => e is T && (name == null || e.Name == name));
            if (el != null)
            {
                el.Parent = null;
                ElementList.Remove(el);
            }
        }

        public void Clear() 
        {
            foreach (UIElement element in ElementList)
                element.Parent = null;
            ElementList.Clear();
        }

        public IEnumerator<UIElement> GetEnumerator()
        {
            return ((IEnumerable<UIElement>)ElementList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)ElementList).GetEnumerator();
        }
    }
}
