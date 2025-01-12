using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav
{
    public class SoulSeekerJavBRID : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private bool isDashing = false;
        private Vector2 dashTarget;
        private bool canDealDamage = false;


        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type]; // 计算每帧的高度
            int frameY = Projectile.frame * frameHeight; // 获取当前帧在纹理中的起始Y坐标
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight); // 定义绘制的源矩形
            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2); // 计算绘制的中心点

            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 判断图像是否需要翻转
            SpriteEffects spriteEffects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 绘制
            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0f);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90000;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 80; // 无敌帧冷却时间为x帧
        }
        public void DashToPosition(Vector2 targetPosition)
        {
            if (!isDashing)
            {
                dashTarget = targetPosition; // 保存初次冲刺的目标位置
                Vector2 dashDirection = (dashTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * 25f;
                Projectile.velocity = dashDirection;
                isDashing = true; // 设置为冲刺状态
                canDealDamage = true; // 允许造成伤害
                Projectile.timeLeft = 600; // 设置冲刺后的存活时间为 600 帧
                SoundEngine.PlaySound(SupremeCalamitas.BrimstoneShotSound, Projectile.Center);

                // 生成正方形粒子效果
                //for (int i = 0; i < 4; i++)
                //{
                //    // 计算正方形的四个顶点角度，每个顶点相隔90度
                //    float angle = MathHelper.PiOver4 + i * MathHelper.PiOver2; // 45度起始角，每90度一个顶点
                //    float nextAngle = MathHelper.PiOver4 + (i + 1) * MathHelper.PiOver2;

                //    // 缩小正方形的边长为原等边三角形边长的一半
                //    Vector2 start = angle.ToRotationVector2() * (16f / 2f); // 缩小到一半大小，原始16f长度调整
                //    Vector2 end = nextAngle.ToRotationVector2() * (16f / 2f);

                //    for (int j = 0; j < 40; j++)
                //    {
                //        Dust squareDust = Dust.NewDustPerfect(Projectile.Center, 267);
                //        squareDust.scale = 2.5f;
                //        squareDust.velocity = Vector2.Lerp(start, end, j / 40f) * 2f;
                //        squareDust.color = Color.Crimson;
                //        squareDust.noGravity = true;
                //    }
                //}


                // 生成完整圆形粒子效果
                int totalPoints = 160; // 圆上的总粒子数量，可根据需要调整
                float radius = 16f / 2f; // 圆的半径，与原正方形的半径一致
                for (int i = 0; i < totalPoints; i++)
                {
                    // 按角度均匀分布粒子
                    float angle = MathHelper.TwoPi * (i / (float)totalPoints); // 计算粒子的角度
                    Vector2 direction = angle.ToRotationVector2(); // 将角度转换为方向向量
                    Vector2 position = Projectile.Center + direction * radius; // 粒子位置在圆周上

                    // 创建粒子
                    Dust circleDust = Dust.NewDustPerfect(position, 267);
                    circleDust.scale = 2.5f;
                    circleDust.velocity = direction * 2f; // 粒子的初始速度沿着圆周的切线方向
                    circleDust.color = Color.Crimson;
                    circleDust.noGravity = true;
                }


            }
        }

        public override void AI()
        {
            if (!isDashing)
            {

                Projectile.penetrate = -1; // 不断的设置自己为无限穿透

                Player player = Main.player[Projectile.owner];
                Vector2 idlePosition = player.Center;

                // 给弹幕添加一个随机的偏移量，使其在玩家周围自由移动
                idlePosition.X += Main.rand.NextFloat(-2000f, 2000f);
                idlePosition.Y += Main.rand.NextFloat(-2000f, 2000f);

                Vector2 directionToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = directionToIdlePosition.Length();

                // 如果距离过大，逐步加速朝向玩家移动
                if (distanceToIdlePosition > 1200f)
                {
                    // 归一化方向向量
                    directionToIdlePosition.Normalize();
                    float maxSpeed = 20f; // 设定一个上限速度
                    float accelerationFactor = 0.5f; // 控制加速度的因子，可以调整以使其更平滑
                    Vector2 acceleration = directionToIdlePosition * accelerationFactor;

                    // 逐步增加速度，限制最大速度
                    Projectile.velocity += acceleration;
                    if (Projectile.velocity.Length() > maxSpeed)
                    {
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
                    }
                }
                else if (distanceToIdlePosition > 20f)
                {
                    // 轻微调整位置，模拟绕玩家自由运动
                    directionToIdlePosition.Normalize();
                    Projectile.velocity = (Projectile.velocity * (30f - 1) + directionToIdlePosition * 8f) / 30f;
                }
                else
                {
                    // 保持位置不动，但稍微调整运动，防止完全静止
                    Projectile.velocity *= 0.96f;
                    Projectile.velocity += new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                }

                // 在盘旋状态下，不允许造成伤害
                canDealDamage = false;
            }

            else
            {
                Projectile.penetrate = 1; // 一旦开始冲刺，那么就只能穿透一次了

                // 在冲刺期间追踪敌人
                NPC target = Projectile.Center.ClosestNPCAt(1200); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 25f, 0.08f); // 追踪速度为25f
                }

            }

            // 修改后的帧切换逻辑，适用于六帧动图
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4) // 调整这个值可以控制帧切换的速度
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5) // 六帧动图时，最大帧数为 5
            {
                Projectile.frame = 0;
            }

            if (isDashing && Projectile.timeLeft % 60 == 0)
            {
                // 获取反向的方向向量（与冲刺方向相反）
                Vector2 oppositeDirection = -Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // 设置扇形角度范围（以弧度表示，例如30度）
                float coneAngle = MathHelper.ToRadians(30f);
                int particleCount = 40; // 粒子数量

                //// 生成粒子形成扇形
                //for (int i = 0; i < particleCount; i++)
                //{
                //    // 计算粒子的随机角度偏移
                //    float randomAngle = MathHelper.Lerp(-coneAngle / 2, coneAngle / 2, i / (float)particleCount);
                //    Vector2 particleDirection = oppositeDirection.RotatedBy(randomAngle); // 旋转方向形成扇形分布
                //    Vector2 particleVelocity = particleDirection * Main.rand.NextFloat(4f, 8f); // 设置粒子的速度，带一定的随机性

                //    // 生成粒子并设置属性
                //    Dust starDust = Dust.NewDustPerfect(Projectile.Center, 267); // 替换为合适的粒子类型
                //    starDust.scale = Main.rand.NextFloat(1.5f, 2.5f); // 控制粒子缩放
                //    starDust.velocity = particleVelocity; // 设置粒子的速度方向
                //    starDust.color = Color.Crimson; // 设置颜色，可以根据需要调整
                //    starDust.noGravity = true; // 取消重力影响
                //}
            }

        }

        // 只有冲刺期间才能造成伤害
        public override bool? CanDamage() => canDealDamage;


        public override void OnKill(int timeLeft)
        {
            // 随机生成2到3个方向
            int particleCount = Main.rand.Next(5, 8); // 随机生成5到7个粒子
            for (int i = 0; i < particleCount; i++)
            {
                // 随机角度生成粒子运动方向
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 0到2π范围内的随机角度
                Vector2 particleDirection = randomAngle.ToRotationVector2(); // 转换为单位向量方向

                // 设置粒子的速度和随机化
                float speed = Main.rand.NextFloat(2f, 4f); // 速度范围在2到4之间
                Vector2 particleVelocity = particleDirection * speed; // 计算粒子速度

                // 生成自定义烟雾粒子
                Vector2 spawnPosition = Projectile.Center; // 粒子生成位置
                float radius = Main.rand.NextFloat(24f, 48f); // 随机设置粒子大小

                // 使用 GruesomeMetaball 生成粒子
                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }

            // 保持原来的音效
            //SoundEngine.PlaySound(SoundID.NPCDeath2, Projectile.Center);


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 60); // 孱弱巫咒
        }





    }
}