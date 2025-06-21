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
        private bool canDash = false;
        private float orbitAngle = 0f;
        private Vector2 orbitOffset;
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
            Projectile.alpha = 255;
        }
        //public void DashToPosition(Vector2 targetPosition)
        //{
        //    if (!isDashing)
        //    {
        //        dashTarget = targetPosition; // 保存初次冲刺的目标位置
        //        Vector2 dashDirection = (dashTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * 25f;
        //        Projectile.velocity = dashDirection;
        //        isDashing = true; // 设置为冲刺状态
        //        canDealDamage = true; // 允许造成伤害
        //        Projectile.timeLeft = 600; // 设置冲刺后的存活时间为 600 帧
        //        SoundEngine.PlaySound(SupremeCalamitas.BrimstoneShotSound, Projectile.Center);

        //        // 生成正方形粒子效果
        //        //for (int i = 0; i < 4; i++)
        //        //{
        //        //    // 计算正方形的四个顶点角度，每个顶点相隔90度
        //        //    float angle = MathHelper.PiOver4 + i * MathHelper.PiOver2; // 45度起始角，每90度一个顶点
        //        //    float nextAngle = MathHelper.PiOver4 + (i + 1) * MathHelper.PiOver2;

        //        //    // 缩小正方形的边长为原等边三角形边长的一半
        //        //    Vector2 start = angle.ToRotationVector2() * (16f / 2f); // 缩小到一半大小，原始16f长度调整
        //        //    Vector2 end = nextAngle.ToRotationVector2() * (16f / 2f);

        //        //    for (int j = 0; j < 40; j++)
        //        //    {
        //        //        Dust squareDust = Dust.NewDustPerfect(Projectile.Center, 267);
        //        //        squareDust.scale = 2.5f;
        //        //        squareDust.velocity = Vector2.Lerp(start, end, j / 40f) * 2f;
        //        //        squareDust.color = Color.Crimson;
        //        //        squareDust.noGravity = true;
        //        //    }
        //        //}

        //        // 生成完整圆形粒子效果
        //        int totalPoints = 160; // 圆上的总粒子数量，可根据需要调整
        //        float radius = 16f / 2f; // 圆的半径，与原正方形的半径一致
        //        for (int i = 0; i < totalPoints; i++)
        //        {
        //            // 按角度均匀分布粒子
        //            float angle = MathHelper.TwoPi * (i / (float)totalPoints); // 计算粒子的角度
        //            Vector2 direction = angle.ToRotationVector2(); // 将角度转换为方向向量
        //            Vector2 position = Projectile.Center + direction * radius; // 粒子位置在圆周上

        //            // 创建粒子
        //            Dust circleDust = Dust.NewDustPerfect(position, 267);
        //            circleDust.scale = 2.5f;
        //            circleDust.velocity = direction * 2f; // 粒子的初始速度沿着圆周的切线方向
        //            circleDust.color = Color.Crimson;
        //            circleDust.noGravity = true;
        //        }


        //    }
        //}
        private bool isTracking = false;
        public void DashToPosition(Vector2 targetPosition)
        {
            if (!isDashing)
            {
                dashTarget = targetPosition;
                Vector2 dashDirection = (dashTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * 25f;
                Projectile.velocity = dashDirection;
                isDashing = true;
                isTracking = true; // 进入追踪模式
                canDealDamage = true;
                Projectile.timeLeft = 600; // 设定冲刺存活时间
                SoundEngine.PlaySound(SupremeCalamitas.BrimstoneShotSound, Projectile.Center);

                GenerateDashEffects();
            }
        }
        private bool hasFired = false; // 记录是否已经开火
        private int fireCooldown = 0; // 开火冷却时间

        // 在小鸟接收到开火命令时调用
        public void ReceiveFireOrder(int damage)
        {
            if (Projectile.ai[0] >= 40 && fireCooldown <= 0) // 仅在椭圆运动期间允许开火，且冷却时间结束
            {
                FireProjectile(damage); // 执行发射逻辑
                fireCooldown = 30; // 设置冷却时间为 30 帧
            }
        }

        // 具体的开火逻辑
        private void FireProjectile(int damage)
        {
            Vector2 fireDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
            Vector2 fireVelocity = fireDirection * 12f; // 设定弹幕的初速度

            // 生成 SoulSeekerJavBRIDFire，伤害倍率 1.0x
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireVelocity, ModContent.ProjectileType<SoulSeekerJavBRIDFire>(), damage, 0f, Projectile.owner);
        }
        public override void AI()
        {
            if (fireCooldown > 0)
            {
                fireCooldown--; // 每帧减少冷却时间
            }

            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] < 40) // 第一阶段
            {
                if (Projectile.ai[0] < 20)
                    Projectile.alpha = Math.Max(Projectile.alpha - 10, 0); // 透明度降低
                else
                    Projectile.alpha = Math.Min(Projectile.alpha + 10, 255); // 透明度增加

                Projectile.ai[0]++;
            }
            else // 第二阶段
            {
                if (Projectile.alpha > 1)
                    Projectile.alpha = Math.Max(Projectile.alpha - 4, 1); // 透明度逐步降低

                // 确保小鸟的椭圆轨迹只初始化一次
                if (Projectile.localAI[0] == 0)
                {
                    orbitOffset = new Vector2(Main.rand.NextFloat(20 * 16, 25 * 16), Main.rand.NextFloat(8 * 16, 10 * 16));
                    Projectile.localAI[0] = 1; // 设为非 0 值，确保仅初始化一次
                }

                // 如果未开启追踪模式，继续围绕玩家运行
                if (!isTracking)
                {
                    // 角速度恒定，绝对速度不恒定来平衡
                    //orbitAngle += 0.02f;
                    //Vector2 ellipticalPosition = player.Center + new Vector2((float)Math.Cos(orbitAngle) * orbitOffset.X, (float)Math.Sin(orbitAngle) * orbitOffset.Y);

                    //Vector2 directionToPosition = ellipticalPosition - Projectile.Center;
                    //float distance = directionToPosition.Length();
                    //if (distance > 2f)
                    //{
                    //    directionToPosition.Normalize();
                    //    Projectile.velocity = directionToPosition * 8f;
                    //}
                    //else
                    //{
                    //    Projectile.velocity *= 0.95f;
                    //}

                    // 绝对速度恒定
                    // 计算当前角度的瞬时半径
                    float a = orbitOffset.X; // 长轴
                    float b = orbitOffset.Y; // 短轴
                    float instantaneousRadius = (a * b) / MathF.Sqrt((b * b * MathF.Cos(orbitAngle) * MathF.Cos(orbitAngle)) + (a * a * MathF.Sin(orbitAngle) * MathF.Sin(orbitAngle)));

                    // 计算角度增量，使得小鸟在椭圆上运动的弧长保持恒定
                    float stepSize = 8f / instantaneousRadius; // 8f 是目标线速度

                    // 角度变化
                    orbitAngle += stepSize;

                    // 计算新的椭圆位置
                    Vector2 ellipticalPosition = player.Center + new Vector2((float)Math.Cos(orbitAngle) * a, (float)Math.Sin(orbitAngle) * b);
                    Vector2 directionToPosition = ellipticalPosition - Projectile.Center;
                    float distance = directionToPosition.Length();

                    if (distance > 2f)
                    {
                        directionToPosition.Normalize();
                        Projectile.velocity = directionToPosition * 8f; // 保持恒定速度
                    }
                    else
                    {
                        Projectile.velocity *= 0.95f;
                    }
                }
                else
                {
                    // **开启追踪模式后，小鸟会自动锁定最近的敌人**
                    NPC target = Projectile.Center.ClosestNPCAt(2800);
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 20f, 0.08f);
                    }
                }

                canDash = true; // 只有第二阶段才能冲刺
            }


            // 右键冲刺逻辑（仅限第二阶段）
            if (canDash && Main.mouseRight && Main.myPlayer == Projectile.owner)
            {
                DashToPosition(Main.MouseWorld);
            }


            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
            {
                Projectile.frame = 0;
            }
        }




        private void GenerateDashEffects()
        {
            int totalPoints = 160;
            float radius = 16f;
            for (int i = 0; i < totalPoints; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)totalPoints);
                Vector2 direction = angle.ToRotationVector2();
                Vector2 position = Projectile.Center + direction * radius;

                Dust circleDust = Dust.NewDustPerfect(position, 267);
                circleDust.scale = 2.5f;
                circleDust.velocity = direction * 2f;
                circleDust.color = Color.Crimson;
                circleDust.noGravity = true;
            }
        }

        public override bool? CanDamage() => canDealDamage;

        public override void OnKill(int timeLeft)
        {
            int particleCount = Main.rand.Next(5, 8);
            for (int i = 0; i < particleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 particleDirection = randomAngle.ToRotationVector2();
                float speed = Main.rand.NextFloat(2f, 4f);
                Vector2 particleVelocity = particleDirection * speed;

                Vector2 spawnPosition = Projectile.Center;
                float radius = Main.rand.NextFloat(24f, 48f);

                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 60);
        }
    }
}