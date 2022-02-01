using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;
using UIKit.UI;
using System.IO;
using Particulator.Tiles;
using Terraria.ModLoader.IO;
using System;

namespace Particulator
{
	public class Particulator : Mod
	{
        public override void Load()
        {
            IL.Terraria.Dust.UpdateDust += Dust_UpdateDust;
            IL.Terraria.Dust.GetAlpha += Dust_GetAlpha;
        }

        public override void Unload()
        {
            IL.Terraria.Dust.UpdateDust -= Dust_UpdateDust;
            IL.Terraria.Dust.GetAlpha -= Dust_GetAlpha;
        }

        public override void LoadResources()
        {
            if (Main.netMode != Terraria.ID.NetmodeID.Server) Interface.Load();
            CompoundLoader.Load();
            base.LoadResources();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(layers.FindIndex(l => l.Name == "Vanilla: Mouse Text"),
                new LegacyGameInterfaceLayer("Particulator: UIRoot",
                () =>
                {
                    Interface.Root.Draw(Main.spriteBatch);
                    return true;
                }, InterfaceScaleType.UI
                ));

            base.ModifyInterfaceLayers(layers);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            Interface.Update();

            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            foreach (TileEntity te in TileEntity.ByID.Values) 
            {
                    if (te is ParticulatorTE) te.Update();
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte type = reader.ReadByte();

            if (type == 1)
            {
                short x = reader.ReadInt16();
                short y = reader.ReadInt16();

                int index = ModContent.GetInstance<ParticulatorTE>().Find(x, y);
                if (index == -1) 
                {
                    Logger.WarnFormat("Cannot find TE at {0}, {1}", x, y);
                    return;
                }

                long pos = reader.BaseStream.Position;

                ParticulatorTE te = (ParticulatorTE)TileEntity.ByID[index];
                te.ReceiveData(reader);

                long diff = reader.BaseStream.Position - pos;

                if (Main.netMode == Terraria.ID.NetmodeID.Server)
                {
                    ModPacket packet = GetPacket();
                    packet.Write((byte)1);
                    packet.Write(x);
                    packet.Write(y);
                    reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    packet.Write(reader.ReadBytes((int)diff));
                    packet.Send(-1, whoAmI);
                }
                else 
                {
                    if (Interface.TE == te) Interface.UpdateData();
                }
            }
        }

        private void Dust_GetAlpha(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Index = 0;

            ILLabel @continue = c.DefineLabel();

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarga, 1);
            c.Emit<Particulator>(OpCodes.Call, "GetDustAhplhaHook");
            c.Emit(OpCodes.Brfalse, @continue);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(@continue);
        }
        private void Dust_UpdateDust(MonoMod.Cil.ILContext il)
        {
            /*
             * 	IL_004c: br IL_5233
	                // loop start (head: IL_5233)

	        	    IL_0051: ldsfld class Terraria.Dust[] Terraria.Main::dust
	        	    IL_0056: ldloc.3
	        	    IL_0057: ldelem.ref
	        	    IL_0058: stloc.s 4

	        	    // if (i < Main.maxDustToDraw)
	        	    IL_005a: ldloc.3
		            IL_005b: ldsfld int32 Terraria.Main::maxDustToDraw
		            IL_0060: bge IL_5227

		            // if (!dust.active)
		            IL_0065: ldloc.s 4
		            IL_0067: ldfld bool Terraria.Dust::active
		            IL_006c: brfalse IL_522f
             */

            ILLabel loopEnd = null;
            int i = -1;
            int dust = -1;

            ILCursor c = new ILCursor(il);
            if (!c.TryGotoNext(
                x => x.MatchBr(out _),

                x => x.MatchLdsfld<Main>("dust"),
                x => x.MatchLdloc(out i),
                x => x.MatchLdelemRef(),
                x => x.MatchStloc(out dust),

                x => x.MatchLdloc(i),
                x => x.MatchLdsfld<Main>("maxDustToDraw"),
                x => x.MatchBge(out _),

                x => x.MatchLdloc(out _),
                x => x.MatchLdfld<Dust>("active"),
                x => x.MatchBrfalse(out loopEnd)
                )) 
            {
                Logger.WarnFormat("Could not patch {0}", il.Method.FullName);
                return;
            }

            c.Index += 5;
            c.Emit(OpCodes.Ldloc, dust);
            c.Emit(OpCodes.Ldloc, i);
            c.Emit<Particulator>(OpCodes.Call, "ShouldUpdateDust");
            c.Emit(OpCodes.Brfalse, loopEnd);
        }
        static bool ShouldUpdateDust(Dust dust, int index) 
        {
            if (dust.customData is Tiles.ParticulatorTE.DustData) return false;
            return true;
        }
        static bool GetDustAhplhaHook(Dust self, ref Color color) 
        {
            if (self.customData is Tiles.ParticulatorTE.DustData)
            {
                float r = self.color.R / 255f;
                float g = self.color.G / 255f;
                float b = self.color.B / 255f;
                float a = self.color.A / 255f;

                color = new Color 
                {
                    R = (byte)(r * color.R),
                    G = (byte)(g * color.G),
                    B = (byte)(b * color.B),
                    A = (byte)(a * color.A),
                };
                return true;
            }
            return false;
        }
    }
}