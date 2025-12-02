using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using CalamityMod.Items.Accessories;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayerSpeed : ModPlayer
    {
        // 活力凝胶, 鹰身女妖戒指, 反击围巾, 天蓝石, 闪避围巾, 深潜者, 宝光盾, 天使靴, 阿斯加德英勇, 利维坦龙涎香,
        // 重力靴, 高统盾, 斯塔提斯忍者腰带, 极乐世界之盾, 进升证章, 老公爵的鳞片, 斯塔提斯虚空腰带, 阿斯加德庇护

        // 敏捷扣链, 琥珀马蹄气球, 两栖靴, 疾风脚镯, 极地潜水装备, 河豚气球, 暴雪气球, 暴雪瓶, 蓝色马蹄气球, 气球束,
        // 马掌气球束, 天界贝壳, 攀爬爪, 云朵气球, 瓶中云, 潜水装备, 沙丘行者靴, 精灵靴, 放屁气球, 放屁罐, 脚蹼, 疾风雪靴,
        // 飞毯, 青蛙脚蹼, 青蛙装备, 青蛙腿, 青蛙蹼, 霜花靴, 绿马掌气球, 狱炎战靴, 赫尔墨斯靴, 蜂蜜气球, 溜冰鞋, 游泳圈,
        // 水母潜水装备, 熔岩护身符, 熔岩靴, 闪电靴, 幸运马蹄铁, 魔光护服, 熔岩头骨, 忍者大师装备, 熔火护身符, 月亮护身符,
        // 月亮贝壳, 海神贝壳, 黑曜石马蹄铁, 黑曜石水上靴, 粉马掌气球, 火箭靴, 旗鱼靴, 沙暴气球, 沙尘暴瓶, 鲨鱼气球,
        // 闪亮红气球, 鞋钉, 幽灵靴, 梯凳, 分趾厚底袜, 泰拉闪耀靴, 大猫猫攀爬装备, 海啸瓶, 水上漂靴, 白色马蹄气球, 黄色马蹄气球
        private static readonly HashSet<int> MobilityAccessoriesMod = new HashSet<int>
        {
            ModContent.ItemType<VitalJelly>(), // 活力凝胶
            ModContent.ItemType<HarpyRing>(), // 鹰身女妖戒指
            ModContent.ItemType<CounterScarf>(), // 反击围巾
            ModContent.ItemType<AeroStone>(), // 天蓝石
            ModContent.ItemType<EvasionScarf>(), // 闪避围巾
            ModContent.ItemType<DeepDiver>(), // 深潜者
            ModContent.ItemType<OrnateShield>(), // 宝光盾
            ModContent.ItemType<AngelTreads>(), // 天使靴
            ModContent.ItemType<AsgardsValor>(), // 阿斯加德英勇
            ModContent.ItemType<LeviathanAmbergris>(), // 利维坦龙涎香
            ModContent.ItemType<GravistarSabaton>(), // 重力靴
            ModContent.ItemType<ShieldoftheHighRuler>(), // 高统盾
            ModContent.ItemType<StatisNinjaBelt>(), // 斯塔提斯忍者腰带
            ModContent.ItemType<ElysianAegis>(), // 极乐世界之盾
            ModContent.ItemType<AscendantInsignia>(), // 进升证章
            ModContent.ItemType<OldDukeScales>(), // 老公爵的鳞片
            ModContent.ItemType<StatisVoidSash>(), // 斯塔提斯虚空腰带
            ModContent.ItemType<AsgardianAegis>(), // 阿斯加德庇护
        };

        private static readonly HashSet<int> MobilityAccessoriesVanilla = new HashSet<int>
        {
            ItemID.Aglet, // 敏捷扣链
            ItemID.BalloonHorseshoeHoney, // 琥珀马蹄气球
            ItemID.AmphibianBoots, // 两栖靴
            ItemID.AnkletoftheWind, // 疾风脚镯
            ItemID.ArcticDivingGear, // 极地潜水装备
            ItemID.BalloonPufferfish, // 河豚气球
            ItemID.BlizzardinaBalloon, // 暴雪气球
            ItemID.BlizzardinaBottle, // 暴雪瓶
            ItemID.BlueHorseshoeBalloon, // 蓝色马蹄气球
            ItemID.BundleofBalloons, // 气球束
            ItemID.HorseshoeBundle, // 马掌气球束
            ItemID.CelestialShell, // 天界贝壳
            ItemID.ClimbingClaws, // 攀爬爪
            ItemID.CloudinaBalloon, // 云朵气球
            ItemID.CloudinaBottle, // 瓶中云
            ItemID.DivingGear, // 潜水装备
            ItemID.SandBoots, // 沙丘行者靴
            ItemID.FairyBoots, // 精灵靴
            ItemID.FartInABalloon, // 放屁气球
            ItemID.FartinaJar, // 放屁罐
            ItemID.Flipper, // 脚蹼
            ItemID.FlurryBoots, // 疾风雪靴
            ItemID.FlyingCarpet, // 飞毯
            ItemID.FrogFlipper, // 青蛙脚蹼
            ItemID.FrogGear, // 青蛙装备
            ItemID.FrogLeg, // 青蛙腿
            ItemID.FrogWebbing, // 青蛙蹼
            ItemID.FrostsparkBoots, // 霜花靴
            ItemID.BalloonHorseshoeFart, // 绿马掌气球
            ItemID.HellfireTreads, // 狱炎战靴
            ItemID.HermesBoots, // 赫尔墨斯靴
            ItemID.HoneyBalloon, // 蜂蜜气球
            ItemID.IceSkates, // 溜冰鞋
            ItemID.FloatingTube, // 游泳圈
            ItemID.JellyfishDivingGear, // 水母潜水装备
            ItemID.LavaCharm, // 熔岩护身符
            ItemID.LavaWaders, // 熔岩靴
            ItemID.LightningBoots, // 闪电靴
            ItemID.LuckyHorseshoe, // 幸运马蹄铁
            ItemID.Magiluminescence, // 魔光护服
            ItemID.LavaSkull, // 熔岩头骨
            ItemID.MasterNinjaGear, // 忍者大师装备
            ItemID.MoltenCharm, // 熔火护身符
            ItemID.MoonCharm, // 月亮护身符
            ItemID.MoonShell, // 月亮贝壳
            ItemID.NeptunesShell, // 海神贝壳
            ItemID.ObsidianHorseshoe, // 黑曜石马蹄铁
            ItemID.ObsidianWaterWalkingBoots, // 黑曜石水上靴
            ItemID.BalloonHorseshoeSharkron, // 粉马掌气球
            ItemID.RocketBoots, // 火箭靴
            ItemID.SailfishBoots, // 旗鱼靴
            ItemID.SandstorminaBalloon, // 沙暴气球
            ItemID.SandstorminaBottle, // 沙尘暴瓶
            ItemID.SharkronBalloon, // 鲨鱼气球
            ItemID.ShinyRedBalloon, // 闪亮红气球
            ItemID.ShoeSpikes, // 鞋钉
            ItemID.SpectreBoots, // 幽灵靴
            ItemID.PortableStool, // 梯凳
            ItemID.Tabi, // 分趾厚底袜
            ItemID.TerrasparkBoots, // 泰拉闪耀靴
            ItemID.TigerClimbingGear, // 大猫猫攀爬装备
            ItemID.TsunamiInABottle, // 海啸瓶
            ItemID.WaterWalkingBoots, // 水上漂靴
            ItemID.WhiteHorseshoeBalloon, // 白色马蹄气球
            ItemID.YellowHorseshoeBalloon // 黄色马蹄气球
        };


        public override void ResetEffects()
        {
            // 确保玩家手持 Sunset
            if (Player.HeldItem.type == ModContent.ItemType<Sunset>())
            {
                // 只有当玩家防御为 0，且只佩戴翅膀和机动性饰品时才激活奖励
                if (Player.statDefense == 0 && HasOnlyMobilityAccessories())
                {
                    ApplySunsetSpeedBonuses();
                }
            }
        }

        private bool HasOnlyMobilityAccessories()
        {
            bool hasWings = false;

            foreach (Item item in Player.armor)
            {
                if (item.wingSlot > 0) // 检测是否为翅膀
                {
                    hasWings = true;
                }
                else if (item.accessory && !MobilityAccessoriesMod.Contains(item.type) && !MobilityAccessoriesVanilla.Contains(item.type))
                {
                    return false; // 发现非机动性饰品，则返回 false
                }
            }

            return hasWings; // 必须至少有一件翅膀
        }

        private void ApplySunsetSpeedBonuses()
        {
            // **赋予强力加成**
            Player.GetDamage(DamageClass.Generic) += 0.35f; // +35% 伤害
            Player.GetArmorPenetration<GenericDamageClass>() += 20; // +20 穿甲
            Player.GetCritChance(DamageClass.Generic) += 20; // +20% 暴击
            Player.runAcceleration *= 1.2f; // +20% 移动速度
            Player.endurance -= 0.25f; // 受到伤害 +25%
            Player.immuneTime = 0; // 禁用无敌帧

            // ★ 无限飞行（最强方式）
            Player.wingTime = Player.wingTimeMax = int.MaxValue;
            Player.rocketTime = Player.rocketTimeMax = int.MaxValue;
            Player.slowFall = false;

            // **最终伤害 x4**
            finalDamageMultiplier = 4f;

            // **检查治疗冷却**
            if (Player.HasBuff(BuffID.PotionSickness))
            {
                ExtendPotionCooldown();
            }
        }

        private void ExtendPotionCooldown()
        {
            int index = Player.FindBuffIndex(BuffID.PotionSickness);
            if (index != -1)
            {
                // 获取当前冷却时间（帧数）
                int remainingTime = Player.buffTime[index];

                // 只增加一次 60 秒（3600 帧）
                Player.buffTime[index] = remainingTime + 3600;
            }
        }

        public float finalDamageMultiplier = 1f;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= finalDamageMultiplier;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= finalDamageMultiplier;
        }
    }



}
