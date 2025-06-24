using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS
{
    internal class DLOASPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/DLOAS/DLOAS";
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
            Projectile.penetrate = 6;
            Projectile.timeLeft = 250;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }


        private int snakeSpawnTimer = 10; // 计时器，20帧后生成第一条蛇
        private int snakeCount = 0; // 记录已生成的蛇数量
        public override void AI()
        {
            {
                // 逐渐降低速度
                Projectile.velocity *= 0.975f;

                // **计算当前速度大小**
                float currentSpeed = Projectile.velocity.Length();
                float speedFactor = MathHelper.Clamp(currentSpeed / 10f, 0f, 1f); // **确保振动与速度同步衰减**

                // **计算振动频率和幅度的衰减**
                float decayFactor = 0.98f; // **衰减系数，控制振动衰减速度**
                Projectile.localAI[0] = MathHelper.Clamp(Projectile.localAI[0] * decayFactor, 0.025f, 0.075f); // **降低 waveFrequency**
                Projectile.localAI[1] = MathHelper.Clamp(Projectile.localAI[1] * decayFactor * speedFactor, 0.05f * 16f, 0.7f * 16f); // **让振幅与速度关联，速度低时振动也会低**

                float waveFrequency = Projectile.localAI[0]; // 频率随时间降低
                float waveAmplitude = Projectile.localAI[1]; // 振幅随时间降低

                // **计算基于时间的 Sin 波动**
                Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero); // 计算垂直方向
                float waveOffset = (float)Math.Sin(Projectile.ai[0] * waveFrequency) * waveAmplitude; // 计算波动偏移量
                Projectile.position += perpendicular * waveOffset * speedFactor; // **让振幅也随速度缩小**

                // 递增 AI 计时器，让 Sin 波动持续
                Projectile.ai[0] += 1f;

                // **让 rotation 也随着波动调整**
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.Sin(Projectile.ai[0] * waveFrequency) * 0.3f * speedFactor + MathHelper.PiOver4;
            }

            // **让 Dust 释放量随时间衰减**
            float dustDecayFactor = MathHelper.Clamp(1f - (Projectile.timeLeft / 250f), 0f, 1f); // 计算衰减比例

            for (int i = 0; i < (int)(3 * (1f - dustDecayFactor)); i++) // 逐步减少 Dust 释放量
            {
                float angleOffset = MathHelper.TwoPi / 3 * i;
                float spiralAngle = Projectile.ai[0] * 0.2f + angleOffset;

                Vector2 dustOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * 10f;
                Vector2 dustPosition = Projectile.Center + dustOffset;

                int dustType = Main.rand.NextBool() ? 37 : 173;
                Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, dustType);
                dust.velocity = dustOffset * 0.1f * (1f - dustDecayFactor); // 让扩散速度也逐渐降低
                dust.noGravity = true;
            }

            // 计数递增，让螺旋持续旋转
            Projectile.ai[0] += 1f;

            // **蠕虫生成逻辑**
            if (snakeCount < 3 && Projectile.timeLeft < 250 - snakeSpawnTimer)
            {
                SpawnSnake();
                snakeSpawnTimer += 10; // 下一次生成间隔 10 帧
                snakeCount++;

                // **仅在第一次生成时播放音效**
                if (snakeCount == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item151, Projectile.Center);
                }
            }

            // **在最后 30 帧内逐渐透明**
            if (Projectile.timeLeft <= 30)
            {
                float alphaIncrease = (1f - (Projectile.timeLeft / 30f)) * 255f; // 计算透明度
                Projectile.alpha = (int)MathHelper.Clamp(alphaIncrease, 0f, 255f); // 限制范围在 0 ~ 255
            }
        }

        // 生成一条蠕虫
        private void SpawnSnake()
        {
            int owner = Projectile.owner;

            // **在 `6 × 16 = 96` 范围内随机选择一个点生成**
            float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
            Vector2 spawnOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 96f;
            Vector2 spawnPosition = Projectile.Center + spawnOffset;

            // **播放椭圆形冲击波特效**
            Particle pulse = new DirectionalPulseRing(
                spawnPosition, // 设定粒子的初始位置
                Projectile.velocity * 0.75f, // 设定冲击波的传播方向与速度
                Color.Purple, // 设定粒子的颜色（紫色）
                new Vector2(1f, 2.5f), // 🔹 `Squish` 参数决定椭圆的长短轴比例
                Projectile.velocity.ToRotation() - MathHelper.PiOver2, // 🔄 使冲击波垂直于蛇的初速度
                0.2f, // 初始缩放大小
                0.03f, // 最终缩放大小
                20 // 粒子存活时间（20 帧）
            );
            GeneralParticleHandler.SpawnParticle(pulse); // 生成冲击波粒子

            // **生成头部**
            int prev = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Projectile.velocity,
                ModContent.ProjectileType<DLOASSnake1Head>(), (int)(Projectile.damage * 1.0), Projectile.knockBack, owner, Projectile.whoAmI);

            // **生成身体**
            for (int j = 0; j < 3; j++)
            {
                prev = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Projectile.velocity,
                    ModContent.ProjectileType<DLOASSnake2Body>(), (int)(Projectile.damage * 0.6), Projectile.knockBack, owner, prev);
            }

            // **生成尾巴**
            int tailID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Projectile.velocity,
                ModContent.ProjectileType<DLOASSnake3Tail>(), (int)(Projectile.damage * 0.3), Projectile.knockBack, owner, prev);
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void OnKill(int timeLeft)
        {           

        }





    }
}
