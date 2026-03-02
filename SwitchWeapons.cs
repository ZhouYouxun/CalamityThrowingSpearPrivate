using CalamityThrowingSpear.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.DraedonsArsenal;

using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.AstralPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Magic;

namespace CalamityThrowingSpear
{
    public class SwitchWeapons : ModPlayer
    {
        // 容器A和B，用于存储二二绑定的武器类类型
        // 注意！！这很重要！！
        // 因为容器是按照顺序进行读取，所以里面的武器顺序必须1:1的对照，如果要添加的话不可打乱，因为只要打乱了一个，后面的就全乱了
        public static readonly List<Type> WeaponSetA = new List<Type>
        {
            typeof(AmidiasTridentJav), // 海王三叉戟
            typeof(GoldplumeJav), // 金羽
            typeof(SausageMakerJav), // 香肠枪
            typeof(YateveoBloomJav), // 绽花毒蔓
            // typeof(RedtideJav), // 赤潮

            typeof(BrimlanceJav), // 硫磺火
            typeof(EarthenJav), // 大地
            typeof(StarnightLanceJav), // 星夜

            typeof(AstralPikeJav), // 幻星矛
            typeof(BotanicPiercerJav), // 璀芒尖枪
            // typeof(DiseasedJav), // 瘟疫长枪
            typeof(GalvanizingGlaiveJav), // 磁能切割
            typeof(HellionFlowerJav), // 刺花长枪
            typeof(TenebreusTidesJav), // 深渊潮涌
            typeof(TyphonsGreedJav), // 台风贪婪
            typeof(VulcaniteLanceJav), // 火山长矛

            typeof(BansheeHookJav), // 女妖之爪
            typeof(ElementalLanceJav), // 元素长枪
            typeof(GildedProboscisJav), // 镀金鸟会

            typeof(DragonRageJav), // 巨龙之怒
            typeof(NadirJav), // 天底
            typeof(ScourgeoftheCosmosJav), // 宇宙吞噬者
            typeof(StreamGougeJav), // 宇宙暗流
            typeof(ViolenceJav), // 恣睢
        };

        public static readonly List<Type> WeaponSetB = new List<Type>
        {
            typeof(AmidiasTrident),
            typeof(GoldplumeSpear),
            typeof(SausageMaker),
            typeof(YateveoBloom),
            // typeof(RedtideSpear),

            typeof(Brimlance),
            typeof(EarthenPike),
            typeof(StarnightLance),

            typeof(AstralPike),
            typeof(BotanicPiercer),
            // typeof(DiseasedPike),
            typeof(GalvanizingGlaive),
            typeof(HellionFlowerSpear),
            typeof(TenebreusTides),
            typeof(TyphonsGreed),
            typeof(VulcaniteLance),

            typeof(BansheeHook),
            typeof(VanishingPoint),
            typeof(GildedProboscis),

            typeof(DragonRage),
            typeof(Nadir),
            typeof(ScourgeoftheCosmos),
            typeof(StreamGouge),
            typeof(Violence),
        };

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 检测快捷键是否按下
            if (KeybindSystem.ChangeSpetoJav.JustPressed && !Main.playerInventory)
            {
                // 遍历容器，检查玩家是否手持其中一个武器
                for (int i = 0; i < WeaponSetA.Count; i++)
                {
                    if (Player.HeldItem.ModItem?.GetType() == WeaponSetA[i])
                    {
                        ReplaceWeapon(WeaponSetB[i]);
                        return;
                    }
                    else if (Player.HeldItem.ModItem?.GetType() == WeaponSetB[i])
                    {
                        ReplaceWeapon(WeaponSetA[i]);
                        return;
                    }
                }
                // 如果未匹配到任何武器，则不进行任何操作
            }
        }

        //private void ReplaceWeapon(Type targetWeaponType)
        //{
        //    // 保存当前武器的词条（前缀）
        //    int currentPrefix = Player.HeldItem.prefix;

        //    // 创建目标武器实例
        //    Item newWeapon = new Item();

        //    // 动态获取目标武器的 type ID
        //    int targetWeaponID = (int)typeof(ModContent).GetMethod("ItemType", new Type[] { })
        //        .MakeGenericMethod(targetWeaponType)
        //        .Invoke(null, null);

        //    newWeapon.SetDefaults(targetWeaponID);

        //    // 继承词条
        //    newWeapon.Prefix(currentPrefix);

        //    // 替换当前手持武器
        //    Player.inventory[Player.selectedItem] = newWeapon;

        //    // 播放音效
        //    SoundEngine.PlaySound(SoundID.Item68);
        //}

        private float galvanizingCharge = 0f; // 用于保存充电量

        private void ReplaceWeapon(Type targetWeaponType)
        {
            // 保存当前武器的词条（前缀）
            int currentPrefix = Player.HeldItem.prefix;

            // 如果当前武器是 GalvanizingGlaive 或 GalvanizingGlaiveJav，记录充电量
            // 获取当前物品的 GlobalItem
            CalamityGlobalItem currentModItem = Player.HeldItem.GetGlobalItem<CalamityGlobalItem>();

            // 检查是否是 GalvanizingGlaive 或 GalvanizingGlaiveJav，并记录充电量
            if (Player.HeldItem.ModItem != null &&
                (Player.HeldItem.ModItem.GetType() == typeof(GalvanizingGlaive) ||
                 Player.HeldItem.ModItem.GetType() == typeof(GalvanizingGlaiveJav)))
            {
                galvanizingCharge = currentModItem.Charge; // 保存充电量
            }


            // 创建目标武器实例
            Item newWeapon = new Item();

            // 动态获取目标武器的 type ID
            int targetWeaponID = (int)typeof(ModContent).GetMethod("ItemType", new Type[] { })
                .MakeGenericMethod(targetWeaponType)
                .Invoke(null, null);

            newWeapon.SetDefaults(targetWeaponID);

            // 如果目标武器是 GalvanizingGlaive 或 GalvanizingGlaiveJav，恢复充电量
            if (targetWeaponType == typeof(GalvanizingGlaive) || targetWeaponType == typeof(GalvanizingGlaiveJav))
            {
                // 获取目标武器的 GlobalItem
                CalamityGlobalItem newModItem = newWeapon.GetGlobalItem<CalamityGlobalItem>();

                // 恢复充电量
                if (newModItem != null)
                {
                    newModItem.Charge = galvanizingCharge; // 恢复充电量
                }
            }

            // 继承词条
            newWeapon.Prefix(currentPrefix);

            // 替换当前手持武器
            Player.inventory[Player.selectedItem] = newWeapon;

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item68);
        }


    }
}
