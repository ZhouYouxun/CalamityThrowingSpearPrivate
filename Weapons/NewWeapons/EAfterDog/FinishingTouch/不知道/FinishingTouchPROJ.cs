//using CalamityMod;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod.Particles;
//using CalamityMod.Projectiles.Typeless;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria.DataStructures;
//using Terraria.GameContent;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    internal class FinishingTouchPROJ : ModProjectile
//    {
//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
//            Main.projFrames[Projectile.type] = 4; // 设置投射物的帧数为 4
//        }

//        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
//        {
//            SpriteBatch spriteBatch = Main.spriteBatch;
//            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

//            // 计算当前动画帧
//            int frameCount = 4; // 总共 4 帧
//            int frameHeight = texture.Height / frameCount; // 每帧的高度
//            int currentFrame = (int)(Main.GameUpdateCount / 6 % frameCount); // 每 6 帧切换一次，总共 4 帧
//            Rectangle sourceRectangle = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);

//            // 设置绘制的原点和位置
//            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2); // 每帧的高度作为原点
//            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

//            // 绘制当前帧
//            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

//            return false;
//        }


//        public override void SetDefaults()
//        {
//            Projectile.width = 14;
//            Projectile.height = 14;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = 6; // 允许6次伤害
//            Projectile.timeLeft = 60;
//            Projectile.light = 0.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = true; // 允许与方块碰撞
//            Projectile.extraUpdates = 1; // 额外更新次数
//            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
//            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
//        }

//        public override void AI()
//        {
//            // 每 6 帧切换一次帧
//            if (++Projectile.frameCounter >= 6)
//            {
//                Projectile.frameCounter = 0; // 重置帧计数器
//                Projectile.frame++; // 切换到下一帧
//                if (Projectile.frame >= Main.projFrames[Projectile.type])
//                {
//                    Projectile.frame = 0; // 如果超过了最大帧数，回到第一帧
//                }
//            }

//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
//            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);
//            Projectile.velocity *= 1.001f;

//            // 刚出现时的初始粒子特效
//            if (Projectile.timeLeft == 180) // Assuming timeLeft is initially 180
//            {
//                GenerateInitialParticles();
//            }

//            //// 每隔 60 帧生成一次火球和粒子特效
//            //Projectile.ai[0]++;
//            //if (Projectile.ai[0] >= 60)
//            //{
//            //    ReleaseFireballs();
//            //    ReleaseLinearParticles();
//            //    Projectile.ai[0] = 0; // 重置计数
//            //}
//        }


//        private void GenerateInitialParticles()
//        {
//            for (float angle = -15f; angle <= 15f; angle += 1f)
//            {
//                Vector2 particleDirectionLeft = Projectile.velocity.RotatedBy(MathHelper.ToRadians(angle));
//                Vector2 particleDirectionRight = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-angle));

//                // 左右方向各释放粒子
//                Particle particleLeft = new SparkParticle(Projectile.Center, particleDirectionLeft * 3f, false, 40, 1.5f, Color.OrangeRed);
//                Particle particleRight = new SparkParticle(Projectile.Center, particleDirectionRight * 3f, false, 40, 1.5f, Color.OrangeRed);

//                GeneralParticleHandler.SpawnParticle(particleLeft);
//                GeneralParticleHandler.SpawnParticle(particleRight);
//            }
//        }

//        private void ReleaseFireballs()
//        {
//            int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
//            float baseAngle = MathHelper.TwoPi / 40; // 每个火球的角度

//            for (int i = 0; i < 40; i++)
//            {
//                Vector2 fireballVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 8.5f; // 初始速度为原来的8.5倍
//                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
//            }
//        }


//        private void ReleaseLinearParticles()
//        {
//            float baseAngle = MathHelper.TwoPi / 20; // 20个粒子的扩散角度

//            for (int i = 0; i < 20; i++)
//            {
//                Vector2 trailPos = Projectile.Center;
//                Vector2 trailVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 0.2f;
//                Color trailColor = Color.OrangeRed;
//                float trailScale = 1.5f;

//                Particle trail = new SparkParticle(trailPos, trailVelocity, false, 60, trailScale, trailColor);
//                GeneralParticleHandler.SpawnParticle(trail);
//            }
//        }

//        public override void OnKill(int timeLeft)
//        {
//            ReleaseFireballs();
//            ReleaseLinearParticles();
//            // 释放爆炸弹幕
//            int explosionType = ModContent.ProjectileType<FuckYou>();
//            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, explosionType, Projectile.damage, Projectile.knockBack, Projectile.owner);
//        }



//    }
//}

