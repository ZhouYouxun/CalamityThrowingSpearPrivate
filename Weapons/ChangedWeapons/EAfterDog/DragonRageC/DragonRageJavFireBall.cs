using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Audio;
using CalamityMod.Particles;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC
{
    public class DragonRageJavFireBall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/Magic/RancorFog"; // 透明烟雾贴图

        private Player owner;
        private int state = 0; // 0: 上升阶段，1: 转圈，2: 冲刺
        private int timer = 0;

        private float verticalSpeed = 2f;
        private float maxSpeed = 4f;

        private Vector2 orbitCenter;
        private Vector2 orbitShape;
        private float orbitAngle;
        private float orbitSpeed;

        private Vector2 dashDirection;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            timer++;
            owner = Main.player[Projectile.owner];

            // 阶段 0：上升阶段，线性加速
            if (state == 0)
            {
                float speedFactor = MathHelper.Lerp(1f, 2f, timer / 40f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, -Vector2.UnitY * verticalSpeed * speedFactor, 0.1f);

                if (timer >= 40)
                {
                    // 切换到轨道阶段
                    state = 1;
                    timer = 0;

                    orbitCenter = owner.Center;
                    orbitAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);

                    // 椭圆形轨道大小更加随机夸张
                    float a = Main.rand.NextFloat(24f, 48f) * 16f;
                    float b = Main.rand.NextFloat(12f, 60f) * 16f;
                    if (Main.rand.NextBool())
                        Utils.Swap(ref a, ref b);

                    orbitShape = new Vector2(a, b);
                    orbitSpeed = Main.rand.NextFloat(0.06f, 0.15f);
                }
            }

            // 阶段 1：大幅椭圆轨道旋转
            else if (state == 1)
            {
                orbitAngle += orbitSpeed;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(orbitAngle) * orbitShape.X,
                    (float)Math.Sin(orbitAngle) * orbitShape.Y
                );
                Projectile.Center = orbitCenter + offset;

                float orbitVelocity = MathHelper.Lerp(2f, 6f, timer / 60f);
                Vector2 tangent = orbitAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                Projectile.velocity = tangent * orbitVelocity;

                if (timer >= 60)
                {
                    // 切换到冲刺阶段
                    state = 2;
                    timer = 0;
                    dashDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                }
            }

            // 阶段 2：冲刺阶段，保持直线飞行
            else if (state == 2)
            {
                float dashSpeed = MathHelper.Lerp(6f, 36f, Utils.GetLerpValue(0f, 60f, timer, true));
                Projectile.velocity = dashDirection * dashSpeed;
            }

            // 发光粒子（线性尾迹）
            if (Main.rand.NextBool(2))
            {
                Particle trail = new SparkParticle(
                    Projectile.Center,
                    Projectile.velocity * 0.2f,
                    false,
                    60,
                    1.0f,
                    Color.Orange
                );
                GeneralParticleHandler.SpawnParticle(trail);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 🔥 多层次火焰爆炸特效
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f + Main.rand.NextFloat(-0.08f, 0.08f);
                float speed = Main.rand.NextFloat(6f, 18f);
                Vector2 velocity = angle.ToRotationVector2() * speed;

                int dustType = Main.rand.NextBool(3) ? DustID.Torch : DustID.SolarFlare;
                Color dustColor = Color.Lerp(Color.Orange, Color.OrangeRed, Main.rand.NextFloat(0.3f, 1f));
                float scale = Main.rand.NextFloat(1.6f, 2.2f);

                int dust = Dust.NewDust(Projectile.Center, 0, 0, dustType, velocity.X, velocity.Y, 0, dustColor, scale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.2f;
            }

            // 💥 中心冲击光球
            for (int i = 0; i < 10; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke, dir.X * 6f, dir.Y * 6f, 100, Color.Orange, 2.2f);
                Main.dust[dust].noGravity = true;
            }

            // ⚡ 炽热高光飞星
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(12f, 24f);
                Particle flash = new SparkParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    40,
                    1.4f,
                    Color.White
                );
                GeneralParticleHandler.SpawnParticle(flash);
            }


            // 加强视觉：生成一些大型粒子
            for (int i = 0; i < 6; i++)
            {
                Vector2 randVel = Main.rand.NextVector2Circular(6f, 6f);
                Particle burst = new SparkParticle(
                    Projectile.Center,
                    randVel,
                    false,
                    40,
                    1.5f,
                    Color.OrangeRed
                );
                GeneralParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D star = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_09").Value;
            Texture2D ring = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            float fixedRotation = Projectile.rotation;
            Vector2 gunTip = Projectile.Center + new Vector2(0f, 0f).RotatedBy(fixedRotation);
            Vector2 screenPos = gunTip - Main.screenPosition;

            // 自转角度（匀速）
            float rotation = Main.GlobalTimeWrappedHourly * 3.2f;

            // 仿 iOS 动画节奏的脉动效果（慢-快-慢）
            float easingPulse = 1f + 0.12f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);

            float baseScale = 0.2f;
            float scale = baseScale * easingPulse;

            Color baseColor = Color.Orange with { A = 0 };
            SpriteEffects flip = SpriteEffects.None;
            Vector2 origin = star.Size() * 0.5f;

            // 准星本体绘制
            Main.EntitySpriteDraw(star, screenPos, null, baseColor, rotation, origin, scale, flip, 0);

            // 外圈 twirl 两层
            for (int i = 0; i < 2; i++)
            {
                float offsetAngle = rotation * (i == 0 ? 1.8f : -1.2f);
                float ringScale = scale * (i == 0 ? 0.8f : 0.7f);
                Color ringColor = (i == 0 ? Color.OrangeRed : Color.White) * 0.6f;
                ringColor.A = 0;

                Main.EntitySpriteDraw(ring, screenPos, null, ringColor, offsetAngle, ring.Size() * 0.5f, ringScale, flip, 0);
            }

            return false; // 不绘制默认贴图
        }
    }
}
