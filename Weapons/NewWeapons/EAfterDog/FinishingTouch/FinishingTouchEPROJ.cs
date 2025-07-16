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

                // 大量 Dust 火花极速爆炸
                for (int i = 0; i < 300; i++)
                {
                    float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(15f, 45f);
                    int dustType = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst;
                    Dust d = Dust.NewDustDirect(
                        Projectile.Center,
                        0, 0,
                        dustType,
                        velocity.X,
                        velocity.Y,
                        0,
                        Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                        Main.rand.NextFloat(1.8f, 3.5f)
                    );
                    d.noGravity = true;
                }

                // 多层方向性冲击波
                for (int i = 0; i < 5; i++)
                {
                    Particle puls2e = new DirectionalPulseRing(
                        Projectile.Center,
                        Vector2.Zero,
                        Color.Lerp(Color.OrangeRed, Color.Yellow, i / 5f),
                        new Vector2(2f + i * 0.5f, 4f + i * 0.6f),
                        Main.rand.NextFloat(0f, MathHelper.TwoPi),
                        0.25f + i * 0.05f,
                        0.02f,
                        35
                    );
                    GeneralParticleHandler.SpawnParticle(puls2e);
                }

                // 多向 SparkParticle 喷射
                int sparkCount = 80;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                    Vector2 dir = angle.ToRotationVector2();
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        dir * Main.rand.NextFloat(18f, 40f),
                        false,
                        60,
                        Main.rand.NextFloat(1.2f, 2f),
                        Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat())
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
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
