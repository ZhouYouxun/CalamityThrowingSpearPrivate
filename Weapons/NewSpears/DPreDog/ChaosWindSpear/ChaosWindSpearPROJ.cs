using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Particles;
using Terraria.GameContent.Drawing;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.ChaosWindSpear
{
    internal class ChaosWindSpearPROJ : ModProjectile
    {
        private int timer = 0;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 230;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.scale = 1.25f;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float spinCycleTime = 50f;

            // If the player is dead, destroy the projectile
            if (player.dead || !player.channel)
            {
                Projectile.Kill();
                player.reuseDelay = 2;
                return;
            }

            int direction = Math.Sign(Projectile.velocity.X);
            Projectile.velocity = new Vector2(direction, 0f);

            // Initial Rotation
            if (Projectile.ai[0] == 0f)
            {
                Projectile.rotation = new Vector2(direction, -player.gravDir).ToRotation() + MathHelper.ToRadians(135f);
                if (Projectile.velocity.X < 0f)
                {
                    Projectile.rotation -= MathHelper.PiOver2;
                }
            }

            Projectile.ai[0] += 1f;
            Projectile.rotation += MathHelper.TwoPi * 2f / spinCycleTime * direction;
            int expectedDirection = (player.SafeDirectionTo(Main.MouseWorld).X > 0f).ToDirectionInt();
            if (Projectile.ai[0] % spinCycleTime > spinCycleTime * 0.5f && expectedDirection != Projectile.velocity.X)
            {
                player.ChangeDir(expectedDirection);
                Projectile.velocity = Vector2.UnitX * expectedDirection;
                Projectile.rotation -= MathHelper.Pi;
                Projectile.netUpdate = true;
            }
            PositionAndRotation(player);
            VisibilityAndLight();

            // 粒子效果随机化释放（圆盘刀光）
            if (timer % 15 == 0) // 每 15 帧释放一次
            {
                Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                particleOffset.X += Main.rand.NextFloat(-1f, 1f); // 随机左右偏移
                Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

                float scaleMultiplier = 1.75f;

                // 设定旋转惯量，让它顺时针旋转
                float rotationalSpeed = MathHelper.ToRadians(5f); // 每帧旋转 5°
                Vector2 rotationalVelocity = new Vector2(0, rotationalSpeed); // 让旋转有速度影响

                Particle Smear = new CircularSmearVFX(
                    particlePosition,
                    Color.White * Main.rand.NextFloat(0.78f, 0.85f),
                    Main.rand.NextFloat(-8, 8),
                    Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier
                );

                // 赋予旋转惯性
                Smear.Velocity = rotationalVelocity;
                Smear.Lifetime = 6; // 让每个特效持续 6 帧

                GeneralParticleHandler.SpawnParticle(Smear);
            }



            {
                // 计算粒子释放的圆形半径
                float radius = 5.5f * 16f;
                float rotationSpeed = MathHelper.TwoPi / 60f; // 约1秒转一圈
                float currentAngle = (Projectile.ai[0] * rotationSpeed) % MathHelper.TwoPi;

                // 计算两个对称释放点
                Vector2 particleSpawnPos1 = player.Center + new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius;
                Vector2 particleSpawnPos2 = player.Center - new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius; // 取相反方向

                // 50% 概率释放轻型烟雾，50% 概率释放其他白色粒子
                if (Main.rand.NextBool(2))
                {
                    // 轻型烟雾
                    int dustType = DustID.Smoke; // 轻烟雾
                    Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f); // 轻微偏移
                    Dust smokeDust = Dust.NewDustPerfect(particleSpawnPos1 + dustOffset, dustType,
                                                         new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.8f, 0.8f)),
                                                         120,
                                                         Color.LightGray * 0.8f,
                                                         Main.rand.NextFloat(1.0f, 1.5f));
                    smokeDust.noGravity = true;

                    // 对称点释放烟雾
                    Dust smokeDust2 = Dust.NewDustPerfect(particleSpawnPos2 + dustOffset, dustType,
                                                          new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.8f, 0.8f)),
                                                          120,
                                                          Color.LightGray * 0.8f,
                                                          Main.rand.NextFloat(1.0f, 1.5f));
                    smokeDust2.noGravity = true;
                }
                else
                {
                    // 其他白色粒子特效（随机选择）
                    int[] possibleParticles = { DustID.Cloud, DustID.WhiteTorch, DustID.Snow }; // 选择 3 种白色相关粒子
                    int chosenParticle = Main.rand.Next(possibleParticles); // 随机选取

                    Vector2 particleOffset = Main.rand.NextVector2Circular(6f, 6f);
                    Dust whiteEffect1 = Dust.NewDustPerfect(particleSpawnPos1 + particleOffset, chosenParticle,
                                                            Vector2.Zero,
                                                            150,
                                                            Color.White * Main.rand.NextFloat(0.8f, 1.2f),
                                                            Main.rand.NextFloat(1.3f, 1.8f));
                    whiteEffect1.noGravity = true;

                    // 对称点释放白色粒子
                    Dust whiteEffect2 = Dust.NewDustPerfect(particleSpawnPos2 + particleOffset, chosenParticle,
                                                            Vector2.Zero,
                                                            150,
                                                            Color.White * Main.rand.NextFloat(0.8f, 1.2f),
                                                            Main.rand.NextFloat(1.3f, 1.8f));
                    whiteEffect2.noGravity = true;
                }
            }

            timer++;
        }


        // 控制旋转
        private void PositionAndRotation(Player player)
        {
            Vector2 plrCtr = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 offset = Vector2.Zero;
            Projectile.Center = plrCtr + offset;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;
            player.itemRotation = MathHelper.WrapAngle(Projectile.rotation);
        }

        // 控制亮度
        private void VisibilityAndLight()
        {
            Lighting.AddLight(Projectile.Center, 1.45f, 1.22f, 0.58f);
            Projectile.alpha -= 128;
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
        }
        private int hitCounter = 0; // 记录击中次数

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Vector2 explosionPosition = target.Center;
            float radius = 3 * 16f; // 3×16 半径
            int numParticles = 8; // 8 个 Keybrand 特效

            for (int i = 0; i < numParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / numParticles + Main.rand.NextFloat(-0.2f, 0.2f); // 轻微随机
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Keybrand,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }

            // 计数击中次数
            hitCounter++;

            // 当击中 60 次时，释放 10 个小旋风
            if (hitCounter >= 50)
            {
                ReleaseWindProjectiles();
                hitCounter = 0; // 计数器归零
            }
        }
        private void ReleaseWindProjectiles()
        {
            Player player = Main.player[Projectile.owner];
            int numProjectiles = 10; // 10 方向均匀分布
            float angleStep = MathHelper.TwoPi / numProjectiles; // 每个弹幕间的角度间隔
            float radius = 40f; // 生成半径

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = angleStep * i;
                Vector2 spawnPosition = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f; // 让旋风沿着方向射出

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<ChaosWindSpearWind>(), // 生成小旋风
                    Projectile.damage,
                    0f,
                    Projectile.owner
                );
            }
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

        }


        // 着色器效果 负责绘制旋转的着色器
        internal float PrimitiveWidthFunction(float completionRatio)
        {
            float tipWidthFactor = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.01f, 0.04f, completionRatio));
            float bodyWidthFactor = (float)Math.Pow(Utils.GetLerpValue(1f, 0.04f, completionRatio), 0.9D);
            return (float)Math.Pow(tipWidthFactor * bodyWidthFactor, 0.1D) * 30f;
        }

        internal Color PrimitiveColorFunction(float completionRatio)
        {
            float fadeInterpolant = (float)Math.Cos(Main.GlobalTimeWrappedHourly * -9f + completionRatio * 6f + Projectile.identity * 2f) * 0.5f + 0.5f;

            // 改为风暴主题：白色 & 天蓝色
            fadeInterpolant = MathHelper.Lerp(0.3f, 0.9f, fadeInterpolant);
            Color frontFade = Color.Lerp(Color.White, Color.SkyBlue, fadeInterpolant);
            frontFade = Color.Lerp(frontFade, Color.LightCyan, 0.5f); // 更明亮的风暴色彩
            Color backFade = Color.White;

            return Color.Lerp(frontFade, backFade, (float)Math.Pow(completionRatio, 1.2D)) * (float)Math.Pow(1f - completionRatio, 1.1D) * Projectile.Opacity;
        }



        public override bool PreDraw(ref Color lightColor)
        {
            // 计算偏移量（距离玩家 4×16 像素）
            Vector2 shaderOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * (4 * 16f);
            Vector2 shaderPosition = Projectile.Center + shaderOffset;

            // 旋转弹幕本体的绘制
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle rectangle = new Rectangle(0, 0, tex.Width, tex.Height);
            Vector2 origin = tex.Size() / 2f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(tex, drawPos, new Microsoft.Xna.Framework.Rectangle?(rectangle), lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // 下面是着色器的调用和绘制
            {
                GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

                Texture2D projectileTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

                // 克隆历史位置用于渲染
                Vector2[] drawPoints = (Vector2[])Projectile.oldPos.Clone();
                Vector2 aimAheadDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();

                if (Projectile.owner == Main.myPlayer)
                {
                    drawPoints[0] += aimAheadDirection * -12f;
                    drawPoints[1] = drawPoints[0] - (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * Vector2.Distance(drawPoints[0], drawPoints[1]);
                }

                for (int i = 0; i < drawPoints.Length; i++)
                {
                    drawPoints[i] -= (Projectile.oldRot[i] + MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.5f;
                }

                // 渲染轨迹，应用新的偏移位置
                if (Projectile.ai[0] > Projectile.oldPos.Length)
                {
                    int numPointsRendered = 24; // 渲染点数
                    PrimitiveRenderer.RenderTrail(drawPoints,
                        new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => shaderPosition, shader: GameShaders.Misc["CalamityMod:TrailStreak"], smoothen: true),
                        numPointsRendered);
                }
            }

            return false;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 36;
        }
        //public override bool PreDraw(ref Color lightColor)
        //{
        //    Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        //    Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        //    Rectangle rectangle = new Rectangle(0, 0, tex.Width, tex.Height);
        //    Vector2 origin = tex.Size() / 2f;
        //    SpriteEffects spriteEffects = SpriteEffects.None;
        //    if (Projectile.spriteDirection == -1)
        //        spriteEffects = SpriteEffects.FlipHorizontally;

        //    Main.EntitySpriteDraw(tex, drawPos, new Microsoft.Xna.Framework.Rectangle?(rectangle), lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

        //    // 添加紫色和蓝色混合的着色器特效
        //    GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
        //    Color blendColor = Color.Lerp(Color.Purple, Color.Blue, 0.5f);
        //    Lighting.AddLight(Projectile.Center + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 0.45f, blendColor.ToVector3() * 0.4f);

        //    return false;
        //}


    }
}
