using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Ranged;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.TidalMechanics;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.ElementalArkJav;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.StarsofDestiny;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.FinishingTouch;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Revelation;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Global
{
    public class CTSDeveloperRecipes : ModSystem
    {
        public override void AddRecipes()
        {
            if (!ModContent.GetInstance<CTSRecipeConfigs>().DeveloperWeaponsCraftable)
            {
                return;
            }

            Recipe.Create(ModContent.ItemType<Revelation>())
                .AddIngredient<Ultima>()
                .AddIngredient<Starmada>()
                .AddTile(ModContent.TileType<CalamityMod.Tiles.Furniture.CraftingStations.CosmicAnvil>())
                .Register();

            Recipe.Create(ModContent.ItemType<StarsofDestiny>())
                .AddIngredient<EssenceofEleum>(15)
                .AddIngredient(ItemID.Glass, 100)
                .AddIngredient(ItemID.FastClock)
                .AddIngredient(ItemID.StarWrath)
                .AddTile(TileID.LunarCraftingStation)
                .Register();

            Recipe.Create(ModContent.ItemType<ElementalArkJav>())
                .AddRecipeGroup(CTSRecipeGroups.ElementalLanceGroup)
                .AddIngredient<GalacticaSingularity>(5)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();

            Recipe.Create(ModContent.ItemType<TidalMechanics>())
                .AddIngredient(ItemID.RazorbladeTyphoon)
                .AddIngredient(ItemID.WaterBucket, 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();

            Recipe.Create(ModContent.ItemType<Sagittarius>())
                .AddIngredient(ItemID.Sundial)
                .AddIngredient(ItemID.SoulofLight, 20)
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddIngredient(ItemID.PixieDust, 30)
                .AddIngredient(ItemID.UnicornHorn, 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void PostAddRecipes()
        {
            if (!ModContent.GetInstance<CTSRecipeConfigs>().DeveloperWeaponsCraftable)
            {
                return;
            }

            int finishingTouchId = ModContent.ItemType<FinishingTouch>();
            int ashesId = ModContent.ItemType<AshesofAnnihilation>();
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe.HasResult(finishingTouchId))
                {
                    recipe.RemoveIngredient(ashesId);
                }
            }
        }
    }
}
