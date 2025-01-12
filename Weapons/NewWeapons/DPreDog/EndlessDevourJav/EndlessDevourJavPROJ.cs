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
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Boss;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    public class EndlessDevourJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/EndlessDevourJav/EndlessDevourJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 在飞行期间绘制黑色残影
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Black, 1);
            return false;
        }

        // 定义一个包含敌方弹幕类型的黑名单数组
        private static readonly int[] projectileBlacklist = new int[]
        {
            ModContent.ProjectileType<InfernadoRevenge>(),
            // 后续可以添加更多的弹幕类型ID
        };

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f; // 修改为黑色光照
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加黑色光源，亮度强度不变
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 在弹幕的后方生成黑色光球粒子
            Vector2 leftPosition = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width / 2;
            GlowOrbParticle blackOrb = new GlowOrbParticle(
                leftPosition, Vector2.Zero, false, 5, 0.55f, Color.Black, true, true
            );
            GeneralParticleHandler.SpawnParticle(blackOrb);

            // 每10帧生成椭圆形粒子特效
            if (Projectile.ai[0] % 15 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        Projectile.velocity * 0.75f,
                        Color.Black,
                        new Vector2(1f, 2.5f),
                        Projectile.rotation - MathHelper.PiOver4,
                        1.0f, // 将初始体型设置为更大，1.0f
                        0.4f, // 将结束体型设置为较小，0.4f
                        20 // 生命周期
                    );

                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }


            // 弹幕逐渐加速
            Projectile.velocity *= 1.005f;

            // 生成黑色气泡粒子特效
            if (Projectile.timeLeft % 3 == 0) // 每3帧生成一个粒子
            {
                // 生成黑色气泡粒子，粒子类型与 NeptunesBountyProjectile 类似
                Dust bubble = Dust.NewDustPerfect(Projectile.Center, DustID.DarkCelestial);
                bubble.noGravity = true;
                bubble.scale = Main.rand.NextFloat(1.5f, 2.5f); // 随机缩放
                bubble.velocity = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f); // 随机速度和方向
                bubble.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机旋转
                bubble.color = Color.Black; // 粒子颜色为黑色
            }
        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item113, Projectile.Center);

            // 独立判定是否召唤额外弹幕
            //if (Main.rand.NextFloat() < 0.5f)
            //{
                SummonAdditionalProjectiles();
            //}

            // 独立判定是否反转敌人弹幕
            //if (Main.rand.NextFloat() < 0.5f)
            //{
                ReverseEnemyProjectiles();
            //}
        }

        // 召唤额外弹幕的逻辑
        private void SummonAdditionalProjectiles()
        {
            // 生成位置：在弹幕方向前方 25 个方块处
            Vector2 spawnCenter = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (25 * 16);

            // 生成 4 个辅助弹幕
            for (int i = 0; i < 4; i++)
            {
                // 在半径为 5 个方块的圆圈内随机位置生成辅助弹幕
                Vector2 randomOffset = Main.rand.NextVector2CircularEdge(5 * 16, 5 * 16); // 随机偏移位置
                Vector2 spawnPosition = spawnCenter + randomOffset;

                // 创建辅助弹幕，并朝主弹幕死亡位置飞行
                Vector2 directionToDeath = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 15f); // 设置速度范围为 10 到 15
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    directionToDeath,
                    ModContent.ProjectileType<EndlessDevourJavOrb>(),
                    (int)(Projectile.damage * 0.55f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // 调用黑色粒子喷射函数
            GenerateForwardParticleEffect();
        }

        // 反转敌人弹幕的逻辑
        private void ReverseEnemyProjectiles()
        {
            // 50%的概率反转范围内的敌人弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.hostile && !proj.friendly)
                {
                    float distance = Vector2.Distance(proj.Center, Projectile.Center);
                    if (distance <= 15 * 16 && Array.IndexOf(projectileBlacklist, proj.type) == -1) // 半径15个方块的范围，且不在黑名单内
                    {
                        // 生成一个新的友方弹幕，复制敌方弹幕的逻辑，但方向反转
                        var source = Projectile.GetSource_FromThis();
                        Projectile newProjectile = Projectile.NewProjectileDirect(
                            source,
                            proj.Center,
                            -proj.velocity, // 设置为反向飞行
                            proj.type,
                            (int)(proj.damage * 12), // 提升伤害
                            proj.knockBack,
                            Projectile.owner
                        );

                        // 新的友方弹幕设置
                        newProjectile.hostile = false;
                        newProjectile.friendly = true;
                        newProjectile.netUpdate = true;

                        // 生成粒子连接线效果
                        Vector2 direction = (proj.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float segmentDistance = 5f; // 每个粒子之间的距离
                        for (float i = 0; i < distance; i += segmentDistance)
                        {
                            Vector2 particlePosition = Projectile.Center + direction * i;
                            Dust dust = Dust.NewDustPerfect(particlePosition, DustID.DarkCelestial);
                            dust.noGravity = true;
                            dust.scale = 1.0f;
                            dust.rotation = direction.ToRotation();
                            dust.color = Color.Black;
                        }
                    }
                }
            }

            // 触发黑色粒子环特效
            GenerateRotatingParticleRing();
        }

        // 生成黑色粒子环的函数（用于叛变弹幕时的特效）
        private void GenerateRotatingParticleRing()
        {
            int particleCount = 50; // 增加到50个粒子
            float radius = 15 * 16f; // 半径15个方块

            for (int i = 0; i < particleCount; i++)
            {
                // 让每个粒子在圆周上均匀分布
                float angle = i * MathHelper.TwoPi / particleCount; // 粒子的角度
                Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius;

                // 使用黑色粒子
                Dust dust = Dust.NewDustPerfect(position, DustID.DarkCelestial);
                dust.noGravity = true;
                dust.scale = 2f;
                dust.rotation = angle;
                dust.color = Color.Black; // 粒子颜色为黑色
                dust.alpha = 0; // 不透明
            }
        }

        // 生成前方粒子喷射特效（用于召唤辅助弹幕时的特效）
        private void GenerateForwardParticleEffect()
        {
            int particleCount = 30; // 生成30个粒子
            for (int i = 0; i < particleCount; i++)
            {
                // 随机在-10度到10度的范围内抛射粒子
                float randomAngle = Main.rand.NextFloat(-MathHelper.ToRadians(10), MathHelper.ToRadians(10));
                Vector2 velocity = Projectile.velocity.RotatedBy(randomAngle) * Main.rand.NextFloat(0.5f, 1.5f); // 随机速度

                // 生成粒子
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.DarkCelestial);
                dust.noGravity = true;
                dust.velocity = velocity;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f); // 随机缩放
                dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机旋转
                dust.color = Color.Black; // 粒子颜色为黑色
                dust.alpha = 0; // 不透明
            }
        }



    }
}

