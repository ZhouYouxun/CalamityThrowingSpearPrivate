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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ElementalArkJav
{
    public class ElementalArkJavFragments : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/ElementalArkJav/EAJFragment";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        private bool hitOnce = false;
        private int hitTimer = 0;
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
            Projectile.penetrate = 3; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 随机生成粒子效果在弹幕后方
            if (Main.rand.NextBool(3)) // 每3帧左右有几率生成粒子
            {
                Vector2 particlePosition = Projectile.Center - Projectile.velocity * Main.rand.NextFloat(0.5f, 1.5f);
                Color particleColor = Main.rand.NextBool() ? Color.OrangeRed : Color.White;
                float particleScale = Main.rand.NextFloat(0.2f, 0.5f);
                Particle critSpark = new CritSpark(particlePosition, -Projectile.velocity * Main.rand.NextFloat(0.5f, 1.5f), Color.White, particleColor, particleScale * 5f, Main.rand.Next(20) + 10, 0.1f, 3);
                GeneralParticleHandler.SpawnParticle(critSpark);
            }

            if (hitOnce)
            {
                Projectile.velocity *= 0.95f;
                Projectile.alpha += 10;
                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                    return;
                }

                hitTimer++;
                if (hitTimer >= 15)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.velocity *= 0.995f;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hitOnce)
            {
                hitOnce = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 在弹幕死亡时释放左右的 EonBolt 弹幕
            Vector2 leftBolt = Projectile.velocity.RotatedBy(-MathHelper.PiOver4);
            Vector2 rightBolt = Projectile.velocity.RotatedBy(MathHelper.PiOver4);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftBolt, ModContent.ProjectileType<ElementalArkJavEonBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightBolt, ModContent.ProjectileType<ElementalArkJavEonBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }



    }
}