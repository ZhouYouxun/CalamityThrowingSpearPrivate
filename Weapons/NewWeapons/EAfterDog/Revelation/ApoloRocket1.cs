//using CalamityMod.Items.Weapons.Magic;
//using CalamityMod.Particles;
//using CalamityMod;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.Audio;
//using Terraria.GameContent;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System;
//using Terraria;
//using Terraria.ModLoader;
//using Microsoft.Xna.Framework;
//using Terraria.ID;
//using Terraria.Audio;
//using CalamityMod.Projectiles.Melee;
//using CalamityMod.Projectiles.Rogue;
//using Mono.Cecil;
//using CalamityMod.Projectiles.Typeless;
//using CalamityMod;
//using CalamityMod.Particles;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria.GameContent;
//using CalamityMod.Items.Weapons.Magic;
//using CalamityMod.Buffs.DamageOverTime;
//using CalamityMod.Projectiles.Ranged;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
//{
//    public class ApoloRocket1 : ModProjectile
//    {
//        private int Time;
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

//        // 设置弹幕的基本属性
//        public override void SetDefaults()
//        {
//            Projectile.width = 40;
//            Projectile.height = 40;
//            Projectile.friendly = true;
//            Projectile.DamageType = DamageClass.Ranged;
//            Projectile.tileCollide = false;
//            Projectile.penetrate = 1;
//            Projectile.timeLeft = 180;
//        }

//        // 定义弹幕的行为
//        public override void AI()
//        {
//            Projectile.velocity *= 1.01f; // 每帧降低弹幕速度
//            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

//            if (Projectile.ai[1] % 30 == 0 && Main.myPlayer == Projectile.owner)
//            {
//                SpriteBatch spriteBatch = Main.spriteBatch;
//                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
//                // 保持飞行方向不变

//                // 添加蓝色光效
//                Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.55f);
//            }

//            // 血液红色粒子特效
//            if (Main.rand.NextBool(5))
//            {
//                Vector2 trailPos = Projectile.Center;
//                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
//                Color trailColor = Color.Green;
//                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
//                GeneralParticleHandler.SpawnParticle(trail);
//            }
//        }
//        public override void OnKill(int timeLeft)
//        {
//            Projectile.position = Projectile.Center;
//            Projectile.width = Projectile.height = 1040;
//            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
//            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
//            if (Projectile.owner == Main.myPlayer)
//            {
//                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ApoloExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
//            }
//            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

//            // 生成四个夹角为90度的AmidiasTridentJavWhirlpool弹幕

//            // 释放随机的线性海蓝色粒子特效
//            int points = 2;
//            float radians = MathHelper.TwoPi / points;
//            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
//            float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
//            for (int k = 0; k < points; k++)
//            {
//                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkGreen, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(Viscera.BoomLifetime * 0.38f), false);
//                GeneralParticleHandler.SpawnParticle(bloodsplosion);
//                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(32, 255, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, Viscera.BoomLifetime);
//                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
//            }
//        }

//        public class ApoloExplosion : ModProjectile
//        {
//            public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

//            public override void SetDefaults()
//            {
//                Projectile.width = 1000;
//                Projectile.height = 1000;
//                Projectile.friendly = true;
//                Projectile.DamageType = DamageClass.Ranged;
//                Projectile.tileCollide = false;
//                // Projectile.penetrate = 1;
//                Projectile.penetrate = -1;
//                Projectile.timeLeft = 180;
//            }

//            public override void AI()
//            {
//                Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.75f / 255f, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0.01f / 255f);
//                if (Projectile.localAI[0] == 0f)
//                {
//                    SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
//                    Projectile.localAI[0] += 1f;
//                }

//                EmitDust();
//            }

//            public void EmitDust()
//            {
//                if (Main.dedServ)
//                    return;

//                for (int i = 0; i < 70; i++)
//                {
//                    // The exponent being greater than 1 gives the randomness a bias towards 0. This means that more dust will spawn
//                    // closer to the center than the edge.
//                    Vector2 dustSpawnOffset = Main.rand.NextVector2Unit() * (float)Math.Pow(Main.rand.NextFloat(), 2.4D) * Projectile.Size * 0.5f;

//                    // Dust should fly off more quickly the farther away it is from the center.
//                    // At 5% out, a speed of 5 pixels/second is achieved. At 85%, a speed of 15 pixels/second is.
//                    // Direction is determined based on the outward direction rotated by anywhere from -90 to 90 degrees.
//                    Vector2 dustVelocity = dustSpawnOffset.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
//                    dustVelocity *= MathHelper.Lerp(5f, 15f, Utils.GetLerpValue(0.05f, 0.85f, (dustSpawnOffset / Projectile.Size / 0.5f).Length()));

//                    // Fire variants.
//                    int dustType = 75;
//                    if (Main.rand.NextBool(4))
//                        dustType = 75;

//                    // Smoke.
//                    if (Main.rand.NextBool(7))
//                        dustType = 31;

//                    Dust flame = Dust.NewDustPerfect(Projectile.Center + dustSpawnOffset, dustType, dustVelocity);
//                    flame.scale = Main.rand.NextFloat(0.85f, 1.3f);
//                    flame.noGravity = true;
//                }
//            }

//            public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.Size.Length() * 0.5f, targetHitbox);
//        }
//    }
//}
