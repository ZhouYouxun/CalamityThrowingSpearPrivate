using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Enums;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Graphics.Effects;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    internal class PrimeMeridianLaser : ModProjectile
    {
        private const int Lifetime = 24;
        private const int BeamDustID = 73;

        private const float MaxBeamScale = 1.92f;

        private const float MaxBeamLength = 1000f;
        private const float BeamTileCollisionWidth = 1f;
        private const float BeamHitboxCollisionWidth = 15f;
        private const int NumSamplePoints = 3;
        private const float BeamLengthChangeFactor = 0.75f;

        private const float OuterBeamOpacityMultiplier = 0.82f;
        private const float InnerBeamOpacityMultiplier = 0.2f;
        private const float MaxBeamBrightness = 1.75f;

        private const float MainDustBeamEndOffset = 14.5f;
        private const float SidewaysDustBeamEndOffset = 4f;
        private const float BeamRenderTileOffset = 10.5f;
        private const float BeamLengthReductionFactor = 14.5f;

        private Vector2 beamVector = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            // The beam itself still stops on tiles, but its invisible "source" projectile ignores them.
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            // The beam lasts for only some frames and fades out over that time.
            Projectile.timeLeft = Lifetime;
        }

        // projectile.ai[0] = Length of the beam (dynamically recalculated every frame in case someone breaks or places some blocks)
        public override void AI()
        {
            // 计算激光的总长度
            float beamLength = Projectile.ai[0];

            // 计算每个光点的间距（2×16 像素 = 32）
            float pointInterval = 32f + Main.rand.NextFloat(-4f, 4f); // 增加随机偏移量

            // 沿着光束路径生成光点
            for (float d = 0; d <= beamLength; d += pointInterval)
            {
                Vector2 pointPos = Projectile.Center + beamVector * d; // 计算光点位置
                Vector2 offset = beamVector.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f))) * 4f; // 让光点稍微偏移

                // 生成光点粒子
                Dust starDust = Dust.NewDustPerfect(pointPos + offset, 267, Vector2.Zero, 0, Color.White, 1.2f);
                starDust.noGravity = true;
                starDust.fadeIn = 1.5f;
            }


            // On frame 1, set the beam vector and rotation, but set the real velocity to zero.
            if (Projectile.velocity != Vector2.Zero)
            {
                beamVector = Vector2.Normalize(Projectile.velocity);
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            // Reduce the "power" and thus scale of the projectile over its lifetime.
            float power = (float)Projectile.timeLeft / Lifetime;
            Projectile.scale = MaxBeamScale * power;

            // Perform a laser scan to calculate the correct length of the beam.
            float[] laserScanResults = new float[NumSamplePoints];

            // A minimum width is forced for the beam scan to prevent massive lag when fired into open areas.
            float scanWidth = Projectile.scale < 1f ? 1f : Projectile.scale;
            Collision.LaserScan(Projectile.Center, beamVector, BeamTileCollisionWidth * scanWidth, MaxBeamLength, laserScanResults);
            float avg = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
                avg += laserScanResults[i];
            avg /= NumSamplePoints;
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], avg, BeamLengthChangeFactor);

            // X = beam length. Y = beam width.
            Vector2 beamDims = new Vector2(beamVector.Length() * Projectile.ai[0], Projectile.width * Projectile.scale);

            Color beamColor = GetBeamColor();
            ProduceBeamDust(beamColor);

            // If the game is rendering (i.e. isn't a dedicated server), make the beam disturb water.
            if (Main.netMode != NetmodeID.Server)
            {
                WaterShaderData wsd = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();
                // A universal time-based sinusoid which updates extremely rapidly. GlobalTimeWrappedHourly is 0 to 3600, measured in seconds.
                float waveSine = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
                Vector2 ripplePos = Projectile.position + new Vector2(beamDims.X * 0.5f, 0f).RotatedBy(Projectile.rotation);
                // WaveData is encoded as a Color. Not sure why, considering Vector3 exists.
                Color waveData = new Color(0.5f, 0.1f * Math.Sign(waveSine) + 0.5f, 0f, 1f) * Math.Abs(waveSine);
                wsd.QueueRipple(ripplePos, waveData, beamDims, RippleShape.Square, Projectile.rotation);
            }

            // Make the beam cast light along its length.
            // v3_1 is an unnamed decompiled variable which is the color of the light cast by DelegateMethods.CastLight
            DelegateMethods.v3_1 = beamColor.ToVector3() * power * MaxBeamBrightness;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + beamVector * Projectile.ai[0], beamDims.Y, DelegateMethods.CastLight);
        }

        // Determines whether the specified target hitbox is intersecting with the beam.
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host crystal), that's good enough.
            if (projHitbox.Intersects(targetHitbox))
                return true;
            // Otherwise, perform an AABB line collision check to check the whole beam.
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + beamVector * Projectile.ai[0], BeamHitboxCollisionWidth * Projectile.scale, ref _);
        }

        // Ensure that the hit direction is correct when hitting enemies.
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();

            // **添加星光光斑特效**
            for (int i = 0; i < 2; i++) // 每次生成两个光斑
            {
                Vector2 sparklePos = target.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-20f, 20f)); // 随机偏移
                Color baseColor = Color.Lerp(Color.Silver, Color.White, 0.5f) * 0.5f;
                Color glowColor = Color.Lerp(Color.LightGray, Color.White, Main.rand.NextFloat()) * 0.8f;

                DrawPrettyStarSparkle(
                    Projectile.Opacity, SpriteEffects.None, sparklePos,
                    baseColor, glowColor, Main.rand.NextFloat(), 0f, 0.5f, 0.5f, 1f,
                    0f, new Vector2(2f, Main.rand.NextFloat(3f, 5f)), Vector2.One * 0.8f);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
           
           
        }

        private Color GetBeamColor()
        {
            Color c = Color.Lerp(Color.Silver, Color.White, 0.5f); 
            c.A = 64;
            return c;
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }


        public override bool PreDraw(ref Color lightColor)
        {
            // 如果激光没有定义方向向量，或者尚未将速度设置为零，则不绘制
            if (beamVector == Vector2.Zero || Projectile.velocity != Vector2.Zero)
                return false;

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            float beamLength = Projectile.ai[0];
            Vector2 centerFloored = Projectile.Center.Floor() + beamVector * Projectile.scale * BeamRenderTileOffset;
            Vector2 scaleVec = new Vector2(Projectile.scale);

            // 根据激光面积减少长度，防止穿透过多方块
            beamLength -= BeamLengthReductionFactor * Projectile.scale * Projectile.scale;

            DelegateMethods.f_1 = 1f; // 未命名的变量，功能未知，保持为 1
            Vector2 beamStartPos = centerFloored - Main.screenPosition;
            Vector2 beamEndPos = beamStartPos + beamVector * beamLength;
            Utils.LaserLineFraming llf = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

            // 原有逻辑：绘制外部激光
            Color beamColor = GetBeamColor();
            DelegateMethods.c_1 = beamColor * OuterBeamOpacityMultiplier * Projectile.Opacity;
            Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);

            // 原有逻辑：绘制内层激光
            for (int i = 0; i < 5; ++i)
            {
                beamColor = Color.Lerp(beamColor, Color.White, 0.4f); // 内层激光逐渐变白
                scaleVec *= 0.85f; // 缩小内层激光
                DelegateMethods.c_1 = beamColor * InnerBeamOpacityMultiplier * Projectile.Opacity;
                Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);
            }


            // **添加外层发光**
            for (int i = 0; i < 3; i++)
            {
                Color glowColor = Color.Lerp(Color.LightGray, Color.White, 0.5f) * (0.3f - i * 0.1f); // 外层颜色变亮
                Vector2 glowScale = scaleVec * (1.1f + i * 0.2f); // 外层光束稍大

                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, tex.Size() / 2f, glowScale, SpriteEffects.None, 0);
            }

            // **绘制主光束**
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, beamColor, Projectile.rotation, tex.Size() / 2f, scaleVec, SpriteEffects.None, 0);

            // **绘制光束拖尾**
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float intensity = 0.8f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly / 10f + i / (float)Projectile.oldPos.Length * MathHelper.Pi);
                intensity *= MathHelper.Lerp(0.3f, 1f, 1f - i / (float)Projectile.oldPos.Length);

                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Color trailColor = Color.Lerp(Color.LightGray, Color.White, intensity) * 0.5f;

                Main.EntitySpriteDraw(tex, drawPosition, null, trailColor, Projectile.rotation, tex.Size() / 2f, new Vector2(2f) * intensity * 0.8f, SpriteEffects.None, 0);
            }

            // 计算激光的总长度
            //float beamLength = Projectile.ai[0];
            float pointInterval = 32f + Main.rand.NextFloat(-4f, 4f); // 光点间距

            // 沿着激光路径生成星光光斑
            for (float d = 0; d <= beamLength; d += pointInterval)
            {
                Vector2 pointPos = Projectile.Center + beamVector * d; // 计算光点位置
                Vector2 offset = beamVector.RotatedByRandom(MathHelper.ToRadians(10f)) * 4f; // 随机偏移
                Vector2 finalPos = pointPos + offset - Main.screenPosition; // 最终绘制位置

                // 计算光斑颜色，使用银白色渐变
                Color baseColor = Color.Lerp(Color.Silver, Color.White, 0.5f) * 0.5f;
                Color glowColor = Color.Lerp(Color.LightGray, Color.White, Main.rand.NextFloat()) * 0.8f;

                // 绘制光斑
                DrawPrettyStarSparkle(
                    Projectile.Opacity, SpriteEffects.None, finalPos,
                    baseColor, glowColor, d / beamLength, 0f, 0.5f, 0.5f, 1f,
                    0f, new Vector2(2f, Main.rand.NextFloat(3f, 5f)), Vector2.One * 0.8f);
            }




            // 返回 false，防止默认的绘制逻辑执行
            return false;
        }

        private static void DrawPrettyStarSparkle(
            float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor,
            Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd,
            float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
        {
            // 获取星光粒子的纹理（Terraria 预设的额外纹理 98 号）
            Texture2D sparkleTexture = TextureAssets.Extra[98].Value;

            // 计算大光斑颜色，透明度 * 0.5
            Color bigColor = shineColor * opacity * 0.5f;
            bigColor.A = 0; // 透明度设为 0，确保渐隐

            // 获取纹理中心点
            Vector2 origin = sparkleTexture.Size() / 2f;

            // 计算小光斑颜色，透明度 * 0.5
            Color smallColor = drawColor * 0.5f;

            // 计算光斑的渐变透明度
            float lerpValue = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true)
                            * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);

            // 计算光斑缩放
            Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X) * lerpValue;
            Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y) * lerpValue;

            // 调整光斑透明度
            bigColor *= lerpValue;
            smallColor *= lerpValue;

            // **绘制光斑**
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, rotation, origin, scaleUpDown, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, rotation, origin, scaleUpDown * 0.6f, dir);
        }

        private void ProduceBeamDust(Color beamColor)
        {
            // Create a few dust per frame a small distance from where the beam ends.
            Vector2 laserEndPos = Projectile.Center + beamVector * (Projectile.ai[0] - MainDustBeamEndOffset * Projectile.scale);
            for (int i = 0; i < 2; ++i)
            {
                // 50% chance for the dust to come off on either side of the beam.
                float dustAngle = Projectile.rotation + (Main.rand.NextBool() ? 1f : -1f) * MathHelper.PiOver2;
                float dustStartDist = Main.rand.NextFloat(1f, 1.8f);
                Vector2 dustVel = dustAngle.ToRotationVector2() * dustStartDist;
                int d = Dust.NewDust(laserEndPos, 0, 0, BeamDustID, dustVel.X, dustVel.Y, 0, beamColor);
                Main.dust[d].color = beamColor;
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.7f;

                // Scale up dust with the projectile if it's large.
                if (Projectile.scale > 1f)
                {
                    Main.dust[d].velocity *= Projectile.scale;
                    Main.dust[d].scale *= Projectile.scale;
                }

                // If the beam isn't at max scale, then make additional smaller dust.
                if (Projectile.scale != MaxBeamScale)
                {
                    Dust smallDust = Dust.CloneDust(d);
                    smallDust.scale /= 2f;
                }
            }

            // Low chance every frame to spawn a large "directly sideways" dust which doesn't move.
            if (Main.rand.NextBool(5))
            {
                // Velocity, flipped sideways, times -50% to 50% of beam width.
                Vector2 dustOffset = beamVector.RotatedBy(MathHelper.PiOver2) * (Main.rand.NextFloat() - 0.5f) * Projectile.width;
                Vector2 dustPos = laserEndPos + dustOffset - Vector2.One * SidewaysDustBeamEndOffset;
                int d = Dust.NewDust(dustPos, 8, 8, BeamDustID, 0f, 0f, 100, beamColor, 1.2f);
                Main.dust[d].velocity *= 0.5f;

                // Force the dust to always move downwards, never upwards.
                Main.dust[d].velocity.Y = -Math.Abs(Main.dust[d].velocity.Y);
            }
        }

        // Automatically iterates through every tile the laser is overlapping to cut grass at all those locations.
        public override void CutTiles()
        {
            // tilecut_0 is an unnamed decompiled variable which tells CutTiles how the tiles are being cut (in this case, via a projectile).
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = DelegateMethods.CutTiles;
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + beamVector * Projectile.ai[0];
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
        }
    }
}
