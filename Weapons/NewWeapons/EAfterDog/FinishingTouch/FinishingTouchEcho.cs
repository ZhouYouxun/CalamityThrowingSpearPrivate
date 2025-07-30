using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouchEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouchEcho";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            //Main.projFrames[Projectile.type] = 4; // 设置投射物的帧数为 4
        }
        private float dnaWaveCounter = 0f; // 用于计算螺旋偏移波动

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 200; // 只允许200次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 能够穿透方块
            Projectile.extraUpdates = 4; // 额外更新次数
            Projectile.ArmorPenetration = 5;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            // 在 AI 内加入一次性判断调用（只在出生时）
            if (Projectile.timeLeft == 600) // 初始时调用
            {
                CTSLightingBoltsSystem.Spawn_SagittariusSpitBirth(Projectile.Center);
            }

            // 加速效果，每帧速度乘以1.01
            Projectile.velocity *= 1.02f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);

            // 造成了一次伤害之后就直接关闭伤害检测并快速降低速度，并让自己停下来
            if (Projectile.penetrate < 200)
            {
                if (Projectile.timeLeft > 60) { Projectile.timeLeft = 60; } //The projectile start shrinking and slowing down. it can still hit for a bit during this, to allow a bit of multi-target if the enemies are really close to eachother.
                Projectile.velocity *= 0.88f;
            }

            // 小型冲击波生成，两个一大一小
            if (Projectile.timeLeft == 600)
            {
                Vector2 smallPulseScale = new Vector2(0.3f, 1.2f); // 小型冲击波
                Vector2 largePulseScale = new Vector2(0.6f, 1.6f); // 大型冲击波

                // 第一个小型垂直椭圆冲击波
                Particle smallPulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, smallPulseScale, MathHelper.PiOver2, 0.3f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(smallPulse);

                // 第二个大型垂直椭圆冲击波
                Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, largePulseScale, MathHelper.PiOver2, 0.2f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(largePulse);
            }

        }
        public override void OnKill(int timeLeft)
        {
            int arcCount = 5; // 弧段数
            int pointsPerArc = 8;
            float baseRadius = 60f;

            for (int arc = 0; arc < arcCount; arc++)
            {
                float arcStartAngle = MathHelper.TwoPi * arc / arcCount + Main.GameUpdateCount * 0.03f;
                float arcSpan = MathHelper.PiOver4;
                float arcRadius = baseRadius + arc * 6f;

                for (int i = 0; i < pointsPerArc; i++)
                {
                    float t = (float)i / (pointsPerArc - 1);
                    float angle = arcStartAngle - arcSpan * 0.5f + arcSpan * t;
                    Vector2 baseDirection = angle.ToRotationVector2();

                    // Dust 位于主弧线圆上（稳定结构）
                    Vector2 dustPos = Projectile.Center + baseDirection * arcRadius;
                    Dust dust = Dust.NewDustPerfect(dustPos, 267, Vector2.Zero, 0, Color.White, 1.3f);
                    dust.noGravity = true;
                }
            }

            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.OrangeRed,
                new Vector2(1.6f, 1.6f),
                0f,
                0.5f,
                1.0f,
                30
                );
            GeneralParticleHandler.SpawnParticle(pulse);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouchEcho").Value;

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 使用金黄色~浅黄色渐变
                Color color = Color.Lerp(Color.Orange, Color.OrangeRed, colorInterpolation) * 0.4f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.5f;

                // 计算强度，使拖尾逐渐变弱
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 如果弹幕即将消失，则拖尾也逐渐消失
                }

                // 计算外部和内部的缩放比例，使拖尾具有渐变效果
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // 如果需要绘制弹幕主体，取消注释以下代码
            //Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
