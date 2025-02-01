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
                // ----------------------------第1部分----------------------------
                // 将本模组的物品转化为原灾的同一物品

                //// 海王三叉戟
                //Recipe recipe0 = Recipe.Create(ModContent.ItemType<AmidiasTrident>(), 1);
                //recipe0.AddIngredient(ModContent.ItemType<AmidiasTridentJav>(), 1);
                //recipe0.Register();


                //// 配方 1 - 金羽长矛
                //Recipe recipe1 = Recipe.Create(ModContent.ItemType<GoldplumeSpear>(), 1);
                //recipe1.AddIngredient(ModContent.ItemType<GoldplumeJav>(), 1);
                //recipe1.Register();

                //// 配方 2 - 香肠制造者
                //Recipe recipe2 = Recipe.Create(ModContent.ItemType<SausageMaker>(), 1);
                //recipe2.AddIngredient(ModContent.ItemType<SausageMakerJav>(), 1);
                //recipe2.Register();

                //// 配方 3 - 叶天花
                //Recipe recipe3 = Recipe.Create(ModContent.ItemType<YateveoBloom>(), 1);
                //recipe3.AddIngredient(ModContent.ItemType<YateveoBloomJav>(), 1);
                //recipe3.Register();

                //// 配方 4 - 炼狱长矛
                //Recipe recipe4 = Recipe.Create(ModContent.ItemType<Brimlance>(), 1);
                //recipe4.AddIngredient(ModContent.ItemType<BrimlanceJav>(), 1);
                //recipe4.Register();

                //// 配方 5 - 大地长枪
                //Recipe recipe5 = Recipe.Create(ModContent.ItemType<EarthenPike>(), 1);
                //recipe5.AddIngredient(ModContent.ItemType<EarthenJav>(), 1);
                //recipe5.Register();

                //// 配方 6 - 星夜长矛
                //Recipe recipe6 = Recipe.Create(ModContent.ItemType<StarnightLance>(), 1);
                //recipe6.AddIngredient(ModContent.ItemType<StarnightLanceJav>(), 1);
                //recipe6.Register();

                //// 配方 7 - 星体长矛
                //Recipe recipe7 = Recipe.Create(ModContent.ItemType<AstralPike>(), 1);
                //recipe7.AddIngredient(ModContent.ItemType<AstralPikeJav>(), 1);
                //recipe7.Register();

                //// 配方 8 - 植物刺枪
                //Recipe recipe8 = Recipe.Create(ModContent.ItemType<BotanicPiercer>(), 1);
                //recipe8.AddIngredient(ModContent.ItemType<BotanicPiercerJav>(), 1);
                //recipe8.Register();

                //// 配方 9 - 疾病长矛
                //Recipe recipe9 = Recipe.Create(ModContent.ItemType<DiseasedPike>(), 1);
                //recipe9.AddIngredient(ModContent.ItemType<DiseasedJav>(), 1);
                //recipe9.Register();

                //// 配方 10 - 电化长矛
                //Recipe recipe10 = Recipe.Create(ModContent.ItemType<GalvanizingGlaive>(), 1);
                //recipe10.AddIngredient(ModContent.ItemType<GalvanizingGlaiveJav>(), 1);
                //recipe10.Register();

                //// 配方 11 - 地狱花
                //Recipe recipe11 = Recipe.Create(ModContent.ItemType<HellionFlowerSpear>(), 1);
                //recipe11.AddIngredient(ModContent.ItemType<HellionFlowerJav>(), 1);
                //recipe11.Register();

                //// 配方 12 - 深渊潮汐
                //Recipe recipe12 = Recipe.Create(ModContent.ItemType<TenebreusTides>(), 1);
                //recipe12.AddIngredient(ModContent.ItemType<TenebreusTidesJav>(), 1);
                //recipe12.Register();

                //// 配方 13 - 台风之贪
                //Recipe recipe13 = Recipe.Create(ModContent.ItemType<TyphonsGreed>(), 1);
                //recipe13.AddIngredient(ModContent.ItemType<TyphonsGreedJav>(), 1);
                //recipe13.Register();

                //// 配方 14 - 火山长矛
                //Recipe recipe14 = Recipe.Create(ModContent.ItemType<VulcaniteLance>(), 1);
                //recipe14.AddIngredient(ModContent.ItemType<VulcaniteLanceJav>(), 1);
                //recipe14.Register();

                //// 配方 15 - 女妖之钩
                //Recipe recipe15 = Recipe.Create(ModContent.ItemType<BansheeHook>(), 1);
                //recipe15.AddIngredient(ModContent.ItemType<BansheeHookJav>(), 1);
                //recipe15.Register();

                //// 配方 16 - 元素长矛
                //Recipe recipe16 = Recipe.Create(ModContent.ItemType<ElementalLance>(), 1);
                //recipe16.AddIngredient(ModContent.ItemType<ElementalLanceJav>(), 1);
                //recipe16.Register();

                //// 配方 17 - 镀金鸟喙枪
                //Recipe recipe17 = Recipe.Create(ModContent.ItemType<GildedProboscis>(), 1);
                //recipe17.AddIngredient(ModContent.ItemType<GildedProboscisJav>(), 1);
                //recipe17.Register();

                //// 配方 18 - 龙之怒
                //Recipe recipe18 = Recipe.Create(ModContent.ItemType<DragonRage>(), 1);
                //recipe18.AddIngredient(ModContent.ItemType<DragonRageJav>(), 1);
                //recipe18.Register();

                //// 配方 19 - 至暗
                //Recipe recipe19 = Recipe.Create(ModContent.ItemType<Nadir>(), 1);
                //recipe19.AddIngredient(ModContent.ItemType<NadirJav>(), 1);
                //recipe19.Register();

                //// 配方 20 - 宇宙灾祸
                //Recipe recipe20 = Recipe.Create(ModContent.ItemType<ScourgeoftheCosmos>(), 1);
                //recipe20.AddIngredient(ModContent.ItemType<ScourgeoftheCosmosJav>(), 1);
                //recipe20.Register();

                //// 配方 21 - 溪流凿
                //Recipe recipe21 = Recipe.Create(ModContent.ItemType<StreamGouge>(), 1);
                //recipe21.AddIngredient(ModContent.ItemType<StreamGougeJav>(), 1);
                //recipe21.Register();

                //// 配方 22 - 暴力之矛
                //Recipe recipe22 = Recipe.Create(ModContent.ItemType<Violence>(), 1);
                //recipe22.AddIngredient(ModContent.ItemType<ViolenceJav>(), 1);
                //recipe22.Register();

            }


        }

    }
}
