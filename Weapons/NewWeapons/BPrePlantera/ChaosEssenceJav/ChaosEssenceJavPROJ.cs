using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav
{
    public class ChaosEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ChaosEssenceJav/ChaosEssenceJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
            Projectile.scale = 0.6f;
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.005f;

            // 生成随机的 Lava 粒子效果
            if (Main.rand.NextBool(3)) // 控制粒子生成频率，1/3 的几率生成一个粒子
            {
                Vector2 dustPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2));
                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Lava);
                dust.velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f); // 粒子速度带有随机化效果
                dust.noGravity = true; // 设置粒子无重力
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f); // 随机缩放
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300); // 原版的狱炎效果
            target.AddBuff(BuffID.OnFire, 300); // 原版的着火效果
            //// 释放 Fuckyou 爆炸弹幕，大小 1.5 倍，伤害 1.0 倍
            //int fuckyouProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 1.0f), 0, Projectile.owner);
            //Main.projectile[fuckyouProj].scale = 1.5f;

            //// 释放菱形特效
            //SpawnDiamondParticleEffect();

            //// 随机发射三个 ChaosEssenceJavFIRE 弹幕
            //for (int i = 0; i < 3; i++)
            //{
            //    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(0f, 360f)); // 0 到 360 度随机角度
            //    Vector2 direction = Vector2.UnitY.RotatedBy(angleOffset); // 从中心向外随机方向
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 30f, ModContent.ProjectileType<ChaosEssenceJavFIRE>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            //}


        }
        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            // 生成一圈原版粒子特效，形成小圆圈，半径为4格（64像素）
            int particleCount = 20;
            float angleIncrement = MathHelper.TwoPi / particleCount;
            float radius = 4 * 16f; // 圆的半径为4格

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = new Vector2(radius, 0f).RotatedBy(angleIncrement * i);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Smoke, Vector2.Zero, 0, Color.Red);
                dust.scale = 1.2f;
                dust.noGravity = true;
            }




            // 释放 Fuckyou 爆炸弹幕，大小 1.5 倍，伤害 1.0 倍
            int fuckyouProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 1.0f), 0, Projectile.owner);
            Main.projectile[fuckyouProj].scale = 1.5f;

            // 释放菱形特效
            SpawnDiamondParticleEffect();

            // 在弹幕死亡位置释放轻型烟雾
            for (int i = 0; i < 50; i++)
            {
                // 生成360度随机方向的速度
                Vector2 dustVelocity = Main.rand.NextVector2Circular(2f, 2f);
                // 设置颜色为血红色
                Color smokeColor = Color.DarkRed;

                // 创建并生成粒子
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), smokeColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 随机发射三个 ChaosEssenceJavFIRE 弹幕
            Player owner = Main.player[Projectile.owner];
            bool isInHell = owner != null && owner.ZoneUnderworldHeight; // 检测玩家是否在地狱区域

            // 根据条件发射弹幕数量
            //int projectileCount = isInHell ? 6 : 3;

            // 就直接发射6个，不加强
            int projectileCount = 6;

            for (int i = 0; i < projectileCount; i++)
            {
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(0f, 360f)); // 0 到 360 度随机角度
                Vector2 direction = Vector2.UnitY.RotatedBy(angleOffset); // 从中心向外随机方向
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 30f, ModContent.ProjectileType<ChaosEssenceJavFIRE>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }


        }

        private void SpawnDiamondParticleEffect()
        {
            Vector2[] diamondCorners = {
                new Vector2(0, -20), // 上顶点
                new Vector2(15, 0),  // 右顶点
                new Vector2(0, 20),  // 下顶点
                new Vector2(-15, 0)  // 左顶点
            };

            // 生成菱形的四条边，每条边用 LineParticle 表示
            for (int i = 0; i < 4; i++)
            {
                Vector2 start = Projectile.Center + diamondCorners[i];
                Vector2 end = Projectile.Center + diamondCorners[(i + 1) % 4];
                Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);

                LineParticle line = new LineParticle(start, direction * 15, false, 30, 0.75f, Color.DarkBlue);
                GeneralParticleHandler.SpawnParticle(line);
            }
        }

    }
}
