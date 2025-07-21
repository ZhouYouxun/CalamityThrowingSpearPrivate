//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod.Projectiles.BaseProjectiles;
//using CalamityMod.Sounds;
//using CalamityMod;
//using ReLogic.Content;
//using System.IO;
//using Terraria.Audio;
//using Terraria.ID;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
//{
//    public class SunEssenceJavBEAM : BaseLaserbeamProjectile
//    {
//        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

//        public int OwnerIndex
//        {
//            get => (int)Projectile.ai[1];
//            set => Projectile.ai[1] = value;
//        }
//        public override float MaxScale => 1f;
//        public override float MaxLaserLength => 1000f;
//        public override float Lifetime => 50;
//        public override Color LaserOverlayColor
//        {
//            get
//            {
//                Color c1 = Color.Goldenrod;
//                Color c2 = Color.Orange;
//                Color color = Color.Lerp(c1, c2, Projectile.identity % 5f / 5f) * 1.1f;
//                color.A = 25;
//                return color;
//            }
//        }
//        public override Color LightCastColor => Color.Transparent;
//        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavBEAM").Value;
//        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavBEAMMiddle").Value;
//        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavBEAMEnd").Value;

//        // 定义类级别的字段，用于存储头部、身体和尾部的长度
//        private float headLength;
//        private float bodyLength;
//        private float tailLength;

//        public override void SetStaticDefaults()
//        {
//            Main.projFrames[Projectile.type] = 10;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 30;
//            Projectile.height = 30;
//            Projectile.friendly = true;
//            Projectile.alpha = 255;
//            Projectile.penetrate = -1;
//            Projectile.tileCollide = false;
//            Projectile.timeLeft = 600;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = -1;
//            Projectile.DamageType = DamageClass.Melee;
//        }

//        public override void SendExtraAI(BinaryWriter writer)
//        {
//            writer.Write(Projectile.localAI[0]);
//            writer.Write(Projectile.localAI[1]);
//            writer.Write(Projectile.scale);
//        }

//        public override void ReceiveExtraAI(BinaryReader reader)
//        {
//            Projectile.localAI[0] = reader.ReadSingle();
//            Projectile.localAI[1] = reader.ReadSingle();
//            Projectile.scale = reader.ReadSingle();
//        }

//        public override void AttachToSomething() { }

//        public override void UpdateLaserMotion()
//        {
//            // Update the direction and rotation of the laser.
//            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
//            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
//        }

//        public override float DetermineLaserLength()
//        {
//            // 初始化头部、身体和尾部的长度
//            headLength = LaserBeginTexture.Height;
//            bodyLength = LaserMiddleTexture.Height;
//            tailLength = LaserEndTexture.Height;

//            // 返回激光的总长度
//            return headLength + bodyLength + tailLength;
//        }
//        public override void PostAI()
//        {
//            if (Projectile.frameCounter == 0)
//                SoundEngine.PlaySound(CommonCalamitySounds.LaserCannonSound, Projectile.Center);

//            // Determine frames.
//            Projectile.frameCounter++;
//            if (Projectile.frameCounter % 5f == 4f)
//                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
//        }

//        // Rapidly decrease damage every hit
//        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.damage = Math.Max(1, (int)(Projectile.damage * 0.3));

//        public override bool PreDraw(ref Color lightColor)
//        {
//            if (Projectile.velocity == Vector2.Zero)
//                return false;

//            Color beamColor = LaserOverlayColor;
//            Rectangle startFrameArea = LaserBeginTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
//            Rectangle middleFrameArea = LaserMiddleTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
//            Rectangle endFrameArea = LaserEndTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

//            // 绘制头部
//            Vector2 centerOnLaser = Projectile.Center;
//            Main.EntitySpriteDraw(LaserBeginTexture,
//                                  centerOnLaser - Main.screenPosition,
//                                  startFrameArea,
//                                  beamColor,
//                                  Projectile.rotation,
//                                  LaserBeginTexture.Size() / 2f,
//                                  Projectile.scale,
//                                  SpriteEffects.None,
//                                  0);

//            // 固定6节身体部分，每节紧密相连
//            int bodySegments = 6;
//            float segmentLength = LaserMiddleTexture.Height; // 每节长度固定为贴图高度

//            for (int i = 0; i < bodySegments; i++)
//            {
//                centerOnLaser += Projectile.velocity * segmentLength;
//                Main.EntitySpriteDraw(LaserMiddleTexture,
//                                      centerOnLaser - Main.screenPosition,
//                                      middleFrameArea,
//                                      beamColor,
//                                      Projectile.rotation,
//                                      LaserMiddleTexture.Size() / 2f,
//                                      Projectile.scale,
//                                      SpriteEffects.None,
//                                      0);
//            }

//            // 绘制尾部，确保与最后一节身体紧密相连
//            centerOnLaser += Projectile.velocity * segmentLength;
//            Main.EntitySpriteDraw(LaserEndTexture,
//                                  centerOnLaser - Main.screenPosition,
//                                  endFrameArea,
//                                  beamColor,
//                                  Projectile.rotation,
//                                  LaserEndTexture.Size() / 2f,
//                                  Projectile.scale,
//                                  SpriteEffects.None,
//                                  0);

//            return false;
//        }





//    }
//}
