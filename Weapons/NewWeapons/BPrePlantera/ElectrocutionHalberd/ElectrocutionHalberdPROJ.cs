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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    public class ElectrocutionHalberdPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ElectrocutionHalberd/ElectrocutionHalberd";

        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
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
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转 (在这基础上再增加一点角度，为了适配这个特殊的贴图)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(25);


            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 为箭矢本体后面添加光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 0, 0); // 改为鲜红色
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item91, Projectile.position);

            //// 在命中时生成4个天蓝色的椭圆形粒子特效，向8个方向扩散
            //for (int i = 0; i < 4; i++)
            //{
            //    float angle = MathHelper.TwoPi / 4 * i; // 每个粒子的角度
            //    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 14f;

            //    Particle pulse = new DirectionalPulseRing(Projectile.Center, velocity, Color.LightSkyBlue, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
            //    GeneralParticleHandler.SpawnParticle(pulse);
            //}

            //SoundStyle fire = new("CalamityMod/Sounds/Item/AuricBulletHit");
            //SoundEngine.PlaySound(fire with { Volume = 0.4f, Pitch = 0f }, Projectile.Center);

            // 在正前方的左右各 60 度范围内生成4个随机角度的CrackParticle粒子特效
            for (int i = 0; i < 4; i++)
            {
                // 随机选择角度范围内的一个角度
                float randomAngle = Projectile.velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));

                // 随机生成大小和速度
                float randomSpeed = Main.rand.NextFloat(5f, 8f);
                float randomScale = Main.rand.NextFloat(0.6f, 1.1f);

                // 设置粒子的速度方向和随机化参数
                Vector2 particleVelocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * randomSpeed;

                Particle bolt = new CrackParticle(
                    Projectile.Center,
                    particleVelocity,
                    Color.Aqua * 0.65f,
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(bolt);
            }


            // 10%的概率生成443号Electrosphere弹幕，伤害倍率为0.9
            // 这玩意儿骗伤挺严重的，看情况吧
            if (Main.rand.NextFloat() < 0.1f) // 10%的概率
            {
                //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, 443, (int)(Projectile.damage * 0.9f), Projectile.knockBack, Projectile.owner);
            }

            // 随机生成5~7个Spark弹幕，向上抛射，随机方向在30度范围内
            int sparkCount = Main.rand.Next(5, 8); // 随机生成5到7个
            for (int i = 0; i < sparkCount; i++)
            {
                // 随机选择一个向上方向，左右30度范围
                float angle = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                Vector2 sparkVelocity = new Vector2(0f, -1f).RotatedBy(angle) * Main.rand.NextFloat(2f, 4f); // 随机速度

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, sparkVelocity, ModContent.ProjectileType<Spark>(), (int)(Projectile.damage * 0.2f), Projectile.knockBack, Projectile.owner);
            }
        }


    }
}