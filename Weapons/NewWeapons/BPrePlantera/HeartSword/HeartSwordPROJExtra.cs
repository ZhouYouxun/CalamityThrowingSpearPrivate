using CalamityMod.Particles;
using CalamityMod;
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

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword
{
    public class HeartSwordPROJExtra : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/HeartSword/HeartSword";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!Main.player[Projectile.owner].channel) // 检查玩家是否松开鼠标左键（进入冲刺阶段）
            {
                // 绘制投射物的拖尾效果，提供动态的视觉反馈
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

                // 添加刀刃亮光效果
                Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value; // 获取用于亮光的半星纹理
                Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale; // 设置亮光的比例
                shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);
                // 根据时间和弹幕ID计算亮光的动态变化，使亮光看起来有轻微的呼吸效果

                Vector2 lensFlareWorldPosition = Projectile.Center; // 将亮光的位置设置为投射物中心
                Color lensFlareColor = Color.Lerp(Color.Red, Color.Orange, 0.23f) with { A = 0 };
                // 设置亮光颜色为红色和橙色之间的渐变，同时将透明度设为0，确保亮光柔和

                // 绘制横向的亮光
                Main.EntitySpriteDraw(
                    shineTex,
                    lensFlareWorldPosition - Main.screenPosition, // 计算屏幕空间中的亮光位置
                    null, // 不指定具体区域，使用完整纹理
                    lensFlareColor, // 使用上面计算的渐变颜色
                    0f, // 无旋转
                    shineTex.Size() * 0.5f, // 设置亮光的原点为纹理中心
                    shineScale * 0.6f, // 缩放比例，略小于纵向亮光
                    0, // 不使用镜像效果
                    0 // 层级设置为0
                );

                // 绘制纵向的亮光
                Main.EntitySpriteDraw(
                    shineTex,
                    lensFlareWorldPosition - Main.screenPosition, // 同样的屏幕空间位置
                    null,
                    lensFlareColor,
                    MathHelper.PiOver2, // 旋转90度以形成十字亮光
                    shineTex.Size() * 0.5f,
                    shineScale, // 使用稍大的缩放比例
                    0,
                    0
                );
                return false; // 终止后续默认的绘制行为，确保仅显示自定义特效
            }
            else // 非冲刺阶段的特效逻辑
            {
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; // 获取投射物的纹理
                Vector2 origin = texture.Size() * 0.5f; // 设置纹理的原点为中心
                Vector2 drawPosition = Projectile.Center - Main.screenPosition; // 计算投射物在屏幕上的绘制位置

                float chargeOffset = 3f; // 控制充能效果的扩散距离
                Color chargeColor = Color.White * 0.6f; // 充能效果颜色为半透明的白色
                chargeColor.A = 0; // 设置透明度为0，避免影响实际纹理的渲染
                SpriteEffects direction = SpriteEffects.None; // 不启用翻转效果

                // 绘制充能的光环效果
                for (int i = 0; i < 8; i++) // 在圆周上均匀分布8个光点
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                    // 通过旋转计算每个光点的偏移位置，形成一个圆环
                    Main.spriteBatch.Draw(
                        texture,
                        drawPosition + drawOffset, // 偏移后的实际绘制位置
                        null,
                        chargeColor, // 充能光环的颜色
                        Projectile.rotation, // 以投射物的旋转角度绘制
                        origin,
                        Projectile.scale, // 使用投射物的缩放比例
                        direction,
                        0f
                    );
                }

                // 渲染实际的投射物
                Main.spriteBatch.Draw(
                    texture,
                    drawPosition, // 不偏移，绘制在中心
                    null,
                    Projectile.GetAlpha(lightColor), // 获取经过光照处理后的颜色
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    direction,
                    0f
                );

                return false; // 终止后续默认的绘制行为
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 25; // 无敌帧冷却时间为25帧
        }

        //public override void AI()
        //{
        //    Player player = Main.player[Projectile.owner];

        //    // 旋转阶段逻辑
        //    if (player.channel)
        //    {
        //        float rotationSpeed = 0.05f;
        //        float radius = 10 * 16f;
        //        Projectile.ai[1] += rotationSpeed;
        //        Projectile.Center = player.Center + new Vector2((float)Math.Cos(Projectile.ai[1] + Projectile.ai[0]), (float)Math.Sin(Projectile.ai[1] + Projectile.ai[0])) * radius;

        //        // 实时指向鼠标
        //        Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation() + MathHelper.PiOver4;

        //        // 无限穿透，刷新时间
        //        Projectile.penetrate = -1;
        //        Projectile.timeLeft = 300;
        //    }
        //    else // 冲刺阶段逻辑
        //    {
        //        if (Projectile.penetrate == -1) // 切换阶段时执行一次性逻辑
        //        {
        //            Projectile.penetrate = 1; // 改为单次穿透
        //            Projectile.timeLeft = 300; // 固定时间
        //            Projectile.tileCollide = true; // 启用与方块碰撞

        //            // 设置冲刺方向和速度
        //            float speed = 30f;
        //            Projectile.velocity = Vector2.Normalize(Main.MouseWorld - Projectile.Center) * speed;
        //        }
        //    }
        //}

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 如果已经进入冲刺阶段
            if (Projectile.localAI[0] == 1)
            {
                DoDashBehavior();

                // === 冲刺特效（保证会执行） ===
                float time = Projectile.localAI[1]++ * 0.35f;

                // 螺旋辉光球轨迹
                Vector2 spiralOffset = new Vector2(
                    (float)Math.Sin(time * 0.9f) * 14f,
                    (float)Math.Cos(time * 0.45f) * 9f
                );
                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center + spiralOffset,
                    Vector2.Zero,
                    false,
                    20,
                    0.6f,
                    Color.Lerp(Color.DarkRed, Color.OrangeRed, (float)Math.Sin(time) * 0.5f + 0.5f),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);

                // 暗淡丝线轨迹
                AltSparkParticle spark = new AltSparkParticle(
                    Projectile.Center - Projectile.velocity * 1.5f,
                    Projectile.velocity * 0.01f,
                    false,
                    8,
                    1.2f,
                    Color.Cyan * 0.13f
                );
                GeneralParticleHandler.SpawnParticle(spark);

                return; // ✅ 冲刺阶段直接结束
            }

            // 检查是否切换到冲刺阶段
            if (!player.channel)
            {
                Projectile.localAI[0] = 1;
                DoDashBehavior();
                return;
            }

            // 旋转阶段逻辑
            DoOrbitBehavior(player);
        }



        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closest = null;
            float closestDist = maxDetectDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this) && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        private void DoOrbitBehavior(Player player)
        {
            float rotationSpeed = 0.05f; // 旋转速度
            float radius = 10 * 16f; // 半径
            Projectile.ai[1] += rotationSpeed; // 更新角度
            Projectile.Center = player.Center + new Vector2((float)Math.Cos(Projectile.ai[1] + Projectile.ai[0]), (float)Math.Sin(Projectile.ai[1] + Projectile.ai[0])) * radius;

            // 实时指向鼠标
            Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation() + MathHelper.PiOver4;

            // 无限穿透，刷新时间
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }

        private void DoDashBehavior()
        {
            if (Projectile.penetrate == -1) // 切换到冲刺阶段时的初始化逻辑
            {
                Projectile.penetrate = 1; // 改为单次穿透
                Projectile.timeLeft = 300; // 固定时间
                Projectile.tileCollide = true; // 启用与方块碰撞

                // 设置冲刺方向和速度
                float speed = 30f;
                Projectile.velocity = Vector2.Normalize(Main.MouseWorld - Projectile.Center) * speed;


 
            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        // 创建三角形粒子特效
        private void CreateTriangleParticles(Vector2 center, Vector2 direction)
        {
            int pointsPerEdge = 10; // 每条边的粒子数量
            float triangleSize = 50f; // 三角形边长
            Vector2 normalizedDirection = Vector2.Normalize(direction);

            // 计算三角形的三个顶点
            Vector2[] vertices = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                float angleOffset = MathHelper.TwoPi / 3 * i; // 每个顶点的角度
                vertices[i] = center + normalizedDirection.RotatedBy(angleOffset) * triangleSize;
            }

            // 为每条边生成粒子
            for (int i = 0; i < 3; i++)
            {
                Vector2 start = vertices[i];
                Vector2 end = vertices[(i + 1) % 3];
                Vector2 edgeDirection = Vector2.Normalize(end - start);
                float edgeLength = Vector2.Distance(start, end);

                for (int j = 0; j <= pointsPerEdge; j++)
                {
                    float progress = (float)j / pointsPerEdge; // 当前粒子在边上的进度
                    Vector2 position = Vector2.Lerp(start, end, progress); // 插值计算粒子位置
                    Vector2 velocity = edgeDirection * 2f; // 粒子速度

                    // 生成三种粒子
                    CreateParticle(position, velocity, DustID.GemRuby, Color.Red);
                    CreateParticle(position, velocity, DustID.LifeCrystal, Color.Pink);
                    CreateParticle(position, velocity, DustID.Blood, Color.DarkRed);
                }
            }
        }

        // 通用的粒子创建方法
        private void CreateParticle(Vector2 position, Vector2 velocity, int dustID, Color color)
        {
            Dust dust = Dust.NewDustPerfect(position, dustID, velocity, 100, color, 1.5f);
            dust.noGravity = true; // 无重力粒子
            dust.fadeIn = 1.2f; // 粒子淡入效果
        }


        public override void OnKill(int timeLeft)
        {
            // 血雾爆炸
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, Main.rand.Next(-3, 3), Main.rand.Next(-3, 3), 100, default, 1.5f);
            }

            // 原有三角 dust
            CreateTriangleParticles(Projectile.Center, Projectile.velocity);

            // 顶点 GlowOrb 强化符纹感
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 50f;
                GlowOrbParticle rune = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    30,
                    0.9f,
                    Color.DarkRed,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(rune);
            }
        }


        //public override bool PreDraw(ref Color lightColor)
        //{
        //    if (!Main.player[Projectile.owner].channel) // 冲刺阶段特效
        //    {
        //        CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
        //        // 添加刀刃亮光效果
        //        Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
        //        Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
        //        shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

        //        Vector2 lensFlareWorldPosition = Projectile.Center;
        //        Color lensFlareColor = Color.Lerp(Color.Red, Color.Orange, 0.23f) with { A = 0 };
        //        Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
        //        Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);
        //        return false;
        //    }
        //    else // 非冲刺阶段特效
        //    {
        //        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        //        Vector2 origin = texture.Size() * 0.5f;
        //        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        //        float chargeOffset = 3f;
        //        Color chargeColor = Color.White * 0.6f;
        //        chargeColor.A = 0;
        //        SpriteEffects direction = SpriteEffects.None;

        //        for (int i = 0; i < 8; i++)
        //        {
        //            Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
        //            Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, Projectile.rotation, origin, Projectile.scale, direction, 0f);
        //        }

        //        Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0f);
        //        return false;
        //    }
        //}
    }
}