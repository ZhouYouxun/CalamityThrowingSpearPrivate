using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using CalamityMod;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM6Headless : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1826"; // 使用原版的贴图

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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 350;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧

        }
        public override void OnSpawn(IEntitySource source)
        {


        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.55f);

            // 南瓜风格的黄色特效
            Color pumpkinColor = new Color(255, 165, 0); // 橙黄色
            float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
            float sparkScale = 1.52f + scaleBoost;
            SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity * 0.1f, false, 7, sparkScale, pumpkinColor);
            GeneralParticleHandler.SpawnParticle(spark);

            // 黑色和黄色 Dust 特效
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? 191 : 195; // 黑色粒子
                Dust blackDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
                blackDust.velocity *= 0.3f;
                blackDust.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                Dust yellowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 174, 0f, 0f, 100, default, 1.2f); // 黄色粒子
                yellowDust.velocity *= 0.5f;
                yellowDust.noGravity = true;
            }


            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 100)
            {
                NPC target = Projectile.Center.ClosestNPCAt(2400); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float desiredRotation = direction.ToRotation(); // 目标方向
                    float currentRotation = Projectile.velocity.ToRotation(); // 当前方向
                    float rotationDifference = MathHelper.WrapAngle(desiredRotation - currentRotation); // 计算角度差
                    // 让 `maxRotation` 随时间增加，每 20 帧增加 `1°`，最大不超过 `90°`
                    float maxRotation = MathHelper.ToRadians(8f + (Projectile.ai[1] / 20f));
                    maxRotation = MathHelper.Clamp(maxRotation, 0f, MathHelper.ToRadians(90f)); // 限制最大角度为 90°

                    // 限制旋转角度
                    float rotationAmount = MathHelper.Clamp(rotationDifference, -maxRotation, maxRotation);
                    Projectile.velocity = Projectile.velocity.RotatedBy(rotationAmount).SafeNormalize(Vector2.Zero) * 18f; // 追踪但受限
                }
            }
            else
            {
                Projectile.ai[1]++;
            }

            Time++;
        }
        public ref float Time => ref Projectile.ai[1];
        public override bool? CanDamage() => Time >= 22f; // 初始的时候不会造成伤害，直到x为止



        public override void OnKill(int timeLeft)
        {

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            // 在以敌人为中心的边长为 150×16 的正方形边上随机生成 2~4 个 FlamingJack 弹幕
            int pumpkinCount = Main.rand.Next(2, 5);
            for (int i = 0; i < pumpkinCount; i++)
            {
                // **生成点：围绕敌人的 150×16 正方形边缘**
                Vector2 spawnPos = target.Center + Main.rand.NextVector2CircularEdge(150 * 16, 150 * 16);

                // **计算生成点相对敌人的方向**
                Vector2 relativePos = spawnPos - target.Center;

                // **选择最接近的正方向**
                Vector2 chosenDirection;
                if (Math.Abs(relativePos.X) > Math.Abs(relativePos.Y))
                {
                    // **左右方向更远**
                    chosenDirection = relativePos.X > 0 ? Vector2.UnitX : -Vector2.UnitX;
                }
                else
                {
                    // **上下方向更远**
                    chosenDirection = relativePos.Y > 0 ? Vector2.UnitY : -Vector2.UnitY;
                }

                // **生成 `FlamingJack` 弹幕**
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    chosenDirection * Projectile.velocity.Length(), // **初始方向为最接近的正方向**
                    ProjectileID.FlamingJack,
                    (int)(Projectile.damage * 1.0f), // 伤害倍率为 1.0
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }



    }
}
