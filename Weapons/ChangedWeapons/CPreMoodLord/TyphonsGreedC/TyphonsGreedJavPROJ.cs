using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC
{
    public class TyphonsGreedJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TyphonsGreedC/TyphonsGreedJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private int phase = 1; // 1表示第1阶段，2表示第2阶段
        private int timeCounter = 0; // 用于计时

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
            Projectile.penetrate = 3; // 允许3次穿透
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            timeCounter++;

            if (phase == 1)
            {
                // 第1阶段：线性减速
                Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.02f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 设置旋转方向

                if (timeCounter >= 100) // 修改为100帧后进入第二阶段
                {
                    phase = 2;
                    timeCounter = 0;
                    ReleaseTransitionParticles(); // 切换状态的粒子特效
                }
            }
            else if (phase == 2)
            {
                // 自转逻辑，逐渐增加自转速度
                Projectile.rotation += MathHelper.ToRadians(5f + timeCounter * 0.1f);

                // 生成海洋效果的水能粒子特效
                if (Main.rand.NextBool(3))
                {
                    CreateOceanEffect();
                }

                // 每45帧发射一个TyphonsGreedJavBubble
                if (timeCounter % 45 == 0 && timeCounter <= 180)
                {
                    Vector2 bubbleDirection = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, bubbleDirection * 5f, ModContent.ProjectileType<TyphonsGreedJavBubble>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }

                // 检查自转时间是否达到180帧
                if (timeCounter >= 180)
                {
                    Projectile.Kill();
                }
            }
        }

        private void ReleaseTransitionParticles()
        {
            // 切换至第二形态时生成一些粒子特效
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1f, 3f);
                Particle particle = new HeavySmokeParticle(Projectile.Center, velocity, Color.Cyan, 20, 1.2f, 0.8f, MathHelper.ToRadians(3f), true);
                GeneralParticleHandler.SpawnParticle(particle);
            }
        }

        private void CreateOceanEffect()
        {
            // 海洋效果的水能粒子特效
            Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 1.5f);
            Particle waterParticle = new HeavySmokeParticle(Projectile.Center, velocity, Color.LightBlue, 15, 0.9f, 0.5f, 0.2f, true);
            GeneralParticleHandler.SpawnParticle(waterParticle);
        }

        public override void OnKill(int timeLeft)
        {
            // 添加更猛烈的线性粒子特效，表示弹幕消失
            for (int i = 0; i < 15; i++)
            {
                Vector2 trailPos = Projectile.Center + Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1f, 4f);
                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, Main.rand.NextFloat(1f, 1.5f), Color.Blue);
                GeneralParticleHandler.SpawnParticle(trail);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 碰撞物块时反弹
            if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
