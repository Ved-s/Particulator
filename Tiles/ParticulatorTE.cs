using Microsoft.Xna.Framework;
using System;
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
        public HashSet<int> MyDusts = new HashSet<int>();
        public SpawnParams CurrentSpawnData => SpawnDataArray[CurrentDataIndex];

        public SpawnParams[] SpawnDataArray = new SpawnParams[4]; // Selected by wires, 0 - Red, 1 - Blue, 2 - Green, 3 - Yellow
        public int CurrentDataIndex = 0;

        private List<int> RemoveDusts = new List<int>();
        private List<int> RemoveMyDusts = new List<int>();

        private float DPFCounter = 0f;
        static Vector2 InvertY = new Vector2(1, -1);

        public ParticulatorTE()
        {
            for (int i = 0; i < 4; i++)
                SpawnDataArray[i] = new SpawnParams();
        }

        public override void Update()
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (CurrentSpawnData.Enabled)
            {
                DPFCounter += CurrentSpawnData.DustsPerFrame;
                while (DPFCounter >= 1f)
                {
                    if (CurrentSpawnData.MaxDust < 0 || MyDusts.Count < CurrentSpawnData.MaxDust)
                    {
                        int id = CreateDust();
                        if (id != -1) MyDusts.Add(id);
                    }
                    DPFCounter -= 1f;
                }
            }

            foreach (int id in MyDusts)
            {
                Dust dust = Main.dust[id];
                if (!dust.active || !(dust.customData is DustData data))
                    RemoveMyDusts.Add(id);
                else if (data.Owner == ID) UpdateDust(Main.dust[id]);
                else RemoveDusts.Add(id);
            }

            foreach (int id in RemoveMyDusts)
            {
                ClearDust(Main.dust[id]);
                MyDusts.Remove(id);
            }

            foreach (int id in RemoveDusts)
            {
                MyDusts.Remove(id);
            }

            RemoveDusts.Clear();
            RemoveMyDusts.Clear();
        }
        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == ModContent.TileType<Particulator>();
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
            foreach (int id in MyDusts)
            {
                Dust dust = Main.dust[id];
                ClearDust(dust);
            }
        }

        public void DataUpdated(SyncDataType type)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)1);
                packet.Write(Position.X);
                packet.Write(Position.Y);
                packet.Write((byte)CurrentDataIndex);
                packet.Write((byte)type);

                SpawnParams d = CurrentSpawnData;

                switch (type)
                {
                    case SyncDataType.State:        break;
                    case SyncDataType.Enabled:      packet.Write(d.Enabled);            break;
                    case SyncDataType.Type:         packet.Write(d.DustType);           break;
                    case SyncDataType.DPF:          packet.Write(d.DustsPerFrame);      break;
                    case SyncDataType.SpawnPos:     d.SpawnPos.Write(packet);           break;
                    case SyncDataType.Velocity:     d.Velocity.Write(packet);           break;
                    case SyncDataType.Acceleration: d.Acceleration.Write(packet);       break;
                    case SyncDataType.Scale:        d.Scale.Write(packet);              break;
                    case SyncDataType.Time:         d.Time.Write(packet);               break;
                    case SyncDataType.StartRot:     d.StartRotation.Write(packet);      break;
                    case SyncDataType.Rotation:     d.Rotation.Write(packet);           break;
                    case SyncDataType.Collision:    packet.Write(d.Collision);          break;
                    case SyncDataType.Dye:          packet.Write(d.Dye?.type ?? 0);     break;
                    case SyncDataType.MaxDust:      packet.Write(d.MaxDust);            break;
                    case SyncDataType.DoLight:      packet.Write(d.DoLight);            break;
                    case SyncDataType.SameLight:    packet.Write(d.LightIsSameAsColor); break;
                    case SyncDataType.Light:        d.LightRange.Write(packet);         break;
                    case SyncDataType.Color:        d.ColorRange.Write(packet);         break;
                }

                packet.Send();
            }
        }
        public void ReceiveData(BinaryReader reader)
        {
            byte dataIndex = reader.ReadByte();
            SpawnParams d = SpawnDataArray[dataIndex];

            SyncDataType syncType = (SyncDataType)reader.ReadByte();

            switch (syncType)
            {
                case SyncDataType.State:        CurrentDataIndex = dataIndex;          break;
                case SyncDataType.Enabled:      d.Enabled = reader.ReadBoolean();      break;
                case SyncDataType.Type:         d.DustType = reader.ReadInt16();       break;
                case SyncDataType.DPF:          d.DustsPerFrame = reader.ReadSingle(); break;
                case SyncDataType.SpawnPos:     d.SpawnPos.Read(reader);               break;
                case SyncDataType.Velocity:     d.Velocity.Read(reader);               break;
                case SyncDataType.Acceleration: d.Acceleration.Read(reader);           break;
                case SyncDataType.Scale:        d.Scale.Read(reader);                  break;
                case SyncDataType.Time:         d.Time.Read(reader);                   break;
                case SyncDataType.StartRot:     d.StartRotation.Read(reader);          break;
                case SyncDataType.Rotation:     d.Rotation.Read(reader);               break;
                case SyncDataType.Collision:    d.Collision = reader.ReadBoolean();    break;
                case SyncDataType.Dye:
                    int dye = reader.ReadInt32();
                    if (dye == 0)
                        d.Dye = null;
                    else
                    {
                        d.Dye = new Item();
                        d.Dye.SetDefaults(dye);
                    }
                    break;
                case SyncDataType.MaxDust:      d.MaxDust = reader.ReadInt32();        break;
                case SyncDataType.DoLight:      d.DoLight = reader.ReadBoolean();      break;
                case SyncDataType.SameLight:    d.LightIsSameAsColor = reader.ReadBoolean(); break;
                case SyncDataType.Light:        d.LightRange.Read(reader);         break;
                case SyncDataType.Color:        d.ColorRange.Read(reader);         break;
            }

            if (Interface.TE == this) Interface.UpdateData();
        }

        public void Wire()
        {
            if (Wiring._currentWireColor - 1 == CurrentDataIndex)
            {
                CurrentSpawnData.Enabled = !CurrentSpawnData.Enabled;
                DataUpdated(SyncDataType.Enabled);
            }
            else
            {
                CurrentDataIndex = Wiring._currentWireColor - 1;
                DataUpdated(SyncDataType.State);
            }

            
            if (Interface.TE == this) Interface.UpdateData();
        }

        public void ClearDust(Dust dust)
        {
            dust.customData = null;
            dust.noGravity = false;
        }
        public int CreateDust()
        {
            Vector2 offset = (CurrentSpawnData.SpawnPos.Random() - Vector2.UnitY) * InvertY;
            Vector2 pos = Position.ToVector2() * 16 + offset * 16;
            Vector2 velocity = CurrentSpawnData.Velocity.Random() * InvertY;

            int id = Dust.NewDust(pos, 1, 1, CurrentSpawnData.DustType);

            if (id == 6000)
            {
                Main.dust[id].active = false;
                return -1;
            }

            Dust dust = Main.dust[id];
            float scale = CurrentSpawnData.Scale.Random();

            Vector2 center = new Vector2(dust.frame.Width, dust.frame.Height) * 0.5f;
            dust.position -= center;

            dust.velocity = velocity;
            dust.noGravity = true;
            dust.rotation = CurrentSpawnData.StartRotation.Random();

            dust.noLight = !CurrentSpawnData.DoLight;
            dust.color = CurrentSpawnData.ColorRange.Random();
            dust.shader = CurrentSpawnData.Dye == null ? null :
                GameShaders.Armor.GetSecondaryShader(CurrentSpawnData.Dye.dye, Main.LocalPlayer);

            dust.customData = new DustData()
            {
                Owner = ID,
                Scale = scale,
                LifeTime = CurrentSpawnData.Time.Random(),
                Rotation = CurrentSpawnData.Rotation.Random(),
                LightColor = (CurrentSpawnData.LightIsSameAsColor ? dust.color : CurrentSpawnData.LightRange.Random()).ToVector3(),
                Acceleration = CurrentSpawnData.Acceleration.Random() * InvertY,
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
                if (CurrentSpawnData.Collision)
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

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["current"] = CurrentDataIndex,
                ["red"] = IndexedSave(0),
                ["blue"] = IndexedSave(1),
                ["green"] = IndexedSave(2),
                ["yellow"] = IndexedSave(3),
            };
        }
        public override void Load(TagCompound tag)
        {
            if (tag.ContainsKey("spawn"))
            {
                CurrentDataIndex = 0;
                IndexedLoad(0, tag["spawn"] as TagCompound);
            }
            else
            {
                tag.TryLoad("current", ref CurrentDataIndex);
                IndexedLoad(0, tag["red"] as TagCompound);
                IndexedLoad(1, tag["blue"] as TagCompound);
                IndexedLoad(2, tag["green"] as TagCompound);
                IndexedLoad(3, tag["yellow"] as TagCompound);
            }
        }

        private TagCompound IndexedSave(int index)
        {
            SpawnParams param = SpawnDataArray[index];

            return new TagCompound()
            {
                ["enabled"] = param.Enabled,
                ["type"] = param.DustType,
                ["dpf"] = param.DustsPerFrame,
                ["spawn"] = param.SpawnPos,
                ["vel"] = param.Velocity,
                ["acc"] = param.Acceleration,
                ["scale"] = param.Scale,
                ["time"] = param.Time,
                ["strot"] = param.StartRotation,
                ["rot"] = param.Rotation,
                ["coll"] = param.Collision,
                ["dye"] = param.Dye == null ? null : ItemIO.Save(param.Dye),
                ["max"] = param.MaxDust,
                ["light"] = param.DoLight,
                ["same"] = param.LightIsSameAsColor,
                ["lightcolor"] = param.LightRange,
                ["color"] = param.ColorRange
            };
        }
        private void IndexedLoad(int index, TagCompound tag)
        {
            if (tag is null) return;

            SpawnParams param = SpawnDataArray[index];

            tag.TryLoad("enabled", ref param.Enabled);
            tag.TryLoad("type", ref param.DustType);
            tag.TryLoad("dpf", ref param.DustsPerFrame);
            tag.TryLoad("spawn", ref param.SpawnPos);
            tag.TryLoad("vel", ref param.Velocity);
            tag.TryLoad("acc", ref param.Acceleration);
            tag.TryLoad("scale", ref param.Scale);
            tag.TryLoad("time", ref param.Time);
            tag.TryLoad("strot", ref param.StartRotation);
            tag.TryLoad("rot", ref param.Rotation);
            tag.TryLoad("coll", ref param.Collision);
            if (tag.ContainsKey("dye")) param.Dye = tag["dye"] == null ? null : ItemIO.Load(tag["dye"] as TagCompound);
            tag.TryLoad("max", ref param.MaxDust);
            tag.TryLoad("light", ref param.DoLight);
            tag.TryLoad("same", ref param.LightIsSameAsColor);
            tag.TryLoad("lightcolor", ref param.LightRange);
            tag.TryLoad("color", ref param.ColorRange);
        }

        public override void NetSend(BinaryWriter writer, bool lightSend)
        {
            for (int i = 0; i < 4; i++)
            {
                SpawnParams data = SpawnDataArray[i];
                writer.Write(data.Enabled);
                writer.Write(data.DustType);
                writer.Write(data.DustsPerFrame);
                data.SpawnPos.Write(writer);
                data.Velocity.Write(writer);
                data.Acceleration.Write(writer);
                data.Scale.Write(writer);
                data.Time.Write(writer);
                data.StartRotation.Write(writer);
                data.Rotation.Write(writer);
                writer.Write(data.Collision);
                writer.Write(data.Dye?.type ?? 0);
                writer.Write(data.MaxDust);
                writer.Write(data.DoLight);
                writer.Write(data.LightIsSameAsColor);
                data.LightRange.Write(writer);
                data.ColorRange.Write(writer);
            }
        }
        public override void NetReceive(BinaryReader reader, bool lightReceive)
        {
            for (int i = 0; i < 4; i++)
            {
                SpawnParams data = SpawnDataArray[i];

                data.Enabled = reader.ReadBoolean();
                data.DustType = reader.ReadInt16();
                data.DustsPerFrame = reader.ReadSingle();
                data.SpawnPos.Read(reader);
                data.Velocity.Read(reader);
                data.Acceleration.Read(reader);
                data.Scale.Read(reader);
                data.Time.Read(reader);
                data.StartRotation.Read(reader);
                data.Rotation.Read(reader);
                data.Collision = reader.ReadBoolean();
                int dye = reader.ReadInt32();
                data.MaxDust = reader.ReadInt32();
                data.DoLight = reader.ReadBoolean();
                data.LightIsSameAsColor = reader.ReadBoolean();
                data.LightRange.Read(reader);
                data.ColorRange.Read(reader);

                if (dye == 0)
                    data.Dye = null;
                else 
                {
                    data.Dye = new Item();
                    data.Dye.SetDefaults(dye);
                }
            }
        }

        public class SpawnParams
        {
            public bool Enabled = true;

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

            public void Write(BinaryWriter writer)
            {
                writer.Write(Min.X);
                writer.Write(Min.Y);
                writer.Write(Max.X);
                writer.Write(Max.Y);
            }

            public void Read(BinaryReader reader)
            {
                Min.X = reader.ReadSingle();
                Min.Y = reader.ReadSingle();
                Max.X = reader.ReadSingle();
                Max.Y = reader.ReadSingle();
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
            public void Write(BinaryWriter writer)
            {
                writer.Write(Min);
                writer.Write(Max);
            }

            public void Read(BinaryReader reader)
            {
                Min = reader.ReadInt32();
                Max = reader.ReadInt32();
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
            public void Write(BinaryWriter writer)
            {
                writer.Write(Min);
                writer.Write(Max);
            }

            public void Read(BinaryReader reader)
            {
                Min = reader.ReadSingle();
                Max = reader.ReadSingle();
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

            public void Write(BinaryWriter writer)
            {
                writer.Write(Min.PackedValue);
                writer.Write(Max.PackedValue);
            }

            public void Read(BinaryReader reader)
            {
                Min.PackedValue = reader.ReadUInt32();
                Max.PackedValue = reader.ReadUInt32();
            }
        }

        public enum SyncDataType : byte
        {
            State, 
            Enabled,
            Type,
            DPF,
            SpawnPos,
            Velocity,
            Acceleration,
            Scale,
            Time,
            StartRot,
            Rotation,
            Collision,
            Dye,
            MaxDust,
            DoLight,
            SameLight,
            Light,
            Color
        }
    }
}
