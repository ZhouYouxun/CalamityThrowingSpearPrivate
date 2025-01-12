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
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles.Magic;
using Terraria.Audio;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC
{
    public class GalvanizingGlaiveJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/GalvanizingGlaiveC/GalvanizingGlaiveJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            Vector2 drawPosition = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 55;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }



        public override void AI()
        {
            // 控制旋转方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Projectile.velocity *= 0.93f;

            // Lighting - 添加亮白色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.55f);

            // 螺旋状粒子特效
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 15f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Projectile.position;
                    dustPos -= Projectile.velocity * ((float)i * 0.25f);
                    Projectile.alpha = 255;

                    // 创建两种不同颜色的粒子（Dust）
                    int dustType = i == 0 ? DustID.Electric : DustID.Torch; // 根据需要选择适合的 Dust 类型
                    int dusty = Dust.NewDust(dustPos, 1, 1, dustType, 0f, 0f, 0, default, 1f);
                    Main.dust[dusty].noGravity = true;
                    Main.dust[dusty].position = dustPos;
                    Main.dust[dusty].scale = (float)Main.rand.Next(70, 110) * 0.013f;
                    Main.dust[dusty].velocity *= 0.2f;
                }

                Projectile.ai[0] += 1f;
                if (Projectile.ai[0] == 48f)
                {
                    Projectile.ai[0] = 0f;
                }
                else
                {
                    Vector2 randVector = new Vector2(5f, 10f);

                    for (int j = 0; j < 2; j++)
                    {
                        int dustType = j == 0 ? DustID.Electric : DustID.Torch; // 同样使用不同的粒子类型
                        Vector2 randDustPos = Vector2.UnitX * -12f;
                        randDustPos = -Vector2.UnitY.RotatedBy((double)(Projectile.ai[0] * 0.1308997f + (float)j * 3.14159274f), default) * randVector * 1.5f;
                        int dusty2 = Dust.NewDust(Projectile.Center, 0, 0, dustType, 0f, 0f, 160, default, 1f);
                        Main.dust[dusty2].scale = 0.75f;
                        Main.dust[dusty2].noGravity = true;
                        Main.dust[dusty2].position = Projectile.Center + randDustPos;
                        Main.dust[dusty2].velocity = Projectile.velocity;
                    }
                }
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 baseVelocity = Projectile.velocity; // 基础速度（当前弹幕的速度方向）
            float baseSpeed = baseVelocity.Length(); // 基础速度的大小

            Player owner = Main.player[Projectile.owner];
            bool hasAdrenalineMode = owner.HasBuff(ModContent.BuffType<AdrenalineMode>());

            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果

            //肾上腺素期间增强移除
            /*if (Main.rand.NextFloat() < 0.8f)
            {
                // 如果玩家拥有 AdrenalineMode Buff，将 GalvanizingGlaiveJavGaussEnergy 的伤害增加至 1.25
                float energyDamageMultiplier = hasAdrenalineMode ? 1.25f : 0.5f;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 spawnVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45)) * 0.6f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, spawnVelocity, ModContent.ProjectileType<GalvanizingGlaiveJavGaussEnergy>(), (int)(Projectile.damage * energyDamageMultiplier), Projectile.knockBack, Projectile.owner);
                }
            }
            else
            {
                // 如果玩家拥有 AdrenalineMode Buff，将 GalvanizingGlaiveJavGaussFlux 的伤害增加至 7.5
                float fluxDamageMultiplier = hasAdrenalineMode ? 7.5f : 3f;

                Vector2 fluxVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * 0.8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fluxVelocity, ModContent.ProjectileType<GalvanizingGlaiveJavGaussFlux>(), (int)(Projectile.damage * fluxDamageMultiplier), Projectile.knockBack, Projectile.owner);
            }*/

            // 生成粒子效果
            float[] particleAngleOffsets = { -2.3f, -1.15f, 1.15f, 2.3f };

            for (int i = 0; i < 4; i++)
            {
                Vector2 particleVelocity = Projectile.velocity.RotatedBy(particleAngleOffsets[i]) * 0.5f;
                PointParticle spark = new PointParticle(Projectile.Center - Projectile.velocity + particleVelocity, particleVelocity, false, 5, 1.1f, Color.SkyBlue);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 播放音效或其他特效
            // SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }





    }
}