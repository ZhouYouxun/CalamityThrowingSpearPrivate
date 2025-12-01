using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget;
using Terraria.DataStructures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        // ========= 能量条系统（蓄力 → 满格 → 释放大招）=========

        // 当前蓄力值（0 ~ energyMax）
        private float energyCharge = 0f;

        // 每次发射一发小弹幕就增加多少能量
        private const float energyGainPerShot = 1f;

        // 满格阈值（例如发射 120 发小弹幕后触发）
        private const float energyMax = 120f;

        // 大招刚触发后锁定 10 帧，避免重复触发
        private int energyReleaseLock = 0;

        public override bool PreDraw(ref Color lightColor)
        {
            // 先画弹幕本体（你原来的效果保持不变）
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // ============ 开始绘制能量条 ============

            if (Main.myPlayer == Projectile.owner)
            {
                var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
                var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

                // 玩家头顶 36 像素位置
                Vector2 drawPos = Owner.Center - Main.screenPosition + new Vector2(0, -48);

                // 能量比例（0~1）
                float p = energyCharge / energyMax;
                p = MathHelper.Clamp(p, 0f, 1f);

                // 裁切前景条显示长度
                Rectangle frame = new Rectangle(0, 0, (int)(barFG.Width * p), barFG.Height);

                // 动态颜色：蓝 → 青 → 白
                float hue = (Main.GlobalTimeWrappedHourly * 0.25f) % 1f;
                Color barColor = Main.hslToRgb(hue, 1f, 0.75f);

                // 后景
                Main.spriteBatch.Draw(
                    barBG,
                    drawPos - barBG.Size() * 0.5f,
                    null,
                    barColor * 0.50f,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );

                // 前景
                Main.spriteBatch.Draw(
                    barFG,
                    drawPos - barFG.Size() * 0.5f,
                    frame,
                    barColor,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }

        public enum BehaviorState
        {
            Aim,
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 🚨 遍历所有投射物，检查是否已有 `Aim` 状态的 `SunsetASunsetRight`
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.whoAmI != Projectile.whoAmI)
                {
                    // 仅检测相同类型的 `Aim` 状态投射物
                    if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() && proj.ModProjectile is SunsetASunsetRight rightProj && rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill(); // ❌ 删除自己（新的投射物）
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetBForgetRight>() && proj.ModProjectile is SunsetBForgetRight forgetProj && forgetProj.CurrentState == SunsetBForgetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() && proj.ModProjectile is SunsetCConceptRight conceptProj && conceptProj.CurrentState == SunsetCConceptRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }
        }
        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
            }
        }

        private int holdTime = 0; // 握持时间（帧）

        private int vfxTimer = 0;
        // 科技蓝主色 + 辅色
        private static readonly Color[] TechBluePalette = new Color[]
        {
    new Color( 80, 200, 255),  // Electric Blue
    new Color(120, 220, 255),  // Light Tech Blue
    Color.Cyan,
    new Color(180, 220, 255),  // 冷白蓝
    Color.WhiteSmoke           // 高光
        };


        // 在类的字段区定义（和 shootTimer / holdTime 一样）
        private int shotIndex = 0;
        private int frameTimer = 0;

        // 记录上一次魔法阵触发时间（用 holdTime 单位计）
        private int lastMagicFireTime = 0;

        // 魔法阵 → 大弹幕之间的 1.5 秒延迟计数器（90 帧）
        private int magicDelayTimer = 0;

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;



            {
                // ============================
                //  小弹幕发射计时系统（新版）
                // ============================
                frameTimer++;
                holdTime++;                     // 👉 用这个来判断60秒是否已到
                Projectile.timeLeft = 300;      // 固定

                const float baseRingRadius = 70f * 16f;

                // --------------------------------------
                // 1) 小弹幕发射频率：从 18 帧 → 每发射 2 个 -1 帧 → 最快 5f
                // --------------------------------------
                int minInterval = 5;
                int startInterval = 18;
                int interval = Math.Max(minInterval, startInterval - (shotIndex / 2));

                if (frameTimer >= interval)
                {
                    frameTimer = 0;

                    // =======================
                    // 生成小弹幕（原逻辑不变）
                    // =======================
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = baseRingRadius * Main.rand.NextFloat(0.65f, 1.35f);
                    Vector2 spawnOffset = angle.ToRotationVector2() * radius;

                    NPC target = FindClosestTarget();
                    Vector2 ringCenter = (target != null) ? target.Center : Projectile.Center;

                    Vector2 spawnPos = ringCenter + spawnOffset;

                    Vector2 shootDir = (target != null) ?
                        (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) :
                        (-spawnOffset).SafeNormalize(Vector2.UnitY);

                    Vector2 velocity = shootDir * 40f;

                    // 顺序循环发射：1 → 2 → 3 → 4
                    int[] projTypes = new int[]
                    {
            ModContent.ProjectileType<SunsetCConceptRightCut1Time>(),
            ModContent.ProjectileType<SunsetCConceptRightCut2Item>(),
            ModContent.ProjectileType<SunsetCConceptRightCut3Space>(),
            ModContent.ProjectileType<SunsetCConceptRightCut4Energy>()
                    };

                    int pickType = projTypes[shotIndex % 4];

                    int totalCrit = (int)Math.Round(Owner.GetTotalCritChance(Projectile.DamageType));

                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        velocity,
                        pickType,
                        Projectile.damage / 2,
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    if (proj.WithinBounds(Main.maxProjectiles))
                    {
                        Projectile small = Main.projectile[proj];
                        small.CritChance = totalCrit;
                        small.scale = 1.5f;
                        small.penetrate = 1;
                        small.tileCollide = true;
                        small.usesLocalNPCImmunity = true;
                        small.localNPCHitCooldown = 10;
                        small.netUpdate = true;
                    }

                    shotIndex++;

                    {
                        // ========= 能量条充能 =========
                        if (energyCharge < energyMax)
                            energyCharge += energyGainPerShot;

                        // 确保不会超过上限
                        energyCharge = MathHelper.Clamp(energyCharge, 0, energyMax);

                    }

                    // ============================================================
                    // 2) 魔法阵 + 大弹幕：每 X0 秒触发一次
                    // ============================================================
                    // ==============================
                    // 大招冷却（测试用 1000）
                    // ==============================
                    const int magicCooldown = 1000;

                    // 每次触发记录上一次时间
                    if (!Projectile.localAI[0].Equals(0f))
                    {
                        lastMagicFireTime = (int)Projectile.localAI[0];
                    }

                    bool cooldownReached = holdTime - lastMagicFireTime >= magicCooldown;

                    // 用来等待魔法阵 → 大弹幕的 1.5 秒延迟
                    if (magicDelayTimer > 0)
                    {
                        magicDelayTimer--;

                        // 倒数完毕 → 释放大弹幕
                        if (magicDelayTimer == 0 && target != null)
                        {
                            Vector2 bigSpawnPos = target.Center + new Vector2(0, -30 * 16);
                            Vector2 bigVelocity = Vector2.UnitY * 30f;

                            int baseDmg = Projectile.damage;

                            float bigCoreMult = 6.5f;
                            float playerPower = Owner.GetDamage(DamageClass.Magic).Multiplicative * 1.0f;
                            float critPower = (Owner.GetTotalCritChance(DamageClass.Magic) / 100f);

                            int finalDamage = (int)(baseDmg * bigCoreMult * playerPower * (1f + critPower));
                            if (finalDamage < baseDmg * 3)
                                finalDamage = baseDmg * 3;

                            int p = Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                bigSpawnPos,
                                bigVelocity,
                                ModContent.ProjectileType<SunsetCConceptRightCutBig>(),
                                finalDamage,
                                Projectile.knockBack * 2f,
                                Projectile.owner
                            );

                            if (p.WithinBounds(Main.maxProjectiles))
                            {
                                Projectile big = Main.projectile[p];
                                big.penetrate = 9;
                                big.usesLocalNPCImmunity = true;
                                big.localNPCHitCooldown = 15;
                                big.extraUpdates = 1;
                                big.scale = 2.7f;
                                big.netUpdate = true;
                            }

                            // 重置充能
                            energyCharge = 0f;
                            energyReleaseLock = 10;
                        }
                    }


                    // ==============================
                    // （A）魔法阵触发
                    // ==============================
                    if (target != null && cooldownReached)
                    {
                        // 记录冷却触发点
                        lastMagicFireTime = holdTime;
                        Projectile.localAI[0] = lastMagicFireTime;

                        // 1. 先出现魔法阵
                        Vector2 magicSpawnPos = target.Center + new Vector2(0, -30 * 16);
                        Vector2 magicVelocity = Vector2.UnitY * 30f;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            magicSpawnPos,
                            magicVelocity,
                            ModContent.ProjectileType<SunsetCConceptRightMagic>(),
                            Projectile.damage * 8,
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        // 2. 延迟 X 秒 = X0 帧
                        magicDelayTimer = 30;
                    }



                }
            }







            {
                // ========= 枪口（枪头）空间点：指向速度方向 5 * 16 =========
                Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 15f;

                // ========= 科技蓝 VFX（瞄准阶段常驻）=========
                vfxTimer++;
                float phase = vfxTimer * 0.1f;

                // —— 环参数：以枪头为圆心的小环发射，观感更“顺滑”，不是硬贴中心 —— //
                int ringCount = 6;                  // 每圈数量
                float ringRadius = 18f;                // 初始半径
                float ringPulse = 4f * (float)Math.Sin(phase * 1.2f); // 轻微呼吸脉动
                float radiusNow = ringRadius + ringPulse;

                // 【1】EXO之光（SquishyLightParticle）：高亮柔光，密度适中（每2帧一圈）
                if ((vfxTimer % 2) == 0)
                {
                    for (int i = 0; i < ringCount; i++)
                    {
                        float a = MathHelper.TwoPi * i / ringCount + phase * 0.25f;  // 有序 + 缓慢相位
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * radiusNow;              // ★ 环上生成
                                                                                      // 径向外喷 + 少量切向（微旋）
                        Vector2 vel = dir * Main.rand.NextFloat(1.6f, 2.8f) + dir.RotatedBy(MathHelper.PiOver2) * 0.3f;

                        // 科技蓝主色
                        Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                        SquishyLightParticle exo = new SquishyLightParticle(
                            pos,
                            vel,
                            Main.rand.NextFloat(0.22f, 0.34f),  // 缩放更“亮点”
                            c,
                            Main.rand.Next(20, 28),             // 短寿命更干净
                            opacity: 1f,
                            squishStrenght: Main.rand.NextFloat(0.9f, 1.2f),
                            maxSquish: 3.2f,
                            hueShift: 0f
                        );
                        GeneralParticleHandler.SpawnParticle(exo);
                    }
                }

                // 【2】辉光球（GlowOrbParticle）：清爽的亮点，稀疏（每3帧，半圈）
                if ((vfxTimer % 3) == 0)
                {
                    int orbCount = ringCount / 2; // 半圈
                    for (int i = 0; i < orbCount; i++)
                    {
                        float a = MathHelper.TwoPi * i / orbCount + phase * 0.35f + 0.7f;
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * (radiusNow + 4f);
                        // 轻微向外
                        Vector2 vel = dir * Main.rand.NextFloat(0.6f, 1.2f);

                        Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                        GlowOrbParticle orb = new GlowOrbParticle(
                            pos,
                            vel,
                            false,
                            Main.rand.Next(6, 10),          // 6~9帧
                            Main.rand.NextFloat(0.75f, 0.95f),
                            c,
                            true,                           // 加法混合，提亮
                            false,
                            true                            // 中心叠白
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }

                // 【3】四方粒子（SquareParticle）：赛博能量片，适度点缀（每4帧 1~2 个）
                if ((vfxTimer % 4) == 0)
                {
                    int squares = Main.rand.Next(1, 3);
                    for (int s = 0; s < squares; s++)
                    {
                        float a = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * Main.rand.NextFloat(radiusNow - 6f, radiusNow + 6f); // 环附近“带宽”
                        Vector2 vel = dir * Main.rand.NextFloat(0.8f, 1.6f) + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.6f, 0.6f);

                        // 主色+少量白/浅金作高光
                        Color c = (Main.rand.NextBool(4) ? Color.Gold : TechBluePalette[Main.rand.Next(TechBluePalette.Length)]);
                        c *= 1.1f;

                        SquareParticle sq = new SquareParticle(
                            pos,
                            vel,
                            false,
                            Main.rand.Next(24, 36),                            // 24~35帧
                            1.2f + Main.rand.NextFloat(0.6f),                 // 1.2~1.8
                            c
                        );
                        GeneralParticleHandler.SpawnParticle(sq);
                    }
                }

            }


            // 检测松手，直接删除自身
            Player player = Main.player[Projectile.owner];
            if (!player.Calamity().mouseRight)
            {
                Projectile.Kill();
            }
        }


        private NPC FindClosestTarget()
        {
            NPC closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy()) // 确保是合法目标
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);

                    // 优先选择 Boss，如果不存在 Boss 则选择最近的小怪
                    if (npc.boss && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                    else if (!closestTarget?.boss ?? true) // 如果当前目标不是 Boss，允许更新为最近的非 Boss 目标
                    {
                        if (distance < closestDistance)
                        {
                            closestTarget = npc;
                            closestDistance = distance;
                        }
                    }
                }
            }
            return closestTarget;
        }


        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒
        }
    }
}