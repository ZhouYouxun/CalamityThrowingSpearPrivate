using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Items.Potions.Alcohol;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.World;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;
using Newtonsoft.Json.Linq;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Fishing;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsBTPLAYER : ModPlayer
    {
        // Calamity武器表
        private static readonly int[] CalamityWeapons = new int[]
        {
            ModContent.ItemType<SeashineSword>(), // 海耀之刃
            ModContent.ItemType<TeardropCleaver>(), // 泪水之刃
            ModContent.ItemType<GeliticBlade>(), // 凝胶波刃
            ModContent.ItemType<EutrophicScimitar>(), // 水华弯刀
            ModContent.ItemType<Floodtide>(), // 鲨潮
            ModContent.ItemType<Greentide>(), // 翡翠之潮
            ModContent.ItemType<BrinyBaron>(), // 海爵剑
            ModContent.ItemType<SubmarineShocker>(), // 潜渊震荡者
            ModContent.ItemType<Riptide>(), // 涡旋
            ModContent.ItemType<TheGodsGambit>(), // 神旨
            ModContent.ItemType<SulphurousGrabber>(), // 硫磺掠夺者
            ModContent.ItemType<RedtideSpear>(), // 赤潮矛
            ModContent.ItemType<AmidiasTrident>(), // 海王三叉戟
            ModContent.ItemType<TenebreusTides>(), // 深渊潮涌
            ModContent.ItemType<UrchinMace>(), // 海胆链枷
            ModContent.ItemType<UrchinFlail>(), // 海胆链球
            ModContent.ItemType<BallOFugu>(), // 河豚链球
            ModContent.ItemType<ClamCrusher>(), // 海蚌锤
            ModContent.ItemType<DepthCrusher>(), // 深渊碾碎者
            ModContent.ItemType<AbyssBlade>(), // 深渊之刃
            ModContent.ItemType<TyphonsGreed>(), // 提丰之贪婪
            ModContent.ItemType<NeptunesBounty>(), // 海神之馈赠

            ModContent.ItemType<Basher>(), // 痛击者
            ModContent.ItemType<BrokenBiomeBlade>(), // 破碎环境之刃
            ModContent.ItemType<TrueBiomeBlade>(), // 环境之刃
            ModContent.ItemType<OmegaBiomeBlade>(), // 真环境之刃
            ModContent.ItemType<VoidEdge>(), // 虚渊之锋
            ModContent.ItemType<ReefclawHamaxe>(), // 礁爪锤斧
            ModContent.ItemType<GreatbayPickaxe>(), // 大湾镐
            ModContent.ItemType<Gelpick>(), // 凝胶⛏
            ModContent.ItemType<InsidiousImpaler>(), // 老猪矛
            ModContent.ItemType<Spadefish>(), // 铲子鱼

            
            ModContent.ItemType<AmidiasTridentJav>(), // 海王三叉戟-投掷
            ModContent.ItemType<TyphonsGreedJav>(), // 提丰之贪婪-投掷
            ModContent.ItemType<RedtideJav>(), // 赤潮-投掷
            ModContent.ItemType<TheLastLance>(), // 最后的骑枪
            ModContent.ItemType<SoulHunterJav>() // 魂狩
        };

        // 原版泰拉瑞亚武器ID表
        private static readonly int[] VanillaWeapons = new int[]
        {
            277,  // 三叉戟
            2331, // 黑曜石剑鱼
            2332, // 剑鱼
            946, // 伞
            4707, // 悲剧雨伞
            2330, // 紫挥棒鱼
            3211, // 舌锋剑
            163, // 蓝月
            4272, // 滴滴怪致残者
            2424, // 锚
            2611, // 猪鲨链球
            3835, // 瞌睡章鱼
            2342, // 锯齿鲨
            2341 // 掠夺鲨
        };

        public override void PostUpdateEquips()
        {
            Player player = Main.LocalPlayer;

            // 检查玩家是否手持 TidalMechanics（潮汐武器）
            bool isHoldingTidalMechanics = player.HeldItem != null && player.HeldItem.type == ModContent.ItemType<TidalMechanics>();

            if (isHoldingTidalMechanics)
            {
                // 使用 HashSet 记录已统计的武器类型
                HashSet<int> countedWeapons = new HashSet<int>();
                int count = 0;

                // 检查 Calamity 武器
                foreach (var item in player.inventory)
                {
                    if (item != null && CalamityWeapons.Contains(item.type) && !countedWeapons.Contains(item.type))
                    {
                        countedWeapons.Add(item.type); // 记录该武器类型
                        count++;
                    }
                }

                // 检查原版武器
                foreach (var item in player.inventory)
                {
                    if (item != null && VanillaWeapons.Contains(item.type) && !countedWeapons.Contains(item.type))
                    {
                        countedWeapons.Add(item.type); // 记录该武器类型
                        count++;
                    }
                }

                // 向下取整计算武器组数
                int weaponSets = count / 5; // 每5种武器算一组

                // 应用增益效果
                if (weaponSets > 0)
                {
                    player.GetDamage(DamageClass.Melee) += weaponSets * 0.75f; // 每组增加75%近战伤害
                    player.GetCritChance(DamageClass.Melee) += weaponSets * 10; // 每组增加10点近战暴击率
                }
            }
        }





    }
}
