using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod;
using static CalamityMod.CalamityUtils;
using CalamityMod.Balancing;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles;
using ReLogic.Content;
using System.Linq;
using Terraria.Localization;
using Terraria.Graphics.Effects;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    public class PrimeMeridianRIGHT : ModProjectile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/PrimeMeridian/PrimeMeridian";        
        public Player Owner => Main.player[Projectile.owner]; // 获取当前弹幕的拥有者（玩家）
                                                            
        const float BladeLength = 180; // 设定武器的刀刃长度 [能够攻击到距离玩家多远的敌方单位]

        // 设定固定的挥砍时间（单位：帧）
        public const int GetSwingTime = 78;

        // 计算当前武器的挥砍计时器[倒计时]
        public float Timer => SwingTime - Projectile.timeLeft;

        // 计算当前挥砍的进度（0 ~ 1 之间）
        public float Progression => Timer / (float)SwingTime;

        // 定义挥砍状态枚举
        public enum SwingState
        {
            Swinging,  // 普通挥砍状态
            // 这里可以添加点东西
        }

        // 记录当前武器的状态（挥砍 / 冲刺[被移除,可以添加别的]）
        public SwingState State
        {
            get => SwingState.Swinging; // 直接返回 Swinging 状态
            set { } // 不再需要设置其他状态
        }
 
        // 记录当前挥砍的总持续时间
        public ref float SwingTime => ref Projectile.localAI[0];

        // 计算武器挥砍的方向（向左-1或向右1）
        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;

        // 计算武器的基础旋转角度
        public float BaseRotation => Projectile.velocity.ToRotation();

        // 设定最大挥砍角度
        public static float MaxSwingAngle = MathHelper.PiOver2 * 1.8f;

        // 定义挥砍动画的曲线段
        // PolyOutEasing: 快到慢（PolyOut）
        // PolyInEasing: 慢到快（PolyIn） 

        // CurveSegment的五个传递值：
        // 缓动方式：PolyOutEasing（快到慢）。PolyInEasing（慢到快）。PolyOutEasing（快到慢）。
        // 开始时间：0.0f（挥砍刚开始）。0.27f（挥砍 27% 进度时开始）。0.85f（挥砍 85% 进度时开始）。
        // 起始高度：-1f（角度起始偏移量）。-0.7f（角度起始偏移）。0.9f（角度起始偏移）。
        // 高度变化：0.3f（过渡的角度变化）。1.6f（快速拉高角度变化）。0.1f（收招角度变化）。
        // 指数级别：2（平方曲线，较平滑）。4（四次方曲线，陡峭加速）。2（平方曲线，较平缓）。
        public CurveSegment SlowStart = new(PolyOutEasing, 0f, -1f, 0.3f, 2);

        public CurveSegment SwingFast = new(PolyInEasing, 0.27f, -0.7f, 1.6f, 4);

        public CurveSegment EndSwing = new(PolyOutEasing, 0.85f, 0.9f, 0.1f, 2);



        // 计算当前 progress（挥砍进度）对应的挥砍角度偏移量
        public float SwingAngleShiftAtProgress(float progress)
            => MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });


        // 计算当前挥砍进度 progress 下武器的旋转角度。
        public float SwordRotationAtProgress(float progress)
            => BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;


        // 计算当前挥砍进度下的形变量【我们这里不会让它发生形变】
        public float SquishAtProgress(float progress) => 1f;


        // 计算当前挥砍进度下武器的方向向量
        public Vector2 DirectionAtProgress(float progress)
              => SwordRotationAtProgress(progress).ToRotationVector2();


        // 计算当前武器角度相对于正前方的偏移量（挥砍角度变化）
        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);

        // 计算当前武器的旋转角度
        public float SwordRotation => SwordRotationAtProgress(Progression);

        // 计算当前武器的方向向量
        public Vector2 SwordDirection => DirectionAtProgress(Progression);


        // 计算拖尾结束点在整个挥砍过程中的进度
        public float TrailEndProgression // 计算拖尾结束点的进度
        {
            get
            {
                // Progression 是整体进度，从0~1
                float endProgression;

                // 前半段挥砍时, 拖尾终点比当前进度略滞后 50%，同时缓慢增长，确保它不会太短
                if (Progression < 0.75f)
                    endProgression = Progression - 0.5f + 0.1f * (Progression / 0.75f);

                // 在后半段进度时，让拖尾终点收缩，使其在 Progression = 1 时完全消失，而不是突然消失
                else
                    endProgression = Progression - 0.4f * (1 - (Progression - 0.75f) / 0.75f);

                return Math.Clamp(endProgression, 0, 1);
            }
        }

        // 计算轨迹完成点的实际进度
        public float RealProgressionAtTrailCompletion(float completion)
            => MathHelper.Lerp(Progression, TrailEndProgression, completion);

        // 计算挥砍过程中武器的方向
        public Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);

            // 获取挥砍角度对应的单位向量
            Vector2 anglePoint = angleShift.ToRotationVector2();

            // 将单位向量转换回旋转角度
            angleShift = anglePoint.ToRotation();

            // 计算最终挥砍方向，不再受 SquishFactor 影响
            return (BaseRotation + angleShift * Direction).ToRotationVector2();
        }


        // 设定挥砍过程中“回收”的运动曲线
        public CurveSegment GoBack = new(SineBumpEasing, 0f, -10f, -14f);

        public static Asset<Texture2D> LensFlare;


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadSingle();
        }

        public override bool? CanDamage() => true;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * BladeLength * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * 30f, ref _);
        }

        public void InitializationEffects(bool startInitialization)
        {
            // 关键：每次新挥砍开始时，重置 HasFired，确保新的挥砍能够触发弹幕
            HasFired = false;

            // 重新计算挥砍方向，使武器朝向鼠标位置
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Owner.Calamity().mouseWorld);

            Projectile.scale = 1f;

            // 设置挥砍持续时间
            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;

            // 触发网络更新，确保多玩家环境下同步数据
            Projectile.netUpdate = true;
            Projectile.netSpam = 0;
        }

        public override void AI()
        {
            // 检查弹幕的生命周期：
            if (Projectile.timeLeft >= 9999)
            {
                InitializationEffects(Projectile.timeLeft >= 9999);
            }

            // 松开左键后，只进行最后一次攻击，然后确保它正确减少 `timeLeft`
            if (!Owner.channel && Projectile.timeLeft > 1)
            {
                Projectile.timeLeft--; // 让 `timeLeft` 正常减少，确保最终消失
            }

            // 根据当前 `State` 决定执行哪种攻击行为
            switch (State)
            {
                case SwingState.Swinging:
                    // 进行普通的挥砍攻击逻辑
                    DoBehavior_Swinging();
                    break;
            }

            // 让武器的位置始终跟随玩家，以保持挥砍时的连贯性
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            // 让玩家角色的 `heldProj` 绑定到当前 `Projectile`，表示正在使用这把武器
            Owner.heldProj = Projectile.whoAmI;

            // 让玩家角色的手部动画保持在“攻击状态”，防止动画提前结束
            Owner.SetDummyItemTime(2);

            // 让角色朝向 `Projectile` 挥砍方向
            Owner.ChangeDir(Direction);

            // 计算玩家的手臂旋转角度，使其与武器的角度匹配
            float armRotation = SwordRotation - MathHelper.PiOver2;

            // 设置前臂动画，使手臂随着挥砍角度进行旋转
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);

        }
        // 追踪当前挥砍是否已经触发过弹幕生成
        public bool HasFired = false;
        public void DoBehavior_Swinging()
        {
            // 如果弹幕的剩余时间等于挥砍时间的 1/5，则播放挥砍音效
            if (Projectile.timeLeft == (int)(SwingTime / 5))
                SoundEngine.PlaySound(SoundID.Item104, Projectile.Center);

            // 在武器挥砍路径上添加光照效果，使其具有发光轨迹
            Lighting.AddLight(
                Owner.MountedCenter + SwordDirection * 100,
                Color.Lerp(Color.Black, Color.DarkGray, (float)Math.Pow(Progression, 3)).ToVector3() * 1.6f * (float)Math.Sin(Progression * MathHelper.Pi)
            );

            // 让粒子在挥砍时间的 1/4 之后开始持续释放
            if (Projectile.timeLeft <= (int)(SwingTime / 4))
            {
                ReleaseSwingParticles();
            }



            // 仅在挥砍进度 >= 50% 时发射弹幕，每次仅发射一次
            if (Main.myPlayer == Projectile.owner && !HasFired && Progression >= 0.5f)
            {
                // 生成X发 PrimeMeridianRIGHTPROJ 弹幕，每次方向不同
                for (int i = 0; i < 6; i++)
                {
                    // 让弹幕在原方向上添加随机偏移，使其发射方向稍有变化
                    Vector2 boltVelocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.2f);
                    boltVelocity *= Owner.ActiveItem().shootSpeed;

                    // 生成 PrimeMeridianRIGHTPROJ 弹幕
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + boltVelocity * 5f,
                        boltVelocity,
                        ModContent.ProjectileType<PrimeMeridianRIGHTPROJ>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }
                // 标记 HasFired 为 true，确保本次挥砍不再重复触发弹幕
                HasFired = true;
            }
        }


        // 计算剑气的宽度
        public float SlashWidthFunction(float completionRatio)
            => Projectile.scale * 60.5f;

        // 计算剑气的颜色
        public Color SlashColorFunction(float completionRatio)
            => Color.Lime * Utils.GetLerpValue(0.9f, 0.4f, completionRatio, true) * Projectile.Opacity;

        // 生成挥砍轨迹的点列表，用于绘制剑气拖尾
        public List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new();

            for (int i = 0; i < 40; i++)
            {
                // 计算当前点的轨迹进度，使轨迹点均匀分布
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);

                // 计算轨迹点在挥砍路径上的位置，并加入列表
                result.Add(DirectionAtProgressScuffed(progress) * (BladeLength - 20f) * Projectile.scale);
            }

            return result;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 120;
        }

        // 预绘制函数，负责在屏幕上渲染挥砍、冲刺轨迹以及武器本体
        public override bool PreDraw(ref Color lightColor)
        {
            // 如果弹幕完全不可见（透明度为 0），则不进行绘制
            if (Projectile.Opacity <= 0f)
                return false;

            // 依次绘制挥砍轨迹和武器本体
            DrawSlash();
            DrawBlade();

            return false; // 这里返回 false，表示不执行默认的弹幕绘制逻辑，而是完全自定义渲染
        }

        // 绘制挥砍轨迹
        public void DrawSlash()
        {
            // 仅在挥砍状态（SwingState.Swinging）且挥砍进度超过 45% 时才进行绘制
            if (State != SwingState.Swinging || Progression < 0.45f)
                return;

            // 进入 Shader 渲染区域
            Main.spriteBatch.EnterShaderRegion();

            // 设定剑气挥砍的 Shader 纹理、颜色等参数
            GameShaders.Misc["CalamityMod:ExobladeSlash"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes")
            );
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseColor(new Color(25, 25, 60)); // 深蓝紫色，类似深渊光晕
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseSecondaryColor(new Color(10, 5, 30)); // 更深的暗蓝紫色，形成渐变
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(new Color(242, 112, 72).ToVector3()); // 这里的颜色需要修改！

            // 设置 Shader 方向，使其适应挥砍方向
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["flipped"].SetValue(Direction == 1);
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Apply();

            // 通过 PrimitiveRenderer 渲染剑气轨迹
            PrimitiveRenderer.RenderTrail(
                GenerateSlashPoints(),
                new(SlashWidthFunction, SlashColorFunction, (_) => Projectile.Center, shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]),
                95
            );

            // 退出 Shader 渲染区域
            Main.spriteBatch.ExitShaderRegion();
        }      

        // 绘制武器本体
        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                // 获取 SwingSprite Shader 以增强武器视觉效果
                Effect swingFX = Filters.Scene["CalamityMod:SwingSprite"].GetShader().Shader;
                swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f));
                swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
                swingFX.Parameters["color"].SetValue(Color.White.ToVector4());

                // 开启即时渲染模式，应用 Shader
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, swingFX, Main.GameViewMatrix.TransformationMatrix);

                // 绘制武器主体
                Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null, Color.White, BaseRotation, texture.Size() / 2f, Projectile.scale * 3f, direction, 0);

                // 关闭即时渲染模式，恢复默认绘制
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // 绘制十字星HalfStar光晕
                if (LensFlare == null)
                    LensFlare = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar");

                Texture2D shineTex = LensFlare.Value;
                Vector2 shineScale = new Vector2(1f, 3f);

                float lensFlareOpacity = (Progression < 0.3f ? 0f : 0.2f + 0.8f * (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.7f)) * 0.6f;
                Color lensFlareColor = Color.Lerp(Color.LimeGreen, Color.Plum, (float)Math.Pow(Progression, 3));
                lensFlareColor.A = 0;

                Main.EntitySpriteDraw(
                    shineTex,
                    Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength - Main.screenPosition,
                    null,
                    lensFlareColor * lensFlareOpacity,
                    MathHelper.PiOver2,
                    shineTex.Size() / 2f,
                    shineScale * Projectile.scale,
                    0,
                    0
                );
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          
        }

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.Calamity().LungingDown = false;
        }

        private void ReleaseSwingParticles()
        {
            // 生成剑气挥砍粒子
            for (int i = 0; i < 5; i++) // 每帧生成 5 组粒子，增加复杂度
            {
                // 计算粒子的起始位置，使其沿着挥砍轨迹分布
                Vector2 particleOrigin = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);

                // 计算粒子扩散方向，使用随机角度偏移
                float angleOffset = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4); // -45° ~ 45° 随机扩散
                Vector2 particleVelocity = SwordDirection.RotatedBy(angleOffset) * Main.rand.NextFloat(2f, 5f); // 速度在 2~5 之间波动

                // 创建深渊风格粒子（修改为暗色）
                Dust abyssDust = Dust.NewDustPerfect(particleOrigin, 267, particleVelocity, 0, new Color(15, 10, 50)); // 深蓝紫色
                abyssDust.noGravity = true;
                abyssDust.scale = Main.rand.NextFloat(0.4f, 0.8f); // 粒子大小随机化
                abyssDust.alpha = 50; // 增加透明度
            }

            // 额外生成深色主题的随机粒子
            for (int j = 0; j < 3; j++) // 每帧额外生成 3 组粒子
            {
                // 计算粒子的起始位置，使其沿着挥砍轨迹分布
                Vector2 particleOrigin = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.2f, 1f);

                // 计算粒子扩散方向，使用更大的扩散范围
                float angleOffset = Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2); // -90° ~ 90° 随机扩散
                Vector2 particleVelocity = SwordDirection.RotatedBy(angleOffset) * Main.rand.NextFloat(3f, 6f); // 速度较快，增强动态感

                // 深色主题粒子颜色
                Color darkDustColor = Color.Lerp(new Color(10, 5, 30), new Color(5, 0, 20), Main.rand.NextFloat());

                // 创建深渊风格粒子
                Dust darkDust = Dust.NewDustPerfect(particleOrigin, 267, particleVelocity, 0, darkDustColor);
                darkDust.scale = Main.rand.NextFloat(0.3f, 0.6f); // 控制彩色粒子大小
                darkDust.fadeIn = Main.rand.NextFloat() * 1.2f; // 让它淡入
                darkDust.noGravity = true; // 悬浮效果
            }
        }

    }
}
