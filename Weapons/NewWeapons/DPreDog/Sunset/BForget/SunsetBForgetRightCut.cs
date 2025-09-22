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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Particles;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetRightCut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // ====================
            // ① 顶端位置计算
            // ====================
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 headPosition = Projectile.Center + forward * (texture.Height * 0.5f); // 顶端
            Vector2 headDrawPos = headPosition - Main.screenPosition;

            // ====================
            // ② Extra_89 脉动环绘制（在顶端）
            // ====================
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[89].Value;
            Texture2D star07 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_07").Value;
            Texture2D star08 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_08").Value;

            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 0.6f;
                // 蓝紫渐变替代原 Lime/ForestGreen
                Color ringColor = Color.Lerp(Color.DeepSkyBlue, Color.MediumPurple, (i % 2 == 0 ? 0.2f : 0.4f)) * 1.2f;
                float scale = (0.25f + 0.05f * i) * pulse * 2.5f;

                // Extra_89
                Main.EntitySpriteDraw(
                    ringTex,
                    headDrawPos,
                    null,
                    ringColor,
                    angle,
                    ringTex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );

                // star_07
                Main.EntitySpriteDraw(
                    star07,
                    headDrawPos,
                    null,
                    Color.Lerp(Color.CornflowerBlue, Color.MediumPurple, 0.25f) * 0.8f,
                    angle * 0.5f,
                    star07.Size() * 0.5f,
                    scale * 0.25f,
                    SpriteEffects.None,
                    0
                );

                // star_08
                Main.EntitySpriteDraw(
                    star08,
                    headDrawPos,
                    null,
                    Color.Lerp(Color.DeepSkyBlue, Color.Violet, 0.35f) * 0.8f,
                    -angle * 0.7f,
                    star08.Size() * 0.5f,
                    scale * 0.25f,
                    SpriteEffects.None,
                    0
                );
            }

            // ====================
            // ③ 周期性旋转呼吸光晕（环绕中心）
            // ====================
            float pulseGlow = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.3f + 0.7f;
            float glowScale = Projectile.scale * (1.0f + pulseGlow * 0.15f);
            float chargeOffset = 3f;

            // 蓝紫渐变呼吸光晕
            int segments = 16;
            float rotationPhase = Main.GlobalTimeWrappedHourly * 2f; // 整体旋转速度

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments + rotationPhase;
                Vector2 drawOffset = angle.ToRotationVector2() * chargeOffset;

                // 改为蓝紫动态过渡
                float lerpFactor = (float)Math.Sin(angle * 2f + Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f;
                Color glowColor = Color.Lerp(Color.DeepSkyBlue, Color.MediumPurple, lerpFactor) * 0.6f;
                glowColor.A = 0;

                Main.spriteBatch.Draw(
                    texture,
                    drawPosition + drawOffset,
                    null,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    glowScale,
                    SpriteEffects.None,
                    0f
                );
            }


            // ====================
            // ④ 渲染实际的投射物本体
            // ====================
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

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
        private bool spawnedTentacles = false;
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            Particle shrinkingpulse = new DirectionalPulseRing(
                Projectile.Center, // 粒子生成位置，与弹幕中心重合
                Vector2.Zero, // 粒子静止不动
                new Color(40, 60, 90), // 冲击波的颜色
                new Vector2(1f, 1f), // 冲击波的初始形状（圆形）
                Main.rand.NextFloat(3f, 6f), // 初始缩放大小
                0.15f, // 最终缩放大小（收缩到非常小）
                1f, // 设定扩散范围
                10 // 粒子的存活时间（10 帧）
            );
            // 生成收缩冲击波粒子
            GeneralParticleHandler.SpawnParticle(shrinkingpulse);

            // ✅ 保险：让二次弹幕在生成帧“自取一次”玩家当下总暴击
            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit;
        }
        private int flightTimer = 0;

        public override void AI()
        {
            if (!spawnedTentacles) // 确保只在生成时触发一次
            {
                spawnedTentacles = true;

                // 在前方 ±10° 角度范围内，随机选择 3~5 个角度释放触手
                int tentacleCount = Main.rand.Next(3, 6);
                for (int i = 0; i < tentacleCount; i++)
                {
                    float randomAngle = Main.rand.NextFloat(-10f, 10f); // 在 -10° 到 10° 之间随机选择角度
                    Vector2 tentacleVelocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(randomAngle)) * 4f;
                    SpawnTentacle(tentacleVelocity);
                }
            }

            flightTimer++;

            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // 命中过一次 → 开始快速停下
            //bool hitEnemy = Projectile.penetrate < 200;
            //if (hitEnemy)
            //{
            //    if (Projectile.timeLeft > 60)
            //        Projectile.timeLeft = 60;

            //    // 给一个衰减系数，后面 velocity 计算完再乘
            //    Projectile.velocity *= 0.88f;
            //}

            // —— 阶段 1：极短倒退 (0~5 帧) ——
            if (flightTimer < 5)
            {
                float t = flightTimer / 5f;
                Projectile.velocity = -forward * MathHelper.Lerp(5f, 0.2f, t);
            }
            // —— 阶段 2：极快前进过渡 (5~15 帧) ——
            else if (flightTimer < 15)
            {
                float t = (flightTimer - 5) / 10f;
                float speed = MathHelper.Lerp(0.5f, 12f, MathF.Pow(t, 0.8f));
                Projectile.velocity = forward * speed;
            }
            // —— 阶段 3：稳定飞行 ——
            else
            {
                Projectile.velocity = forward * 12f;
            }

            // 命中过的情况再叠加减速
            //if (hitEnemy)
            //{
            //    Projectile.velocity *= 0.88f;
            //}




            // 始终保持朝向 = 正前
            Projectile.rotation = forward.ToRotation() + MathHelper.PiOver4;

            {
                // =================== 优雅蓝紫配色 ===================
                Color[] techColors = {
    new Color(80, 160, 255),   // 明亮蓝
    new Color(50, 120, 220),   // 深蓝
    new Color(140, 100, 200),  // 紫罗兰
    new Color(100, 80, 160)    // 深紫
};

                // Spark 主干能流（带轻微螺旋）
                if (Main.rand.NextBool(3))
                {
                    float angleOffset = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f; // 螺旋感
                    Vector2 vel = -Projectile.velocity.RotatedBy(angleOffset) * 0.2f;

                    Particle trail = new SparkParticle(
                        Projectile.Center,
                        vel,
                        false,
                        30,
                        1.2f,
                        techColors[Main.rand.Next(techColors.Length)]
                    );
                    trail.Rotation = Projectile.rotation;
                    GeneralParticleHandler.SpawnParticle(trail);
                }

                // 方块碎片（带随机旋转 + 波动）
                if (Main.rand.NextBool(5))
                {
                    float wave = (float)Math.Sin(Main.GameUpdateCount * 0.3f + Projectile.whoAmI) * 6f;
                    Vector2 offset = forward.RotatedBy(MathHelper.PiOver2) * wave;

                    SquareParticle sq = new SquareParticle(
                        Projectile.Center + offset,
                        forward * 0.6f,
                        false,
                        25,
                        1.3f,
                        techColors[Main.rand.Next(techColors.Length)]
                    );
                    sq.Rotation = Projectile.rotation + Main.rand.NextFloat(-0.4f, 0.4f);
                    GeneralParticleHandler.SpawnParticle(sq);
                }

                // 水味能雾（带轻微旋转扩散）
                if (Main.rand.NextBool(4))
                {
                    float spiral = Main.GameUpdateCount * 0.05f; // 螺旋扩散
                    Vector2 vel = forward.RotatedBy(spiral + Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.4f, 1.0f);

                    WaterFlavoredParticle mist = new WaterFlavoredParticle(
                        Projectile.Center,
                        vel,
                        false,
                        18,
                        1.0f,
                        techColors[Main.rand.Next(techColors.Length)] * 0.8f
                    );
                    GeneralParticleHandler.SpawnParticle(mist);
                }

                // Dust 背景点缀（紫蓝混合）
                if (Main.rand.NextBool(6))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.BlueCrystalShard,
                        forward.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 1.5f),
                        140,
                        Color.Lerp(Color.CornflowerBlue, Color.MediumPurple, Main.rand.NextFloat()),
                        1.1f
                    );
                    d.noGravity = true;
                    d.rotation = Projectile.rotation;
                }

            }
        }



        private void SpawnTentacle(Vector2 tentacleVelocity)
        {
            int damage = Projectile.damage / 2;
            float kb = Projectile.knockBack;

            float ai0 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);
            float ai1 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, tentacleVelocity, ModContent.ProjectileType<SunsetBForgetTantacle>(), damage, kb, Projectile.owner, ai0, ai1);
        }

        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒

            Vector2 explosionPosition = target.Center;

            // 让 SquishyLightParticle 呈 **方形辐射状扩散**
            int particleCount = 16; // 粒子数量
            float boxSize = 32f; // 方形的边长

            for (int i = 0; i < particleCount; i++)
            {
                // 在方形区域内均匀生成粒子
                Vector2 offset = new Vector2(
                    Main.rand.NextFloat(-boxSize / 2, boxSize / 2),
                    Main.rand.NextFloat(-boxSize / 2, boxSize / 2)
                );

                // 计算粒子的运动方向（朝外扩散）
                Vector2 direction = (offset).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f);

                // 生成方形爆炸粒子
                SquishyLightParticle explosion = new SquishyLightParticle(
                    explosionPosition + offset, // 生成位置
                    direction, // 运动方向
                    0.35f, // 初始缩放
                    Color.Orange, // 颜色
                    30 // 生命周期
                );

                GeneralParticleHandler.SpawnParticle(explosion);
            }

            // 播放击中音效
            SoundEngine.PlaySound(SoundID.Item88, Projectile.position);
        }
    }
}