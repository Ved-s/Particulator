using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UIKit.UI;

namespace UIKit.Extensions
{
    public class Rotateable : UIExtension
    {
        public override string ShortId => "rotate";

        public int DragTargetSearchDepth { get; set; } = int.MaxValue;
        public string DragTarget { get; set; } = null;
        public Align RotateAlign { get; set; } = Align.TopLeft;
        public MouseKeys RotateKey { get; set; } = MouseKeys.Middle;

        private Vec2f MouseOffset;

        public override void MouseKeyUpdate(UIElement element, MouseKeys key, EventType type)
        {
            if (key != RotateKey) return;

            UIElement target = element;
            if (DragTarget != null)
                target = element.GetElement(DragTarget, DragTargetSearchDepth);

            if (target == null) return;

            if (type == EventType.Presssed)
                MouseOffset = target.TransformedMousePosition;

            if (type == EventType.Hold)
            {
                Vec2f rotationPoint = RotateAlign.CalculateAlign(target.Size.Value, Vec2f.Zero);
                Angle angle = (target.TransformedMousePosition - rotationPoint).Angle - (MouseOffset - rotationPoint).Angle;

                //Debug.WriteLine(angle);
                target.Angle += angle;
                target.SetPos(target.Pos.Value.Rotate(angle, target.Pos.Value + rotationPoint));

                target.RecalculateAllTransforms();
                MouseOffset = target.TransformedMousePosition;
            }
        }

    }
}
