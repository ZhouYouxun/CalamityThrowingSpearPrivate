using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class SagittariusPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Sagittarius/Sagittarius";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";

        private static int shotCounter = 0;

        // ====== 自定义计时器（禁止用localAI）======
        private int timer = 0;
        private bool hasCharged = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D projTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D squareTex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSquareParticleThick").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = projTex.Size() * 0.5f;

            Color glowColor = Color.LightGoldenrodYellow with { A = 0 };

            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            // ===== 脉冲 =====
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.5f + 0.5f;
            float pulseScale = MathHelper.Lerp(0.15f, 0.45f, pulse);
            float pulseAlpha = MathHelper.Lerp(0.2f, 0.7f, pulse);

            // ===== 前后往返（核心）=====
            float move = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f); // -1~1
            float offsetDist = move * (2f * 16f); // 2×16范围

            // 使用rotation方向，而不是velocity（修正90°问题）
            Vector2 forwardDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();

            Vector2[] positions =
            {
    drawPos + (-forwardDir * 2f * 16f),
    drawPos,
    drawPos + (forwardDir * 2f * 16f)
};

            // 当前运动位置（在三点之间往返）
            Vector2 movingPos = drawPos + forward * offsetDist;

            // ===== 方形光晕（脉冲 + 旋转 + 往返）=====
            if (pulse > 0.15f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float rot = Main.GlobalTimeWrappedHourly * 3f + i * 0.5f;

                    Main.EntitySpriteDraw(
                        squareTex,
                        movingPos,
                        null,
                        glowColor * (pulseAlpha - i * 0.15f),
                        rot + MathHelper.PiOver4,
                        squareTex.Size() * 0.5f,
                        Projectile.scale * (pulseScale + i * 0.1f),
                        SpriteEffects.None
                    );
                }
            }

            // ===== 自身光晕（红白结构版，但改成白黄）=====
            float radius = 4f;            // 固定半径（不再脉冲）
            float segmentWidth = 8f;      // 控制颜色间隔

            for (int i = 0; i < 12; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * radius;

                // ❗白黄间隔逻辑（核心）
                Color c = ((i * radius) % (2 * segmentWidth) < segmentWidth)
                    ? Color.White * 0.7f
                    : Color.Goldenrod * 0.7f;

                c.A = 0;

                Main.EntitySpriteDraw(
                    projTex,
                    drawPos + offset,
                    null,
                    c,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None
                );
            }

            // ===== 残影 =====
            for (int i = 0; i < 3; i++)
            {
                Vector2 trailOffset = -Projectile.velocity * i * 0.15f;

                Main.EntitySpriteDraw(
                    projTex,
                    drawPos + trailOffset,
                    null,
                    glowColor * (0.5f - i * 0.15f),
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None
                );
            }

            // ===== 主体 =====
            Main.EntitySpriteDraw(
                projTex,
                drawPos,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None
            );

            return false;
        }










        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 420;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
        }

        public override void AI()
        {
            timer++;

            // ================= 第一阶段：锁敌+减速 =================
            if (timer <= 35)
            {
                NPC target = Projectile.Center.ClosestNPCAt(8200f);

                if (target != null)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float targetRot = dir.ToRotation() + MathHelper.PiOver4;

                    // 限制角速度（核心）
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRot, 0.12f);
                }

                // 减速
                Projectile.velocity *= 0.95f;
            }
            else
            {
                // ================= 第二阶段：冲刺 =================
                if (!hasCharged)
                {
                    hasCharged = true;

                    SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);

                    // 重新锁定一次目标（关键！）
                    NPC target = Projectile.Center.ClosestNPCAt(8200f);

                    Vector2 forward;

                    // 如果有目标 → 精准冲向目标
                    if (target != null)
                    {
                        forward = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    }
                    // 如果没有目标 → 保持当前方向
                    else
                    {
                        forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    }

                    // 冲刺
                    Projectile.velocity = forward * 48f;

                    // 同步rotation（只用于显示）
                    Projectile.rotation = forward.ToRotation() + MathHelper.PiOver4;

                    // ====== 冲击波（参考harpoon风格）======
                    for (int i = 0; i < 2; i++)
                    {
                        Particle pulse = new DirectionalPulseRing(
                            Projectile.Center - forward * 10f,
                            -forward * 4f,
                            Color.Gold,
                            new Vector2(1.2f, 2.0f),
                            forward.ToRotation(),
                            0.2f,
                            0.4f,
                            25
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }



                    // ====== 初始爆闪（鱼骨+鱼肉结构重构版）======
                    Vector2 backward = -forward;

                    int count = 18;
                    float spread = MathHelper.ToRadians(60f);

                    // ❗主骨架（Glow）固定数量
                    float strengthMul = 1.25f;

                    // ❗肉层密度倍率
                    int densityMul = 3;

                    for (int i = 0; i < count; i++)
                    {
                        float t = (float)i / (count - 1);
                        float angle = MathHelper.Lerp(-spread, spread, t);

                        Vector2 dir = backward.RotatedBy(angle);

                        // ================= 鱼骨（主结构）=================
                        Vector2 vel = dir * Main.rand.NextFloat(8f, 14f) * strengthMul;

                        Particle spark = new GlowSparkParticle(
                            Projectile.Center,
                            vel,
                            false,
                            Main.rand.Next(6, 9),
                            0.04f * strengthMul,
                            Color.LightGoldenrodYellow * 0.7f,
                            new Vector2(2.2f, 0.5f),
                            true,
                            false,
                            1
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // ================= 鱼肉层（线性流）=================
                        for (int j = 0; j < densityMul; j++)
                        {
                            // ❗数学分层：内外层速度梯度
                            float layer = (j + 1f) / densityMul;

                            Vector2 meatVel =
                                dir.RotatedByRandom(MathHelper.ToRadians(8f + 10f * layer)) *
                                MathHelper.Lerp(3f, 9f, layer);

                            Particle trail = new SparkParticle(
                                Projectile.Center,
                                meatVel + Projectile.velocity * 0.08f * layer,
                                false,
                                Main.rand.Next(35, 65),
                                0.5f + 0.4f * layer,
                                Color.Lerp(Color.Orange, Color.Goldenrod, layer)
                            );

                            GeneralParticleHandler.SpawnParticle(trail);
                        }

                        // ================= EXO肉层（高能随机点）=================
                        int exoCount = 2 + Main.rand.Next(2); // 每骨架2~3个

                        for (int k = 0; k < exoCount; k++)
                        {
                            // ❗数学扰动：角度 + 半径联动
                            float chaos = Main.rand.NextFloat(0.2f, 1f);

                            Vector2 randVel =
                                dir.RotatedByRandom(MathHelper.ToRadians(20f + 20f * chaos)) *
                                MathHelper.Lerp(0.4f, 1.4f, chaos);

                            SquishyLightParticle exoEnergy = new(
                                Projectile.Center,
                                randVel,
                                0.12f + 0.1f * chaos,
                                Color.Lerp(Color.Orange, Color.Yellow, chaos),
                                14 + (int)(chaos * 10),
                                opacity: 0.85f,
                                squishStrenght: 0.8f + chaos * 0.5f,
                                maxSquish: 1.8f + chaos,
                                hueShift: 0f
                            );

                            GeneralParticleHandler.SpawnParticle(exoEnergy);
                        }
                    }








                }

                // ====== VelChangingSpark（完全仿官方结构，但改为“前 → 回弹幕”）======

                Vector2 forwardDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                // ❗“虚构玩家位置” = 弹幕前方一段距离
                Vector2 fakePlayerPos = Projectile.Center + forwardDir * 220f;

                // ❗从“靠近目标但不贴”的位置生成
                Vector2 sparkPos = fakePlayerPos - forwardDir * 220f * Utils.GetLerpValue(0, 200, 32f);

                // 强度倍率（你这里没有stack，就固定一个动态感）
                float intensity = 2.2f;
                float sizeBonus = 1.8f;

                // 粒子数量（模拟官方particleLevel）
                int particleLevel = 4;

                for (int d = 0; d < particleLevel; d++)
                {
                    // 颜色（完全照搬风格）
                    Color color = Main.rand.NextBool()
                        ? Color.Goldenrod
                        : Color.Lerp(Color.OrangeRed, Color.Orange, Main.rand.NextFloat());

                    // ❗速度强度
                    float velAdjust = Main.rand.NextFloat(4f, 10f) * intensity * sizeBonus;

                    // ❗终点速度：朝“弹幕本体”
                    Vector2 endVel = (Projectile.Center - sparkPos).SafeNormalize(Vector2.UnitX) * velAdjust;

                    // ❗起始速度：在终点基础上随机偏移（制造弧线）
                    Vector2 startVel = endVel.RotatedByRandom(0.6f * intensity);

                    Particle sparks = new VelChangingSpark(
                        sparkPos,
                        startVel,
                        endVel,
                        "CalamityMod/Particles/SmallBloom",
                        Main.rand.Next(18, 23),
                        Main.rand.NextFloat(0.1f, 0.25f) * sizeBonus,
                        color * 0.75f,
                        new Vector2(0.7f, 1f),
                        true,
                        false,
                        0,
                        false,
                        0.45f,
                        0.1f
                    );

                    GeneralParticleHandler.SpawnParticle(sparks);

                    // ===== Dust辅助（照搬）=====
                    if (Main.rand.NextBool())
                    {
                        Dust dust = Dust.NewDustPerfect(
                            sparkPos,
                            57, // 用一个安全ID
                            startVel,
                            0,
                            color,
                            Main.rand.NextFloat(0.5f, 0.9f) * Math.Min(sizeBonus, 1.3f)
                        );

                        dust.noGravity = true;
                        dust.noLightEmittence = true;
                    }
                }


            }

            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);

            // ====== 强化命中特效 ======
            Vector2 pos = target.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            // 核心爆闪
            for (int i = 0; i < 6; i++)
            {
                Particle core = new GlowSparkParticle(
                    pos,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    false,
                    6,
                    0.18f,
                    Color.Gold * 0.9f,
                    new Vector2(1.4f, 0.6f),
                    true,
                    false,
                    1
                );
                GeneralParticleHandler.SpawnParticle(core);
            }

            // 喷流
            for (int i = 0; i < 14; i++)
            {
                Vector2 vel =
                    forward.RotatedByRandom(MathHelper.ToRadians(12f))
                    * Main.rand.NextFloat(5f, 10f);

                Particle jet = new GlowSparkParticle(
                    pos,
                    vel,
                    false,
                    Main.rand.Next(8, 12),
                    Main.rand.NextFloat(0.12f, 0.18f),
                    new Color(255, 210, 80),
                    new Vector2(2.4f, 0.45f),
                    true,
                    false,
                    1
                );
                GeneralParticleHandler.SpawnParticle(jet);
            }

            // 冠环
            int count = 14;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                Particle corona = new GlowSparkParticle(
                    pos,
                    vel,
                    false,
                    10,
                    0.14f,
                    Color.Orange * 0.8f,
                    new Vector2(1.5f, 0.5f),
                    true,
                    false,
                    1
                );
                GeneralParticleHandler.SpawnParticle(corona);
            }

            shotCounter++;
            if (shotCounter >= 4)
            {
                // ===== 目标头顶随机区域 =====
                Vector2 targetCenter = target.Center;

                // 上方X×16为基准，再左右随机Y×16范围
                Vector2 spawnPos = targetCenter
                    + new Vector2(
                        Main.rand.NextFloat(-10f * 16f, 10f * 16f),   // 左右随机
                        -55f * 16f + Main.rand.NextFloat(-1f * 16f, 1f * 16f) // 上方浮动
                    );

                // 向目标砸下
                Vector2 direction = (targetCenter - spawnPos).SafeNormalize(Vector2.UnitY);

                // 速度=原速度1.3倍
                Vector2 velocity = direction * Projectile.velocity.Length() * 1.3f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<SagittariusPROJECHO>(),
                    (int)(Projectile.damage * 5.5f), // 伤害5.5倍
                    Projectile.knockBack,
                    Projectile.owner
                );

                shotCounter = 0;
            }
        }
    }
}