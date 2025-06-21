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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd
{
    public class FestiveHalberdPROJ : ModProjectile, ILocalizedModType
    {
        //public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/FestiveHalberd/FestiveHalberd";

        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 背光效果参数
            float chargeOffset = 6f; // 控制充能效果扩散的偏移量
            float segmentWidth = 10f; // 每段颜色切换间隔的像素点数
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 确保旋转方向同步

            // 绘制红白相间的背光效果
            for (int i = 0; i < 16; i++) // 绘制16次圆周效果
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * chargeOffset;

                // 根据偏移位置计算红白间隔
                Color backlightColor = (i * chargeOffset) % (2 * segmentWidth) < segmentWidth ? Color.Red * 0.5f : Color.White * 0.6f;
                backlightColor.A = 0;

                Main.spriteBatch.Draw(
                    texture,
                    drawPosition + drawOffset,
                    null,
                    backlightColor,
                    rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色改为浅红色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.LightCoral.ToVector3() * 0.55f);

            // 受到重力影响，但是会逐渐对抗重力
            //Projectile.velocity.Y -= 0.1f;


            // 为箭矢本体后面添加光束特效
            //if (Projectile.numUpdates % 3 == 0)
            //{
            //    Color outerSparkColor = new Color(255, 69, 0); // 将颜色改为橙红色
            //    float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
            //    float outerSparkScale = 1.2f + scaleBoost;
            //    SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
            //    GeneralParticleHandler.SpawnParticle(spark);
            //}

            // 每隔15帧射出一个往正上方的弹幕
            if (Projectile.timeLeft % 15 == 0)
            {
                Vector2 upwardDirection = new Vector2(0, -1); // 绝对正上方
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    upwardDirection * (Projectile.velocity.Length() * 1.6f), // 速度为弹幕速度的1.6倍
                    335,
                    (int)(Projectile.damage * 0.625f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 烟雾特效往正上方喷射
                int smokeCount = Main.rand.Next(15, 21); // 随机生成15~20个
                for (int i = 0; i < smokeCount; i++)
                {
                    Vector2 smokeVelocity = new Vector2(0, Main.rand.NextFloat(-4f, -8f)); // 随机向正上方喷射
                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center,
                        smokeVelocity,
                        Color.LightYellow,
                        18,
                        Main.rand.NextFloat(0.9f, 1.6f),
                        0.35f,
                        Main.rand.NextFloat(-1, 1),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        // 在OnKill函数中处理死亡逻辑
        public override void OnKill(int timeLeft)
        {
            // PineNeedleFriendly发射逻辑
            int pineNeedleCount = Main.rand.Next(6, 11); // 随机生成6~10个弹幕
            for (int i = 0; i < pineNeedleCount; i++)
            {
                // 在绝对正上方左右各20度内随机选择一个角度
                float angle = MathHelper.ToRadians(-20 + Main.rand.NextFloat(40));
                Vector2 absoluteUpward = new Vector2(0, -1).RotatedBy(angle); // 绝对正上方为基准的随机角度
                Vector2 velocity = absoluteUpward * Main.rand.NextFloat(7.1f, 16.9f); // 随机速度

                // 创建 PineNeedleFriendly 弹幕
                int pineNeedle = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ProjectileID.PineNeedleFriendly,
                    (int)(Projectile.damage * Main.rand.NextFloat(0.25f, 0.55f)), // 保留伤害倍率
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 设置自定义数值
                Projectile proj = Main.projectile[pineNeedle];
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = 3; // 设置穿透次数
                proj.localNPCHitCooldown = 60; // 设置局部无敌时间
                proj.usesLocalNPCImmunity = true; // 启用局部无敌帧
                proj.timeLeft = 120; // 弹幕存活时间
            }

            // OrnamentFriendly发射逻辑
            int ornamentCount = Main.rand.Next(15, 21);
            for (int i = 0; i < ornamentCount; i++)
            {
                Vector2 randomPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-280, 280), Main.rand.NextFloat(-280, 280));
                Vector2 outwardVelocity = Vector2.Normalize(randomPosition - Projectile.Center) * Main.rand.NextFloat(2f, 6f);
                int ornament = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    randomPosition,
                    outwardVelocity,
                    ProjectileID.OrnamentFriendly,
                    (int)(Projectile.damage * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner
                );
                Projectile proj = Main.projectile[ornament];
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = 3;
                proj.localNPCHitCooldown = 60;
                proj.usesLocalNPCImmunity = true;
                proj.timeLeft = 120;
            }

            // Present发射逻辑
            int presentCount = Main.rand.Next(3, 12);
            for (int i = 0; i < presentCount; i++)
            {
                Vector2 spawnCircle = Projectile.Center + Main.rand.NextVector2CircularEdge(880, 880); // 圆形随机生成
                Vector2 initialVelocity = Vector2.Normalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(30)) * Projectile.velocity.Length();
                int present = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnCircle,
                    initialVelocity,
                    ProjectileID.Present,
                    (int)(Projectile.damage * 0.25f),
                    Projectile.knockBack,
                    Projectile.owner
                );
                Projectile proj = Main.projectile[present];
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = 3;
                proj.localNPCHitCooldown = 20;
                proj.usesLocalNPCImmunity = true;
                proj.timeLeft = 360;
            }

            // FestiveHalberdSTAR发射逻辑
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                new Vector2(0, -1) * Projectile.velocity.Length(),
                ModContent.ProjectileType<FestiveHalberdSTAR>(),
                (int)(Projectile.damage * 4),
                Projectile.knockBack,
                Projectile.owner
            );

            // 调用粒子特效函数
            SpawnKillParticles();
        }

        private void SpawnKillParticles()
        {
            // 粒子大小倍率范围
            float minScale = 2.2f;
            float maxScale = 2.5f;

            // 棕色长方形（树干）边框绘制
            {
                // 树干宽度：3*16
                int width = 3 * 16;
                int height = (int)(15 * 16 * 0.6f);
                int pixelStep = 2; // 每隔2像素生成一个粒子

                for (int x = -width / 2; x <= width / 2; x += pixelStep)
                {
                    for (int y = 0; y <= height; y += pixelStep)
                    {
                        // 仅绘制边框，忽略中间填充
                        if (x == -width / 2 || x == width / 2 || y == 0 || y == height)
                        {
                            Vector2 position = Projectile.Center + new Vector2(x, y);
                            Vector2 velocity = (position - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f; // 辐射状加速度
                            Dust dust = Dust.NewDustPerfect(position, 7, velocity, 0, default, Main.rand.NextFloat(minScale, maxScale));
                            dust.noGravity = true; // 无重力
                            dust.fadeIn = 0.7f; // 渐入效果
                        }
                    }
                }
            }

            // 绿色三角形绘制
            {
                int[] widths = { 20 * 16, 16 * 16, 11 * 16, 7 * 16 }; // 各三角形的宽度
                float heightRatio = 0.7f; // 高度与宽度比例
                int[] offsets = { 0, -8 * 16, -18 * 16, -24 * 16 }; // 每个三角形底边相对于中心点的垂直偏移量（像素）
                int pixelStep = 2; // 每隔2像素生成一个粒子

                for (int i = 0; i < widths.Length; i++)
                {
                    int width = widths[i];
                    int height = (int)(width * heightRatio);
                    Vector2 basePosition = Projectile.Center + new Vector2(0, offsets[i]); // 控制底边位置
                    Vector2 topVertex = basePosition - new Vector2(0, height); // 当前三角形的顶点
                    Vector2 leftVertex = basePosition + new Vector2(-width / 2, 0); // 底边左点
                    Vector2 rightVertex = basePosition + new Vector2(width / 2, 0); // 底边右点

                    // 绘制三角形边框粒子
                    for (float t = 0; t <= 1f; t += (float)pixelStep / width)
                    {
                        // 左边到顶点
                        Vector2 leftToTop = Vector2.Lerp(leftVertex, topVertex, t);
                        SpawnGreenDust(leftToTop, minScale, maxScale);

                        // 顶点到右边
                        Vector2 topToRight = Vector2.Lerp(topVertex, rightVertex, t);
                        SpawnGreenDust(topToRight, minScale, maxScale);

                        // 左边到右边
                        Vector2 leftToRight = Vector2.Lerp(leftVertex, rightVertex, t);
                        SpawnGreenDust(leftToRight, minScale, maxScale);
                    }
                }
            }


            // 红色小圆圈粒子特效
            {
                int ornamentCount = Main.rand.Next(6, 8); // 随机生成6~7个圆圈
                float radius = 1 * 16; // 每个小圆圈的半径

                for (int i = 0; i < ornamentCount; i++)
                {
                    // 随机选择一个绿色三角形范围内的圆圈中心
                    Vector2 circleCenter = Projectile.Center + new Vector2(
                        Main.rand.Next(-8 * 16, 8 * 16),
                        Main.rand.Next(-24 * 16, -8 * 16)
                    );

                    // 绘制圆圈上的粒子
                    for (int j = 0; j < 10; j++) // 每个圆圈至少10个粒子
                    {
                        float angle = MathHelper.TwoPi / 10 * j; // 计算每个粒子的位置
                        Vector2 position = circleCenter + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                        Vector2 velocity = (position - circleCenter).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f); // 粒子扩散速度

                        Dust dust = Dust.NewDustPerfect(position, 130, velocity, 0, default, Main.rand.NextFloat(minScale, maxScale));
                        dust.noGravity = true; // 无重力
                        dust.fadeIn = 0.7f; // 渐入效果
                    }
                }
            }
        }

        // 生成绿色粒子方法
        private void SpawnGreenDust(Vector2 position, float minScale, float maxScale)
        {
            // 随机使用两种绿色 Dust
            int[] greenDustIDs = { 157, 107 };
            int selectedDustID = greenDustIDs[Main.rand.Next(0, greenDustIDs.Length)];

            Dust dust = Dust.NewDustPerfect(position, selectedDustID, Vector2.Zero, 0, default, Main.rand.NextFloat(minScale, maxScale));
            dust.noGravity = true; // 无重力
            dust.fadeIn = 0.7f; // 渐入效果
        }


    }
}