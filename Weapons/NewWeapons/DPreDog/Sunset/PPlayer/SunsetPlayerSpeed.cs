using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayerSpeed : ModPlayer
    {
        // 无防护假设：破绽暴露


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

            ModContent.ItemType<ReaperToothNecklace>(),      // 猎魂鲨牙项链
            ModContent.ItemType<DimensionalSoulArtifact>(),  // 维魂神物
            ModContent.ItemType<SandSharkToothNecklace>(),   // 旱海狂鲨项链
            ModContent.ItemType<VoidofExtinction>(),         // 终结虚空
            ModContent.ItemType<TheAmalgam>(),               // 聚合之脑
            ModContent.ItemType<BadgeofBravery>(),           // 勇气徽章
            ModContent.ItemType<ElementalGauntlet>(),        // 元素之握

            // ---------------- 翅膀 / 翼靴（统一视作合法机动装备） ----------------
            ModContent.ItemType<SkylineWings>(),        // 天羽之翼
            ModContent.ItemType<SoulofCryogen>(),       // 极寒之魂
            ModContent.ItemType<StarlightWings>(),      // 星光之翼
            ModContent.ItemType<MOAB>(),                // 气球之母

            ModContent.ItemType<AureateBooster>(),      // 玉金喷射器
            ModContent.ItemType<HadarianWings>(),       // 哈达尔星翼
            ModContent.ItemType<HadalMantle>(),         // 幽渊斗篷
            ModContent.ItemType<ExodusWings>(),         // 逸界之翼

            ModContent.ItemType<TracersCelestial>(),    // 天界翼靴
            ModContent.ItemType<TarragonWings>(),       // 龙蒿叶之翼
            ModContent.ItemType<ElysianWings>(),        // 极乐之翼
            ModContent.ItemType<TracersElysian>(),      // 极乐翼靴

            ModContent.ItemType<SilvaWings>(),           // 始源林海之翼
            ModContent.ItemType<WingsofRebirth>(),      // 涅槃龙翼
            ModContent.ItemType<TracersSeraph>(),       // 炽天使翼靴



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
            ItemID.YellowHorseshoeBalloon, // 黄色马蹄气球

            // ---------------- Vanilla 翅膀 ----------------
            4978, // 雏翼
            493,  // 天使之翼
            492,  // 恶魔之翼
            761,  // 仙灵之翼
            2494, // 鳍翼
            822,  // 冰冻之翼
            785,  // 鸟妖之翼
            748,  // 喷气背包
            665,  // Red 的翅膀
            1583, // D-Town 的翅膀
            1584, // Will 的翅膀
            1585, // Crowno 的翅膀
            1586, // Cenx 的翅膀
            3228, // Lazure 的屏障台
            3580, // Yoraiz0r 的魔法
            3582, // Jim 的翅膀
            3588, // Skiphs 的爪子
            3592, // Loki 的翅膀
            3924, // Arkhalis 的飞翼
            3928, // Leinfors 的卷缠斗篷
            4730, // Ghostar 的无极翼
            4746, // Safeman 的毛毯斗篷
            4750, // FoodBarbarian 的褴褛龙之翼
            4754, // Grox The Great 的翅膀
            1162, // 叶之翼
            1165, // 蝙蝠之翼
            1515, // 蜜蜂之翼
            749,  // 蝴蝶之翼
            821,  // 烈焰之翼
            1866, // 悬浮板
            786,  // 骨之翼
            2770, // 蛾怪之翼
            823,  // 幽灵之翼
            2280, // 甲虫之翼
            1871, // 喜庆之翼
            1830, // 阴森之翼
            1797, // 褴褛仙灵之翼
            948,  // 蒸汽朋克之翼
            3883, // 双足翼龙之翼
            4823, // 女皇之翼
            2609, // 猪龙鱼之翼
            3468, // 日耀之翼
            3469, // 星旋强化翼
            3470, // 星云斗篷
            3471, // 星尘之翼
            4954, // 天界星盘
        };

        // 无防护假设：特效开关（每帧 ResetEffects 会重置，满足条件时再置 true）
        private bool noArmorHypothesisActive = false;

        // 计时器（节流+相位推进）：保证“有序/无序”都在持续演化
        private int noArmorVfxTimer = 0;

        // 可调范围：15×16
        private const float NoArmorVfxRadius = 15f * 16f;

        public override void ResetEffects()
        {
            finalDamageMultiplier = 1f;
            noArmorHypothesisActive = false;
        }


        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            // ❗所有判断都在这里
            if (Player.HeldItem.type == ModContent.ItemType<Sunset>() &&
                Player.statDefense == 0 &&
                AccessoriesAreWhitelistOnly())
            {
                // === 计时器推进 ===
                noArmorVfxTimer++;

                // === 数值效果 ===
                ApplySunsetSpeedBonuses();

                // === 特效节流 ===
                if ((noArmorVfxTimer % 2) == 0)
                {
                    float speed = Player.velocity.Length();
                    float speedFactor = MathHelper.Clamp(speed / 12f, 0f, 1f);
                    SpawnNoArmorHypothesisVfx(speedFactor);
                }
            }
            else
            {
                // 条件一旦不成立，状态立刻断
                noArmorVfxTimer = 0;
            }
        }


        private bool AccessoriesAreWhitelistOnly()
        {
            bool hasAnyAccessory = false;

            // 正确：只检查饰品槽位
            for (int i = 3; i < 10; i++) // 3~9 是标准饰品位
            {
                Item item = Player.armor[i];

                if (item == null || item.IsAir)
                    continue;

                hasAnyAccessory = true;

                // 出现任意非白名单饰品 → 直接失败
                if (!MobilityAccessoriesMod.Contains(item.type) &&
                  !MobilityAccessoriesVanilla.Contains(item.type))
                {
                    return false;
                }
            }

            // ✔ 只要没出现非法饰品：
            // - 饰品为空 → true
            // - 饰品全是白名单 → true
            return true;
        }

        private void ApplySunsetSpeedBonuses()
        {
            // 调试用：在玩家头顶弹字，确认函数是否被调用
            //CombatText.NewText(
            //    Player.getRect(),      // 玩家碰撞箱 → 字会出现在头顶
            //    Color.Cyan,            // 颜色随便，用亮一点的方便看
            //    "DEBUG_SPEED_ACTIVE",  // 内容无所谓，能看到就行
            //    true                   // 显示为“暴击样式”，更显眼
            //);

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
            noArmorHypothesisActive = true;
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





        /// <summary>
        /// 无防护假设特效：半径 15×16 圆域内生成尾流
        /// 70% 无序（受控随机），30% 有序（确定性数学轨迹）
        /// 三种粒子：Point / WaterMist / Square
        /// 颜色：天蓝 + 浅灰（不使用光晕定点粒子）
        /// </summary>
        /// <summary>
        /// 无防护假设特效：以“玩家中心向下 2×16”的点作为矩形核心点，
        /// 在一个 3×16（横向） × 1×16（纵向）的长方形内生成尾流，然后统一向后喷射。
        /// 70% 无序（受控随机），30% 有序（确定性数学轨迹）
        /// 三种粒子：Point / WaterMist / Square
        /// 颜色：天蓝 + 浅灰（不使用光晕定点粒子）
        /// </summary>
        private void SpawnNoArmorHypothesisVfx(float speedFactor)
        {
            // 1️⃣ 喷口核心点：玩家中心向下 2×16
            Vector2 rectCenter = Player.Center + new Vector2(0f, Player.gfxOffY + 2f * 16f);

            // 2️⃣ 喷口矩形尺寸：3×16（横） × 1×16（纵）
            const float rectHalfWidth = 3f * 16f * 0.5f;
            const float rectHalfHeight = 1f * 16f * 0.5f;

            // 3️⃣ 后方主方向（保持你原来正确的逻辑）
            Vector2 backDir = new Vector2(-Player.direction, 0f);

            // 4️⃣ 单侧扇形最大展开角（原来是 ±x，现在是 0 → 2x）
            float maxAngle = MathHelper.ToRadians(28f); // 原来若是 ±14°，现在就是 0~28°

            int count = 10 + (int)(12f * speedFactor);

            for (int i = 0; i < count; i++)
            {
                // ──────────────
                // 出生点：严格限制在矩形喷口内
                // ──────────────
                Vector2 offset = new Vector2(
                    Main.rand.NextFloat(-rectHalfWidth, rectHalfWidth),
                    Main.rand.NextFloat(-rectHalfHeight, rectHalfHeight)
                );

                Vector2 spawnPos = rectCenter + offset;

                // ──────────────
                // ⭐ 核心修改点 ⭐
                // 单侧角度：0 → +2x
                // ──────────────
                float angleOffset = Main.rand.NextFloat(-maxAngle, 0f);

                // 为了左右一致性：朝右时反向旋转
                if (Player.direction == 1)
                    angleOffset = -angleOffset;

                Vector2 shootDir = backDir.RotatedBy(angleOffset);

                float speed = Main.rand.NextFloat(8f, 14f) * (0.6f + 0.7f * speedFactor);
                Vector2 velocity = shootDir * speed;

                // ──────────────
                // 粒子混合（与你给的三种一致）
                // ──────────────
                if (Main.rand.NextBool(3))
                {
                    PointParticle p = new PointParticle(
                        spawnPos,
                        velocity,
                        false,
                        Main.rand.Next(12, 18),
                        1.0f + Main.rand.NextFloat(0.4f),
                        Color.Lerp(Color.LightSkyBlue, Color.LightGray, 0.35f)
                    );
                    GeneralParticleHandler.SpawnParticle(p);
                }
                else if (Main.rand.NextBool(2))
                {
                    WaterFlavoredParticle mist = new WaterFlavoredParticle(
                        spawnPos,
                        velocity * 0.8f,
                        false,
                        Main.rand.Next(18, 26),
                        0.9f + Main.rand.NextFloat(0.3f),
                        Color.LightSkyBlue * 0.9f
                    );
                    GeneralParticleHandler.SpawnParticle(mist);
                }
                else
                {
                    SquareParticle sq = new SquareParticle(
                        spawnPos,
                        velocity * 0.6f,
                        false,
                        Main.rand.Next(20, 30),
                        1.2f + Main.rand.NextFloat(0.5f),
                        Color.Cyan * 1.1f
                    );
                    sq.Rotation = velocity.ToRotation();
                    GeneralParticleHandler.SpawnParticle(sq);
                }
            }
        }

        // =========================
        // 工具函数：全部写在本类里，避免依赖不确定扩展
        // =========================

        private static Vector2 RandomInCircle(float radius)
        {
            // sqrt 采样：保证圆域均匀密度（“无序也要优雅”）
            float a = Main.rand.NextFloat(MathHelper.TwoPi);
            float r = (float)Math.Sqrt(Main.rand.NextFloat()) * radius;
            return a.ToRotationVector2() * r;
        }

        private static Vector2 SafeNormalize(Vector2 v, Vector2 fallback)
        {
            float lenSq = v.LengthSquared();
            if (lenSq < 1e-6f)
                return fallback;
            float inv = 1f / (float)Math.Sqrt(lenSq);
            return v * inv;
        }

        private static float Frac(float x) => x - (float)Math.Floor(x);















        public bool NoArmorHypothesisActive => noArmorHypothesisActive;

        public static void ApplyNoArmorHypothesisHitEffect(
                Projectile projectile,
                NPC target,
                ref NPC.HitModifiers modifiers
            )
        {
            Player owner = Main.player[projectile.owner];
            SunsetPlayerSpeed sp = owner.GetModPlayer<SunsetPlayerSpeed>();

            // 技能未开启，直接返回，保持原版逻辑
            if (!sp.NoArmorHypothesisActive)
                return;

            // =========================
            // 无视防御
            // =========================
            modifiers.DefenseEffectiveness *= 0f;

            // =========================
            // 无视伤害减免（DR）
            // =========================
            float dr = target.Calamity().DR;
            if (dr > 0f && dr < 0.999f)
                modifiers.FinalDamage /= (1f - dr);
        }



    }



}

