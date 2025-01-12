using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.AstralPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.StarGunSagittarius;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ElementalArkJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.MiracleMatterJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.ShadowJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TheOtherMiracleMatterJav;
using CalamityThrowingSpear.Global;

namespace CalamityThrowingSpear
{
    internal class GlobalThrowingSpear : GlobalItem
    {
        // 定义一个静态的武器容器
        private static readonly List<Type> WeaponSetA = new List<Type>
        {
            typeof(AmidiasTridentJav), // 海王三叉戟
            typeof(GoldplumeJav), // 金羽
            typeof(SausageMakerJav), // 香肠枪
            typeof(YateveoBloomJav), // 绽花毒蔓

            typeof(BrimlanceJav), // 硫磺火
            typeof(EarthenJav), // 大地
            typeof(StarnightLanceJav), // 星夜

            typeof(AstralPikeJav), // 幻星矛
            typeof(BotanicPiercerJav), // 璀芒尖枪
            typeof(DiseasedJav), // 瘟疫长枪
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



            typeof(BraisedPorkJav), // 卤肉
            typeof(ElectrocoagulationTenmonJav), // 十文字
            typeof(GraniteJav), // 花岗岩
            typeof(RedtideJav), // 赤潮
            typeof(WulfrimJav), // 钨钢

            typeof(ChaosEssenceJav), // 混沌精华
            typeof(ElectrocutionHalberd), // 电磁长矛
            typeof(HeartSword), // 心之刺剑
            typeof(PearlwoodJav), // 珍珠矛
            typeof(PolarEssenceJav), // 极地精华
            typeof(SHPCK), // 超热等离子
            typeof(SunEssenceJav), // 日光精华

            typeof(FestiveHalberd), // 节日长枪

            typeof(BloodstoneJav), // 血岩
            typeof(ChaosWindJav), // 狂风
            typeof(EndlessDevourJav), // 黑噬
            typeof(InfiniteDarknessJav), // 无边黑暗
            typeof(SoulHunterJav), // 狩魂
            typeof(TerraLance), // 泰拉

            typeof(AuricJav), // 电池
            typeof(MiracleMatterJav), // 轻星流
            typeof(ShadowJav), // 无极
            typeof(SoulSeekerJav), // 残翼
            typeof(TheOtherMiracleMatterJav), // 重星流

            typeof(TheLastLance), // 最后的骑枪
            typeof(StarGunSagittarius), // 人马座
            typeof(TidalMechanics), // 潮汐
            typeof(ElementalArkJav), // 元素
            typeof(FinishingTouch), // 画龙点睛
            typeof(Revelation), // 启示录
            typeof(SawBladeForkHornJav), // SBFHJ
        };

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults(Item item)
        {
            // 检查当前物品类型是否在容器中
            if (WeaponSetA.Contains(item.ModItem?.GetType()))
            {
                // 根据 CTSConfigs.EnableMeleeSpeed 的状态设置 BonusAttackSpeedMultiplier
                if (ModContent.GetInstance<CTSConfigs>().EnableMeleeSpeed)
                {
                    // 启用全局攻速加成禁用
                    ItemID.Sets.BonusAttackSpeedMultiplier[item.type] = 0.000f;
                }
                else
                {
                    // 禁用状态下恢复正常
                    ItemID.Sets.BonusAttackSpeedMultiplier[item.type] = 1.000f;
                }
            }
        }




    }
}
