using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC
{
    public class AmidiasTridentJavWhirlpool : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制本体（带独特数学效果）
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // 独特数学扰动：基于时间和位置制造微小闪烁漂移（看起来像“时空扭曲”）
            float time = Main.GameUpdateCount * 0.05f;
            Vector2 waveOffset = new Vector2(
                (float)Math.Sin(time + Projectile.Center.Y * 0.05f),
                (float)Math.Cos(time + Projectile.Center.X * 0.05f)
            ) * 3f;

            // 独特颜色变换：基于 Sin 时间函数在蓝色-青色之间呼吸变色
            float colorFactor = (float)Math.Sin(time * 2f) * 0.5f + 0.5f;
            Color drawColor = Color.Lerp(Color.DeepSkyBlue, Color.Cyan, colorFactor) * 0.8f;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition + waveOffset,
                frame,
                drawColor,
                Projectile.rotation + (float)Math.Sin(time) * 0.05f, // 微小旋转扰动
                origin,
                Projectile.scale * (1f + (float)Math.Sin(time * 1.5f) * 0.05f), // 微小缩放脉动
                effects,
                0
            );
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 58;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 270;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧

        }

        public override void AI()
        {
            // 一直缓慢自身旋转
            Projectile.rotation += 0.1f;

            // 查找最近敌人用于优雅锁定
            NPC target = null;
            float maxDistance = 600f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && !npc.friendly)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < maxDistance)
                    {
                        maxDistance = distance;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                // 优雅插值追踪：使用 Lerp 平滑过渡方向
                Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 9f; // 最大追踪速度
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.05f); // 平滑跟踪

                // 产生精美海蓝螺旋 Dust 特效
                float time = Main.GameUpdateCount * 0.15f;
                int points = 6;
                float radius = 24f;
                for (int i = 0; i < points; i++)
                {
                    float angle = time + MathHelper.TwoPi / points * i;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, offset.SafeNormalize(Vector2.Zero) * 0.5f, 100, Color.Aqua, 1.1f);
                    d.noGravity = true;
                }

                // 气泡粒子混用
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WaterCandle, Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1f, 3f), 80, Color.LightBlue, Main.rand.NextFloat(0.8f, 1.2f));
                    d.noGravity = true;
                }
            }
            else
            {
                // 无敌人则缓慢漂移，速度极低
                Projectile.velocity *= 0.98f;
            }

            // 柔和光效
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.4f);
        }



        // 阻止前30帧内对敌人造成伤害
        //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        //{
        //    if (Projectile.timeLeft > 90)  // 前30帧内不造成伤害
        //    {
        //        modifiers.SetMaxDamage(0);  // 设置伤害为0
        //    }
        //}



        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(30, 255, 253);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int k = 0; k < 20; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Water, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 0, new Color(0, 142, 255), 1f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 300); // 激流
        }
    }
}
