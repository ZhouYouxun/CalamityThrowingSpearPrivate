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
using CalamityMod.Projectiles.Ranged;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.GameContent.Drawing;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC
{
    public class EarthenJavPROJ : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/BPrePlantera/EarthenC/EarthenJav";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //if (disableDraw)
            //    return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7; // 设置为7次穿透
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }
        private bool hasCollided = false;          // 是否已经命中地面
        private bool disableDraw = false;          // 是否关闭 PreDraw
        private bool beginSpawning = false;        // 是否开始地表发射流程
        private int spawnTimer = 0;                // 发射间隔计时器
        private int spawnCount = 0;                // 已发射次数
        private Vector2 spawnDirection;            // 发射方向（正左或正右）
        private Vector2 cachedDirection;


        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);



            // 飞行时留下卡其色的烟雾特效
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 6f)
            {
                for (int d = 0; d < 5; d++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f)];
                    dust.velocity = Vector2.Zero;
                    dust.position -= Projectile.velocity / 5f * d;
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.noLight = true;
                }
            }


            {
                // 🌋 飞行期间猛烈土石尘尾
                if (Main.rand.NextBool(1)) // 每帧
                {
                    int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType);
                    d.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                    d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    d.noGravity = Main.rand.NextBool(3); // 部分无重力
                }              

                // 深棕色 SparkParticle 模拟碎石飞溅
                if (Main.rand.NextBool(6))
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(2f, 2f),
                        false,
                        30,
                        1.0f,
                        new Color(90, 60, 40) // 深棕色
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

            }

            // 模拟重力效果
            if (Projectile.velocity.Y < 24f)
            {
                //Projectile.velocity.Y += 0.1f; // Y 轴速度逐渐增加
            }

            if (Projectile.timeLeft == 400) // 初始帧缓存方向
                cachedDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            if (beginSpawning)
            {
                spawnTimer--;
                if (spawnTimer <= 0 && spawnCount < 6)
                {
                    Vector2 spawnPos = Projectile.Center + spawnDirection * 64f * spawnCount; // xxf是每两个之间的间隔

                    // 发射一枚向上的分裂弹幕
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        new Vector2(0f, -16f),
                        ModContent.ProjectileType<EarthenJavSHARD>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    spawnCount++;
                    spawnTimer = 3; // 每次间隔 X 帧
                }
            }

        }



    

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!hasCollided)
            {
                hasCollided = true;
                disableDraw = true;
                beginSpawning = true;

                Projectile.friendly = false;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.timeLeft = 70;
                //Projectile.alpha = 255;
                Projectile.velocity = new Vector2(0f, -6f); // 给一个向上的初速（可调）


                // 朝向判定（只允许水平两方向）
                spawnDirection = cachedDirection.X < 0 ? -Vector2.UnitX : Vector2.UnitX;

                {
                    // 粉碎土石尘暴爆发
                    for (int i = 0; i < 25; i++)
                    {
                        int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                        d.velocity = Main.rand.NextVector2Circular(6f, 6f);
                        d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                        d.noGravity = Main.rand.NextBool();
                    }

                    // 重型烟雾环绕
                    for (int i = 0; i < 8; i++)
                    {
                        Particle smoke = new HeavySmokeParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            Main.rand.NextVector2Circular(1.5f, 1.5f),
                            Color.SandyBrown,
                            Main.rand.Next(25, 40),
                            Main.rand.NextFloat(0.6f, 0.9f),
                            0.8f,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }

                    // 强烈“咚”声
                    SoundEngine.PlaySound(SoundID.Item70, Projectile.position);

                    // 增强震屏
                    float shakePower = 5f; // 强烈震动
                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                }
            }
            return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 粉碎
        }





        public override void OnKill(int timeLeft)
        {

            // 爆炸弹幕：X个 EarthenJavSHARD 弹片
            for (int i = 0; i < 3; i++)
            {
                Vector2 shardVelocity = Main.rand.NextVector2Circular(5f, 5f); // 随机方向
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shardVelocity,
                    ModContent.ProjectileType<EarthenJavSHARD>(),
                    (int)(Projectile.damage * 0.5f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            {
                // 大量泥土尘爆发
                for (int i = 0; i < 60; i++)
                {
                    int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                    d.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    d.scale = Main.rand.NextFloat(1.3f, 2.2f);
                    d.noGravity = Main.rand.NextBool();
                }

                // 土石碎片飞散（SparkParticle）
                for (int i = 0; i < 12; i++)
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(10f, 10f),
                        false,
                        40,
                        1.2f,
                        new Color(120, 72, 40)
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 强爆炸声
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.8f }, Projectile.Center);

                // 强震动
                float shakePower = 6f;
                float distanceFactor = Utils.GetLerpValue(1200f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            }



            // 可选：爆炸声
            //SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/钻地武器进入地面的音效"), Projectile.Center);

        }



    }
}
