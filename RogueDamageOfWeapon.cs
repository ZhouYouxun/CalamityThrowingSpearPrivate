using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Rogue;
using CalamityMod;
using CalamityThrowingSpear.Global;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.AstralPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius;
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

namespace CalamityThrowingSpear
{
    public class RogueDamageOfWeapon : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            // 如果启用了 EnableRogueSpeed 设定
            if (ModContent.GetInstance<CTSConfigs>().EnableRogue)
            {
                // 检查当前物品是否在全局武器列表中
                if (MeleeSpeedOfThrowingSpear.WeaponSetA.Contains(item.ModItem?.GetType()))
                {
                    item.DamageType = ModContent.GetInstance<RogueDamageClass>(); // 将职业类型改为盗贼
                }

                // 自动检测是否为本模组武器
                //if (MeleeSpeedOfThrowingSpear.ThisModWeaponTypes.Contains(item.type))
                //{
                //    item.DamageType = ModContent.GetInstance<RogueDamageClass>(); // 改为盗贼职业类型
                //}

            }
        }
    }

    public class RogueDamageOfProjectile : GlobalProjectile
    {
        public override void SetDefaults(Projectile projectile)
        {
            // 如果启用了 EnableRogueSpeed 设定
            if (ModContent.GetInstance<CTSConfigs>().EnableRogue)
            {
                // 检查当前弹幕是否属于指定的弹幕容器
                if (ProjectileSet.Contains(projectile.type))
                {
                    projectile.DamageType = ModContent.GetInstance<RogueDamageClass>(); // 将职业类型改为盗贼
                }
            }
        }

        // 弹幕容器 - 存放所有被影响的弹幕
        private static readonly List<int> ProjectileSet = new List<int>
        {
            ModContent.ProjectileType<ElementalArkJavEonBolt>(), // 元素爆裂弹
            ModContent.ProjectileType<BloodstoneJavPROJ>(), // 血炎长枪
            ModContent.ProjectileType<ChaosWindJavPROJ>(), // 无序狂风
            ModContent.ProjectileType<ElementalArkJavBlade>(), // 鸢素方舟刀片
            ModContent.ProjectileType<ElementalArkJavSUPERPROJ>(), // 鸢素方舟聚合片
            ModContent.ProjectileType<EndlessDevourJavPROJ>(), // 无止之噬
            ModContent.ProjectileType<InfiniteDarknessJavPROJ>(), // 无边黑暗
            ModContent.ProjectileType<SoulHunterJavPROJ>(), // 魂狩
            ModContent.ProjectileType<TerraLanceBEAM>(), // 泰拉巨枪光束
            ModContent.ProjectileType<TerraLancePROJ>(), // 泰拉巨枪

            ModContent.ProjectileType<SunEssenceJavBEAM>(), // 光耀日激光
            ModContent.ProjectileType<ChaosEssenceJavFIRE>(), // 凯奥斯焰
            ModContent.ProjectileType<HeartSwordPROJ>(), // 心之刺剑
            ModContent.ProjectileType<PolarEssenceJavPROJ>(), // 霜雪川
            ModContent.ProjectileType<SunEssenceJavFeather>(), // 光耀日羽毛
            ModContent.ProjectileType<TheLastLancePROJ>(), // 最后的骑枪

            ModContent.ProjectileType<BraisedPorkJavCloud>(), // 卤肉矛毒云
            ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>(), // 电凝十文字电光矢
            ModContent.ProjectileType<GraniteJavPROJ>(), // 花岗岩矛
            ModContent.ProjectileType<WulfrimJavPROJ>(), // 钨钢矛
            ModContent.ProjectileType<TheBrokenPROJ>(), // 破旧的长枪

            ModContent.ProjectileType<FestiveHalberdPROJ>(), // 节日长戟
            ModContent.ProjectileType<SagittariusPROJ>(), // 贯星枪
            ModContent.ProjectileType<TidalMechanicsBubbles>(), // 潮汐泡泡

            ModContent.ProjectileType<AuricJavPROJ>(), // 金源量子带电调试棒
            ModContent.ProjectileType<FinishingTouchBALL>(), // 画龙点睛火球
            ModContent.ProjectileType<MiracleMatterJavLight>(), // 恒辉之矛光束
            ModContent.ProjectileType<SawBladeForkHornJavPROJ>(), // 锯刃叉角枪
            ModContent.ProjectileType<ShadowJavPROJ>(), // 无极
            ModContent.ProjectileType<RevelationSTAR>(), // 启示录落星
            ModContent.ProjectileType<RevelationSpark>(), // 启示录火焰
            ModContent.ProjectileType<SoulSeekerJavPROJ>(), // 终幕残翼

            ModContent.ProjectileType<AmidiasTridentJavPROJ>(), // 海王三叉戟
            ModContent.ProjectileType<BrimlanceJavPROJ>(), // 硫磺火矛
            ModContent.ProjectileType<StarnightLanceJavBeam>(), // 星夜长枪光束
            ModContent.ProjectileType<AstralPikeJavPROJ>(), // 幻星矛
            ModContent.ProjectileType<DiseasedJavLight>(), // 瘟疫长枪能量弹
            ModContent.ProjectileType<GildedProboscisJavPROJ>(), // 镀金鸟喙
            ModContent.ProjectileType<DragonRageJavPROJ>(), // 巨龙之怒
            ModContent.ProjectileType<StreamGougeJavPROJ>(), // 宇宙暗流
            ModContent.ProjectileType<ViolenceJavPROJ>(), // 恣睢
        };
    }
}
