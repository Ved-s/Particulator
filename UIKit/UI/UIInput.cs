using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UIKit.UI
{
    public class UIInput : UIElement
    {
        public override string TypeName => "input";

        public int CursorPos { get; set; } = 0;
        public bool AllowEnter { get; set; }

        public override string Text { get => InnerText.ToString(); set { InnerText.Clear(); InnerText.Append(value); } }
        public Align TextAlign { get; set; } = Align.TopLeft;
        public Regex ValidationRegex { get; set; } = null;
        public string HintText { get; set; }

        public Action<UIInput> OnTextChanged;

        private bool Init;
        private StringBuilder InnerText = new StringBuilder();
        private Regex NonWordChar = new Regex("\\W", RegexOptions.Compiled);
        private float Blinker = 1f;

        public override void UpdateSelf()
        {
            if (!Init)
            {
                Init = true;
                Platform.Current.TextInput += Input;
            }

            if (Blinker < 0) Blinker = 1f;
            else Blinker -= 1/40f;

        }

        public override void DrawSelf(TransformedGraphics graphics)
        {
            string text = InnerText.ToString();

            List<(int start, int length)> lines = new List<(int, int)>();

            int length = 0;
            int currentLineIndex = 0;

            Point cursorPos = Point.Zero;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (i == CursorPos)
                {
                    cursorPos.X = length;
                    cursorPos.Y = lines.Count;
                }

                if (c == '\n')
                {
                    lines.Add((currentLineIndex, length));
                    currentLineIndex = i + 1;
                    length = 0;
                }
                else
                {
                    length++;
                }
            }
            lines.Add((currentLineIndex, length));

            if (CursorPos == InnerText.Length)
            {
                cursorPos.X = length;
                cursorPos.Y = lines.Count - 1;
            }

            if (text.Length == 0 && !HintText.isNullEmptyOrWhitespace())
            {
                graphics.DrawText(Font, HintText, Bounds, TextAlign, Colors.Foreground * 0.5f, Color.Black);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                (int start, int lineLength) = lines[i];

                StringBuilder line = new StringBuilder(text, start, lineLength, lineLength);
                Vec2f size = (Vec2f)Font.MeasureString(line);
                Vec2f pos = GetLinePos(i, lines.Count, size.X);

                graphics.DrawText(Font, line, pos, Angle.Zero, Colors.Foreground, Color.Black);

                if (Active && i == cursorPos.Y)
                {
                    StringBuilder builder = new StringBuilder(text, start, cursorPos.X, cursorPos.X);
                    float xOff = Font.MeasureString(builder).X;
                    pos.X += xOff + Font.DrawOffset.X;
                    Vec2f pos2 = new Vec2f(pos.X, pos.Y + Font.LineSpacing - Font.DrawOffset.Y);
                    pos.Y += Font.DrawOffset.Y;
                    graphics.DrawLine(pos, pos2, Color.White * Blinker);
                }
            }
        }

        public override void OnActiveChanged()
        {
            Blinker = 1f;
        }

        private Vec2f GetLinePos(int lineIndex, int linesTotal, float lineWidth) 
        {
            int height = Font.LineSpacing;

            Vec2f pos = TextAlign.CalculateAlign(Bounds.Size, new Vec2f(lineWidth, linesTotal * height));
            pos.Y += height * lineIndex;
            return pos;
        }

        public override void KeyUpdate(Keys key, EventType type)
        {
            if (!Active) return;
            if (type == EventType.Presssed) 
            {
                FixCursor();

                if (key == Keys.Right)
                {
                    if (CursorPos == InnerText.Length) return;
                    CursorPos++;
                    if (Root.CurrentKeys.Ctrl())
                        while (CursorPos < InnerText.Length && !NonWordChar.IsMatch(InnerText[CursorPos].ToString()))
                            CursorPos++;
                    Blinker = 1f;
                    return;
                }
                else if (key == Keys.Left)
                {
                    if (CursorPos == 0) return;
                    CursorPos--;
                    if (Root.CurrentKeys.Ctrl())
                        while (CursorPos > 0 && !NonWordChar.IsMatch(InnerText[CursorPos].ToString()))
                            CursorPos--;
                    Blinker = 1f;
                    return;
                }
                else if (Root.CurrentKeys.Ctrl())
                {
                    if (key == Keys.C)
                    {
                        Platform.Current.Clipboard = Text;
                        return;
                    }
                    else if (key == Keys.X)
                    {
                        Platform.Current.Clipboard = Text;
                        Text = "";
                        FixCursor();
                        return;
                    }
                    else if (key == Keys.V)
                    {
                        string text = Platform.Current.Clipboard;

                        foreach (char chr in text)
                            InserChar(chr);
                        return;
                    }
                }
            }
        }

        private void Input(Keys key, char c)
        {
            if (!Active) return;

            if (key == Keys.OemMinus) c = '-';
            else if (key == Keys.OemQuestion) c = '?';
            else if (key == Keys.OemPlus) c = '+';
            else if (key == Keys.OemPeriod) c = '.';
            else if (key == Keys.OemComma) c = ',';

            if (Root.CurrentKeys.Shift()) c = c.ToString().ToUpper()[0];
            else c = c.ToString().ToLower()[0];

            if (key == Keys.Enter || c == 13)
            {
                if (!AllowEnter) return;
                else
                {
                    InnerText.Insert(CursorPos, '\n');
                    CursorPos++;
                    Blinker = 1f;
                    OnTextChanged?.Invoke(this);
                    return;
                }
            }
            else if (key == Keys.Back || c == 8)
            {
                if (CursorPos == 0) return;
                CursorPos--;
                InnerText.Remove(CursorPos, 1);
                if (Root.CurrentKeys.Ctrl())
                    while (CursorPos > 0 && !NonWordChar.IsMatch(InnerText[CursorPos-1].ToString()))
                    {
                        CursorPos--;
                        InnerText.Remove(CursorPos, 1);
                    }
                OnTextChanged?.Invoke(this);
                Blinker = 1f;
                return;
            }
            else if (key == Keys.Delete || c == 127)
            {
                if (CursorPos == InnerText.Length) return;
                InnerText.Remove(CursorPos, 1);
                if (Root.CurrentKeys.Ctrl())
                    while (CursorPos < InnerText.Length && !NonWordChar.IsMatch(InnerText[CursorPos].ToString()))
                    {
                        InnerText.Remove(CursorPos, 1);
                    }
                OnTextChanged?.Invoke(this);
                Blinker = 1f;
                return;
            }

            FixCursor();
            InserChar(c);
        }

        private void InserChar(char c)
        {
            if (!AllowEnter && c == '\n') return;
            InnerText.Insert(CursorPos, c);

            if (ValidationRegex != null && !ValidationRegex.IsMatch(InnerText.ToString()))
            {
                InnerText.Remove(CursorPos, 1);
            }
            else
            {
                CursorPos++;
                Blinker = 1f;
                OnTextChanged?.Invoke(this);
            }
        }

        private void FixCursor()
        {
            if (CursorPos > InnerText.Length) CursorPos = InnerText.Length;
            else if (CursorPos < 0) CursorPos = 0;
        }
    }
}
