using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.BraisedPorkSpear
{
    internal class BraisedPorkSpearCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewSpears.APreHardMode";
        public bool StartFading = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 60;

            Projectile.width = Projectile.height = 28;

            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 25; // 无敌帧冷却时间为25帧
            Projectile.penetrate = 5; // 允许5次伤害

        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f; // 让速度每帧衰减

            // Does the animation of the projectile, the number in the middle determines the speed of the animatino.
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 8 % Main.projFrames[Projectile.type];

            // If the projectile is about to die or it collides with a tile, start fading.
            // Depending on how strong the fade-out speed is, it'll adapt by the amount of time left for it do die.
            // Just be careful to not make it too slow and it starts fading immeadiately.
            if (Collision.SolidCollision(Projectile.Center, Projectile.width, Projectile.height) || Projectile.timeLeft < (255 / ShaderainStaff.FadeoutSpeed))
                StartFading = true;

            // The projectile will fade away.
            if (StartFading)
                Projectile.alpha += ShaderainStaff.FadeoutSpeed;

            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Makes a dust effect.
            for (int dustIndex = 0; dustIndex < 40; dustIndex++)
            {
                // I choose .position (Which is the top left) instead of .Center because Dust.NewDust was made to spawn given .position.
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Demonite, 0, 0, 0, default, 0.5f);
            }

            target.AddBuff(ModContent.BuffType<BrainRot>(), 120);
        }
    }
}
