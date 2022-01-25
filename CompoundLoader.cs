using Microsoft.Xna.Framework;
using Particulator.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace Particulator
{
    static class CompoundLoader
    {
        public static void Load() 
        {
            TagSerializer.AddSerializer(new Vector2RangeSerializer());
            TagSerializer.AddSerializer(new FloatRangeSerializer());
            TagSerializer.AddSerializer(new IntRangeSerializer());
            TagSerializer.AddSerializer(new ColorRangeSerializer());
        }

        class Vector2RangeSerializer : TagSerializer<ParticulatorTE.Vector2Range, TagCompound>
        {
            public override ParticulatorTE.Vector2Range Deserialize(TagCompound tag)
            {
                return new ParticulatorTE.Vector2Range()
                {
                    Min = tag.ContainsKey("min") ? tag.Get<Vector2>("min") : Vector2.Zero,
                    Max = tag.ContainsKey("min") ? tag.Get<Vector2>("max") : Vector2.Zero
                };
            }

            public override TagCompound Serialize(ParticulatorTE.Vector2Range value)
            {
                return new TagCompound() { ["min"] = value.Min, ["max"] = value.Max };
            }
        }
        class FloatRangeSerializer : TagSerializer<ParticulatorTE.FloatRange, TagCompound>
        {
            public override ParticulatorTE.FloatRange Deserialize(TagCompound tag)
            {
                return new ParticulatorTE.FloatRange()
                {
                    Min = tag.ContainsKey("min") ? tag.Get<float>("min") : 0,
                    Max = tag.ContainsKey("min") ? tag.Get<float>("max") : 0
                };
            }

            public override TagCompound Serialize(ParticulatorTE.FloatRange value)
            {
                return new TagCompound() { ["min"] = value.Min, ["max"] = value.Max };
            }
        }
        class IntRangeSerializer : TagSerializer<ParticulatorTE.IntRange, TagCompound>
        {
            public override ParticulatorTE.IntRange Deserialize(TagCompound tag)
            {
                return new ParticulatorTE.IntRange()
                {
                    Min = tag.ContainsKey("min") ? tag.Get<int>("min") : 0,
                    Max = tag.ContainsKey("min") ? tag.Get<int>("max") : 0
                };
            }

            public override TagCompound Serialize(ParticulatorTE.IntRange value)
            {
                return new TagCompound() { ["min"] = value.Min, ["max"] = value.Max };
            }
        }
        class ColorRangeSerializer : TagSerializer<ParticulatorTE.ColorRange, TagCompound>
        {
            public override ParticulatorTE.ColorRange Deserialize(TagCompound tag)
            {
                return new ParticulatorTE.ColorRange()
                {
                    Min = tag.ContainsKey("min") ? tag.Get<Color>("min") : Color.Transparent,
                    Max = tag.ContainsKey("min") ? tag.Get<Color>("max") : Color.Transparent
                };
            }

            public override TagCompound Serialize(ParticulatorTE.ColorRange value)
            {
                return new TagCompound() { ["min"] = value.Min, ["max"] = value.Max };
            }
        }
    }
}
