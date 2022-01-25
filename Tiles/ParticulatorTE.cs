using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Particulator.Tiles
{
    public class ParticulatorTE : ModTileEntity
    {
        public HashSet<int> myDusts = new HashSet<int>();
        public SpawnParams spawnData = new SpawnParams();

        private List<int> removeDusts = new List<int>();
        private List<int> removeMyDusts = new List<int>();


        float dpfCounter = 0f;
        static Vector2 InvertY = new Vector2(1, -1);

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == ModContent.TileType<Particulator>();
        }

        public void DataUpdated()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)1);
                packet.Write(Position.X);
                packet.Write(Position.Y);
                NetSend(packet, false);
                packet.Send();
            }
        }

        public override void Update()
        {
            if (Main.netMode == NetmodeID.Server) return;

            dpfCounter += spawnData.DustsPerFrame;
            while (dpfCounter >= 1f)
            {
                if (spawnData.MaxDust < 0 || myDusts.Count < spawnData.MaxDust)
                {
                    int id = CreateDust();
                    if (id != -1) myDusts.Add(id);
                }
                dpfCounter -= 1f;
            }

            foreach (int id in myDusts)
            {
                Dust dust = Main.dust[id];
                if (!dust.active || !(dust.customData is DustData data))
                    removeMyDusts.Add(id);
                else if (data.Owner == ID) UpdateDust(Main.dust[id]);
                else removeDusts.Add(id);
            }

            foreach (int id in removeMyDusts)
            {
                ClearDust(Main.dust[id]);
                myDusts.Remove(id);
            }

            foreach (int id in removeDusts)
            {
                myDusts.Remove(id);
            }

            removeDusts.Clear();
            removeMyDusts.Clear();
        }

        public void ClearDust(Dust dust)
        {
            dust.customData = null;
            dust.noGravity = false;
        }

        public int CreateDust()
        {
            Vector2 offset = (spawnData.SpawnPos.Random() - Vector2.UnitY) * InvertY;
            Vector2 pos = Position.ToVector2() * 16 + offset * 16;
            Vector2 velocity = spawnData.Velocity.Random() * InvertY;

            int id = Dust.NewDust(pos, 1, 1, spawnData.DustType);

            if (id == 6000)
            {
                Main.dust[id].active = false;
                return -1;
            }

            Dust dust = Main.dust[id];
            float scale = spawnData.Scale.Random();

            Vector2 center = new Vector2(dust.frame.Width, dust.frame.Height) * 0.5f;
            dust.position -= center;

            dust.velocity = velocity;
            dust.noGravity = true;
            dust.rotation = spawnData.StartRotation.Random();

            dust.noLight = !spawnData.DoLight;
            dust.color = spawnData.ColorRange.Random();
            dust.shader = spawnData.Dye == null ? null :
                GameShaders.Armor.GetSecondaryShader(spawnData.Dye.dye, Main.LocalPlayer);

            dust.customData = new DustData()
            {
                Owner = ID,
                Scale = scale,
                LifeTime = spawnData.Time.Random(),
                Rotation = spawnData.Rotation.Random(),
                LightColor = (spawnData.LightIsSameAsColor ? dust.color : spawnData.LightRange.Random()).ToVector3(),
                Acceleration = spawnData.Acceleration.Random() * InvertY,
            };

            return id;
        }

        public void UpdateDust(Dust dust)
        {
            if (!(dust.customData is DustData data)) return;

            if (data.Time > data.LifeTime) { dust.active = false; return; }

            Vector2 posDiff = Vector2.Zero;
            if (dust.velocity.X != 0 || dust.velocity.Y != 0)
            {
                posDiff = dust.velocity;
                if (spawnData.Collision)
                {
                    if (Collision.SolidCollision(dust.position, 1, 1))
                    {
                        data.Rotation = 0f;
                        dust.velocity = Vector2.Zero;
                        posDiff = Vector2.Zero;
                    }

                    //Vector4 collision = Collision.SlopeCollision(dust.position, dust.velocity, 1, 1);
                    //posDiff = new Vector2(collision.Z, collision.W);
                }
            }

            data.Time++;
            dust.position += posDiff;
            dust.velocity += data.Acceleration;
            dust.rotation += data.Rotation;

            float time = ((float)data.LifeTime - data.Time) / data.LifeTime;
            dust.scale = data.Scale * time;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, data.LightColor * time);

        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public override void OnKill()
        {
            foreach (int id in myDusts)
            {
                Dust dust = Main.dust[id];
                ClearDust(dust);
            }
        }

        public class SpawnParams
        {
            public short DustType = DustID.Fire;
            public float DustsPerFrame = 20f;
            public Vector2Range SpawnPos = new Vector2Range(0f, 1.5f, 1f, 1.5f);
            public Vector2Range Velocity = new Vector2Range(0f, 1f, 0f, 1.2f);
            public Vector2Range Acceleration = new Vector2Range(0f, 0f, 0f, 0f);
            public FloatRange Scale = new FloatRange(1f, 2f);
            public IntRange Time = new IntRange(30, 120);
            public FloatRange StartRotation = new FloatRange(0, 6.29f);
            public FloatRange Rotation = new FloatRange(0, 6.29f);

            public bool Collision = false;
            public Item Dye = null;

            public int MaxDust = 100;

            public bool DoLight = true;
            public bool LightIsSameAsColor = true;

            public ColorRange LightRange = new ColorRange(Color.Lime, Color.Lime);
            public ColorRange ColorRange = new ColorRange(Color.Orange, Color.Yellow);
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["spawn"] = new TagCompound() 
                {
                    ["type"]  = spawnData.DustType,
                    ["dpf"]   = spawnData.DustsPerFrame,
                    ["spawn"] = spawnData.SpawnPos,
                    ["vel"]   = spawnData.Velocity,
                    ["acc"]   = spawnData.Acceleration,
                    ["scale"] = spawnData.Scale,
                    ["time"]  = spawnData.Time,
                    ["strot"] = spawnData.StartRotation,
                    ["rot"]   = spawnData.Rotation,
                    ["coll"]  = spawnData.Collision,
                    ["dye"]   = spawnData.Dye == null ? null : ItemIO.Save(spawnData.Dye),
                    ["max"]   = spawnData.MaxDust,
                    ["light"] = spawnData.DoLight,
                    ["same"]  = spawnData.LightIsSameAsColor,
                    ["lightcolor"] = spawnData.LightRange,
                    ["color"] = spawnData.ColorRange
                }
            };
        }

        public override void Load(TagCompound tag)
        {
            if (tag.ContainsKey("spawn")) 
            {
                TagCompound spawn = tag["spawn"] as TagCompound;

                spawn.TryLoad("type",  ref spawnData.DustType);
                spawn.TryLoad("dpf",   ref spawnData.DustsPerFrame);
                spawn.TryLoad("spawn", ref spawnData.SpawnPos);
                spawn.TryLoad("vel",   ref spawnData.Velocity);
                spawn.TryLoad("acc",   ref spawnData.Acceleration);
                spawn.TryLoad("scale", ref spawnData.Scale);
                spawn.TryLoad("time",  ref spawnData.Time);
                spawn.TryLoad("strot", ref spawnData.StartRotation);
                spawn.TryLoad("rot",   ref spawnData.Rotation);
                spawn.TryLoad("coll",  ref spawnData.Collision);
                if (spawn.ContainsKey("dye")) spawnData.Dye = spawn["dye"] == null ? null : ItemIO.Load(spawn["dye"] as TagCompound);
                spawn.TryLoad("max",   ref spawnData.MaxDust);
                spawn.TryLoad("light", ref spawnData.DoLight);
                spawn.TryLoad("same",  ref spawnData.LightIsSameAsColor);
                spawn.TryLoad("lightcolor", ref spawnData.LightRange);
                spawn.TryLoad("color", ref spawnData.ColorRange);
            }
        }

        public override void NetSend(BinaryWriter writer, bool lightSend)
        {
            long pos = writer.BaseStream.Position;
            TagIO.Write(Save(), writer);
            long diff = writer.BaseStream.Position - pos;
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive)
        {
            long pos = reader.BaseStream.Position;
            Load(TagIO.Read(reader));
            long diff = reader.BaseStream.Position - pos;
        }

        public class DustData
        {
            public int Owner;
            public Vector2 Acceleration;
            public float Scale;
            public int LifeTime;
            public int Time;
            public float Rotation;
            public Vector3 LightColor;
        }

        public struct Vector2Range
        {
            public Vector2 Max;
            public Vector2 Min;

            public Vector2Range(Vector2 min, Vector2 max)
            {
                Max = max;
                Min = min;
            }

            public Vector2Range(float minx, float miny, float maxx, float maxy)
            {
                Min = new Vector2(minx, miny);
                Max = new Vector2(maxx, maxy);
            }

            public Vector2 Random()
            {
                float x;
                float y;

                if (Max.X == Min.X) x = Max.X;
                else if (Max.X > Min.Y) x = Main.rand.NextFloat(Min.X, Max.X);
                else x = Main.rand.NextFloat(Max.X, Min.X);

                if (Max.Y == Min.Y) y = Max.Y;
                else if (Max.Y > Min.Y) y = Main.rand.NextFloat(Min.Y, Max.Y);
                else y = Main.rand.NextFloat(Max.Y, Min.Y);

                return new Vector2(x, y);
            }
        }
        public struct IntRange
        {
            public int Max;
            public int Min;

            public IntRange(int min, int max)
            {
                Max = max;
                Min = min;
            }

            public int Random()
            {
                if (Max == Min) return Max;

                if (Max > Min)
                    return Main.rand.Next(Min, Max);
                else
                    return Main.rand.Next(Max, Min);
            }
        }
        public struct FloatRange
        {
            public float Max;
            public float Min;

            public FloatRange(float min, float max)
            {
                Max = max;
                Min = min;
            }

            public float Random()
            {
                if (Max == Min) return Max;

                if (Max > Min)
                    return Main.rand.NextFloat(Min, Max);
                else
                    return Main.rand.NextFloat(Max, Min);
            }
        }
        public struct ColorRange
        {
            public Color Max;
            public Color Min;

            public ColorRange(Color min, Color max)
            {
                Max = max;
                Min = min;
            }

            public ColorRange(int minr, int ming, int minb, int maxr, int maxg, int maxb)
            {
                Max = new Color(maxr, maxg, maxb);
                Min = new Color(minr, ming, minb);
            }

            public ColorRange(int minr, int ming, int minb, int mina, int maxr, int maxg, int maxb, int maxa)
            {
                Max = new Color(maxr, maxg, maxb, maxa);
                Min = new Color(minr, ming, minb, mina);
            }

            public Color Random()
            {
                float r = Main.rand.NextFloat();
                float g = Main.rand.NextFloat();
                float b = Main.rand.NextFloat();
                float a = Main.rand.NextFloat();

                return new Color
                {
                    R = (byte)(Min.R * (1 - r) + Max.R * r),
                    G = (byte)(Min.G * (1 - g) + Max.G * g),
                    B = (byte)(Min.B * (1 - b) + Max.B * b),
                    A = (byte)(Min.A * (1 - a) + Max.A * a),
                };
            }
        }
    }
}
