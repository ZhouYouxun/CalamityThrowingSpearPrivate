using CalamityMod.Particles;
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
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd
{
    public class FestiveHalberdSTAR : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        internal Color ColorFunction(float completionRatio)
        {
            // 计算末端的淡化效果
            float fadeToEnd = MathHelper.Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            // 控制拖尾的不透明度，越接近末尾越透明
            float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;

            // 拖尾颜色以 HSL 渐变
            Color colorHue = Main.hslToRgb(0.1f, 1, 0.8f); // 色相设置为金色

            // 动态颜色效果
            Color endColor = Color.Lerp(colorHue, Color.PaleTurquoise, (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);

            return Color.Lerp(Color.Yellow, endColor, fadeToEnd) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio)
        {
            // 拖尾宽度随位置衰减，越靠近末端越窄
            float expansionCompletion = (float)Math.Pow(1 - completionRatio, 3); // 位置越远，衰减越快
            return MathHelper.Lerp(0f, 22 * Projectile.scale * Projectile.Opacity, expansionCompletion);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 未触发粘附时保留原有的所有绘制效果
            // 背光效果部分 - 白色光晕
            Texture2D textureGlow = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 originGlow = textureGlow.Size() * 0.5f;
            Vector2 drawPositionGlow = Projectile.Center - Main.screenPosition;

            // 白色光晕
            float chargeOffset = 3f;
            Color chargeColorWhite = Color.White * 0.6f;
            chargeColorWhite.A = 0;
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(textureGlow, drawPositionGlow + drawOffset, null, chargeColorWhite, rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 金色光晕
            Color chargeColorGold = Color.Gold * 0.5f;
            chargeColorGold.A = 0;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset * 1.5f;
                Main.spriteBatch.Draw(textureGlow, drawPositionGlow + drawOffset, null, chargeColorGold, rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 渲染实际的投射物本体
            Main.EntitySpriteDraw(textureGlow, drawPositionGlow, null, Projectile.GetAlpha(lightColor), rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);

            //Projectile.rotation += 0.95f; // 在绘制时调整旋转逻辑
            //Main.EntitySpriteDraw(textureGlow, drawPositionGlow, null, Projectile.GetAlpha(lightColor), Projectile.rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);

            // 拖尾特效
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);

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
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色改为浅红色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 0.55f);

            // 每帧生成金色火焰特效
            CreateGoldFlameEffect();

            // 在触发追踪之前，每帧速度乘以0.98
            if (Projectile.ai[1] <= 120)
            {
                Projectile.velocity *= 0.98f; // 每帧减速
                Projectile.rotation += 0.95f;
                Projectile.ai[1]++;
            }
            else
            {
                // 追踪敌人逻辑
                NPC target = Projectile.Center.ClosestNPCAt(2400); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 13f, 0.08f); // 追踪速度
                    Projectile.rotation += 0.95f;
                }
            }

            Time++;
        }

        // 添加金色火焰特效的方法
        private void CreateGoldFlameEffect()
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GoldFlame,
                Projectile.velocity * 0.3f, 0, default, Main.rand.NextFloat(2.2f, 2.5f)); // 设置特效大小
            dust.noGravity = true; // 无重力效果
            dust.fadeIn = 0.7f; // 渐入效果
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机旋转
        }
        public ref float Time => ref Projectile.ai[1];

        public override bool? CanDamage() => Time >= 52f; // 初始的时候不会造成伤害，直到x为止

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item107, Projectile.Center); // 实在是找不到圣诞彩灯是哪一种了，因此只能用这种来替代了
            target.AddBuff(BuffID.Frostburn2, 300); // 原版的冻伤效果
        }
        public override void OnKill(int timeLeft)
        {
            // 调用五角星特效函数
            CreateStarEffect(Projectile.Center, 6 * 16);
        }

        // 五角星特效生成方法
        private void CreateStarEffect(Vector2 center, float radius)
        {
            int points = 5; // 五角星的顶点数
            int particlesPerEdge = 10; // 每条边的粒子数
            float innerRadius = radius * 0.5f; // 五角星内圈半径

            // 计算五角星顶点和内点位置
            Vector2[] vertices = new Vector2[points * 2];
            for (int i = 0; i < points; i++)
            {
                float outerAngle = MathHelper.TwoPi / points * i; // 外圈顶点角度
                float innerAngle = MathHelper.TwoPi / points * (i + 0.5f); // 内圈顶点角度
                vertices[i * 2] = center + new Vector2((float)Math.Cos(outerAngle), (float)Math.Sin(outerAngle)) * radius;
                vertices[i * 2 + 1] = center + new Vector2((float)Math.Cos(innerAngle), (float)Math.Sin(innerAngle)) * innerRadius;
            }

            // 生成五角星边上的粒子
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 startPoint = vertices[i];
                Vector2 endPoint = vertices[(i + 1) % vertices.Length]; // 当前边的终点

                for (int j = 0; j <= particlesPerEdge; j++)
                {
                    float progress = j / (float)particlesPerEdge; // 粒子位置进度
                    Vector2 position = Vector2.Lerp(startPoint, endPoint, progress); // 插值计算粒子位置
                    Vector2 velocity = (position - center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f); // 粒子辐射速度

                    // 创建黄色粒子
                    Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, velocity, 0, default, Main.rand.NextFloat(2.2f, 2.5f));
                    dust.noGravity = true; // 无重力
                    dust.fadeIn = 0.7f; // 渐入效果
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机旋转
                }
            }
        }


    }
}