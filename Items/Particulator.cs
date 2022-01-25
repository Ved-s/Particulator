using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Particulator.Items
{
    internal class Particulator : ModItem
    {
        public override string Texture => mod.Name + "/Tiles/Particulator";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Particulator");
            Tooltip.SetDefault("Creates particles, useful for decoration");
        }

        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTurn = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.autoReuse = true;
            item.maxStack = 999;
            item.consumable = true;
            item.height = 16;
            item.width = 16;
            item.createTile = ModContent.TileType<Tiles.Particulator>();
        }

        public override void AddRecipes()
        {
            RecipeGroup group = new RecipeGroup(() => "Any colored rocket", new int[] 
            {
                ItemID.RedRocket,
                ItemID.BlueRocket,
                ItemID.GreenRocket,
                ItemID.YellowRocket,
            });
            RecipeGroup.RegisterGroup("AnyColoredRocket", group);

            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Glass, 5);
            recipe.AddIngredient(ItemID.Diamond, 1);
            recipe.AddIngredient(ItemID.Lens, 1);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 2);
            recipe.AddIngredient(ItemID.FireworkFountain, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Glass, 5);
            recipe.AddIngredient(ItemID.Diamond, 2);
            recipe.AddIngredient(ItemID.Lens, 2);
            recipe.AddIngredient(ItemID.Cog, 1);
            recipe.AddRecipeGroup("AnyColoredRocket", 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
}
