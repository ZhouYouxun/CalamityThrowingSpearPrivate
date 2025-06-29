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
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class SurfeiterDrumINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";

        public override string Texture => $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string texturePath = $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;

            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
        private int drumForm = 0; // 0-4 对应五种形态

        // 设定模式的方法
        public void SetDrumForm(int form)
        {
            drumForm = form;
            Projectile.netUpdate = true; // 同步数据
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
        }
        public bool exploding
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value == true ? 1f : 0f;
        }
        //1.笞:Flogging-敌怪获得1.02倍的易伤
        //2.杖:Beating-敌怪的接触伤害减少30%
        //3.徒:Imprisoning-敌怪的移动速度减少50%
        //4.流:Banishing-敌怪防御降低40
        //5.死:Executing-2秒的倒计时结束后造成5000点伤害
        public override void AI()
        {
            // 每帧微微左拐（以屏幕为视角）
            Projectile.velocity = Projectile.velocity.RotatedBy(-0.01f);

            // 更慢的减速
            Projectile.velocity *= 0.993f;

            // 设置朝向：只在正常飞行阶段
            if (!exploding)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 添加暗红色调光照
            Lighting.AddLight(Projectile.Center, Color.SaddleBrown.ToVector3() * 0.6f);

            Projectile.alpha += 5;

            // 🌫️ 重型黑灰烟雾（飞行期间每几帧一次）
            if (Main.rand.NextBool(3)) // 每 3 帧触发一次
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f); // 略偏离轨迹
                Vector2 particlePos = Projectile.Center + offset;

                Particle smokeH = new HeavySmokeParticle(
                    particlePos,
                    new Vector2(0, -1f) * Main.rand.NextFloat(1f, 4f), // 往上飘
                    Main.rand.NextBool() ? Color.IndianRed : Color.Gray,
                    Main.rand.Next(28, 42), // 生命周期
                    Projectile.scale * Main.rand.NextFloat(0.8f, 1.4f),
                    0.75f, // 透明度
                    Main.rand.NextFloat(-0.03f, 0.03f), // 轻微旋转
                    true // 重型粒子
                );

                GeneralParticleHandler.SpawnParticle(smokeH);
            }


            // === 飞行尘雾效果 ===
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Sandnado, Projectile.velocity * 0.2f);
                dust.scale = Main.rand.NextFloat(0.7f, 1.1f);
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.SaddleBrown, Color.DarkRed, Main.rand.NextFloat());
            }

            // === 弹出粒子星点（深棕+偏移） ===
            if (Main.rand.NextBool(4))
            {
                Vector2 randomOffset = Main.rand.NextVector2Circular(12f, 12f); // 添加随机偏移

                Particle sparkle = new GenericSparkle(
                    Projectile.Center + randomOffset, // 添加偏移
                    Vector2.Zero,
                    new Color(60, 30, 10), // 更加深沉的棕色主色
                    new Color(80, 50, 30), // 次色稍微亮一点
                    Main.rand.NextFloat(1.0f, 1.5f), // 尺寸略大
                    10,
                    Main.rand.NextFloat(-0.015f, 0.015f), // 微旋转
                    1.3f
                );

                GeneralParticleHandler.SpawnParticle(sparkle);
            }

            // === 爆炸阶段逻辑 ===
            if (Projectile.timeLeft <= 65)
                exploding = true;

            if (exploding)
            {
                Projectile.velocity *= 0.93f;

                if (Projectile.timeLeft > 65)
                    Projectile.timeLeft = 65;

                if (Projectile.alpha < 255)
                    Projectile.alpha = Projectile.alpha + 3;

                if (Projectile.timeLeft == 65)
                {
                    Particle blastRing = new CustomPulse(
                        Projectile.Center,
                        Vector2.Zero,
                        Color.SaddleBrown,
                        "CalamityThrowingSpear/Texture/KsTexture/light_01",
                        Vector2.One,
                        Main.rand.NextFloat(-10, 10),
                        0.12f,
                        0f,
                        25
                    );
                    GeneralParticleHandler.SpawnParticle(blastRing);

                    SoundStyle fire = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeImpact");
                    SoundEngine.PlaySound(fire with { Volume = 1.25f, Pitch = -0.2f, PitchVariance = 0.15f }, Projectile.Center);



                    // ✅ 在准备爆炸时向正后方发射视觉特效弹幕
                    Vector2 back = -Projectile.rotation.ToRotationVector2();
                    Vector2 velocity = back * 6f;

                    Projectile backProj = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity * 3f,
                        ModContent.ProjectileType<SurfeiterDrumINVBack>(),
                        0, // 无伤害
                        0f,
                        Projectile.owner
                    );

                    // 可传递 drumForm（若需要区分颜色、效果）
                    if (backProj.ModProjectile is SurfeiterDrumINVBack backModProj)
                    {
                        backModProj.SetDrumForm(drumForm);
                    }

                }
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void OnKill(int timeLeft)
        {
            // 设置伤害倍率（可以调整数值以匹配实际需求）
            float boomDamageMultiplier = 1.0f; // 可以根据需求调整
            int boomDamage = (int)(Projectile.damage * boomDamageMultiplier);

            // 创建爆炸弹幕
            Projectile explosion = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<SurfeiterDrumINVEXP>(),
                boomDamage,
                Projectile.knockBack,
                Projectile.owner
            );

            // 设置爆炸弹幕的额外属性
            float maxRadius = Main.rand.NextFloat(110f, 200f); // 随机最大半径
            explosion.ai[1] = maxRadius;
            explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // 插值步长
            explosion.netUpdate = true;

            // 传递模式
            if (explosion.ModProjectile is SurfeiterDrumINVEXP drumEXP)
            {
                drumEXP.SetDrumForm(drumForm);
            }

            //// 传递模式
            //if (Main.projectile[projID].ModProjectile is SurfeiterDrumINVEXP drumEXP)
            //{
            //    drumEXP.SetDrumForm(drumForm);
            //}

            {
                // 💀 灵魂骷髅粒子（中心逸散）
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Particle skull = new DesertProwlerSkullParticle(
                        Projectile.Center,
                        vel,
                        Color.DarkGray * 0.8f,
                        Color.LightGray,
                        Main.rand.NextFloat(0.6f, 1.2f),
                        180
                    );
                    GeneralParticleHandler.SpawnParticle(skull);
                }

                // 🟤 棕色 Dust 粒子
                for (int i = 0; i < 25; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                    int dustType = Main.rand.NextBool() ? DustID.BorealWood : DustID.RichMahogany;
                    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, dustVel);
                    d.scale = Main.rand.NextFloat(1.0f, 1.5f);
                    d.noGravity = true;
                    d.color = Color.Lerp(new Color(80, 40, 20), Color.SaddleBrown, Main.rand.NextFloat());
                }

                // 🔥 火花线性粒子
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Particle trail = new SparkParticle(
                        Projectile.Center + dir * 4f,
                        dir * Main.rand.NextFloat(3f, 6f),
                        false,
                        60,
                        Main.rand.NextFloat(0.9f, 1.4f),
                        Color.OrangeRed
                    );
                    GeneralParticleHandler.SpawnParticle(trail);
                }

                // 🌊 冲击波环形粒子
                //Particle pulse = new CustomPulse(
                //    Projectile.Center,
                //    Vector2.Zero,
                //    new Color(100, 60, 20), // 暗棕色冲击
                //    "CalamityMod/Particles/HighResHollowCircleHardEdge",
                //    Vector2.One,
                //    Main.rand.NextFloat(-8f, 8f),
                //    0.05f,
                //    0.20f,
                //    20
                //);
                //GeneralParticleHandler.SpawnParticle(pulse);

                // 🌪️ 灰尘破片（偏低/重力）
                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Dirt,
                        Main.rand.NextVector2Circular(2f, 2f),
                        100,
                        Color.DarkGray,
                        Main.rand.NextFloat(1.0f, 1.5f)
                    );
                    d.noGravity = false;
                }

                // 💥 爆音
                //SoundStyle sound = new("CalamityMod/Sounds/Item/CrushCannonExplosion");
                //SoundEngine.PlaySound(sound with { Volume = 1.0f, Pitch = -0.1f }, Projectile.Center);

            }


        }
    }
}