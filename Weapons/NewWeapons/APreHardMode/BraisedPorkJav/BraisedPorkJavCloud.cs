using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav
{
    public class BraisedPorkJavCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public bool StartFading = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 600;

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
            // Makes the projectile deaccelerate.
            Projectile.velocity *= ShaderainStaff.DeaccelerationStrenght;

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
            // 恶心腐化毒雾特效
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.8f, 1.5f);

                // 紫黑与腐绿交替混色
                Color color = Main.rand.NextBool()
                    ? Color.Lerp(new Color(80, 0, 100), Color.Black, 0.4f) // 紫黑
                    : Color.Lerp(new Color(50, 150, 50), Color.Black, 0.3f); // 腐绿

                Dust pusDust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Demonite,
                    velocity,
                    80,
                    color,
                    Main.rand.NextFloat(1.4f, 1.7f)
                );
                pusDust.noGravity = true;
            }

            target.AddBuff(ModContent.BuffType<BrainRot>(), 120);
        }



    }
}
