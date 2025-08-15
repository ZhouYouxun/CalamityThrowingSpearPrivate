using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC
{
    public class AmidiasTridentJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";

        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/AmidiasTridentC/AmidiasTridentJav";
        private bool stuck;
        private NPC stuckTarget;
        private bool returning;
        private float lockedRotation;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 38;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                .UseColor(new Color(120, 220, 255))            // 浅海蓝
                .UseSecondaryColor(new Color(180, 255, 255))   // 极浅蓝青
                .Apply();

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    ratio => MathHelper.SmoothStep(12f, 2f, ratio),
                    _ => Color.Cyan,
                    _ => Projectile.Size * 0.5f,
                    shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                ),
                100
            );

            Main.spriteBatch.ExitShaderRegion();

            // 绘制本体（稳定简洁版）
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // 稳定轻微缩放呼吸（可选）
            float time = Main.GameUpdateCount * 0.05f;
            float scale = Projectile.scale * (1f + (float)Math.Sin(time * 1.2f) * 0.02f); // 仅 ±2% 呼吸

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Projectile.GetAlpha(lightColor), // 使用 lightColor，无额外染色
                Projectile.rotation,
                origin,
                scale,
                effects,
                0
            );

            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 9; // 伤害次数
            Projectile.timeLeft = 450;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 不穿透物块
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow;

        }
        public override void OnSpawn(IEntitySource source)
        {
            // 为每颗弹幕生成独立的随机相位偏移 [0, 2π)
            Projectile.localAI[0] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.6f);

            if (stuck && stuckTarget != null && stuckTarget.active)
            {
                Projectile.Center = stuckTarget.Center;
                Projectile.velocity = Vector2.Zero;
            }
            else if (stuck && stuckTarget == null)
            {
                Projectile.velocity = Vector2.Zero;
            }

            if (!stuck && !returning)
            {
                {
                    // 海蓝三螺旋粒子特效（混合 SparkParticle）
                    float time = Main.GameUpdateCount * 0.12f; // 稍慢旋转速率
                    int spiralCount = 3; // 三螺旋
                    float radius = 14f;

                    for (int i = 0; i < spiralCount; i++)
                    {
                        float angle = time + MathHelper.TwoPi / spiralCount * i;
                        Vector2 offset = angle.ToRotationVector2() * radius;

                        // Dust 粒子（33号烟雾，透明度高，柔和）
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            33,
                            -Projectile.velocity * 0.05f,
                            100,
                            Color.Cyan * 0.8f,
                            Main.rand.NextFloat(0.8f, 1.2f)
                        );
                        d.noGravity = true;

                        // 混合 SparkParticle
                        if (Main.rand.NextBool(3))
                        {
                            SparkParticle spark = new SparkParticle(
                                Projectile.Center + offset,
                                offset.SafeNormalize(Vector2.Zero) * 0.5f,
                                false,
                                20,
                                Main.rand.NextFloat(0.5f, 0.8f),
                                Color.Lerp(Color.Cyan, Color.White, 0.3f)
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }

                }
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                Projectile.rotation = lockedRotation;
            }

            if (Main.player[Projectile.owner].controlUseTile && stuck && !returning)
            {
                returning = true;
                stuck = false;
                Projectile.tileCollide = false;
                Projectile.velocity = (Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 20f;
                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/海王三叉戟回收") with { Volume = 1.0f, Pitch = 0.0f }, Projectile.Center);
            }

            if (returning)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                Player player = Main.player[Projectile.owner];
                Vector2 toPlayer = player.Center - Projectile.Center;
                float distance = toPlayer.Length();
                Vector2 direction = toPlayer.SafeNormalize(Vector2.UnitY);

                // 设置基础飞行速度更快（28f，可调）
                float speed = 28f;

                // 螺旋参数
                float spiralRadius = MathHelper.Clamp(distance * 0.1f, 8f, 48f); // 根据距离动态缩放螺旋半径
                float spiralSpeed = Main.GameUpdateCount * 0.4f + Projectile.localAI[0]; // 螺旋角度随时间变化

                Vector2 spiralOffset = new Vector2(
                    (float)Math.Cos(spiralSpeed),
                    (float)Math.Sin(spiralSpeed)
                ) * spiralRadius;

                // 综合速度向量
                Projectile.velocity = (direction * speed + spiralOffset * 0.5f).SafeNormalize(Vector2.UnitY) * speed;

                if (Projectile.Hitbox.Intersects(player.Hitbox))
                {
                    {
                        int whirlpoolCount = Main.rand.Next(1, 3);

                        for (int i = 0; i < whirlpoolCount; i++)
                        {
                            Vector2 direction1 = Main.rand.NextVector2Unit();
                            Vector2 spawnPos = player.Center + direction1 * 20f + Main.rand.NextVector2Circular(8f, 8f);
                            Vector2 velocity = direction1.RotatedByRandom(MathHelper.ToRadians(8)) * Main.rand.NextFloat(8f, 12f);

                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                spawnPos,
                                velocity,
                                ModContent.ProjectileType<AmidiasTridentJavWhirlpool>(),
                                (int)(Projectile.damage * 1.8f),
                                2f,
                                Projectile.owner
                            );
                        }
                    }
                    Projectile.Kill();
                }

            }




        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                stuck = true;
                lockedRotation = Projectile.rotation;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 900;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 180); // 激流

            if (!stuck && !returning)
            {
                stuck = true;
                stuckTarget = target;
                lockedRotation = Projectile.rotation;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 900;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (stuckTarget != null)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AmidiasTridentJavWaterWall>(), (int)(Projectile.damage * 1.5f), 5f, Projectile.owner);
            }
            else if (stuck)
            {
                Vector2 spawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-50, 50), -800);
                Vector2 velocity = Vector2.UnitY * 20f;
                int lightningProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, ProjectileID.CultistBossLightningOrbArc, (int)(Projectile.damage * 2.1f), 0f, Projectile.owner, MathHelper.PiOver2, Main.rand.Next(100));
                Projectile proj = Main.projectile[lightningProjectile];
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = -1;
                proj.localNPCHitCooldown = 60;
                proj.usesLocalNPCImmunity = true;
                proj.extraUpdates = 4;
            }

            // 复杂的海蓝色特效
            for (int i = 0; i < 40; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Main.rand.NextVector2Circular(10f, 10f), 150, Color.Cyan, 1.5f);
                d.noGravity = true;
            }
        }



    }
}
