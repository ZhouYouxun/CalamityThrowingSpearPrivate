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
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/BForget/SunsetBForgetLeft"
            ).Value;

            // 1. 绘制拖尾（原逻辑保留）
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Color color = i % 2 == 0 ? Color.Yellow : Color.LightGreen;
                color *= 0.6f;
                color.A = 0;

                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f
                                       - Main.screenPosition
                                       + new Vector2(0f, Projectile.gfxOffY);

                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    null,
                    color,
                    Projectile.rotation,
                    texture.Size() * 0.5f,
                    0.8f,
                    SpriteEffects.None,
                    0
                );
            }

            // 2. 在武器头部绘制 Extra_89 脉动圆环
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[89].Value;

            // 计算武器头部位置（基于朝向和贴图一半宽度）
            Vector2 headOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY)
                                 * (texture.Width * 0.5f * 0.8f); // 0.8f = 上面拖尾缩放
            Vector2 headPos = Projectile.Center + headOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 0.6f;
                Color ringColor = Color.Lime * 1.3f; // 改成亮绿色
                float scale = (0.25f + 0.05f * i) * pulse * 2.5f;

                Main.EntitySpriteDraw(
                    ringTex,
                    headPos,
                    null,
                    ringColor,
                    angle,
                    ringTex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            return false; // 告诉游戏我们自己画完了
        }


        public static class SunsetBForgetParticleManager
        {
            public static readonly int[] YellowDusts = { 169, 159, 133 };
            public static readonly int[] BlueDusts = { 80, 67, 48 }; 
            public static readonly int[] GreenDusts = { 3, 46, 89, 128 };
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;




            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            float forwardAngle = forward.ToRotation();

            // 1. 主干能量藤蔓（粗壮）
            if (Main.rand.NextBool(2))
            {
                Particle trail = new SparkParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.25f,
                    false,
                    50, // 更长寿
                    2.2f, // 粗壮
                    Main.rand.NextBool() ? Color.Green : Color.LimeGreen
                );
                trail.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 2. 并行能流（左右分布）
            if (Main.rand.NextBool(3))
            {
                Vector2 side = forward.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-6f, 6f);
                AltSparkParticle wire = new AltSparkParticle(
                    Projectile.Center + side,
                    forward * 0.02f,
                    false,
                    20,
                    1.8f, // 更大
                    Color.GreenYellow * 0.8f
                );
                wire.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(wire);
            }

            // 3. 爆裂能块（绿色碎片）
            if (Main.rand.NextBool(4))
            {
                SquareParticle sq = new SquareParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    forward * Main.rand.NextFloat(0.5f, 1.5f),
                    false,
                    30,
                    2.0f, // 放大
                    Color.ForestGreen
                );
                sq.Rotation = forwardAngle + Main.rand.NextFloat(-0.2f, 0.2f);
                GeneralParticleHandler.SpawnParticle(sq);
            }

            // 4. 蒸腾气雾（大范围）
            if (Main.rand.NextBool(3))
            {
                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    forward.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1.2f),
                    false,
                    Main.rand.Next(20, 26),
                    1.5f + Main.rand.NextFloat(0.4f), // 比之前更大
                    Color.LimeGreen * 0.9f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // 4b. 少量重烟雾（撑场）
            if (Main.rand.NextBool(1))
            {
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(1f, 2f),
                    Color.DarkGreen,
                    30,
                    Main.rand.NextFloat(0.2f,0.8f),
                    0.6f,
                    Main.rand.NextFloat(-1f, 1f),
                    true
                );
                smoke.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 5. Dust 背景散射
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.CursedTorch,
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.8f, 2f),
                    120,
                    Color.Green,
                    Main.rand.NextFloat(1.2f, 1.6f) // 大一点
                );
                d.noGravity = true;
                d.rotation = forwardAngle;
                d.fadeIn = 0.8f;
            }

            // 波浪能流（左右各一条，摆动更夸张）
            float time = Main.GameUpdateCount * 0.25f; // 控制波动频率

            for (int i = 0; i < 2; i++) // 左右两条波浪
            {
                float side = (i == 0 ? 1f : -1f);
                // 波动位移：正弦函数 * 振幅
                Vector2 offset = forward.RotatedBy(MathHelper.PiOver2) * side * (float)Math.Sin(time + i * MathHelper.Pi) * 12f;

                AltSparkParticle wave = new AltSparkParticle(
                    Projectile.Center + offset,               // 位置 = 弹幕中心 + 波动偏移
                    forward * 0.02f,                          // 仍然沿前进方向微速前进
                    false,
                    24,                                       // 寿命
                    1.6f,                                     // 粗一些
                    Color.LimeGreen * 0.9f
                );
                wave.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(wave);
            }


        }



        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 spawnPosition = target.Center;

            // 计算随机触手数量（3~6个）
            int tentacleCount = Main.rand.Next(3, 7);

            // 计算单个触手的伤害（总伤害固定为 1.0 倍）
            int individualDamage = (int)(Projectile.damage / (float)tentacleCount);

            // 生成多个随机方向的触手
            for (int i = 0; i < tentacleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi); // 0° 到 360° 全角度随机
                Vector2 tentacleVelocity = randomAngle.ToRotationVector2() * 4f; // 全方向随机扩散

                SpawnGreenTentacle(tentacleVelocity, individualDamage);
            }

            // 播放撞击音效
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);

            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒
        }

        // 生成绿色触手的方法
        private void SpawnGreenTentacle(Vector2 tentacleVelocity, int damage)
        {
            float kb = Projectile.knockBack;

            float ai0 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);
            float ai1 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, tentacleVelocity, ModContent.ProjectileType<SunsetBForgetTantacle>(), damage, kb, Projectile.owner, ai0, ai1);
        }


    }
}