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

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 模拟重力效果
            if (Projectile.velocity.Y < 24f)
            {
                //Projectile.velocity.Y += 0.1f; // Y 轴速度逐渐增加
            }

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

            // 冷却计时器递减
            if (Projectile.localAI[1] > 0)
                Projectile.localAI[1]--;
            // 穿墙计时器递减
            if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
                if (Projectile.localAI[0] == 0)
                {
                    Projectile.tileCollide = true; // 穿墙结束，恢复碰撞
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 使用钥匙大剑的特效【不必外包，因为没这必要】【搞错了，这个是一个圈，不是那个】
            //{
            //    // ✦ Keybrand 光环特效（在圆内随机位置）
            //    Vector2 keybrandPosition = Projectile.Center + Main.rand.NextVector2Circular(35f, 35f);

            //    FadingParticle ringParticle = new FadingParticle();
            //    ringParticle.SetBasicInfo(TextureAssets.Extra[174], null, Vector2.Zero, keybrandPosition);
            //    ringParticle.SetTypeInfo(40); // 生命周期：40帧

            //    // 颜色：橙黄光环
            //    Color orangeColor = new Color(1f, 0.6f, 0.2f, 1f);
            //    ringParticle.ColorTint = orangeColor;
            //    ringParticle.ColorTint.A = 200;

            //    // 大小与扩散
            //    ringParticle.Scale = Vector2.One * Main.rand.NextFloat(0.6f, 1.0f);
            //    ringParticle.ScaleVelocity = Vector2.One * 0.1f;
            //    ringParticle.ScaleAcceleration = -ringParticle.ScaleVelocity / 40f; // 让扩散逐渐减缓

            //    // 淡入淡出时间
            //    ringParticle.FadeInNormalizedTime = 0.1f;
            //    ringParticle.FadeOutNormalizedTime = 0.9f;

            //    // 加入粒子系统
            //    Main.ParticleSystem_World_OverPlayers.Add(ringParticle);
            //}

            {
                // ✦ Keybrand 原版粒子特效：在以弹幕中心为圆心、半径35的范围内随机一个点
                Vector2 randomOffset = Main.rand.NextVector2Circular(35f, 35f);
                Vector2 keybrandPosition = Projectile.Center + randomOffset;

                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Keybrand,
                    new ParticleOrchestraSettings
                    {
                        PositionInWorld = keybrandPosition
                    },
                    Projectile.owner
                );
            }


            // 如果在冷却中，忽略
            if (Projectile.localAI[1] > 0)
                return false;

            // 设置穿墙状态：只持续X帧
            Projectile.tileCollide = false;
            Projectile.localAI[0] = 50;



            // 如果还在冷却中，则忽略本次碰撞
            if (Projectile.localAI[1] > 0)
                return false;

            // 设置碰撞冷却为10帧
            Projectile.localAI[1] = 10;

            // 减少穿透次数
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // ✦ 发射 FossilShard 弹片（固定方向：正上方 ±45°）
            for (int i = 0; i < Main.rand.Next(5, 7); i++)
            {
                // 以正上为中心 ±45°
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f));
                Vector2 shardDirection = -Vector2.UnitY.RotatedBy(angleOffset);

                // 随机速度与抖动
                float speed = Main.rand.NextFloat(6f, 9f);
                Vector2 shardVelocity = shardDirection * speed;

                // 发射弹片
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shardVelocity,
                    ModContent.ProjectileType<EarthenJavSHARD>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }


            // ✦ 撞击特效粒子
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0, 0, 150, default, 1.2f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0, 0, 150, default, 1.2f);
            }

            // ✦ 屏幕震动效果
            float shakePower = 1.5f;
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // ✦ 以绝对正上方为基准偏转 ±30°
            float randomAngle = MathHelper.ToRadians(Main.rand.Next(-60, 61));
            Vector2 baseDirection = -Vector2.UnitY; // 正上方方向（注意Y轴向下为正）
            Vector2 newDirection = baseDirection.RotatedBy(randomAngle);

            // 保持原有速度大小，只改变方向
            Projectile.velocity = newDirection * Projectile.velocity.Length();

            return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 粉碎
        }

    }
}
