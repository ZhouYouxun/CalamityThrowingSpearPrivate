using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyRStandField : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 每 2~5 帧随机生成一个 Bloom 特效
            //if (Main.rand.NextBool(2, 6)) // 随机帧间隔
            //{
            //    Texture2D bloomTex = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
            //    Vector2 randomPosition = Projectile.Center + new Vector2(
            //        Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2),
            //        Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2)
            //    );

            //    float scale = Main.rand.NextFloat(0.5f, 1f); // 随机缩放
            //    float alpha = Main.rand.NextFloat(0.3f, 0.6f); // 随机透明度

            //    Color bloomColor = Color.White * alpha; // 设置淡入淡出的颜色
            //    Main.EntitySpriteDraw(
            //        bloomTex,
            //        randomPosition - Main.screenPosition,
            //        null,
            //        bloomColor,
            //        0f,
            //        bloomTex.Size() * 0.5f,
            //        scale,
            //        SpriteEffects.None,
            //        0
            //    );
            //}

            // 保留原有的绘制逻辑
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 400;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 720 * 8;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 8; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 240; // 无敌帧冷却时间
        }

        private int smokeTimer = 0; // 用于记录当前生成烟雾的位置

        public override void AI()
        {

            // 优先跟随指定敌人
            if (Projectile.ai[1] >= 0)
            {
                int npcIndex = (int)Projectile.ai[1];

                if (Main.npc[npcIndex].active && !Main.npc[npcIndex].friendly && !Main.npc[npcIndex].dontTakeDamage)
                {
                    Projectile.Center = Main.npc[npcIndex].Center;
                    return; // ✅ 已经锁定特定目标，不再往下执行
                }
                else
                {
                    Projectile.ai[1] = -1f; // 目标失效，转入通用追踪
                }
            }

            // === 通用追踪逻辑 ===
            NPC closest = null;
            float closestDist = 1500f; // 追踪范围

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            if (closest != null)
            {
                Projectile.Center = closest.Center;
            }













            // 烟雾生成逻辑
            int perimeterLength = (Projectile.width + Projectile.height) * 2; // 边框的总长度
            smokeTimer = (smokeTimer + 6) % perimeterLength; // 每帧前进2像素，循环生成

            // 计算当前烟雾的生成位置
            Vector2 smokePosition;
            if (smokeTimer < Projectile.width) // 上边框
                smokePosition = Projectile.TopLeft + new Vector2(smokeTimer, 0);
            else if (smokeTimer < Projectile.width + Projectile.height) // 右边框
                smokePosition = Projectile.TopRight + new Vector2(0, smokeTimer - Projectile.width);
            else if (smokeTimer < 2 * Projectile.width + Projectile.height) // 下边框
                smokePosition = Projectile.BottomRight - new Vector2(smokeTimer - (Projectile.width + Projectile.height), 0);
            else // 左边框
                smokePosition = Projectile.BottomLeft - new Vector2(0, smokeTimer - (2 * Projectile.width + Projectile.height));

            // 生成烟雾粒子
            Vector2 smokeVelocity = Vector2.Normalize(smokePosition - Projectile.Center) * Main.rand.NextFloat(3f, 7f); // 放射状向外
            //Particle smoke = new HeavySmokeParticle(
            //    smokePosition,
            //    smokeVelocity,
            //    Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f),
            //    Main.rand.Next(30, 60),
            //    Main.rand.NextFloat(0.25f, 0.5f),
            //    1.0f,
            //    MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
            //    true
            //);
            //GeneralParticleHandler.SpawnParticle(smoke);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 特效参数
            int particleCount = 20; // 粒子数量（较 OnKill 少得多）
            float particleRadius = 80f; // 粒子扩散半径（小范围）
            float particleSpeedMin = 4f; // 最小粒子速度
            float particleSpeedMax = 8f; // 最大粒子速度

            for (int i = 0; i < particleCount; i++)
            {
                // 粒子位置和速度
                float angle = MathHelper.TwoPi / particleCount * i; // 规则分布的角度
                Vector2 direction = angle.ToRotationVector2(); // 粒子扩散方向
                Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(0, particleRadius * 0.3f); // 粒子起点稍随机
                Vector2 velocity = direction * Main.rand.NextFloat(particleSpeedMin, particleSpeedMax); // 粒子速度

                // 随机粒子类型
                int particleType = Main.rand.Next(3); // 三种类型随机
                switch (particleType)
                {
                    case 0: // 烟雾粒子
                        Particle smoke = new HeavySmokeParticle(
                            position,
                            velocity,
                            Color.Lerp(Color.White, Color.LightGray, Main.rand.NextFloat(0.6f, 1f)), // 淡白至浅灰
                            Main.rand.Next(15, 30), // 生命周期较短
                            Main.rand.NextFloat(0.2f, 0.4f), // 缩放
                            1.0f,
                            MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                        break;

                    case 1: // 电弧粒子
                        Particle electricParticle = new SparkParticle(
                            position,
                            velocity,
                            false,
                            30, // 生命周期更短
                            Main.rand.NextFloat(0.5f, 1f), // 缩放
                            Color.White
                        );
                        GeneralParticleHandler.SpawnParticle(electricParticle);
                        break;

                    case 2: // 火花粒子
                        Vector2 sparkVelocity = direction * Main.rand.NextFloat(0.5f, 1.5f) * 0.5f;
                        Particle critSpark = new CritSpark(
                            position,
                            sparkVelocity,
                            Color.White,
                            Color.LightGray,
                            Main.rand.NextFloat(1f, 1.5f), // 缩放
                            Main.rand.Next(10, 20), // 生命周期短
                            0.1f,
                            2
                        );
                        GeneralParticleHandler.SpawnParticle(critSpark);
                        break;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/玻璃破碎").WithVolumeScale(0.93f));


            // 粒子总数
            int totalParticles = 300;

            // 循环生成粒子
            for (int i = 0; i < totalParticles; i++)
            {
                float angle = MathHelper.TwoPi / totalParticles * i; // 角度
                Vector2 direction = angle.ToRotationVector2(); // 方向向量
                Vector2 particlePosition = Projectile.Center + direction * Main.rand.NextFloat(0, 200f); // 粒子初始位置
                Vector2 particleVelocity = direction * Main.rand.NextFloat(8f, 19f); // 粒子速度

                // 随机选择粒子类型
                int particleType = Main.rand.Next(4);
                switch (particleType)
                {
                    case 0: // 烟雾粒子
                        Particle smoke = new HeavySmokeParticle(
                            particlePosition,
                            particleVelocity,
                            Color.Lerp(Color.White, Color.LightGray, Main.rand.NextFloat(0.5f, 1f)),
                            Main.rand.Next(30, 60),
                            Main.rand.NextFloat(0.25f, 0.5f),
                            1.0f,
                            MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                        break;

                    case 1: // 线性粒子
                        Particle electricParticle = new SparkParticle(
                            particlePosition,
                            particleVelocity,
                            false,
                            60,
                            Main.rand.NextFloat(0.8f, 1.2f),
                            Color.White
                        );
                        GeneralParticleHandler.SpawnParticle(electricParticle);
                        break;

                    case 2: // 火花粒子
                        Vector2 critSparkVelocity = direction * Main.rand.NextFloat(0.5f, 1.5f) * 0.33f;
                        Particle critSpark = new CritSpark(
                            particlePosition,
                            critSparkVelocity,
                            Color.White,
                            Color.LightGray,
                            Main.rand.NextFloat(1.5f, 2.5f),
                            Main.rand.Next(20) + 10,
                            0.1f,
                            3
                        );
                        GeneralParticleHandler.SpawnParticle(critSpark);
                        break;
                }
            }
        }


    }
}