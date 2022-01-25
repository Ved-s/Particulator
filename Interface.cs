using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particulator.Tiles;
using Particulator.UI;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Graphics;
using UIKit;
using UIKit.Extensions;
using UIKit.Modular;
using UIKit.UI;

namespace Particulator
{
    static class Interface
    {
        private static ParticulatorTE te;

        public static UIRoot Root;
        public static UIContainer Container;

        private static Dictionary<short, string> DustSearchMap;
        private static List<short> SearchResult;
        private static bool SelectionLocked;

        public static ParticulatorTE TE
        {
            get => te;
            set
            {
                te = value;
                if (te != null)
                {
                    Main.PlaySound(Terraria.ID.SoundID.MenuOpen);
                    UpdateData();
                }
                else
                {
                    Main.PlaySound(Terraria.ID.SoundID.MenuClose);
                    Main.blockInput = false;
                }
            }
        }
        public static void UpdateData()
        {
            if (te == null) return;

            ParticulatorTE.SpawnParams data = te.spawnData;

            UpdateSearch();
            Container.GetElement<LabelInput>("./topBox/dpf").InputText = data.DustsPerFrame.ToString();
            Container.GetElement<LabelInput>("./topBox/maxDust").InputText = data.MaxDust.ToString();
            Container.GetElement<UICheckButton>("./topBox/collision").Checked = data.Collision;
            Container.GetElement<XYRange>("./spawnRange").SetRange(data.SpawnPos);
            Container.GetElement<XYRange>("./velocityRange").SetRange(data.Velocity);
            Container.GetElement<XYRange>("./accRange").SetRange(data.Acceleration);
            Container.GetElement<FloatRange>("./scaleRange").SetRange(data.Scale);
            Container.GetElement<IntRange>("./timeRange").SetRange(data.Time);
            Container.GetElement<FloatRange>("./initRotationRange").SetRange(data.StartRotation);
            Container.GetElement<FloatRange>("./rotationRange").SetRange(data.Rotation);
            Container.GetElement<UIItemSlot>("./shader").Item = data.Dye ?? new Item();
            Container.GetElement<ColorRange>("./colors/colorRange").SetRange(data.ColorRange);
            Container.GetElement<ColorRange>("./colors/lightRange").SetRange(data.LightRange);
            Container.GetElement<UICheckButton>("./colors/light").Checked = data.DoLight;
            Container.GetElement<UICheckButton>("./colors/lightSame").Checked = data.LightIsSameAsColor;
        }
        public static void UpdateSearch()
        {
            if (te is null) return;

            UIList list = Container.GetElement<UIList>("./topBox/dustType");
            UIInput input = Container.GetElement<UIInput>("./topBox/dustTypeSearch");
            SearchResult.Clear();
            list.Items.Clear();

            string search = input.Text.ToLower();

            SearchResult.Add(te.spawnData.DustType);
            list.Items.Add(DustSearchMap[te.spawnData.DustType]);

            SelectionLocked = true;
            list.Selected = 0;
            SelectionLocked = false;

            HashSet<short> foundIDs = new HashSet<short>(SearchResult);

            foreach (KeyValuePair<short, string> kvp in DustSearchMap) 
            {
                if (!kvp.Value.ToLower().Contains(search) || foundIDs.Contains(kvp.Key)) continue;

                SearchResult.Add(kvp.Key);
                list.Items.Add(kvp.Value);
            }
        }
        private static void SearchSelected(int index) 
        {
            if (SelectionLocked) return;
            if (te == null) return;
            if (index < 0 || index >= SearchResult.Count) return;

            te.spawnData.DustType = SearchResult[index];
            te.DataUpdated();
        }

        internal static void Unload() 
        {
            Root = null;
            Container = null;
            DustSearchMap = null;
            SearchResult = null;
            SelectionLocked = false;
            te = null;
        }

        public static void Update()
        {
            if (TE != null)
            {
                Vec2f pos = (Vec2f)TE.Position.ToVector2() * 16;
                pos -= Main.LocalPlayer.position;
                pos /= 16;
                if (pos.X > Main.LocalPlayer.lastTileRangeX || pos.Y > Main.LocalPlayer.lastTileRangeY) TE = null;
            }

            Root.Visible = TE != null;
            Root.Update();
        }
        public static void Load()
        {
            DustSearchMap = new Dictionary<short, string>();
            SearchResult = new List<short>();
            Root = new UIRoot(Main.instance)
            {
                DebugEnabled = false,
                Visible = false,
                Elements =
                {
                    //new UIButton()
                    //{
                    //    Pos = new PercentPos(0.5f, -200, 0.5f, -380),
                    //    Size = new PercentPos(80, 24),
                    //    Text = "Reload",
                    //    OnClick = (b) => InitUI(),
                    //    TextAlign = Align.Center,
                    //    Colors = new Colors(Color.White, Color.Transparent, Color.Yellow),
                    //    HoverColors = new Colors(Color.White, Color.White * 0.2f),
                    //}
                }
            };

            Root.UITextures["border"] = TextureManager.Load("Images/UI/PanelBorder");
            Root.UITextures["bg"] = TextureManager.Load("Images/UI/PanelBackground");

            Root.Font = new DynamicFont(Main.fontMouseText);

            Root.OnGlobalActiveChanged += (prev, now) =>
            {
                Main.blockInput = now != null;
            };
            Root.OnGlobalHoverChanged += (prev, now) =>
            {
                if (now is UIButton) 
                {
                    Main.PlaySound(Terraria.ID.SoundID.MenuTick);
                }
            };

            InitUI();

            Type dustId = typeof(Terraria.ID.DustID);

            foreach (FieldInfo field in dustId.GetFields()) 
            {
                if (field.FieldType != typeof(short)) continue;
                DustSearchMap[(short)field.GetValue(null)] = field.Name;
            }
        }
        public static void InitUI()
        {
            Container = new UIContainer()
            {
                Colors = new Colors(Color.White, new Color(63, 82, 151) * 0.7f),
                Pos = new PercentPos(0.5f, -200, 0.5f, -350),
                Size = new PercentPos(400, 700),
                Scale = 1f,
                Extensions =
                {
                    new Draggable() { LimitDragByParent = false },
                    new BorderedBG("bg") { Global = true },
                    new Resizeable(),
                    new Scalable()
                },
                Elements =
                {
                    new UIButton()
                    {
                        Text = "X",
                        Size = new PercentPos(24,24),
                        TextAlign = Align.Center,
                        Pos = new PercentPos(1, -29, 0, 5),
                        Colors = new Colors(Color.Red, new Color(63, 82, 151) * 0.7f),
                        HoverColors = new Colors(Color.Red, new Color(80, 90, 160) * 0.7f),
                        OnClick = (b) => { TE = null; }
                    },
                    new UILabel()
                    {
                        Name = "header",
                        Pos = new PercentPos(0.5f, -100, 0, -20),
                        Size = new PercentPos(200, 40),
                        AutoSize = false,
                        Text = "Particulator",
                        TextScale = 1.5f,
                        TextAlign = Align.Center,
                        Colors = new Colors(Color.White, new Color(63, 82, 151))
                    },

                    new UIContainer()
                    {
                        Name = "topBox",
                        Pos = new PercentPos(30, 19),
                        Size = new PercentPos(1, -60, 0, 120),

                        Elements =
                        {
                            new UIInput()
                            {
                                Name = "dustTypeSearch",
                                Pos = new PercentPos(10, 10),
                                Size = new PercentPos(0.5f, -35, 0, 24),
                                TextAlign = Align.Center,
                                HintText = "Search dust",
                                OnTextChanged = (i) => UpdateSearch()
                            },
                            new UIList()
                            {
                                Name = "dustType",
                                Pos = new PercentPos(10, 43),
                                Size = new PercentPos(0.5f, -35, 0, 60),
                                Items = { "Dust type A", "Dust type B", "Dust type C", "Dust type D" },
                                TextAlign = Align.Center,
                                OnSelectedChanged = (l) => SearchSelected(l.Selected),
                            },
                            new LabelInput()
                            {
                                Name = "dpf",
                                Colors = new Colors(Color.White, Color.Transparent),
                                InputBackground = new Color(63, 82, 151) * 0.7f,
                                Text = "Dusts per frame:",
                                InputWidth = 45,
                                Pos = new PercentPos(0.5f, -13, 0, 5),
                                Size = new PercentPos(0.5f, 10, 0, 34),
                                InputRegex = new Regex("^[0-9]*(\\.[0-9]*)?$"),
                                InputText = "0.1",
                                OnInputTextChanged = (s) =>
                                {
                                    if (float.TryParse(s, out float v))
                                    {
                                        te.spawnData.DustsPerFrame = v;
                                        te.DataUpdated();
                                    }
                                }
                            },
                            new LabelInput()
                            {
                                Name = "maxDust",
                                Colors = new Colors(Color.White, Color.Transparent),
                                InputBackground = new Color(63, 82, 151) * 0.7f,
                                Text = "Max dust:",
                                InputWidth = 45,
                                Pos = new PercentPos(0.5f, -13, 0, 35),
                                Size = new PercentPos(0.5f, 10, 0, 34),
                                InputRegex = new Regex("^-?[0-9]*$"),
                                InputText = "500",
                                OnInputTextChanged = (s) =>
                                {
                                    if (int.TryParse(s, out int v))
                                    {
                                        te.spawnData.MaxDust = v;
                                        te.DataUpdated();
                                    }
                                }
                            },
                            new UICheckButton()
                            {
                                Name = "collision",
                                Pos = new PercentPos(0.5f, -10, 0, 70),
                                Size = new PercentPos(0.35f, 00, 0, 24),
                                Text = "Collision",
                                CheckedColors = new Colors(Color.White, Color.Lime * 0.6f),
                                HoverColors = new Colors(Color.White, new Color(80, 90, 160) * 0.7f),
                                OnCheckedChanged = (b) =>
                                {
                                    te.spawnData.Collision = b.Checked;
                                    te.DataUpdated();
                                }
                            }
                        }
                    },
                    new XYRange()
                    {
                        Name = "spawnRange",
                        Text = "Spawn range",
                        Pos = new PercentPos(30, 145),
                        Size = new PercentPos(0.5f, -35, 0, 100),
                        Min = Vec2f.Zero,
                        Max = new Vec2f(1.5f),
                        OnRangeChanged = (r) => 
                        {
                            te.spawnData.SpawnPos = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new XYRange()
                    {
                        Name = "velocityRange",
                        Text = "Velocity range",
                        Pos = new PercentPos(0.5f, 5, 0, 145),
                        Size = new PercentPos(0.5f, -35, 0, 100),
                        Min = Vec2f.Zero,
                        Max = new Vec2f(1.5f),
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.Velocity = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new XYRange()
                    {
                        Name = "accRange",
                        Text = "Acceleration range",
                        Pos = new PercentPos(30, 250),
                        Size = new PercentPos(0.5f, -35, 0, 100),
                        Min = Vec2f.Zero,
                        Max = new Vec2f(1.5f),
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.Acceleration = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new FloatRange()
                    {
                        Name = "scaleRange",
                        Text = "Scale range",
                        Pos = new PercentPos(0.5f, 5, 0, 250),
                        Size = new PercentPos(0.5f, -35, 0, 73),
                        Min = 0,
                        Max = 1.5f,
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.Scale = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new IntRange()
                    {
                        Name = "timeRange",
                        Text = "Time range",
                        Pos = new PercentPos(0.5f, 5, 0, 328),
                        Size = new PercentPos(0.5f, -35, 0, 73),
                        Min = 20,
                        Max = 60,
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.Time = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new FloatRange()
                    {
                        Name = "initRotationRange",
                        Text = "Initial rotation",
                        Pos = new PercentPos(30, 355),
                        Size = new PercentPos(0.5f, -35, 0, 73),
                        Min = 0,
                        Max = 6.28f,
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.StartRotation = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new FloatRange()
                    {
                        Name = "rotationRange",
                        Text = "Rotation",
                        Pos = new PercentPos(0.5f, 5, 0, 405),
                        Size = new PercentPos(0.5f, -35, 0, 73),
                        Min = 0,
                        Max = 6.28f,
                        OnRangeChanged = (r) =>
                        {
                            te.spawnData.Rotation = r.GetRange();
                            te.DataUpdated();
                        }
                    },
                    new UIItemSlot()
                    {
                        Name = "shader",
                        Pos = new PercentPos(0.5f, -51, 0f, 432),
                        Scale = 0.88f,
                        Context = Terraria.UI.ItemSlot.Context.InventoryItem,
                        TakeItemFromCursor = false,
                        ItemFilter = (i) => i.dye != 0,
                        OnItemChanged = (i) => 
                        {
                            te.spawnData.Dye = i.Item;
                            te.DataUpdated();
                        }
                    },
                    new UILabel()
                    {
                        Pos = new PercentPos(30, 432),
                        Size = new PercentPos(0.5f, -85, 0f, 47),
                        Text = "Dye:",
                        AutoSize = false,
                        TextAlign = Align.Right,
                        Colors = new Colors(Color.White, Color.Transparent)
                    },
                    new UIContainer()
                    {
                        Name = "colors",
                        Pos = new PercentPos(30, 482),
                        Size = new PercentPos(1, -60, 0, 200),
                        Elements =
                        {
                            new UICheckButton()
                            {
                                Name = "light",
                                Text = "Light",
                                Pos = new PercentPos(0.5f, 5, 0, 5),
                                Size = new PercentPos(0.15f, 0, 0, 24),
                                CheckedColors = new Colors(Color.White, Color.Lime * 0.6f),
                                HoverColors = new Colors(Color.White, new Color(80, 90, 160) * 0.7f),
                                OnCheckedChanged = (b) =>
                                {
                                    te.spawnData.DoLight = b.Checked;
                                    te.DataUpdated();
                                }
                            },
                            new UICheckButton()
                            {
                                Name = "lightSame",
                                Text = "Same as Color",
                                Pos = new PercentPos(0.65f, 10, 0, 5),
                                Size = new PercentPos(0.35f, -16, 0, 24),
                                CheckedColors = new Colors(Color.White, Color.Lime * 0.6f),
                                HoverColors = new Colors(Color.White, new Color(80, 90, 160) * 0.7f),
                                OnCheckedChanged = (b) =>
                                {
                                    te.spawnData.LightIsSameAsColor = b.Checked;
                                    te.DataUpdated();
                                }
                            },
                            new ColorRange()
                            {
                                Name = "colorRange",
                                Pos = new PercentPos(6, 34),
                                Size = new PercentPos(0.5f, -11, 0, 160),
                                Min = Color.Black,
                                Max = Color.White,
                                OnRangeChanged = (r) =>
                                {
                                    te.spawnData.ColorRange = r.GetRange();
                                    te.DataUpdated();
                                }
                            },
                            new ColorRange()
                            {
                                Name = "lightRange",
                                Pos = new PercentPos(0.5f, 5, 0, 34),
                                Size = new PercentPos(0.5f, -11, 0, 160),
                                Text = "Light color range",
                                Min = Color.Black,
                                Max = Color.White,
                                NoAlpha = true,
                                OnRangeChanged = (r) =>
                                {
                                    te.spawnData.LightRange = r.GetRange();
                                    te.DataUpdated();
                                }
                            },
                        }
                    }
                }
            };
            Root.Elements.Remove<UIContainer>();
            Root.Elements.Add(Container);
            UpdateData();
        }
        public class DynamicFont : IFont
        {
            public DynamicSpriteFont Font;

            public DynamicFont(DynamicSpriteFont font)
            {
                Font = font;
            }

            public int LineSpacing => Font.LineSpacing;
            public float CharacterSpacing => Font.CharacterSpacing;
            public Vec2f DrawOffset => new Vec2f(0, 5);

            public void Draw(SpriteBatch spriteBatch, string text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth)
            {
                spriteBatch.DrawString(Font, text, pos, color, angle, origin, scale * 0.8f, spriteEffects, layerDepth);
            }

            public void Draw(SpriteBatch spriteBatch, StringBuilder text, Vec2f pos, Color color, float angle, Vec2f origin, Vec2f scale, SpriteEffects spriteEffects, float layerDepth)
            {
                spriteBatch.DrawString(Font, text, pos, color, angle, origin, scale * 0.8f, spriteEffects, layerDepth);
            }

            public Vec2f MeasureString(string text)
            {
                return (Vec2f)Font.MeasureString(text) * 0.8f;
            }

            public Vec2f MeasureString(StringBuilder text)
            {
                return MeasureString(text.ToString());
            }
        }
    }
}
