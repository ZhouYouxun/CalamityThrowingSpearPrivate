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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod.Particles;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC
{
    public class StreamGougeJavPROJSPLIT : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/StreamGougeC/StreamGougeJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 50;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // **如果透明度为 0，直接跳过绘制**
            if (Projectile.Opacity <= 0f || swingProgress >= 1f)
                return false;

            DrawSlash(); // **新增刀光着色器**
            DrawSwingTrail(); // 绘制挥砍拖尾
            DrawBlade(); // 绘制刀身本体

            return false; // 手动绘制，不执行默认绘制
        }
        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;

        // 绘制刀光着色器轨迹
        private void DrawSlash()
        {
            if (swingProgress < 0.45f) // 挥砍进度小于 45% 不绘制
                return;

            // 进入 Shader 渲染区域
            Main.spriteBatch.EnterShaderRegion();

            // 设定剑气挥砍的 Shader 纹理、颜色等参数
            GameShaders.Misc["CalamityMod:ExobladeSlash"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes")
            );
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseColor(new Color(70, 50, 150)); // 深紫星云色
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseSecondaryColor(new Color(40, 20, 100)); // 深蓝星空色
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(new Color(150, 60, 200).ToVector3()); // 明亮的紫粉色

            // 设置 Shader 方向，使其适应挥砍方向
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["flipped"].SetValue(Direction == 1);
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Apply();

            // 通过 PrimitiveRenderer 渲染剑气轨迹
            PrimitiveRenderer.RenderTrail(
                GenerateSlashPoints(),
                new(SlashWidthFunction, SlashColorFunction, (completionRatio, vertexPos) => Projectile.Center, shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]),
                95
            );

            // 退出 Shader 渲染区域
            Main.spriteBatch.ExitShaderRegion();
        }

        private List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new();

            for (int i = 0; i < 40; i++)
            {
                float progress = i / 40f;
                Vector2 trailPos = Projectile.Center + DirectionAtProgress(progress) * 80f; // 根据挥舞角度计算轨迹点
                result.Add(trailPos);
            }
            return result;
        }

        // 计算挥舞进度下的方向
        private Vector2 DirectionAtProgress(float progress)
        {
            return (swingAngle * progress).ToRotationVector2();
        }

        // 计算刀光轨迹宽度
        // 旧版签名：private float SlashWidthFunction(float completionRatio)
        private float SlashWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            return Projectile.scale * 50f;
        }

        // 计算刀光颜色
        // 旧版签名：private Color SlashColorFunction(float completionRatio)
        private Color SlashColorFunction(float completionRatio, Vector2 vertexPos)
        {
            return Color.Lerp(Color.MediumPurple, Color.Violet, (float)Math.Sin(completionRatio * MathHelper.Pi)) * 0.8f;
        }

        // 绘制挥砍刀光轨迹
        private void DrawSwingTrail()
        {
            if (swingAngle < MaxSwingAngle * 0.5f) // 挥砍进度 < 50% 时不绘制
                return;

            Main.spriteBatch.EnterShaderRegion();

            // **应用 Shader 效果**
            GameShaders.Misc["CalamityMod:ExobladeSlash"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes")
            );
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseColor(new Color(120, 50, 255)); // 星云紫
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseSecondaryColor(new Color(60, 20, 200)); // 深紫色
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(new Vector3(0.5f, 0.1f, 0.6f)); // 红紫色
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Apply(); // 预先应用 Shader

            // **渲染刀光轨迹**
            PrimitiveRenderer.RenderTrail(
                GenerateSwingTrailPoints(),
                //new(width => 20f, completion => Color.Red * 0.8f, (_) => swingCenter, true), // 将第四个参数设为 true
                // 新版的写法为:
                new PrimitiveSettings(
                    (completionRatio, vertexPos) => 20f,
                    (completionRatio, vertexPos) => Color.Red * 0.8f,
                    (completionRatio, vertexPos) => swingCenter,
                    true
                ),
                40
            );

            Main.spriteBatch.ExitShaderRegion();
        }


        // 生成刀光轨迹点
        private List<Vector2> GenerateSwingTrailPoints()
        {
            List<Vector2> result = new();
            for (int i = 0; i < 40; i++)
            {
                float progress = i / 40f;
                Vector2 trailPos = swingCenter + new Vector2((float)Math.Cos(progress * swingAngle), (float)Math.Sin(progress * swingAngle)) * 60f;
                result.Add(trailPos);
            }
            return result;
        }

        private void DrawBlade()
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects direction = SpriteEffects.None;

            // **根据 swingProgress 计算透明度**
            float alpha = 1f; // 默认完全不透明
            if (swingProgress >= 0.95f) // **最后 5% 时间逐渐变透明**
            {
                alpha = MathHelper.Lerp(1f, 0f, (swingProgress - 0.95f) / 0.05f); // 从完全不透明到完全透明
            }

            // **计算淡紫色发光颜色**
            Color glowColor = Color.Lerp(Color.MediumPurple, Color.Violet, 0.6f) * alpha; // 颜色偏淡紫色
            glowColor.A = (byte)(150 * alpha); // 动态调整透明度

            // **绘制主刀身**
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                glowColor, // 使用发光的紫色
                Projectile.rotation, // 旋转角度
                origin,
                Projectile.scale,
                direction,
                0
            );

            // **绘制枪头的十字星**
            //DrawFlareEffect();
        }

        private void DrawFlareEffect()
        {
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Vector2 shineScale = new Vector2(1.5f, 4.0f); // **稍微加大比例，增强视觉效果**
            Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(15f, 15f);

            // **提升透明度，让发光效果更明显**
            float flareOpacity = (swingProgress < 0.3f ? 0f : 0.5f + 0.5f * (float)Math.Sin(MathHelper.Pi * (swingProgress - 0.3f) / 0.7f)) * 0.9f;
            Color flareColor = Color.Lerp(new Color(180, 120, 255), new Color(100, 50, 200), (float)Math.Pow(swingProgress, 3));
            flareColor.A = 255; // **确保透明度不会太低**

            Main.EntitySpriteDraw(
                shineTex,
                gunTip - Main.screenPosition,
                null,
                flareColor * flareOpacity,
                MathHelper.PiOver2,
                shineTex.Size() / 2f,
                shineScale * Projectile.scale,
                SpriteEffects.None,
                0
            );
        }


        private Vector2 swingCenter; // 旋转中心（枪尾）
        private float swingAngle = 0f; // 旋转角度
        private float swingSpeed = 0f; // 旋转速度
        private float MaxSwingAngle = MathHelper.ToRadians(180f); // 最大挥舞角度（°）
        
        //private float swingProgress => swingAngle / MaxSwingAngle; // 归一化挥舞进度（0-1）
        private float swingProgress => Math.Abs(swingAngle / MaxSwingAngle); // 改良版本


        private NPC lockedTarget; // 锁定的目标
        private Vector2 chargeDirection; // 冲刺方向
        private float currentSpeed = 0f; // 当前冲刺速度
        private const float maxChargeSpeed = 3.5f; // 最大冲刺速度
        private int swingDirection; // 1 = 顺时针, -1 = 逆时针

        private float GetSwingSpeed()
        {
            if (swingProgress < 0.15f) // **前 15% 静止但缓慢增加 swingAngle**
            {
                return 0.0175f * swingDirection; // **缓慢增加 swingAngle，确保 swingProgress 能增加**
            }

            if (swingProgress < 0.75f) // **中间 60% 线性加速**
            {
                float progressInPhase = (swingProgress - 0.15f) / 0.6f;
                return MathHelper.Lerp(0.035f, 1.28f, progressInPhase) * swingDirection;
            }

            if (swingProgress < 0.95f) // **后 20% 线性减速**
            {
                float progressInPhase = (swingProgress - 0.75f) / 0.2f;
                return MathHelper.Lerp(0.08f, 0.09f, progressInPhase) * swingDirection;
            }

            return 0f; // **最后 5% 停止旋转**
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1; // 对一个敌人只能造成一次伤害
            Projectile.scale = 1.05f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            swingCenter = Projectile.Center; // 旋转中心设置为弹幕生成时的位置
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

            // **随机决定顺时针 (1) 或逆时针 (-1) 旋转**
            swingDirection = Main.rand.NextBool() ? 1 : -1;

            // **寻找最近的敌人**
            lockedTarget = Main.npc
                .Where(npc => npc.active && !npc.friendly && npc.life > 0)
                .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                .FirstOrDefault();

            // **如果找到敌人，则计算冲刺方向**
            if (lockedTarget != null)
            {
                chargeDirection = (lockedTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            }
            else
            {
                chargeDirection = Projectile.velocity.SafeNormalize(Vector2.Zero); // 没有敌人则使用默认方向
            }
        }
        public override bool? CanDamage()
        {
            return swingProgress >= 0.15f ? true : false; // 前 15% 不造成伤害
        }
        public override void AI()
        {
            swingSpeed = GetSwingSpeed();
            swingAngle += swingSpeed; // **确保 swingAngle 在静止阶段也能增长**

            if (swingProgress < 0.15f) // **前 15% 静止，但 swingAngle 仍然缓慢增加**
            {
                Projectile.velocity = Vector2.Zero;
            }
            else // **后 85% 开始冲刺**
            {
                //currentSpeed = MathHelper.Lerp(currentSpeed, maxChargeSpeed, 0.1f);
                //Projectile.velocity = chargeDirection * currentSpeed;

                // 暂时禁用冲刺
                Projectile.velocity = Vector2.Zero;
            }

            // **计算旋转位置**
            Vector2 offset = new Vector2((float)Math.Cos(swingAngle), (float)Math.Sin(swingAngle)) * 60f;
            Projectile.Center = swingCenter + offset;

            // **平滑旋转**
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, swingAngle + MathHelper.PiOver4, 0.15f);

            if (swingAngle >= MaxSwingAngle)
            {
                Projectile.Kill();
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 center = target.Center;

            // 生成 3 个随机方向的 ImpactParticle
            for (int i = 0; i < 3; i++)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度
                Vector2 position = center + new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * Main.rand.NextFloat(20f, 40f); // 随机半径

                ImpactParticle impactParticle = new ImpactParticle(position, 0.1f, 20, 0.5f, Color.Cyan);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

            // 释放扩散状 Metaball 粒子
            for (int i = 0; i < 20; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 position = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                Vector2 velocity = (position - center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f);
                StreamGougeMetaball.SpawnParticle(position, velocity, Main.rand.NextFloat(20f, 40f));
            }
        }


    }
}
