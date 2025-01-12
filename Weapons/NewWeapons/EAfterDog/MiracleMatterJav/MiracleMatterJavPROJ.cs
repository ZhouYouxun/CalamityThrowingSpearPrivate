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
using Terraria.Audio;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.MiracleMatterJav
{
    public class MiracleMatterJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/MiracleMatterJav/MiracleMatterJav";
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
            Projectile.penetrate = 8; // 允许8次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20; // 无敌帧冷却时间为14帧
        }
        public override void AI()
        {

            Vector2 armPosition = Projectile.Center;
            Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 0.45f;

            // 发光效果
            Color energyColor = Color.Orange;
            Vector2 verticalOffset = Vector2.UnitY.RotatedBy(Projectile.rotation) * 8f;
            if (Math.Cos(Projectile.rotation) < 0f)
                verticalOffset *= -1f;

            // 发射橙色光粒子
            if (Main.rand.NextBool(1))
            {
                // 使用默认的生成位置，不进行偏移
                SquishyLightParticle exoEnergy = new(tipPosition, -Vector2.UnitY.RotatedByRandom(0.39f) * Main.rand.NextFloat(0.4f, 1.6f), 0.28f, energyColor, 25);
                GeneralParticleHandler.SpawnParticle(exoEnergy);
            }


            // 增加透明度渐变
            Projectile.Opacity = Utils.GetLerpValue(0f, 3f, Projectile.timeLeft, true);

            // 添加光照
            DelegateMethods.v3_1 = energyColor.ToVector3();
            Utils.PlotTileLine(tipPosition - verticalOffset, tipPosition + verticalOffset, 10f, DelegateMethods.CastLightOpen);
            Lighting.AddLight(tipPosition, energyColor.ToVector3());




            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            if (Projectile.ai[0] == 0f)
            {
                Projectile.localAI[1] += 1f;
                if (Projectile.localAI[1] >= 60f)
                {
                    Projectile.velocity.X *= 0.99f;
                    Projectile.velocity.Y += 0.3f;

                    if (Projectile.velocity.Y > 16f)
                        Projectile.velocity.Y = 16f;
                }
            }

            int dustType = 171;
            if (Main.rand.NextBool(3))
            {
                dustType = 46;
            }
            if (Main.rand.NextBool(9))
            {
                //Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
            if (Projectile.ai[0] == 0f)
            {
                Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                Projectile.rotation += Projectile.spriteDirection * MathHelper.ToRadians(45f);
            }
            //Sticky Behaviour
            Projectile.StickyProjAI(15);
            if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity *= 0f;
            }
        }

        //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(20);

        //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        //{
        //    Projectile.ModifyHitNPCSticky(20);
        //}

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 调用原始的ModifyHitNPCSticky方法，确保粘附逻辑正常
            Projectile.ModifyHitNPCSticky(20);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);

            // 随机生成四种颜色的sparkColor
            Color sparkColor = Main.rand.Next(4) switch
            {
                0 => Color.Red,
                1 => Color.MediumTurquoise,
                2 => Color.Orange,
                _ => Color.LawnGreen,
            };

            // 从本体弹幕的周围随机360度发射DirectionalPulseRing粒子
            for (int i = 0; i < 2; i++)  // 每次生成两个粒子
            {
                // 生成随机角度
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

                // 将速度放大3~4倍 (选择倍数范围内的随机值)
                direction *= Main.rand.NextFloat(3f, 4f) * 0.8f; // 保持原先的粒子速度比例

                // 创建并发射DirectionalPulseRing粒子
                DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center, direction, sparkColor, new Vector2(1, 1), 0, Main.rand.NextFloat(0.2f, 0.35f), 0f, 40);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0] = 2f;
            Projectile.timeLeft = 300;
            return false;
        }

        public override void OnKill(int timeLeft)
        {

            // 1. 随机选择2~4个角度发射2~4个 MiracleMatterJavLight 弹幕
            int numProjectiles = Main.rand.Next(4, 7);  // 随机选择发射4到6个弹幕
            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));  // 随机角度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;  // 设置速度
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<MiracleMatterJavLight>(), (int)(Projectile.damage * 1.1f), Projectile.knockBack, Main.myPlayer);
            }

            // 2. 发射25个大小和速度各异的线性粒子特效
            for (int i = 0; i < 25; i++)
            {
                Vector2 trailPos = Projectile.position + new Vector2(Main.rand.NextFloat(-20, 20), Main.rand.NextFloat(-20, 20));
                float trailScale = Main.rand.NextFloat(0.5f, 1.5f);
                Color trailColor = Color.Lerp(Color.White, Color.Red, Main.rand.NextFloat());  // 假设的颜色渐变
                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 3. 魔法尘埃特效
            for (int i = 0; i < 75; i++)
            {
                float offsetAngle = MathHelper.TwoPi * i / 75f;
                float unitOffsetX = (float)Math.Pow(Math.Cos(offsetAngle), 3D);
                float unitOffsetY = (float)Math.Pow(Math.Sin(offsetAngle), 3D);

                Vector2 puffDustVelocity = new Vector2(unitOffsetX, unitOffsetY) * 5f;
                Dust magic = Dust.NewDustPerfect(Projectile.Center, 267, puffDustVelocity);  // 267为魔法尘埃的类型
                magic.scale = 1.8f;
                magic.fadeIn = 0.5f;
                magic.color = CalamityUtils.MulticolorLerp(i / 75f, CalamityUtils.ExoPalette);  // 使用 ExoPalette 的渐变效果
                magic.noGravity = true;
            }


            // 1. 生成较小的橙黄色和淡黄色爆炸特效（超新星的那个光圈逐渐缩小的特效）
            Vector2 spawnPosition = Projectile.Center;
            Color lightYellowColor = Color.LightYellow;
            float smallerScale = 1.5f; // 较小的扩散大小
            float rotationSpeed = Main.rand.NextFloat(-10f, 10f); // 随机旋转速度

            // 创建两个爆炸粒子，颜色为橙黄色和淡黄色// 调整初始大小以使特效更小
            Particle yellowExplosion = new CustomPulse(spawnPosition, Vector2.Zero, lightYellowColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.5f, 0.5f), -rotationSpeed, smallerScale, smallerScale - 0.5f, 15);

            GeneralParticleHandler.SpawnParticle(yellowExplosion);

        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300); // 超位崩解
            SoundEngine.PlaySound(SoundID.Item132.WithVolumeScale(2.5f), Projectile.Center);
        }


    }
}
