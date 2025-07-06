//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ModLoader;
//using Terraria.Graphics.Shaders;
//using Terraria.Audio;
//using CalamityMod.Particles;
//using CalamityMod;
//using Microsoft.Xna.Framework.Graphics;
//using CalamityMod.Projectiles.BaseProjectiles;
//using CalamityMod.Items.Weapons.Ranged;
//using Terraria.ID;
//using CalamityMod.Projectiles.Magic;
//using CalamityMod.Projectiles.Melee;
//using CalamityMod.Projectiles.Typeless;
//using CalamityMod.Sounds;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
//using Terraria.DataStructures;
//using Terraria.GameContent;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    public class FinishingTouchDASH : ModProjectile
//    {
//        private const int MaxChargeTime = 180;
//        private const int releaseFireballInterval = 60;

//        private const float MinChargeSpeed = 30f;
//        private const float MaxChargeSpeed = 60f;

//        public Vector2 IdealVelocity;

//        private int fireballTimer = 0;

//        private int fireballReleaseCount = 0; // 火球释放计数器

//        private Vector2 lockedDirection; // 添加用于存储锁定方向的变量

//        public override void SetDefaults()
//        {
//            Projectile.width = 64;
//            Projectile.height = 64;
//            Projectile.friendly = false;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = MaxChargeTime + 60;
//            Projectile.tileCollide = true;
//            Projectile.netImportant = true;
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

//            Player owner = Main.player[Projectile.owner];
//            if (owner.dead || !owner.active)
//            {
//                Projectile.Kill();
//                return;
//            }


//            if (Projectile.velocity == Vector2.Zero)
//            {
//                // 对准鼠标方向并进行蓄力
//                //Projectile.rotation = Projectile.AngleTo(Main.MouseWorld) + MathHelper.PiOver4;
//                Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
//                if (Projectile.spriteDirection == -1)
//                    Projectile.rotation += MathHelper.PiOver2;
//                else
//                    Projectile.rotation += MathHelper.PiOver4;

//                //Projectile.Center = owner.MountedCenter + new Vector2(-Projectile.width / 2, 0f);
//                Projectile.Center = owner.MountedCenter;
//                //Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter, true) + Projectile.rotation.ToRotationVector2() * 30f;
//                owner.heldProj = Projectile.whoAmI;

//                // 蓄力期间不再释放三圈火球
//                //if (Projectile.ai[0] == 60 || Projectile.ai[0] == 120 || Projectile.ai[0] == 180)
//                //{
//                //    ReleaseFireballs();
//                //    ReleaseLinearParticles();
//                //}

//                //UpdatePlayerVisuals(owner); // 手动调用 UpdatePlayerVisuals


//                if (Projectile.ai[0] >= MaxChargeTime)
//                {
//                    StartLunge(owner);
//                }
//                else
//                {
//                    Projectile.ai[0]++;
//                }
//            }
//            else
//            {
//                //// 冲刺阶段，同步玩家速度和位置
//                //owner.velocity = Projectile.velocity;
//                //owner.Center = Projectile.Center;

//                // 动态调整冲刺方向，使其始终朝向鼠标（不再需要）
//                // Vector2 directionToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
//                // Projectile.velocity = directionToMouse * Projectile.velocity.Length(); // 保持当前速度大小

//                // 在冲刺期间持续更新旋转，以匹配蓄力阶段的效果（不再需要）
//                // Projectile.rotation = directionToMouse.ToRotation() + MathHelper.PiOver4;

//                Projectile.velocity = lockedDirection * Projectile.velocity.Length();

//                // 冲刺阶段，同步玩家速度和位置
//                owner.velocity = Projectile.velocity;
//                owner.Center = Projectile.Center;

//                // 不再每 15 帧生成一个新的 FinishingTouchBALL 弹幕
//                //fireballTimer++;
//                //if (fireballTimer >= 15)
//                //{
//                //    int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
//                //    Vector2 fireballVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f; // 设置火球速度
//                //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
//                //    fireballTimer = 0; // 重置计数器
//                //}

//                // 每隔18帧释放一次火球
//                fireballTimer++;
//                if (fireballTimer >= 18)
//                {
//                    ReleaseFireballs();
//                    fireballTimer = 0; // 重置计时器
//                }

//                // 每 60 帧释放粒子拖尾和烟雾
//                if (Projectile.ai[0] % 60 == 0)
//                {
//                    AddTrailParticles();
//                    AddSmokeParticles();
//                    AddHeavySmokeParticles();
//                }
//            }


//        }



//        private void AddHeavySmokeParticles()
//        {
//            // 烟雾粒子基本参数
//            Color smokeColor = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
//            float smokeSpeed = 5f; // 烟雾的初始速度
//            int smokeLifetime = 30; // 烟雾粒子的生存时间

//            // 计算烟雾粒子释放的基础方向（投射物的反方向）
//            Vector2 baseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero);

//            // 随机在 -15 度到 15 度之间变化
//            float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
//            Vector2 smokeVelocity = baseDirection.RotatedBy(randomAngle) * smokeSpeed;

//            // 生成重型烟雾粒子
//            Particle smoke = new HeavySmokeParticle(
//                Projectile.Center,
//                smokeVelocity,
//                smokeColor,
//                smokeLifetime,
//                Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f),
//                1.0f,
//                MathHelper.ToRadians(2f),
//                required: true
//            );

//            // 生成粒子
//            GeneralParticleHandler.SpawnParticle(smoke);
//        }



//        private void UpdatePlayerVisuals(Player owner)
//        {
//            Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter, true) + Projectile.rotation.ToRotationVector2() * 30f;

//            Projectile.direction = Projectile.spriteDirection = (Math.Cos(Projectile.rotation) > 0).ToDirectionInt();

//            owner.ChangeDir(Projectile.direction);
//            owner.heldProj = Projectile.whoAmI;
//            owner.itemTime = 2;
//            owner.itemAnimation = 2;
//            owner.itemRotation = Projectile.rotation;

//            Projectile.rotation += MathHelper.PiOver4;
//            if (Projectile.spriteDirection == -1)
//                Projectile.rotation += MathHelper.PiOver2;
//        }


//        private void UpdateHoldoutPosition(Player owner)
//        {
//            Vector2 ownerCenter = owner.RotatedRelativePoint(owner.MountedCenter, true);
//            Vector2 directionToMouse = Main.MouseWorld - ownerCenter;
//            directionToMouse.Normalize();

//            Projectile.Center = ownerCenter + directionToMouse * 40f;
//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 保持旋转
//            Projectile.spriteDirection = directionToMouse.X > 0 ? 1 : -1;
//        }

//        private void ReleaseFireballs()
//        {
//            int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
//            float baseAngle = MathHelper.TwoPi / 60;

//            for (int i = 0; i < 60; i++)
//            {
//                Vector2 fireballVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 8.5f; // 初始速度为原来的8.5倍
//                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
//            }
//        }

//        private void ReleaseLinearParticles()
//        {
//            float baseAngle = MathHelper.TwoPi / 20;

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


//        private void StartLunge(Player owner)
//        {
//            // 设置初始冲刺方向并锁定
//            lockedDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
//            float chargeSpeed = MathHelper.Lerp(MinChargeSpeed, MaxChargeSpeed, Projectile.ai[0] / MaxChargeTime);
//            Projectile.velocity = lockedDirection * chargeSpeed;

//            // 播放冲刺音效和特效
//            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
//            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.OrangeRed, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
//            GeneralParticleHandler.SpawnParticle(pulse);

//            Projectile.netUpdate = true;
//        }
//        private void AddSmokeParticles()
//        {
//            int dustCount = 4;
//            float rotationSpeed = 0.3f;
//            Vector2 spinningPoint = new Vector2(0, -40f);

//            for (int i = 0; i < dustCount; i++)
//            {
//                Vector2 dustPosition = spinningPoint.RotatedBy(rotationSpeed * i) + Projectile.Center;
//                Color dustColor = (i % 2 == 0) ? Color.Red : Color.Yellow;

//                Particle smoke = new HeavySmokeParticle(dustPosition, Vector2.Zero, dustColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
//                GeneralParticleHandler.SpawnParticle(smoke);
//            }
//        }

//        private void AddTrailParticles()
//        {
//            Vector2 offset1 = Projectile.velocity.RotatedBy(2.3f) * 0.5f;
//            Vector2 offset2 = Projectile.velocity.RotatedBy(-2.3f) * 0.5f;
//            Color particleColor = Color.OrangeRed;

//            PointParticle spark1 = new PointParticle(Projectile.Center - Projectile.velocity + offset1, offset1, false, 15, 1.1f, particleColor);
//            PointParticle spark2 = new PointParticle(Projectile.Center - Projectile.velocity + offset2, offset2, false, 15, 1.1f, particleColor);
//            GeneralParticleHandler.SpawnParticle(spark1);
//            GeneralParticleHandler.SpawnParticle(spark2);
//        }

//        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//        {
//            int slashCount = Main.rand.Next(2, 5);
//            for (int i = 0; i < slashCount; i++)
//            {
//                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
//                int slashID = ModContent.ProjectileType<OrangeSLASH>();
//                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner);
//            }
//            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
//        }


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



//    }
//}