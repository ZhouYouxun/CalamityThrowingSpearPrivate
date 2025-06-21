using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;
using CalamityMod.Particles;
using Terraria.Audio;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavTinyFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private float rotationAngle = 0f; // 旋转角度
        private const float rotationSpeed = 0.05f; // 旋转速度

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 350;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            // 更新旋转角度
            rotationAngle += rotationSpeed;
            if (rotationAngle > MathHelper.TwoPi)
            {
                rotationAngle -= MathHelper.TwoPi;
            }

            // 在 `timeLeft <= 325` 时开始生成粒子
            if (Projectile.timeLeft <= 325)
            {
                // 生成两条对称的粒子
                float[] angles = { 0f, MathHelper.Pi }; // 夹角为180度
                float radius = 0.7765f * 16f; // 粒子轨迹半径

                foreach (float initialAngle in angles)
                {
                    float angle = initialAngle + rotationAngle; // 计算粒子的当前角度
                    Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                    // 创建粒子特效
                    int fiery = Dust.NewDust(
                        position,
                        0,
                        0,
                        DustID.InfernoFork, // 使用的粒子类型
                        0f,
                        0f,
                        100,
                        default,
                        Main.rand.NextFloat(1.85f, 2.35f) // 粒子大小
                    );
                    Main.dust[fiery].noGravity = true;
                    Main.dust[fiery].velocity = Vector2.Zero; // 粒子初始速度为零
                }
            }

            // 追踪逻辑
            if (Projectile.timeLeft < 275)
                CalamityUtils.HomeInOnNPC(Projectile, true, 1800f, 10f, 20f);
        }
        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item69, Projectile.Center);

            // 创建正八边形粒子特效
            float radius = 5 * 16f; // 八边形半径
            int numSides = 8; // 八边形的边数
            for (int i = 0; i < numSides; i++)
            {
                float angle = MathHelper.TwoPi / numSides * i; // 每条边的角度
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                // 创建粒子
                int fiery = Dust.NewDust(
                    position,
                    0,
                    0,
                    DustID.InfernoFork, // 使用同样的粒子类型
                    0f,
                    0f,
                    100,
                    default,
                    Main.rand.NextFloat(1.85f, 2.35f) // 粒子大小
                );
                Main.dust[fiery].noGravity = true;
                Main.dust[fiery].velocity = (position - Projectile.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f); // 粒子往外扩散
            }

            // 播放额外的爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Yellow, // 熔岩的亮黄色
                "CalamityThrowingSpear/Texture/ThebigExplosion1",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.078f,
                0.450f,
                30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 90); 
        }
    }
}
