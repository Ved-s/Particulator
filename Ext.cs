using Particulator.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using UIKit;
using UIKit.Modular;

namespace Particulator
{
    public static class Ext
    {
        public static void SetRange(this XYRange xy, ParticulatorTE.Vector2Range range)
        {
            xy.Max = (Vec2f)range.Max;
            xy.Min = (Vec2f)range.Min;
        }
        public static ParticulatorTE.Vector2Range GetRange(this XYRange xy) 
        {
            return new ParticulatorTE.Vector2Range(xy.Min, xy.Max);
        }
        public static void SetRange(this FloatRange xy, ParticulatorTE.FloatRange range)
        {
            xy.Max = range.Max;
            xy.Min = range.Min;
        }
        public static ParticulatorTE.FloatRange GetRange(this FloatRange xy)
        {
            return new ParticulatorTE.FloatRange(xy.Min, xy.Max);
        }
        public static void SetRange(this IntRange xy, ParticulatorTE.IntRange range)
        {
            xy.Max = range.Max;
            xy.Min = range.Min;
        }
        public static ParticulatorTE.IntRange GetRange(this IntRange xy)
        {
            return new ParticulatorTE.IntRange(xy.Min, xy.Max);
        }
        public static void SetRange(this ColorRange xy, ParticulatorTE.ColorRange range)
        {
            xy.Max = range.Max;
            xy.Min = range.Min;
        }
        public static ParticulatorTE.ColorRange GetRange(this ColorRange xy)
        {
            return new ParticulatorTE.ColorRange(xy.Min, xy.Max);
        }
        public static bool TryLoad<T>(this TagCompound tag, string key, ref T value) 
        {
            if (tag.ContainsKey(key)) 
            {
                value = tag.Get<T>(key);
                return true;
            }
            return false;
        }
    }
}
