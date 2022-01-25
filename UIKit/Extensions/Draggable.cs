using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public class Draggable : UIExtension
    {
        public override string ShortId => "drag";

        public int DragTargetSearchDepth { get; set; } = int.MaxValue;
        public string DragTarget { get; set; } = null;
        public MouseKeys DragKey { get; set; } = MouseKeys.Left;

        public bool LimitDragByParent { get; set; } = true;

        private Vec2f MouseOffset;

        public override void MouseKeyUpdate(UIElement element, MouseKeys key, EventType type)
        {
            if (key != DragKey) return;

            foreach (UIExtension ext in element.Extensions)
                if (ext is Resizeable resize && resize.Key == DragKey && resize.Dragging)
                    return;

            UIElement target = element;
            if (DragTarget != null)
                target = element.GetElement(DragTarget, DragTargetSearchDepth);

            if (target == null) return;

            if (type == EventType.Presssed)
                MouseOffset = target.TransformedMousePosition;

            if (type == EventType.Hold) 
            {
                Vec2f diff = target.TransformedMousePosition - MouseOffset;
                diff.Angle += target.Angle;
                target.AddPos(diff * target.Scale);
                

                if (LimitDragByParent && target.Parent != null) 
                {
                    Vec2f size = target.Bounds.Size * target.Scale;
                    Vec2f parentSize = target.Parent.Bounds.Size;
                    Vec2f pos = target.Pos.Value + parentSize * target.Pos.Percent;

                    if (pos.X < 0) pos.X = 0;
                    if (pos.Y < 0) pos.Y = 0;
                    if (pos.X > parentSize.X - size.X) pos.X = parentSize.X - size.X;
                    if (pos.Y > parentSize.Y - size.Y) pos.Y = parentSize.Y - size.Y;

                    target.SetPos(pos - parentSize * target.Pos.Percent);
                }

                target.RecalculateAllTransforms();
                MouseOffset = target.TransformedMousePosition;
            }
        }
    }
}
