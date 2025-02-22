using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetRightEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 每帧在 500×500 碰撞体积内随机选一个点生成太阳耀斑粒子
            Vector2 randomPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-250, 250), Main.rand.NextFloat(-250, 250));

            ParticleOrchestrator.RequestParticleSpawn(
                clientOnly: false,
                ParticleOrchestraType.Excalibur,
                new ParticleOrchestraSettings { PositionInWorld = randomPos },
                Projectile.owner
            );
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}
