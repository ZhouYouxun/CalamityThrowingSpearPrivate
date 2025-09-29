using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using Terraria.Audio;
using Terraria.DataStructures;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchEPROJ : ModProjectile, ILocalizedModType
    {
        private bool hasCollidedWithTile = false;

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouch";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            Main.projFrames[Projectile.type] = 4; // 设置投射物的帧数为 4
        }

        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 计算当前动画帧
            int frameCount = 4; // 总其 4 帧
            int frameHeight = texture.Height / frameCount; // 每帧的高度
            int currentFrame = (int)(Main.GameUpdateCount / 6 % frameCount); // 每 6 帧切换一次，总其 4 帧
            Rectangle sourceRectangle = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);

            // 设置绘制的原点和位置
            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2); // 每帧的高度作为原点
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 绘制当前帧
            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 80;
            Projectile.hostile = false; // 敌对弹帧，能够对玩家造成有效伤害
            Projectile.friendly = true; // 我方弹帧，能够对敌人造成有效伤害
            Projectile.tileCollide = true;
            Projectile.damage = 300; 
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6000; // 存在时间
            Projectile.velocity = new Vector2(0, 40); // 初始向下速度
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1; // 无敌帧冷却时间为x帧
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!hasCollidedWithTile)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

                // 每 6 帧切换一次帧
                if (++Projectile.frameCounter >= 6)
                {
                    Projectile.frameCounter = 0; // 重置帧计数器
                    Projectile.frame++; // 切换到下一帧
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                    {
                        Projectile.frame = 0; // 如果超过了最大帧数，回到第一帧
                    }
                }

                // 生成红色和橙色的粒子特效，每帧生成5个以增加视觉效果
                for (int i = 0; i < 5; i++) // 每帧生成 5 个粒子
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
                    dust.velocity *= 2f; // 增强粒子的速度以使效果更明显
                    dust.scale = 1.5f; // 增大粒子的尺寸
                    dust.noGravity = true; // 粒子无重力
                    dust.color = Main.rand.NextBool() ? Color.Orange : Color.Red; // 随机选择红色或橙色
                }

                if (Projectile.ai[0] % 60 == 0) // 每 60 帧释放一次烟雾粒子特效
                {
                    AddHeavySmokeParticles(); // 重型烟雾粒子特效
                    AddSmokeParticles(); // 轻型烟雾粒子特效
                }
            }
            else
            {
                Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);
            }

            if (hasCollidedWithTile)
            {
                Projectile.frame = 0; // 固定在第一帧
                // 如果弹幕已经与方块发生碰撞，检查是否与玩家发生碰撞
                //AddHeavySmokeParticles();
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player targetPlayer = Main.player[i];
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                    {
                        Projectile.Kill();
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/mega") with { Volume = 2f });
                        return;
                    }
                }
            }







            {
                // 每 6 帧生成冲击波
                if (Main.GameUpdateCount % 6 == 0)
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * -20f,
                        Projectile.velocity.SafeNormalize(Vector2.UnitY) * -3f,
                        Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                        new Vector2(1f, 2.5f),
                        Projectile.rotation - MathHelper.PiOver4,
                        0.2f,
                        0.02f,
                        25
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                // 每帧生成火花拖尾
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                    AltSparkParticle spark = new AltSparkParticle(
                        Projectile.Center + offset,
                        Projectile.velocity * -0.05f,
                        false,
                        15,
                        1.3f,
                        Color.OrangeRed * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 每 20 帧生成重型烟雾
                if (Main.GameUpdateCount % 20 == 0)
                {
                    Particle heavySmoke = new HeavySmokeParticle(
                        Projectile.Center,
                        Projectile.velocity.SafeNormalize(Vector2.UnitY) * -4f + Main.rand.NextVector2Circular(1f, 1f),
                        Main.rand.NextBool() ? Color.Orange : Color.Red,
                        40,
                        Main.rand.NextFloat(0.9f, 1.4f),
                        1f,
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        required: false
                    );
                    GeneralParticleHandler.SpawnParticle(heavySmoke);
                }

            }

        }

        public override void OnKill(int timeLeft)
        {
            // 生成掉落物
            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<FinishingTouch>());

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player targetPlayer = Main.player[i];
                if (targetPlayer.active && !targetPlayer.dead)
                {
                    // 为玩家施加 创造胜利，持续时间为 300 帧（5 秒）
                    targetPlayer.AddBuff(ModContent.BuffType<FinishingTouch10PBuff>(), 180);
                }
            }

            // 生成大量的重型烟雾粒子特效
            for (int i = 0; i < 50; i++) // 生成大量烟雾粒子
            {
                Vector2 smokeVelocity = new Vector2(Main.rand.NextFloat(-35f, 35f), Main.rand.NextFloat(-35f, 35f)); // 随机速度
                Color smokeColor = Color.Gray; // 使用灰色作为烟雾颜色
                float smokeLifetime = Main.rand.NextFloat(1.0f, 2.0f); // 随机生成粒子的存活时间

                // 生成重型烟雾粒子
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    smokeVelocity,
                    smokeColor,
                    (int)smokeLifetime,
                    Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), // 生成不同大小的粒子
                    1.0f,
                    MathHelper.ToRadians(2f),
                    required: true
                );

                // 生成粒子
                GeneralParticleHandler.SpawnParticle(smoke);
            }


            // 播放爆炸音效（可选）
            //Main.PlaySound(SoundID.Item14, Projectile.position);
        }


        private void AddHeavySmokeParticles() // 重型烟雾粒子特效
        {
            // 烟雾粒子基本参数
            Color smokeColor = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
            float smokeSpeed = 5f; // 烟雾的初始速度
            int smokeLifetime = 30; // 烟雾粒子的生存时间

            // 计算烟雾粒子释放的基础方向（投射物的反方向）
            Vector2 baseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero);

            // 随机在 -15 度到 15 度之间变化
            float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
            Vector2 smokeVelocity = baseDirection.RotatedBy(randomAngle) * smokeSpeed;

            // 生成重型烟雾粒子
            Particle smoke = new HeavySmokeParticle(
                Projectile.Center,
                smokeVelocity,
                smokeColor,
                smokeLifetime,
                Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f),
                1.0f,
                MathHelper.ToRadians(2f),
                required: true
            );

            // 生成粒子
            GeneralParticleHandler.SpawnParticle(smoke);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 屏幕震动效果
            float shakePower = 150f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(9000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), false); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/RayquazaRoar") with { Volume = 2f });
            int numberOfDusts = 88;
            float rotFactor = 360f / numberOfDusts;

            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.OrangeRed, new Vector2(10f, 10f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
            GeneralParticleHandler.SpawnParticle(pulse);


            // 遍历所有玩家并为其施加 创造胜利 DeBuff
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player targetPlayer = Main.player[i];
                if (targetPlayer.active && !targetPlayer.dead)
                {
                    // 为玩家施加 创造胜利，持续时间为 300 帧（5 秒）
                    targetPlayer.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), 180);
                }
            }

            // 保持原来的旋转角度
            Projectile.rotation = oldVelocity.ToRotation() + MathHelper.PiOver4;

            // 碰到方块后，将伤害永久降低为零
            Projectile.damage = 0;
            // 设置为不再对玩家产生伤害
            Projectile.hostile = false;
            // 停止弹幕移动，模拟粘附在方块上的效果
            Projectile.velocity = Vector2.Zero;
            // 设置存活时间为无限，除非手动销毁
            Projectile.timeLeft = int.MaxValue;

            // 打开碰撞开关
            hasCollidedWithTile = true;





            {
                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/RayquazaRoar") with { Volume = 2.5f }, Projectile.Center);

                // ==================== 播放一次性极限爆炸视觉特效 ====================

                // ==================== 播放一次性极限爆炸视觉特效 ====================
                {
                    // === 爆炸特效 第1部分 开始 ===

                    // [Layer 1] 黄金角 Dust 爆散 --------------------------------------------------
                    {
                        int seeds = 420;
                        float golden = MathHelper.ToRadians(137.50776f);
                        for (int i = 0; i < seeds; i++)
                        {
                            float k = i;
                            float theta = k * golden;
                            float r = 4f * (float)Math.Sqrt(k);

                            Vector2 dir = theta.ToRotationVector2();
                            Vector2 spawn = Projectile.Center + dir * r;
                            Vector2 vel = dir * Main.rand.NextFloat(12f, 28f);

                            Dust d = Dust.NewDustPerfect(
                                spawn,
                                Main.rand.NextBool() ? DustID.OrangeTorch : DustID.FlameBurst,
                                vel,
                                0,
                                Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                                Main.rand.NextFloat(1.2f, 2.4f)
                            );
                            d.noGravity = true;
                        }
                    }

                    // [Layer 2] 环形呼吸 Dust ------------------------------------------------------
                    {
                        int rings = 10;
                        for (int r = 0; r < rings; r++)
                        {
                            float radius = 40f + r * 20f;
                            int points = 120 + r * 12;
                            for (int i = 0; i < points; i++)
                            {
                                float ang = MathHelper.TwoPi * i / points + Main.rand.NextFloat(-0.02f, 0.02f);
                                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;
                                Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);

                                Dust d = Dust.NewDustPerfect(
                                    pos,
                                    DustID.Torch,
                                    vel,
                                    0,
                                    Color.Lerp(Color.Orange, Color.Goldenrod, Main.rand.NextFloat()),
                                    Main.rand.NextFloat(0.9f, 1.5f)
                                );
                                d.noGravity = true;
                            }
                        }
                    }

                    // [Layer 3] Hypotrochoid GlowOrb ------------------------------------------------
                    {
                        float R = 40f, r = 9f, d = 22f;
                        int count = 600;
                        for (int i = 0; i < count; i++)
                        {
                            float t = i * 0.2f;
                            float k = (R - r) / r;
                            float x = (R - r) * (float)Math.Cos(t) + d * (float)Math.Cos(k * t);
                            float y = (R - r) * (float)Math.Sin(t) - d * (float)Math.Sin(k * t);
                            Vector2 pos = Projectile.Center + new Vector2(x, y) * 2f;

                            GlowOrbParticle orb = new GlowOrbParticle(
                                pos,
                                Main.rand.NextVector2Circular(2f, 2f),
                                false,
                                20,
                                Main.rand.NextFloat(0.8f, 1.3f),
                                Main.rand.NextBool() ? Color.OrangeRed : Color.Yellow,
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // [Layer 4] SparkParticle 爆裂 --------------------------------------------------
                    {
                        for (int j = 0; j < 300; j++)
                        {
                            float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                            Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(15f, 40f);
                            SparkParticle sp = new SparkParticle(
                                Projectile.Center,
                                vel,
                                false,
                                Main.rand.Next(40, 70),
                                Main.rand.NextFloat(1.2f, 2.0f),
                                Color.Lerp(Color.Orange, Color.Gold, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(sp);
                        }
                    }

                    // [Layer 5] 分形树 Dust ---------------------------------------------------------
                    {
                        void SpawnBranch(Vector2 start, Vector2 dir, int depth, float length)
                        {
                            if (depth <= 0) return;
                            Vector2 end = start + dir * length;

                            for (int i = 0; i < 6; i++)
                            {
                                Vector2 pos = Vector2.Lerp(start, end, i / 6f);
                                Dust d = Dust.NewDustPerfect(
                                    pos,
                                    DustID.FlameBurst,
                                    Vector2.Zero,
                                    0,
                                    Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                                    Main.rand.NextFloat(1.0f, 1.8f)
                                );
                                d.noGravity = true;
                            }

                            float angleVar = Main.rand.NextFloat(0.2f, 0.5f);
                            SpawnBranch(end, dir.RotatedBy(angleVar), depth - 1, length * 0.7f);
                            SpawnBranch(end, dir.RotatedBy(-angleVar), depth - 1, length * 0.7f);
                        }

                        for (int k = 0; k < 12; k++)
                        {
                            float ang = MathHelper.TwoPi * k / 12f;
                            SpawnBranch(Projectile.Center, ang.ToRotationVector2(), 5, 40f);
                        }
                    }

                    // [Layer 6] SquishyLight EXO Energy --------------------------------------------
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            SquishyLightParticle exo = new(
                                Projectile.Center,
                                Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 12f),
                                0.35f,
                                Color.Orange,
                                25,
                                opacity: 1f,
                                squishStrenght: 1.2f,
                                maxSquish: 3.5f,
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }
                    }

                    // [Layer 7] 十字星 Sparkle ------------------------------------------------------
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            GenericSparkle star = new GenericSparkle(
                                Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                                Vector2.Zero,
                                Color.Gold,
                                Color.OrangeRed,
                                Main.rand.NextFloat(1.8f, 2.6f),
                                12,
                                Main.rand.NextFloat(-0.05f, 0.05f),
                                1.8f
                            );
                            GeneralParticleHandler.SpawnParticle(star);
                        }
                    }

                    // === 爆炸特效 第1部分 结束 ===



                    // === 爆炸特效 第2部分 开始 ===

                    // [Layer 8] 大范围 Dust 爆散（稀疏但高速） ------------------------------
                    {
                        int seeds = 300;
                        float golden = MathHelper.ToRadians(137.50776f);
                        for (int i = 0; i < seeds; i++)
                        {
                            float theta = i * golden;
                            float r = 10f * (float)Math.Sqrt(i);

                            Vector2 dir = theta.ToRotationVector2();
                            Vector2 spawn = Projectile.Center + dir * r * Main.rand.NextFloat(2f, 6f);
                            Vector2 vel = dir * Main.rand.NextFloat(25f, 70f);

                            Dust d = Dust.NewDustPerfect(
                                spawn,
                                DustID.FlameBurst,
                                vel,
                                0,
                                Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                                Main.rand.NextFloat(1.5f, 2.8f)
                            );
                            d.noGravity = true;
                        }
                    }

                    // [Layer 9] 外摆线 GlowOrb 阵列 -------------------------------------------
                    {
                        float R = 80f, r = 15f, d = 30f;
                        int count = 300;
                        for (int i = 0; i < count; i++)
                        {
                            float t = i * 0.3f;
                            float k = (R + r) / r;
                            float x = (R + r) * (float)Math.Cos(t) - d * (float)Math.Cos(k * t);
                            float y = (R + r) * (float)Math.Sin(t) - d * (float)Math.Sin(k * t);
                            Vector2 pos = Projectile.Center + new Vector2(x, y) * 3f;

                            GlowOrbParticle orb = new GlowOrbParticle(
                                pos,
                                Main.rand.NextVector2Circular(3f, 3f) * Main.rand.NextFloat(1.2f, 3f),
                                false,
                                22,
                                Main.rand.NextFloat(1.0f, 1.6f),
                                Main.rand.NextBool() ? Color.Orange : Color.Red,
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // [Layer 10] 稀疏火花喷射 -------------------------------------------------
                    {
                        for (int j = 0; j < 180; j++)
                        {
                            float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                            Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(25f, 65f);
                            SparkParticle sp = new SparkParticle(
                                Projectile.Center,
                                vel,
                                false,
                                Main.rand.Next(30, 55),
                                Main.rand.NextFloat(1.0f, 1.8f),
                                Color.Lerp(Color.OrangeRed, Color.Goldenrod, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(sp);
                        }
                    }

                    // [Layer 11] 大半径呼吸环 Dust -------------------------------------------
                    {
                        int rings = 6;
                        for (int r = 0; r < rings; r++)
                        {
                            float radius = 200f + r * 60f;
                            int points = 60 + r * 8;
                            for (int i = 0; i < points; i++)
                            {
                                float ang = MathHelper.TwoPi * i / points;
                                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;
                                Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);

                                Dust d = Dust.NewDustPerfect(
                                    pos,
                                    DustID.OrangeTorch,
                                    vel,
                                    0,
                                    Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat()),
                                    Main.rand.NextFloat(1.2f, 2.0f)
                                );
                                d.noGravity = true;
                            }
                        }
                    }

                    // [Layer 12] SquishyLight 高速光能量 -----------------------------------
                    {
                        for (int i = 0; i < 90; i++)
                        {
                            SquishyLightParticle exo = new(
                                Projectile.Center,
                                Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(10f, 25f),
                                0.42f,
                                Color.Orange,
                                28,
                                opacity: 1f,
                                squishStrenght: 1.4f,
                                maxSquish: 3.8f,
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }
                    }

                    // [Layer 13] 星芒阵列（GenericSparkle） ---------------------------------
                    {
                        int spokes = 12;
                        int layers = 5;
                        for (int l = 0; l < layers; l++)
                        {
                            float radius = 80f + l * 40f;
                            for (int s = 0; s < spokes; s++)
                            {
                                float ang = MathHelper.TwoPi * s / spokes + l * 0.2f;
                                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;
                                GenericSparkle sp = new GenericSparkle(
                                    pos,
                                    ang.ToRotationVector2() * Main.rand.NextFloat(8f, 14f),
                                    Color.Gold,
                                    Color.OrangeRed,
                                    2.0f,
                                    18,
                                    Main.rand.NextFloat(-0.03f, 0.03f),
                                    1.7f
                                );
                                GeneralParticleHandler.SpawnParticle(sp);
                            }
                        }
                    }

                    // [Layer 14] 稀疏烟雾 -----------------------------------------------------
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            HeavySmokeParticle smoke = new HeavySmokeParticle(
                                Projectile.Center + Main.rand.NextVector2Circular(80f, 80f),
                                Main.rand.NextVector2Circular(1.5f, 1.5f),
                                Color.Lerp(Color.DarkOrange, Color.Black, 0.5f),
                                Main.rand.Next(35, 55),
                                Main.rand.NextFloat(1.5f, 2.5f),
                                1.0f,
                                Main.rand.NextFloat(-0.1f, 0.1f),
                                true
                            );
                            GeneralParticleHandler.SpawnParticle(smoke);
                        }
                    }

                    // [Layer 15] Desert Skull 粒子 -------------------------------------------
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            Particle skull = new DesertProwlerSkullParticle(
                                Projectile.Center + Main.rand.NextVector2Circular(50f, 50f),
                                Main.rand.NextVector2Circular(2f, 2f),
                                Color.DarkGray * 0.85f,
                                Color.LightGray * 0.9f,
                                Main.rand.NextFloat(0.6f, 1.0f),
                                150
                            );
                            GeneralParticleHandler.SpawnParticle(skull);
                        }
                    }

                    // === 爆炸特效 第2部分 结束 ===

                    // === 爆炸特效 第3部分 开始 ===

                    // [Layer 16] EXO之光 —— 镁粉燃烧外壳 ---------------------------------
                    {
                        int exoCount = 300;
                        float maxRadius = 900f;
                        for (int i = 0; i < exoCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / exoCount + Main.rand.NextFloat(-0.02f, 0.02f);
                            float r = maxRadius + Main.rand.NextFloat(-60f, 60f);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * r;

                            SquishyLightParticle exo = new(
                                pos,
                                -ang.ToRotationVector2() * Main.rand.NextFloat(2f, 6f), // 向内轻轻收缩
                                0.35f + Main.rand.NextFloat(0.1f, 0.25f),
                                Main.rand.NextBool() ? Color.OrangeRed : Color.Goldenrod,
                                Main.rand.Next(25, 40),
                                opacity: 1f,
                                squishStrenght: 1f + Main.rand.NextFloat(0.2f, 0.6f),
                                maxSquish: 3f + Main.rand.NextFloat(0.5f, 1.5f),
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }
                    }

                    // [Layer 17] 边界辉光球（抖动外圈） ------------------------------------
                    {
                        int orbCount = 220;
                        float radius = 800f;
                        for (int i = 0; i < orbCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / orbCount;
                            Vector2 dir = ang.ToRotationVector2();
                            Vector2 pos = Projectile.Center + dir * (radius + Main.rand.NextFloat(-40f, 40f));

                            GlowOrbParticle orb = new GlowOrbParticle(
                                pos,
                                dir * Main.rand.NextFloat(4f, 10f) + Main.rand.NextVector2Circular(2f, 2f),
                                false,
                                Main.rand.Next(12, 22),
                                1.0f + Main.rand.NextFloat(0.2f, 0.5f),
                                Main.rand.NextBool() ? Color.Red : Color.Orange,
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // [Layer 18] 四方粒子外环（数字能量感） --------------------------------
                    {
                        int squares = 180;
                        float radius = 700f;
                        for (int i = 0; i < squares; i++)
                        {
                            float t = i / (float)squares * MathHelper.TwoPi;
                            // 外摆线参数：形成菱形/方形轨迹
                            float x = radius * (float)Math.Cos(t) + 50f * (float)Math.Cos(3 * t);
                            float y = radius * (float)Math.Sin(t) + 50f * (float)Math.Sin(3 * t);
                            Vector2 pos = Projectile.Center + new Vector2(x, y);

                            SquareParticle sq = new SquareParticle(
                                pos,
                                new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)),
                                false,
                                Main.rand.Next(28, 45),
                                1.6f + Main.rand.NextFloat(0.4f),
                                Color.Lerp(Color.Cyan, Color.OrangeRed, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(sq);
                        }
                    }

                    // [Layer 19] 巨大呼吸尘环 -------------------------------------------------
                    {
                        int rings = 3;
                        for (int r = 0; r < rings; r++)
                        {
                            float radius = 650f + r * 120f;
                            int points = 120;
                            for (int i = 0; i < points; i++)
                            {
                                float ang = MathHelper.TwoPi * i / points;
                                float breath = 1f + 0.18f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * 0.5f);
                                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * (radius * breath);
                                Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);

                                Dust d = Dust.NewDustPerfect(
                                    pos,
                                    DustID.SolarFlare,
                                    vel,
                                    0,
                                    Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat()),
                                    Main.rand.NextFloat(1.2f, 2.2f)
                                );
                                d.noGravity = true;
                            }
                        }
                    }

                    // [Layer 20] 星光脉冲（GenericSparkle） ---------------------------------
                    {
                        int pulses = 5;
                        for (int p = 0; p < pulses; p++)
                        {
                            int spokes = 16;
                            float radius = 950f;
                            for (int i = 0; i < spokes; i++)
                            {
                                float ang = MathHelper.TwoPi * i / spokes + p * 0.3f;
                                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;
                                GenericSparkle sparkle = new GenericSparkle(
                                    pos,
                                    ang.ToRotationVector2() * Main.rand.NextFloat(6f, 12f),
                                    Color.Gold,
                                    Color.OrangeRed,
                                    2.4f,
                                    26,
                                    Main.rand.NextFloat(-0.05f, 0.05f),
                                    2.2f
                                );
                                GeneralParticleHandler.SpawnParticle(sparkle);
                            }
                        }
                    }

                    // === 爆炸特效 第3部分 结束 ===

                    // === 爆炸特效 第4部分 开始（优雅收尾层） ===
                    {
                        float baseRadius = 600f; // 稍微小于第3部分的范围
                        float t = (float)Main.GameUpdateCount * 0.08f; // 慢速时间相位

                        // [Layer 21] 柔和 EXO 光点（呼吸式）
                        int exoCount = 80;
                        for (int i = 0; i < exoCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / exoCount;
                            float radius = baseRadius + 40f * (float)Math.Sin(t + i * 0.4f);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;

                            SquishyLightParticle exo = new(
                                pos,
                                Vector2.Zero, // 几乎静止
                                0.25f + 0.05f * (float)Math.Sin(t + i * 0.6f),
                                Color.Lerp(Color.Orange, Color.Gold, 0.5f + 0.5f * (float)Math.Sin(t * 1.3f + i)),
                                36,
                                opacity: 0.8f,
                                squishStrenght: 0.8f,
                                maxSquish: 2.2f,
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }

                        // [Layer 22] 柔和辉光球环（交错旋转）
                        int orbCount = 60;
                        for (int i = 0; i < orbCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / orbCount + t * 0.5f * (i % 2 == 0 ? 1 : -1);
                            float radius = baseRadius - 50f + 20f * (float)Math.Cos(t * 0.7f + i);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;

                            GlowOrbParticle orb = new GlowOrbParticle(
                                pos,
                                Vector2.Zero,
                                false,
                                28,
                                0.9f + 0.3f * (float)Math.Sin(t * 0.9f + i),
                                Color.Lerp(Color.OrangeRed, Color.Yellow, (float)Math.Sin(i + t) * 0.5f + 0.5f),
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }

                        // [Layer 23] 十字星点缀（渐隐闪烁）
                        int stars = 25;
                        for (int i = 0; i < stars; i++)
                        {
                            float ang = MathHelper.TwoPi * i / stars;
                            float radius = baseRadius - 80f + 30f * (float)Math.Sin(t * 1.1f + i);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;

                            GenericSparkle sparkle = new GenericSparkle(
                                pos,
                                Vector2.Zero,
                                Color.Lerp(Color.Orange, Color.SandyBrown, Main.rand.NextFloat()),
                                Color.Lerp(Color.DarkOrange, Color.Red, Main.rand.NextFloat()),
                                1.5f + 0.3f * (float)Math.Sin(t + i),
                                20,
                                Main.rand.NextFloat(-0.02f, 0.02f),
                                1.4f
                            );
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }

                        // [Layer 24] 柔和四方粒子（随机消散）
                        int sqCount = 30;
                        for (int i = 0; i < sqCount; i++)
                        {
                            float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                            float radius = baseRadius - 100f + Main.rand.NextFloat(-20f, 20f);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;

                            SquareParticle sq = new SquareParticle(
                                pos,
                                Main.rand.NextVector2Circular(1.5f, 1.5f),
                                false,
                                Main.rand.Next(30, 45),
                                1.3f + Main.rand.NextFloat(0.4f),
                                Color.Lerp(Color.OrangeRed, Color.Gold, Main.rand.NextFloat())
                            );
                            GeneralParticleHandler.SpawnParticle(sq);
                        }

                        // [Layer 25] 烟雾丝带（优雅收束）
                        int smokeCount = 40;
                        for (int i = 0; i < smokeCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / smokeCount;
                            float radius = baseRadius - 120f + Main.rand.NextFloat(-30f, 30f);
                            Vector2 pos = Projectile.Center + ang.ToRotationVector2() * radius;

                            Dust d = Dust.NewDustPerfect(
                                pos,
                                DustID.Smoke,
                                ang.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f),
                                100,
                                Color.Lerp(Color.Black, Color.Orange, 0.3f),
                                Main.rand.NextFloat(1.1f, 1.8f)
                            );
                            d.noGravity = true;
                        }
                    }
                    // === 爆炸特效 第4部分 结束（优雅收尾层） ===


                    // === 爆炸特效 第5部分 开始（横向能量圆柱体） ===
                    {
                        float t = (float)Main.GameUpdateCount * 0.12f; // 时间相位
                        float halfHeight = 16f; // 圆柱体半径（直径 2×16）
                        float expansion = 24f;  // 固定扩散距离（一次性爆炸，不需要 lifeTimer）

                        // [Layer 26] EXO之光带
                        int exoBands = 14;
                        for (int i = 0; i < exoBands; i++)
                        {
                            float offsetY = MathHelper.Lerp(-halfHeight, halfHeight, i / (float)(exoBands - 1));
                            Vector2 basePos = Projectile.Center + new Vector2(0f, offsetY);

                            for (int side = -1; side <= 1; side += 2)
                            {
                                Vector2 pos = basePos + new Vector2(expansion * side, 0f);

                                SquishyLightParticle exo = new(
                                    pos,
                                    Vector2.Zero,
                                    0.35f + 0.1f * (float)Math.Sin(t + i),
                                    Color.Lerp(Color.Orange, Color.Gold, 0.5f + 0.5f * (float)Math.Sin(t * 1.4f + i)),
                                    36,
                                    opacity: 1f,
                                    squishStrenght: 1.2f,
                                    maxSquish: 3.4f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);
                            }
                        }

                        // [Layer 27] 辉光球带
                        int orbBands = 10;
                        for (int i = 0; i < orbBands; i++)
                        {
                            float offsetY = MathHelper.Lerp(-halfHeight, halfHeight, i / (float)(orbBands - 1));
                            Vector2 basePos = Projectile.Center + new Vector2(0f, offsetY);

                            for (int side = -1; side <= 1; side += 2)
                            {
                                Vector2 pos = basePos + new Vector2(expansion * side, 0f);

                                GlowOrbParticle orb = new GlowOrbParticle(
                                    pos,
                                    Vector2.Zero,
                                    false,
                                    28,
                                    1.0f + 0.2f * (float)Math.Sin(t + i),
                                    Color.Lerp(Color.Red, Color.OrangeRed, 0.7f + 0.3f * (float)Math.Cos(t + i)),
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);
                            }
                        }

                        // [Layer 28] 中央加强辉光（圆柱体“内核”）
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Vector2 core = Projectile.Center + new Vector2(expansion * j, 0f);
                            GlowOrbParticle orbCore = new GlowOrbParticle(
                                core,
                                Vector2.Zero,
                                false,
                                20,
                                1.6f,
                                Color.OrangeRed,
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orbCore);
                        }
                    }
                    // === 爆炸特效 第5部分 结束（横向能量圆柱体） ===


                    // === 爆炸特效 第6部分 开始（巨大的厚重菱形扩散） ===
                    {
                        float diamondRadius = 120f; // 菱形的半径（控制大小）
                        int segments = 32;          // 菱形外框点数
                        float thickness = 12f;      // 菱形边缘厚度

                        // 中心位置
                        Vector2 C = Projectile.Center;

                        // === 绘制菱形四边（上、右、下、左） ===
                        for (int side = 0; side < 4; side++)
                        {
                            // 方向：上、右、下、左
                            Vector2 dir = (MathHelper.PiOver2 * side).ToRotationVector2();

                            for (int i = 0; i < segments; i++)
                            {
                                // 点在该边的插值
                                float t = i / (float)(segments - 1);
                                Vector2 edgePos = C + dir * (diamondRadius * (1f - Math.Abs(t - 0.5f) * 2f));

                                // 往外抖动形成扩散感
                                Vector2 jitter = Main.rand.NextVector2Circular(thickness, thickness);

                                // === 亮橙 EXO之光 ===
                                SquishyLightParticle exo = new(
                                    edgePos + jitter,
                                    dir * Main.rand.NextFloat(4f, 8f), // 向外喷发
                                    0.35f + Main.rand.NextFloat(0.15f),
                                    Color.Lerp(Color.Orange, Color.Gold, 0.6f),
                                    Main.rand.Next(25, 40),
                                    opacity: 1f,
                                    squishStrenght: 1.2f,
                                    maxSquish: 3.5f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);

                                // === 深红辉光球 ===
                                GlowOrbParticle orb = new GlowOrbParticle(
                                    edgePos + jitter,
                                    dir * Main.rand.NextFloat(3f, 6f),
                                    false,
                                    Main.rand.Next(20, 30),
                                    1.2f + Main.rand.NextFloat(0.3f),
                                    Color.OrangeRed * 0.9f,
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);

                                // === 厚重火花（加权） ===
                                if (Main.rand.NextBool(3))
                                {
                                    GenericSparkle sp = new GenericSparkle(
                                        edgePos + jitter,
                                        dir * Main.rand.NextFloat(5f, 9f),
                                        Color.OrangeRed,
                                        Color.Gold,
                                        2.2f,
                                        22,
                                        Main.rand.NextFloat(-0.02f, 0.02f),
                                        1.6f
                                    );
                                    GeneralParticleHandler.SpawnParticle(sp);
                                }
                            }
                        }
                    }
                    // === 爆炸特效 第6部分 结束（巨大的厚重菱形扩散） ===

                }

            }
            return false; // 保持弹帧不被销毁
        }

        private void AddSmokeParticles() // 轻型烟雾粒子特效
        {
            int dustCount = 4;
            float rotationSpeed = 0.3f;
            Vector2 spinningPoint = new Vector2(0, -40f);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustPosition = spinningPoint.RotatedBy(rotationSpeed * i) + Projectile.Center;
                Color dustColor = (i % 2 == 0) ? Color.Red : Color.Yellow;

                Particle smoke = new HeavySmokeParticle(dustPosition, Vector2.Zero, dustColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }


    }
}
