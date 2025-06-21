using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Sounds;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.GraniteSpear
{
    internal class GraniteSpearPROJ : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 120;
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
            //if (timer % 5 == 0) // 每 5 帧释放一次
            //{
            //    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
            //    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
            //    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

            //    float scaleMultiplier = 3.1f;
            //    Particle Smear = new CircularSmearVFX(
            //        particlePosition,
            //        Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f),
            //        Main.rand.NextFloat(-8, 8),
            //        Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier
            //    );
            //    GeneralParticleHandler.SpawnParticle(Smear);
            //}

            {
                // 计算粒子释放的圆形半径
                float radius = 2.5f * 16f;
                float rotationSpeed = MathHelper.TwoPi / 60f; // 约1秒转一圈
                float currentAngle = (Projectile.ai[0] * rotationSpeed) % MathHelper.TwoPi;

                // 计算两个对称释放点
                Vector2 particleSpawnPos1 = player.Center + new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius;
                Vector2 particleSpawnPos2 = player.Center - new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius; // 取相反方向

                // 释放蓝色粒子
                if (Main.rand.NextBool(2)) // 控制频率
                {
                    // 释放第一个粒子
                    Dust dust1 = Dust.NewDustPerfect(particleSpawnPos1, DustID.BlueTorch, Vector2.Zero, 150, Color.LightBlue, 1.8f);
                    dust1.noGravity = true;
                    dust1.velocity = (dust1.position - player.Center).SafeNormalize(Vector2.Zero) * 2f;

                    // 释放第二个粒子（对称点）
                    Dust dust2 = Dust.NewDustPerfect(particleSpawnPos2, DustID.BlueTorch, Vector2.Zero, 150, Color.LightBlue, 1.8f);
                    dust2.noGravity = true;
                    dust2.velocity = (dust2.position - player.Center).SafeNormalize(Vector2.Zero) * 2f;
                }
            }
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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 播放 Item14 音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Vector2 dustCenter = target.Center;
            bool isBoss = target.boss || target.type == NPCID.DungeonGuardian;

            // 生成 Dust，构造菱形或随机叉叉
            int dustAmount = 20;
            float randomRotation = Main.rand.NextFloat(MathHelper.TwoPi); // 让整个形状有一个随机角度
            for (int i = 0; i < dustAmount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustAmount; // 均匀分布
                angle += randomRotation; // 让整个形状有个随机旋转
                Vector2 dustOffset;

                if (!isBoss)
                {
                    // 生成随机菱形（X 轴和 Y 轴有扰动）
                    float xOffset = 24 + Main.rand.NextFloat(-5f, 5f); // 在 ±5 像素内随机
                    float yOffset = 24 + Main.rand.NextFloat(-5f, 5f);
                    dustOffset = new Vector2(xOffset * (float)Math.Cos(angle), yOffset * (float)Math.Sin(angle));
                }
                else
                {
                    // 生成随机叉叉（角度更随机）
                    dustOffset = new Vector2(Main.rand.NextFloat(18, 30), 0).RotatedBy(angle + Main.rand.NextFloat(-0.3f, 0.3f));
                }

                Dust dust = Dust.NewDustDirect(dustCenter + dustOffset, 2, 2, DustID.BlueTorch,
                                               Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f,
                                               150, default, 2.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.boss || target.type == NPCID.DungeonGuardian)
            {
                modifiers.FinalDamage *= 0.5f; // 对 Boss 伤害减半
            }
        }


        //// 着色器效果 负责绘制旋转的着色器
        //internal float PrimitiveWidthFunction(float completionRatio)
        //{
        //    float tipWidthFactor = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.01f, 0.04f, completionRatio));
        //    float bodyWidthFactor = (float)Math.Pow(Utils.GetLerpValue(1f, 0.04f, completionRatio), 0.9D);
        //    return (float)Math.Pow(tipWidthFactor * bodyWidthFactor, 0.1D) * 30f;
        //}

        //internal Color PrimitiveColorFunction(float completionRatio)
        //{
        //    float fadeInterpolant = (float)Math.Cos(Main.GlobalTimeWrappedHourly * -9f + completionRatio * 6f + Projectile.identity * 2f) * 0.5f + 0.5f;

        //    // 动态蓝紫色渐变
        //    fadeInterpolant = MathHelper.Lerp(0.2f, 0.8f, fadeInterpolant);
        //    Color frontFade = Color.Lerp(Color.Blue, Color.Purple, fadeInterpolant);
        //    frontFade = Color.Lerp(frontFade, Color.DarkSlateBlue, 0.5f); // 更深色的蓝紫渐变
        //    Color backFade = Color.Blue;

        //    return Color.Lerp(frontFade, backFade, (float)Math.Pow(completionRatio, 1.2D)) * (float)Math.Pow(1f - completionRatio, 1.1D) * Projectile.Opacity;
        //}


        public override bool PreDraw(ref Color lightColor)
        {
            // 旋转弹幕本体的绘制
            {
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Rectangle rectangle = new Rectangle(0, 0, tex.Width, tex.Height);
                Vector2 origin = tex.Size() / 2f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (Projectile.spriteDirection == -1)
                    spriteEffects = SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(tex, drawPos, new Microsoft.Xna.Framework.Rectangle?(rectangle), lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }

            // 下面是着色器的调用和绘制
            //{
            //    GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            //    Texture2D projectileTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            //    // 克隆历史位置用于渲染
            //    Vector2[] drawPoints = (Vector2[])Projectile.oldPos.Clone();
            //    Vector2 aimAheadDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();

            //    if (Projectile.owner == Main.myPlayer)
            //    {
            //        drawPoints[0] += aimAheadDirection * -12f;
            //        drawPoints[1] = drawPoints[0] - (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * Vector2.Distance(drawPoints[0], drawPoints[1]);
            //    }

            //    for (int i = 0; i < drawPoints.Length; i++)
            //    {
            //        drawPoints[i] -= (Projectile.oldRot[i] + MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.5f;
            //    }

            //    // 渲染轨迹
            //    if (Projectile.ai[0] > Projectile.oldPos.Length)
            //    {
            //        int numPointsRendered = 24; // 渲染点数
            //        PrimitiveRenderer.RenderTrail(drawPoints, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"], smoothen: true), numPointsRendered);
            //    }

            //    // 绘制投射物本体及残影
            //    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            //    for (int i = 0; i < 6; i++)
            //    {
            //        float rotation = Projectile.oldRot[i] - MathHelper.PiOver2;
            //        if (Projectile.owner == Main.myPlayer)
            //        {
            //            rotation += 0.2f;
            //        }

            //        Color afterimageColor = Color.Lerp(lightColor, Color.Transparent, 1f - (float)Math.Pow(Utils.GetLerpValue(0, 6, i), 1.4D)) * Projectile.Opacity;
            //        //Main.EntitySpriteDraw(projectileTexture, drawPosition, null, afterimageColor, rotation, projectileTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            //    }
            //}

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
