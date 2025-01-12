using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Particles;
using System;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
{
    public class GildedProboscisJavBIRD : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        private bool isDashing = false;
        private Vector2 dashTarget;
        private bool canDealDamage = false;
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type]; // 计算每帧的高度
            int frameY = Projectile.frame * frameHeight; // 获取当前帧在纹理中的起始Y坐标
            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight); // 定义绘制的源矩形
            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2); // 计算绘制的中心点

            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 判断图像是否需要左右翻转
            SpriteEffects spriteEffects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 绘制
            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0f);
            return false;
        }


        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        //// 定义一个包含敌方弹幕类型的黑名单数组
        //private static readonly int[] projectileBlacklist = new int[]
        //{
        //    ModContent.ProjectileType<InfernadoRevenge>(),
        //    // 后续可以添加更多的弹幕类型ID
        //};

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90000;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.25f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
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
            }
        }

        public override void AI()
        {
            if (!isDashing)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 idlePosition = player.Center;

                // 给弹幕添加一个随机的偏移量，使其在玩家周围自由移动
                idlePosition.X += Main.rand.NextFloat(-1250f, 1250f);
                idlePosition.Y += Main.rand.NextFloat(-1200f, 1200f);

                Vector2 directionToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = directionToIdlePosition.Length();

                // 如果距离过大，逐步加速朝向玩家移动
                if (distanceToIdlePosition > 600f)
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
                //// 在冲刺状态下，与敌人的弹幕发生接触时摧毁彼此
                //for (int i = 0; i < Main.maxProjectiles; i++)
                //{
                //    Projectile otherProj = Main.projectile[i];
                //    if (otherProj.active && otherProj.hostile && otherProj.Hitbox.Intersects(Projectile.Hitbox))
                //    {
                //        // 检查是否为特定的敌人弹幕类型（黑名单）
                //        if (Array.IndexOf(projectileBlacklist, otherProj.type) == -1)
                //        {
                //            otherProj.Kill(); // 销毁敌方弹幕
                //            isDashing = false; // 只在冲刺期间销毁一个弹幕后停止
                //            break;
                //        }
                //    }
                //}
            }

            // 原有帧切换逻辑
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 1)
            {
                Projectile.frame = 0;
            }

            // 一旦冲刺开始，那么就会每隔30帧生成：尖刺型“叉叉”特效，间隔90度生成
            if (isDashing && Projectile.timeLeft % 60 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.PiOver2 * i; // 每个尖刺间隔90度（PiOver2即90度）
                    Vector2 spikeVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                    PointParticle spike = new PointParticle(Projectile.Center, spikeVelocity, false, 10, 2f, Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spike);
                }
            }
        }
        public override bool? CanDamage() => canDealDamage;


        public override void OnKill(int timeLeft)
        {
            //// 橙色和红色粒子特效
            //for (int i = 0; i < 20; i++)
            //{
            //    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
            //    Vector2 particleVelocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(0.8f, 1.2f);
            //    PointParticle spark = new PointParticle(Projectile.Center, particleVelocity, false, 7, 1.5f, Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat()));
            //    GeneralParticleHandler.SpawnParticle(spark);
            //}

            // 尖刺型“叉叉”特效，间隔90度生成
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i; // 每个尖刺间隔90度（PiOver2即90度）
                Vector2 spikeVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                PointParticle spike = new PointParticle(Projectile.Center, spikeVelocity, false, 10, 2f, Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spike);
            }

            SoundEngine.PlaySound(SoundID.NPCHit51, Projectile.Center);

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
        }


    }
}
