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
using CalamityMod.Buffs.DamageOverTime;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav
{
    public class BraisedPorkJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/BraisedPorkJav/BraisedPorkJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawAfterimagesSmartRotation(lightColor);
            return false;
        }

        private void DrawAfterimagesSmartRotation(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int y = frameHeight * Projectile.frame;
            Rectangle frame = new Rectangle(0, y, texture.Width, frameHeight);
            Vector2 origin = frame.Size() / 2f;
            Vector2 centerOffset = Projectile.Size / 2f;
            Color baseColor = Projectile.GetAlpha(lightColor);
            float scale = Projectile.scale;

            bool facingLeft = Projectile.velocity.X < 0;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + centerOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                float rotation = Projectile.oldRot[i] + (facingLeft ? MathHelper.PiOver2 : 0f);
                SpriteEffects fx = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Color color = baseColor * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);

                Main.spriteBatch.Draw(texture, drawPos, frame, color, rotation, origin, scale, fx, 0f);
            }

            // 绘制本体（非必须，如果主绘制中会画就不画）
            Vector2 currentPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            float currentRotation = Projectile.rotation + (facingLeft ? MathHelper.PiOver2 : 0f);
            Main.spriteBatch.Draw(texture, currentPos, frame, baseColor, currentRotation, origin, scale, facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2; // 允许2次伤害
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
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;



            {
                // 优化后的飞行期间尾迹腐化紫黑烟雾
                for (int d = 0; d < 3; d++) // 数量小幅减少防止堆积，但每帧触发
                {
                    int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DemonTorch, 0f, -Main.rand.NextFloat(0.5f, 1.2f), 80, default, 0.35f);
                    Dust dust = Main.dust[dustIndex];

                    dust.velocity = Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.2f); // 始终正上方
                    dust.position -= Projectile.velocity / 4f * d; // 延展拖尾
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.3f, 0.45f); // 大幅缩小体积
                    dust.noLight = true;
                    dust.color = Color.Lerp(Color.Purple, Color.Black, 0.5f); // 紫黑腐化色
                }

                // 紫黑腐化轻烟雾尾迹（多、小、柔）
                if (Main.rand.NextBool(2)) // 高密度
                {
                    Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                    Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Demonite, vel, 100, Color.Lerp(Color.Purple, Color.Black, 0.5f), Main.rand.NextFloat(0.6f, 0.9f));
                    smoke.noGravity = true;
                }

                // 周期性生成小型 HeavySmokeParticle 扩散漂浮
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Particle darkSmoke = new HeavySmokeParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        Color.Lerp(Color.Purple, Color.Black, 0.6f),
                        Main.rand.Next(20, 32), // 生命周期
                        Main.rand.NextFloat(0.4f, 0.7f), // 小体积
                        0.8f, // 轻度透明
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(darkSmoke);
                }

            }



            // 每帧增加 ai[x] 计数
            Projectile.ai[1]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[1] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrainRot>(), 300);
            // 生成 BraisedPorkJavCloud 弹幕
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BraisedPorkJavCloud>(), damageDone, Projectile.knockBack, Projectile.owner, 0f, 1.0f);

            {
                // 替代原浓烟：生成大量小体积黑色浓烟
                for (int i = 0; i < 20; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                    Particle smallSmoke = new HeavySmokeParticle(
                        Projectile.Center + offset,
                        new Vector2(0, -Main.rand.NextFloat(1f, 2f)),
                        Color.Lerp(Color.Purple, Color.Black, 0.7f),
                        Main.rand.Next(20, 35),
                        Main.rand.NextFloat(0.4f, 0.6f),
                        0.9f,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(smallSmoke);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {

            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 5.2f, Pitch = -0.2f }, Projectile.Center);

            // 抛射一连串紫黑色的重型烟雾粒子
            Color smokeColor = Color.Lerp(Color.Purple, Color.Black, 0.5f); // 紫黑色
            int particleCount = 5; // 生成的粒子数量
            float delay = 0.1f; // 每个粒子生成的延迟时间

            for (int i = 0; i < particleCount; i++)
            {
                // 使用延迟位置来生成连续效果
                Vector2 offset = new Vector2(0, -1) * (5f + i * delay);
                Particle smoke = new HeavySmokeParticle(Projectile.Center + offset, new Vector2(0, -1) * 5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), 1.0f, MathHelper.ToRadians(2f), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            {
                // 腐化紫黑尘埃爆发
                for (int i = 0; i < 50; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Dust corruptionDust = Dust.NewDustPerfect(Projectile.Center, DustID.CorruptGibs, velocity, 80, Color.Purple, Main.rand.NextFloat(0.8f, 1.4f));
                    corruptionDust.noGravity = true;
                }

              


            }
        }





    }
}
