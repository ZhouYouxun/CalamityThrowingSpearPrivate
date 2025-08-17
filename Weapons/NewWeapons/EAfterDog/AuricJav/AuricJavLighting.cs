using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.ID;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    public class AuricJavLighting : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public int time = 0;

        public override bool PreDraw(ref Color lightColor)
        {
            // 电能波动缩放因子（加入 sin 波动让电感更自然）
            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            // 外圈贴图：灵魂漩涡
            Texture2D vortexTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f - Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 1.5f; // ⚡反向旋转 + 加速
                Color outerColor = Color.Lerp(Color.Cyan, Color.White, 0.5f + 0.5f * (float)Math.Sin(time * 0.2f + i)); // ⚡亮白交替蓝
                outerColor.A = 0;

                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                float scale = ((Projectile.scale * (1 - i * 0.07f)) * 0.2f) * pulse;

                Main.EntitySpriteDraw(
                    vortexTexture,
                    drawPosition,
                    null,
                    outerColor * Projectile.Opacity,
                    angle + MathHelper.PiOver2,
                    vortexTexture.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // 内圈贴图：BloomCircle
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/Lightning").Value;
            float randSize = Main.rand.NextFloat(0.05f, 0.03f);

            // 中心闪电蓝 + 外圈白
            Color innerColor = Color.Lerp(Color.LightBlue, Color.White, 0.4f);
            innerColor.A = 0;

            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(bloom, drawPos, null, innerColor, 0f, origin, 0.28f * randSize * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(bloom, drawPos, null, Color.White with { A = 0 }, 0f, origin, 0.12f * randSize * pulse, SpriteEffects.None, 0);

            return false;
        }



        public override void OnSpawn(IEntitySource source)
        {
            // 屏幕震动效果
            float shakePower = 50f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 4;
        }


        public override void AI()
        {
            float angleRange = MathHelper.ToRadians(25f);
            float randomAngle = Main.rand.NextFloat(-angleRange, angleRange);
            Vector2 particleVelocity = Projectile.velocity.RotatedBy(MathHelper.Pi + randomAngle).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);

            //Color particleColor = Main.rand.NextBool() ? Color.LightPink : Color.LightSalmon;
            //float randomScale = Main.rand.NextFloat(0.85f, 1.25f);
            //Particle bolt = new CrackParticle(
            //    Projectile.Center,
            //    particleVelocity,
            //    particleColor * 0.65f,
            //    Vector2.One * randomScale,
            //    0, 0,
            //    randomScale,
            //    11
            //);
            //GeneralParticleHandler.SpawnParticle(bolt);

            // 🌩️ 电球飞行特效视觉整合

            // 1. 三分之一圆切割闪电
            if (Main.rand.NextBool(3))
            {
                TrientCircularSmear smear = new TrientCircularSmear(
                    Projectile.Center,
                    Color.White * Main.rand.NextFloat(0.75f, 0.85f),
                    Main.rand.NextFloat(-8f, 8f),
                    Main.rand.NextFloat(1.5f, 2.1f)
                );
                GeneralParticleHandler.SpawnParticle(smear);
            }



            {
                // === 飞行特效：Auric Lightning Orb（36层动态版）===

                float globalTime = Main.GlobalTimeWrappedHourly * 60f; // 全局计时器
                int layerCount = 16; // 层数
                float baseRadius = 8f;

                for (int layer = 0; layer < layerCount; layer++)
                {
                    // 每层独立扰动：半径 + 角度 + 呼吸缩放
                    float timeShift = globalTime * (0.02f + layer * 0.001f);
                    float angleShift = timeShift + layer * 0.35f + Main.rand.NextFloat(-0.05f, 0.05f);
                    float radius = baseRadius + layer * 2.5f + (float)Math.Sin(globalTime * 0.05f + layer) * 3f;

                    // 每层多点（动态旋转）
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = angleShift + i * MathHelper.TwoPi / 8f;
                        Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                        // --- 动态火花 ---
                        LineParticle spark = new LineParticle(
                            pos,
                            angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f),
                            false,
                            1 + Main.rand.Next(10),
                            1.9f + (float)Math.Sin(globalTime * 0.1f + layer) * 0.3f,
                            Color.Lerp(Color.OrangeRed, Color.Gold, Main.rand.NextFloat())
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // --- 点状能量 ---
                        if (Main.rand.NextBool(3))
                        {
                            PointParticle dot = new PointParticle(
                                pos,
                                -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.6f),
                                false,
                                15 + Main.rand.Next(10),
                                0.8f + Main.rand.NextFloat(0.6f),
                                Color.Lerp(Color.Cyan, Color.LightYellow, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(dot);
                        }

                        // --- 方形科技光点 ---
                        if (Main.rand.NextBool(6))
                        {
                            SquareParticle sq = new SquareParticle(
                                pos,
                                Main.rand.NextVector2Circular(0.5f, 0.5f),
                                false,
                                25 + Main.rand.Next(15),
                                1.0f + (float)Math.Cos(globalTime * 0.08f + i + layer) * 0.5f,
                                Color.Lerp(Color.Cyan, Color.DeepSkyBlue, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(sq);
                        }
                    }
                }

                // 🔥 扩散喷流（随机方向，每帧不同角度偏移）
                for (int i = 0; i < 200; i++)
                {
                    float dynamicAngle = globalTime * 0.02f + i * 0.07f + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 dir = dynamicAngle.ToRotationVector2();
                    float speed = Main.rand.NextFloat(6f, 12f);
                    LineParticle burst = new LineParticle(
                        Projectile.Center,
                        dir * speed,
                        false,
                        2 + Main.rand.Next(15),
                        1.7f + Main.rand.NextFloat(0.6f),
                        Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat())
                    );
                    GeneralParticleHandler.SpawnParticle(burst);
                }

               

                // 💎 GlowOrb 动态闪烁
                if (Main.rand.NextBool(2))
                {
                    GlowOrbParticle orb = new GlowOrbParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                        Vector2.Zero,
                        false,
                        15,
                        1.0f + Main.rand.NextFloat(0.5f),
                        Color.Lerp(Color.LightBlue, Color.Yellow, (float)Math.Sin(globalTime * 0.1f)),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }






                // 3️⃣ 电弧裂纹（黄色，扩散叠加）
                if (Main.rand.NextBool(2))
                {
                    for (int j = 0; j < 2; j++)
                    {
                        CrackParticle crack = new CrackParticle(
                            Projectile.Center,
                            Main.rand.NextVector2Circular(1f, 1f),
                            Color.Lerp(Color.Yellow, Color.Orange, Main.rand.NextFloat()),
                            new Vector2(1f, 1f),
                            Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                            0.1f,
                            0.5f,
                            25 + Main.rand.Next(10)
                        );
                        GeneralParticleHandler.SpawnParticle(crack);
                    }
                }

                // 4️⃣ 原有刀盘粒子（保持，不动）
                // 这部分是 PreDraw 里的 twirl_01 + Lightning，那些不要删

                // 5️⃣ 外圈“电子环绕”强化（更稳定的四角小光点）
                float rot = Main.GameUpdateCount * 0.12f;
                for (int k = 0; k < 4; k++)
                {
                    Vector2 orbit = (rot + k * MathHelper.PiOver2).ToRotationVector2() * 20f;
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + orbit,
                        DustID.Electric,
                        Vector2.Zero,
                        100,
                        Color.Lerp(Color.Cyan, Color.LightYellow, Main.rand.NextFloat()),
                        1.3f
                    );
                    d.noGravity = true;
                }

            }















            time++;

        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 选择最近的敌人，但排除：
            // - 刚刚命中的 `target`
            // - 距离小于 20 像素的敌人（防止来回弹射相同目标）
            NPC closestNPC = Main.npc
                .Where(npc => npc.active && !npc.friendly && npc.life > 0
                    && npc.whoAmI != target.whoAmI // 不选同一个敌人
                    && Vector2.Distance(npc.Center, Projectile.Center) > 20f) // 至少 20 像素外
                .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                .FirstOrDefault();

            if (closestNPC != null)
            {
                // 计算新方向
                Vector2 direction = Vector2.Normalize(closestNPC.Center - Projectile.Center);
                Projectile.velocity = direction * Projectile.velocity.Length();

                // 立即调整位置，防止卡在原地
                //Projectile.position = closestNPC.Center - Projectile.Size * 0.5f;
            }
            else
            {
                // 没有可弹射目标时，减少穿透次数
                //Projectile.penetrate--;
            }


            {
                // ⚡命中爆裂闪电特效（朝前扩散）
                Vector2 impactDirection = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                int amount = Main.rand.Next(5, 8);

                for (int i = 0; i < amount; i++)
                {
                    // 随机扩散角度 ±30度
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                    Vector2 velocity = impactDirection.RotatedBy(angleOffset) * Main.rand.NextFloat(2f, 5f);

                    Color color = Main.rand.NextBool() ? Color.Cyan : Color.LightBlue;
                    float scale = Main.rand.NextFloat(0.9f, 1.4f);

                    Particle spark = new CrackParticle(
                        Projectile.Center + impactDirection * 6f,
                        velocity,
                        color * 0.8f,
                        Vector2.One * scale,
                        0, 0,
                        scale,
                        14
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }


                {
                    // 电场几何 Dust 爆发
                    int[] electricDustFancy = new int[] { 230, 226, 187 };

                    // ① 正六边方向扩散
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                        Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 6f;

                        Dust d = Dust.NewDustPerfect(pos, electricDustFancy[Main.rand.Next(electricDustFancy.Length)], velocity, 100, Color.White, 1.1f);
                        d.noGravity = true;
                        d.fadeIn = 1.2f;
                    }

                    // ② 星形扩散（细分方向）
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.1f, 0.1f);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                        Vector2 pos = Projectile.Center;

                        Dust d = Dust.NewDustPerfect(pos, electricDustFancy[Main.rand.Next(electricDustFancy.Length)], velocity, 120, Color.Cyan, 0.9f);
                        d.noGravity = true;
                        d.fadeIn = 1.3f;
                    }

                    // ③ 同心圆震波
                    for (int ring = 1; ring <= 3; ring++)
                    {
                        float radius = 8f * ring;
                        for (int i = 0; i < 10; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.05f, 0.05f);
                            Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                            Vector2 velocity = angle.ToRotationVector2() * 0.5f;

                            Dust d = Dust.NewDustPerfect(pos, electricDustFancy[Main.rand.Next(electricDustFancy.Length)], velocity, 150, Color.LightBlue, 0.7f);
                            d.noGravity = true;
                        }
                    }

                    // ④ 边缘闪点
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                        Vector2 velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                        Dust d = Dust.NewDustPerfect(pos, electricDustFancy[Main.rand.Next(electricDustFancy.Length)], velocity, 180, Color.White, 1.2f);
                        d.noGravity = true;
                        d.fadeIn = 1.1f;
                    }
                }


            }



            // 播放命中音效
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/AuricBulletHit"), Projectile.Center);
        }


    }

}
