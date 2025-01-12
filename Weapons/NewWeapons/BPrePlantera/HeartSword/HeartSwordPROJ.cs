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
using CalamityMod.Balancing;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;



namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword
{
    public class HeartSwordPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/HeartSword/HeartSword";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            // 添加刀刃亮光效果
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
            shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

            // 设置亮光的位置为弹幕的中心
            Vector2 lensFlareWorldPosition = Projectile.Center; // 移除偏移，直接使用弹幕中心

            // 亮光颜色为红色和橙色渐变
            Color lensFlareColor = Color.Lerp(Color.Red, Color.Orange, 0.23f) with { A = 0 };
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 生成血红色烟雾特效
            int Dusts = 2;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.DarkRed, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        //public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        //{
        //    // 生命偷取效果，回复10%的伤害作为生命值
        //    int heal = (int)Math.Round(damageDone * 0.1);
        //    if (heal > BalancingConstants.LifeStealCap)
        //        heal = BalancingConstants.LifeStealCap;

        //    if (Main.player[Main.myPlayer].lifeSteal <= 0f || heal <= 0 || target.lifeMax <= 5)
        //        return;

        //    // 生成 RoyalHeal 弹幕为玩家回复生命
        //    CalamityGlobalProjectile.SpawnLifeStealProjectile(Projectile, Main.player[Projectile.owner], heal, ModContent.ProjectileType<RoyalHeal>(), BalancingConstants.LifeStealRange);

        //    // 1%的概率触发幸运红心效果
        //    if (Main.rand.NextFloat() < 0.01f)
        //    {
        //        int lostHealth = Main.player[Projectile.owner].statLifeMax2 - Main.player[Projectile.owner].statLife;
        //        int totalHearts = lostHealth * 2 / 20; // 计算红心数量
        //        for (int i = 0; i < totalHearts; i++)
        //        {
        //            Vector2 spawnPosition = Main.player[Projectile.owner].Center + new Vector2(Main.rand.Next(-550, 550), Main.rand.Next(-100, 100));
        //            Item.NewItem(Main.player[Projectile.owner].GetSource_FromThis(), spawnPosition, ItemID.Heart);
        //        }

        //        // 生成鲜红色线性粒子特效，扩散36个，每隔10度
        //        for (int i = 0; i < 36; i++)
        //        {
        //            float angle = MathHelper.ToRadians(i * 10); // 每10度一个粒子
        //            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f; // 粒子速度

        //            Particle trail = new SparkParticle(Main.player[Projectile.owner].Center, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Red);
        //            GeneralParticleHandler.SpawnParticle(trail);
        //        }
        //    }
        //}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 生成 RoyalHeal 弹幕为玩家回复10%生命
            //int healAmount = (int)(damageDone * 0.1f); // 计算10%的生命值回复
            //CalamityGlobalProjectile.SpawnLifeStealProjectile(Projectile, Main.player[Projectile.owner], healAmount, ModContent.ProjectileType<RoyalHeal>(), BalancingConstants.LifeStealRange);

            // 为玩家回复生命
            int healAmount = (int)(damageDone * 0.04f); // 计算4%的生命值回复
            Main.player[Projectile.owner].statLife += healAmount;
            Main.player[Projectile.owner].HealEffect(healAmount);


            // 1%的概率触发幸运红心效果
            if (Main.rand.NextFloat() < 0.01f)
            {
                int lostHealth = Main.player[Projectile.owner].statLifeMax2 - Main.player[Projectile.owner].statLife;
                int totalHearts = lostHealth * 2 / 20; // 计算红心数量
                for (int i = 0; i < totalHearts; i++)
                {
                    Vector2 spawnPosition = Main.player[Projectile.owner].Center + new Vector2(Main.rand.Next(-550, 550), Main.rand.Next(-100, 100));
                    Item.NewItem(Main.player[Projectile.owner].GetSource_FromThis(), spawnPosition, ItemID.Heart);
                }

                // 生成鲜红色线性粒子特效，扩散36个，每隔10度
                for (int i = 0; i < 36; i++)
                {
                    float angle = MathHelper.ToRadians(i * 10); // 每10度一个粒子
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f; // 粒子速度

                    Particle trail = new SparkParticle(Main.player[Projectile.owner].Center, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Red);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }

            int Dusts = 10;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.DarkRed, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
        }
    }
}