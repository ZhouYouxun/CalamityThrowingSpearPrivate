using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    public class ElectrocutionHalberdRIGHTJav : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ElectrocutionHalberd/ElectrocutionHalberdJav";
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
            Projectile.width = Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3; // 额外更新次数
            Projectile.timeLeft = 40 * Projectile.extraUpdates;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不允许与方块碰撞
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //Projectile.scale = 0.7f; //
            Projectile.alpha = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0.7f;
        }
        private float selfRotation = 0f;
        public override void AI()
        {
            // === 速度逐渐衰减到 0 ===
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.velocity *= 0.97f; // 每帧慢慢衰减
            else
                Projectile.velocity = Vector2.Zero;

            {
                // 自转角度累积
                selfRotation += MathHelper.ToRadians(12f); // 每帧自转 12°

                // 最终 rotation = 飞行朝向 + 自转角度
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + selfRotation;
            }

            {
                // 每帧累积时间
                Projectile.localAI[0]++;

                // 每 x 帧触发一次音效
                if (Projectile.localAI[0] % 2 == 0)
                {
                    // 递增音调：随存活时间越来越尖锐
                    float progress = (float)Projectile.timeLeft / 300f; // 你可以换成自己的范围
                    float pitch = MathHelper.Lerp(-0.5f, 1.0f, 1f - progress);

                    SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with
                    {
                        Volume = 2.5f,
                        Pitch = pitch
                    }, Projectile.Center);
                }

            }

            // === 光效 ===
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);

            CreateSideParticles();
        }

        private void CreateSideParticles()
        {
            // 以弹幕的 rotation 为基准，算出左右两个喷口位置
            Vector2 perp = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2); // 垂直方向
            Vector2 leftPos = Projectile.Center - perp * 4 * 16f;  // 左边偏移
            Vector2 rightPos = Projectile.Center + perp * 2 * 16f; // 右边偏移

            // 左喷口（银色）
            SpawnRadialPoints(leftPos, Color.Silver);

            // 右喷口（红色）
            SpawnRadialPoints(rightPos, Color.Red);
        }

        private void SpawnRadialPoints(Vector2 origin, Color color)
        {
            int count = 1; // 每次喷射数量
            float spread = MathHelper.ToRadians(1f); // 放射角度范围（X°）

            for (int i = 0; i < count; i++)
            {
                // 在 0~360° 范围内随机放射
                float angle = Main.rand.NextFloat(-spread, spread);
                Vector2 dir = (Projectile.rotation.ToRotationVector2()).RotatedBy(angle);

                PointParticle spark = new PointParticle(
                    origin,
                    dir * Main.rand.NextFloat(3f, 7f), // 放射速度
                    false,
                    25,                                // 生命周期
                    1.5f + Main.rand.NextFloat(0.4f),  // 粒子大小
                    color
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/新机炮") with { Volume = 1.5f, Pitch = 0.0f }, Projectile.Center);

            Vector2 headPosition = Projectile.Center; // 从自身正中心起始

            int count = 3;
            float baseAngle = Projectile.velocity.ToRotation(); // 参考原弹幕朝向
            float speed = 16f; // 固定速度

            for (int i = 0; i < count; i++)
            {
                // 平均分布：每颗间隔 120°
                float angle = baseAngle + MathHelper.TwoPi / count * i;

                // 发射方向
                Vector2 direction = angle.ToRotationVector2();

                // 固定速度
                Vector2 finalVelocity = direction * speed;

                // 生成子弹
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    headPosition,
                    finalVelocity,
                    ModContent.ProjectileType<ElectrocutionHalberdRIGHT>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }




        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
        }
    }
}
