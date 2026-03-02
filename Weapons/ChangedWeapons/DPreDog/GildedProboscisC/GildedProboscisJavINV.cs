using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
{
    internal class GildedProboscisJavINV : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 使用透明贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 7;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Player owner = Main.player[Projectile.owner];

            {
                Projectile.localAI[0]++; // 时间计时

                // === 初始化随机参数（仅执行一次） ===
                if (Projectile.localAI[0] == 1f)
                {
                    float direction = Main.rand.NextBool() ? 1f : -1f; // 随机旋转方向
                    float speedFactor1 = Main.rand.NextFloat(0.8f, 1.2f); // 转速因子
                    Projectile.localAI[1] = direction * (1000f + speedFactor1 * 1000f); // 编码入 localAI[1]

                    Projectile.ai[0] = Main.rand.NextFloat(1f, 60f); // 随机消失距离
                    Projectile.ai[1] = Main.rand.NextFloat(0.85f, 1.15f); // 半径缩放
                }

                float time = Projectile.localAI[0];
                float maxTime = 180f;
                float progress = MathHelper.Clamp(time / maxTime, 0f, 1f);

                // === 解析随机参数 ===
                float sign = Math.Sign(Projectile.localAI[1]);
                float speedFactor = (Math.Abs(Projectile.localAI[1]) - 1000f) / 1000f;

                // === 螺旋参数 ===
                float startRadius = 240f * Projectile.ai[1];
                float endRadius = 40f * Projectile.ai[1];
                float radius = MathHelper.Lerp(startRadius, endRadius, progress);

                float baseAngularSpeed = 0.15f;
                float angularSpeed = baseAngularSpeed * speedFactor * sign;
                float angle = time * angularSpeed;

                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 targetPosition = owner.Center + offset;

                // 平滑插值移动
                Vector2 toTarget = targetPosition - Projectile.Center;
                float moveSpeed = MathHelper.Lerp(6f, 14f, progress);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.Zero) * moveSpeed, 0.1f);

                // 自身旋转
                Projectile.rotation += 0.3f * sign;

                // 距离玩家过近时消失（使用初始化时确定的随机距离）
                if (Vector2.Distance(Projectile.Center, owner.Center) < Projectile.ai[0])
                {
                    Projectile.Kill();
                }
            }


            {
                // === GlowOrbParticle 单/双螺旋特效 ===
                if (Main.GameUpdateCount % 1 == 0)
                {
                    Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Vector2 right = forward.RotatedBy(MathHelper.PiOver2);

                    float time = Main.GlobalTimeWrappedHourly * 6f; // 控制旋转速度
                    float frequency = 1f; // 螺旋频率
                    float amplitude = 10f; // 螺旋振幅

                    // 单螺旋
                    Vector2 offset = right * MathF.Sin(time * frequency) * amplitude;

                    GlowOrbParticle orb = new GlowOrbParticle(
                        Projectile.Center + offset,
                        Vector2.Zero,
                        false,
                        20,
                        0.7f,
                        Color.Gold,
                        true,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);

                    // 可选：双螺旋（取消注释即可）

                    Vector2 offset2 = right * MathF.Sin(time * frequency + MathHelper.Pi) * amplitude;

                    GlowOrbParticle orb2 = new GlowOrbParticle(
                        Projectile.Center + offset2,
                        Vector2.Zero,
                        false,
                        20,
                        0.7f,
                        Color.Gold,
                        true,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb2);

                }


                //// === Dust 狂野随机点缀特效 ===
                //if (Main.GameUpdateCount % 2 == 0)
                //{
                //    Vector2 randomVelocity = Main.rand.NextVector2Circular(2f, 2f);
                //    Dust goldDust = Dust.NewDustPerfect(
                //        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                //        DustID.GoldFlame,
                //        randomVelocity,
                //        150,
                //        Color.Gold,
                //        Main.rand.NextFloat(0.8f, 1.3f)
                //    );
                //    goldDust.noGravity = true;

                //    Dust redDust = Dust.NewDustPerfect(
                //        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                //        DustID.Blood,
                //        randomVelocity * 0.8f,
                //        150,
                //        Color.Red,
                //        Main.rand.NextFloat(0.8f, 1.3f)
                //    );
                //    redDust.noGravity = true;
                //}

            }




        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int birdDamage = Projectile.damage; // 继承 INV 的伤害
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<GildedProboscisJavBIRD>(),
                    birdDamage,
                    0f,
                    Projectile.owner
                );

                // 可选：在消失时释放小型金色 Dust / Spark 做视觉提示
                for (int i = 0; i < 12; i++)
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.GoldFlame,
                        Main.rand.NextVector2Circular(3f, 3f),
                        100,
                        Color.Gold,
                        Main.rand.NextFloat(0.8f, 1.2f)
                    );
                    dust.noGravity = true;
                }
            }
        }


        private float PrimitiveWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            // 定义轨迹宽度变化
            return MathHelper.Lerp(6f, 20f, completionRatio);
        }

        private Color PrimitiveColorFunction(float completionRatio, Vector2 vertexPos)
        {
            // 定义轨迹颜色渐变（示例蓝紫色调）
            Color startColor = new Color(40, 0, 80);
            Color endColor = new Color(0, 0, 0);
            return Color.Lerp(startColor, endColor, completionRatio);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/spark_07"));
            //Vector2 offset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
            //PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (completionRatio, vertexPos) => offset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 50);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中敌人时可触发的特效或附加效果
        }
    }
}
