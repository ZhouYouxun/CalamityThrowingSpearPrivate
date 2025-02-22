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

            // 计算呼吸灯节奏
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.3f + 0.7f; // 0.4 ~ 1.0 之间变化
            float glowScale = Projectile.scale * (1.0f + pulse * 0.15f); // 让光晕稍微扩张收缩
            Color glowColor = Color.Lerp(Color.LightGreen, Color.LimeGreen, pulse) * 0.5f; // 交替变色

            // 绘制发光边框
            float chargeOffset = 3f; // 充能光晕的扩散偏移
            glowColor.A = 0; // 透明度

            for (int i = 0; i < 8; i++) // 环绕 8 个光点
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, glowColor, Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }

            // 渲染实际的投射物本体
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
                Color.White, // 冲击波的颜色
                new Vector2(1f, 1f), // 冲击波的初始形状（圆形）
                Main.rand.NextFloat(6f, 10f), // 初始缩放大小
                0.15f, // 最终缩放大小（收缩到非常小）
                3f, // 设定扩散范围
                10 // 粒子的存活时间（10 帧）
            );
            // 生成收缩冲击波粒子
            GeneralParticleHandler.SpawnParticle(shrinkingpulse);
        }

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

            // 直线飞行
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
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