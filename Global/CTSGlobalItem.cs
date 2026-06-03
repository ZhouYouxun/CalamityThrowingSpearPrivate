using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static tModPorter.ProgressUpdate;
using System;
using MonoMod.RuntimeDetour;
using static Humanizer.In;
using Microsoft.Xna.Framework.Input;
using System.Drawing;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityMod.Buffs.DamageOverTime;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.AstralPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC;



namespace CalamityThrowingSpear.Global
{
    public class CTSGlobalItem : GlobalItem
    {
        public class modRecipes : ModSystem
        {
            public override void AddRecipes()
            {
                Recipe.Create(ItemID.ScourgeoftheCorruptor)
                    .AddIngredient<CalamityMod.Items.Materials.LifeAlloy>(3)
                    .AddIngredient(ItemID.ShadowScale)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();

                Recipe.Create(ItemID.DarkLance)
                    .AddIngredient(ItemID.ShadowKey)
                    .AddIngredient(ItemID.ObsidianBrick, 50)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();

            }


        }

    }
}
