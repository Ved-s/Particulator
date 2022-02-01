using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Particulator.Tiles
{
    internal class Particulator : ModTile
    {
        ParticulatorTE MyTE => ModContent.GetInstance<ParticulatorTE>();

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileSolidTop[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(MyTE.Hook_AfterPlacement, -1, 0, true);
            TileObjectData.newTile.AnchorBottom = new AnchorData((AnchorType)255, 1, 0);
            TileObjectData.addTile(Type);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            MyTE.Kill(i, j);
        }

        public override bool Drop(int i, int j)
        {
            Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<Items.Particulator>());
            return true;
        }

        public override bool NewRightClick(int i, int j)
        {
            int index = MyTE.Find(i, j);
            if (index == -1) return false;

            ParticulatorTE te = (ParticulatorTE)TileEntity.ByID[index];

            Interface.TE = te;
            return true;
        }

        public override void HitWire(int i, int j)
        {
            int index = MyTE.Find(i, j);
            if (index == -1) return;

            ParticulatorTE te = (ParticulatorTE)TileEntity.ByID[index];

            te.Wire();
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.drawDiag)
            {
                int index = MyTE.Find(i, j);
                if (index == -1) return;

                ParticulatorTE te = (ParticulatorTE)TileEntity.ByID[index];

                string text = te.MyDusts.Count.ToString();

                Vector2 size = Main.fontMouseText.MeasureString(text);
                Vector2 screen = (new Vector2(i, j) * 16) - Main.screenPosition;

                if (Lighting.NotRetro) 
                {
                    screen += new Vector2(12 * 16);
                }

                Vector2 pos = screen + new Vector2(8) - size / 2;
                Utils.DrawBorderString(spriteBatch, text, pos, Color.White);
            }
        }
    }
}
