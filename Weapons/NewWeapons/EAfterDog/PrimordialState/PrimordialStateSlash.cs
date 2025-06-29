using CalamityMod.Items.Weapons.Melee;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimordialState
{
    public class PrimordialStateSlash : ModProjectile
    {
        internal const float StartingScale = 1f;

        internal const float SunlightBladeMaxVelocity = DefiledGreatsword.ShootSpeed * 2f;
        internal const int SunlightBladePierce = 10;

        internal const float FadeInTime = 30f;
        internal const float FadeOutTime = 30f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            // 设置弹幕的轨迹缓存长度为20帧。也就是说，弹幕在运动时会留下20帧的轨迹。

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            // 设置弹幕的尾迹模式为2，表示尾迹效果是按照一定比例的透明度递减模式显示。

            ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Projectile.type] = true;
            // 禁用近战速度对弹幕速度的影响，保持弹幕速度恒定。

            Main.projFrames[Projectile.type] = 4;
            // 设置该弹幕具有4帧的动画效果，即弹幕在飞行过程中会循环播放4帧的动画。
        }


        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // Blazing blades and hyper blades hit four times, sunlight blades hit ten times.
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 600;
            Projectile.noEnchantmentVisuals = true;
            Projectile.scale = StartingScale;
        }

        public override void AI()
        {
            // 确保与主弹幕绑定
            int parentID = (int)Projectile.ai[0];
            if (Main.projectile.IndexInRange(parentID))
            {
                Projectile parent = Main.projectile[parentID];
                if (parent.active && parent.type == ModContent.ProjectileType<PrimordialStatePROJ>())
                {
                    Projectile.Center = parent.Center;
                    Projectile.rotation = parent.rotation;
                }
                else
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.Kill();
            }

            // 改进后的粒子特效
            for (int i = 0; i < 3; i++) // 每帧生成 3 个粒子
            {
                float rotation = Projectile.rotation + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 offset = rotation.ToRotationVector2() * Main.rand.NextFloat(30f, 50f);
                Vector2 position = Projectile.Center + offset;

                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, null, 150, Color.OrangeRed, Main.rand.NextFloat(1f, 1.5f));
                dust.velocity = offset * -0.1f; // 反方向的速度
                dust.noGravity = true;
            }

            // 尾迹效果（流线型）
            for (int i = 0; i < 2; i++) // 每帧生成 2 个尾迹粒子
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * i * 0.5f;
                Dust trailDust = Dust.NewDustPerfect(trailPos, DustID.Torch, null, 100, Color.Gold, 0.8f);
                trailDust.velocity *= 0.2f;
                trailDust.noGravity = true;
            }

            // 附魔效果的视觉表现
            Vector2 boxPosition = Projectile.position;
            int boxWidth = Projectile.width;
            int boxHeight = Projectile.height;
            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2)
            {
                Rectangle rect = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 70f * Projectile.scale, new Vector2(60f * Projectile.scale, 60f * Projectile.scale));
                // 生成矩形区域，用于生成附魔视觉效果
                Projectile.EmitEnchantmentVisualsAt(rect.TopLeft(), rect.Width, rect.Height); // 发射附魔视觉效果
            }


            // 淡入淡出效果
            if (Projectile.localAI[0] == 0f) // 弹幕首次激活时
            {
                Projectile.Opacity = 0f;
            }

            Projectile.localAI[0] += 1f;
            Projectile.Opacity = MathHelper.Clamp(Utils.Remap(Projectile.localAI[0], 0f, 30f, 0f, 1f) * Utils.Remap(Projectile.localAI[0], 570f, 600f, 1f, 0f), 0f, 1f);

            if (Projectile.localAI[0] >= 600f) // 达到生命周期后销毁弹幕
            {
                Projectile.Kill();
            }

            //{
            //    // True Night's Edge 的AI （注释掉的代码，表示曾参考或借用了 True Night's Edge 的 AI 机制）
            //    //float fadeInTime = 30f;
            //    //float fadeOutTime = 30f;

            //    // Defiled Greatsword 有三个不同的弹幕变种
            //    float fullyVisibleDuration = Projectile.ai[1]; // 弹幕完全可见的持续时间，由 ai[1] 决定
            //    bool hyperBlade = fullyVisibleDuration == DefiledGreatsword.ProjectileFullyVisibleDuration + DefiledGreatsword.ProjectileFullyVisibleDurationIncreasePerAdditionalProjectile;
            //    // 判断是否是 hyperBlade 变种，通过完全可见时间与指定条件的比较确定
            //    bool sunlightBlade = fullyVisibleDuration == DefiledGreatsword.ProjectileFullyVisibleDuration + DefiledGreatsword.ProjectileFullyVisibleDurationIncreasePerAdditionalProjectile * 2f;
            //    // 判断是否是 sunlightBlade 变种

            //    float timeBeforeFadeOut = fullyVisibleDuration + FadeInTime; // 计算弹幕开始淡出的时间
            //    float projectileDuration = timeBeforeFadeOut + FadeOutTime; // 计算弹幕的总持续时间

            //    if (Projectile.localAI[0] == 0f) // 弹幕首次激活时，播放声音效果
            //        SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);

            //    Projectile.localAI[0] += 1f; // 增加弹幕存在的时间
            //    Projectile.Opacity = Utils.Remap(Projectile.localAI[0], 0f, fullyVisibleDuration, 0f, 1f) * Utils.Remap(Projectile.localAI[0], timeBeforeFadeOut, projectileDuration, 1f, 0f);
            //    // 根据弹幕存在的时间，调整弹幕的透明度（从逐渐出现到逐渐消失）

            //    if (Projectile.localAI[0] >= projectileDuration) // 弹幕寿命结束时，销毁弹幕
            //    {
            //        Projectile.localAI[1] = 1f;
            //        Projectile.Kill();
            //        return;
            //    }

            //    Player player = Main.player[Projectile.owner]; // 获取弹幕所属的玩家
            //    Projectile.direction = (Projectile.spriteDirection = (int)Projectile.ai[0]); // 设置弹幕的方向和旋转方向

            //    Projectile.localAI[1] += 1f; // 更新弹幕旋转和行为的时间
            //    Projectile.rotation += Projectile.ai[0] * MathHelper.TwoPi * (4f + Projectile.Opacity * 4f) / 90f; // 旋转弹幕，根据透明度变化调整旋转速度
            //    Projectile.scale = Utils.Remap(Projectile.localAI[0], fullyVisibleDuration + 2f, projectileDuration, 1.12f, 1f) * Projectile.ai[2] * StartingScale;
            //    // 根据弹幕存在时间调整其缩放比例

            //    float randomDustSpawnLocation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f; // 随机生成尘埃位置
            //    Vector2 dustPosition = Projectile.Center + randomDustSpawnLocation.ToRotationVector2() * 84f * Projectile.scale;
            //    if (Main.rand.NextBool(3)) // 每3帧生成一次尘埃效果
            //    {
            //        Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Venom, null, 100, default, 1.4f); // 创建毒液尘埃
            //        dust.noGravity = true; // 尘埃没有重力效果
            //        dust.velocity *= 0f;
            //        dust.fadeIn = 1.5f; // 尘埃淡入效果
            //    }

            //    // 生成额外的尘埃效果，根据弹幕的透明度增加尘埃数量
            //    for (int i = 0; (float)i < 3f * Projectile.Opacity; i++)
            //    {
            //        Vector2 dustVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX); // 获取尘埃的速度方向
            //        int dustType1 = sunlightBlade ? DustID.IchorTorch : hyperBlade ? DustID.CursedTorch : DustID.CrimsonTorch; // 不同类型的弹幕生成不同的尘埃
            //        int dustType2 = sunlightBlade ? DustID.YellowTorch : hyperBlade ? DustID.GreenTorch : DustID.RedTorch;
            //        int dustType = ((Main.rand.NextFloat() < Projectile.Opacity) ? dustType1 : dustType2); // 根据透明度选择尘埃类型
            //        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, Projectile.velocity * 0.2f + dustVelocity * 3f, 100, default, 1.4f);
            //        dust.noGravity = true; // 尘埃没有重力效果
            //        dust.customData = Projectile.Opacity * 0.2f; // 调整尘埃的视觉效果
            //    }

            //    // 自动追踪目标，若弹幕不是 sunlightBlade 则启用自动追踪
            //    if (!sunlightBlade)
            //    {
            //        CalamityUtils.HomeInOnNPC(Projectile, true, hyperBlade ? 500f : 250f, hyperBlade ? 16f : 8f, hyperBlade ? 10f : 15f);
            //        // 调用自动追踪方法，hyperBlade 的追踪距离和速度更高
            //    }

            //    // Sunlight Blades 会加速
            //    else
            //    {
            //        if (Projectile.velocity.Length() < SunlightBladeMaxVelocity) // 若当前速度小于最大速度
            //        {
            //            Projectile.velocity *= 1.05f; // 增加速度
            //            if (Projectile.velocity.Length() > SunlightBladeMaxVelocity) // 若超过最大速度，则调整为最大速度
            //            {
            //                Projectile.velocity.Normalize();
            //                Projectile.velocity *= SunlightBladeMaxVelocity;
            //            }
            //        }
            //    }  
            //}

        }


        //public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        //{
        //    Vector2 distanceFromTarget = targetHitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
        //    // 计算弹幕中心与目标矩形最近点之间的距离
        //    distanceFromTarget.SafeNormalize(Vector2.UnitX);
        //    // 将距离向量标准化为单位向量（防止向量长度为零）
        //    float projectileSize = 100f * Projectile.scale;
        //    // 定义弹幕的判定大小，基于弹幕的缩放比例，100像素为基础大小
        //    if (distanceFromTarget.Length() < projectileSize && Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0))
        //        // 如果距离小于弹幕的判定大小，并且能够碰撞到目标（没有被地形阻挡），则返回 true，表示碰撞发生
        //        return true;
        //    return null;
        //    // 如果不满足碰撞条件，则返回 null，表示不发生碰撞
        //}

        //public override void CutTiles()
        //{
        //    Vector2 startPoint = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
        //    // 计算弹幕旋转角度 - 45度（MathHelper.PiOver4 为 45度），并转换为向量，再乘以弹幕大小，得到开始点
        //    Vector2 endPoint = (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * 60f * Projectile.scale;
        //    // 计算弹幕旋转角度 + 45度，并转换为向量，得到结束点
        //    float projectileSize = 60f * Projectile.scale;
        //    // 设置弹幕的碰撞范围大小为 60 像素乘以缩放比例
        //    Utils.PlotTileLine(Projectile.Center + startPoint, Projectile.Center + endPoint, projectileSize, DelegateMethods.CutTiles);
        //    // 使用 `PlotTileLine` 绘制从开始点到结束点的一条线，用于破坏路径上的地形
        //}


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D asset = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = asset.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float num = Projectile.scale * 1.5f;
            SpriteEffects effects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);

            float fromValue = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3D);
            fromValue = Utils.Remap(fromValue, 0.1f, 1f, 0.2f, 1f);
            float num3 = MathHelper.Min(0.15f + fromValue * 0.85f, Utils.Remap(Projectile.localAI[0], 30f, 96f, 1f, 0f));
            float num4 = 4f;

            for (float num5 = num4; num5 >= 0f; num5 -= 1f)
            {
                if (!(Projectile.oldPos[(int)num5] == Vector2.Zero))
                {
                    Vector2 vectorScale = Projectile.Center - Projectile.velocity * 0.5f * num5;
                    float num6 = Projectile.oldRot[(int)num5];
                    Vector2 position = vectorScale - Main.screenPosition;
                    float num7 = 1f - num5 / num4;
                    float num8 = Projectile.Opacity * num7 * num7 * 0.95f;
                    float amount = Projectile.Opacity * Projectile.Opacity;

                    Color colorOne = Color.Lerp(
                        new Color(30, 30, 30, 120),  // 深黑色
                        new Color(80, 80, 80, 120),  // 暗灰色
                        amount);

                    Color colorTwo = Color.Lerp(
                        new Color(10, 10, 10),  // 更深的黑色
                        new Color(50, 50, 50),  // 暗黑色
                        amount);

                    Main.spriteBatch.Draw(asset, position, rectangle, colorOne * num3 * num8, num6, origin, num, effects, 0f);

                    float num9 = 4f;
                    for (float num10 = -MathHelper.TwoPi + MathHelper.TwoPi / num9; num10 < 0f; num10 += MathHelper.TwoPi / num9)
                    {
                        float num11 = Utils.Remap(num10, -MathHelper.TwoPi, 0f, 0f, 0.5f);
                        Main.spriteBatch.Draw(asset, position, rectangle, colorTwo * num11, num6 + num10, origin, num, effects, 0f);
                    }
                }
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //bool sunlightBlade = Projectile.ai[1] == DefiledGreatsword.ProjectileFullyVisibleDuration + DefiledGreatsword.ProjectileFullyVisibleDurationIncreasePerAdditionalProjectile * 2f;
            //if (Projectile.numHits >= (sunlightBlade ? SunlightBladePierce : 3))
            //{
            //    Projectile.localAI[0] = Projectile.ai[1] + FadeInTime;
            //}
        }

        public override bool? CanDamage() => Projectile.localAI[0] > Projectile.ai[1] + FadeInTime ? false : null;
    }
}