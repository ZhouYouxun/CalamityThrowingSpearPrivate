using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Pets;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.ElementalArkJav;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.FinishingTouch;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Revelation;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.StarsofDestiny;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.TidalMechanics;
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
                .AddIngredient<CosmiliteBar>(5)
                .AddTile(ModContent.TileType<CalamityMod.Tiles.Furniture.CraftingStations.CosmicAnvil>())
                .Register();

            Recipe.Create(ModContent.ItemType<StarsofDestiny>())
                .AddIngredient<RuinousSoul>(15)
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
                .AddIngredient(ItemID.PiercingStarlight)
                .AddIngredient(ItemID.SoulofLight, 12)
                .AddIngredient(ItemID.UnicornHorn, 4)
                .AddTile(TileID.MythrilAnvil)
                .Register();

            Recipe.Create(ModContent.ItemType<Sunset>())
                .AddIngredient<DivineGeode>(15)
                .AddIngredient(ItemID.DaybloomSeeds, 1)
                .AddIngredient(ItemID.Terrarium, 1)
                .AddTile(TileID.LunarCraftingStation)
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
