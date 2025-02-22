//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ModLoader;
//using Terraria;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
//{
//    internal class SunsetCConceptLeftEXP : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
//        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

//        public override void SetDefaults()
//        {
//            Projectile.width = 1920;
//            Projectile.height = 1080;
//            Projectile.friendly = true;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 3;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = -1;
//        }

//        public override void AI()
//        {

//        }
//        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
//        {
//            base.ModifyHitNPC(target, ref modifiers);

//            // 计算目标最大生命值的 1% 作为最终伤害
//            float forcedDamage = target.lifeMax * 0.01f;

//            // 确保伤害不会低于 1
//            forcedDamage = Math.Max(forcedDamage, 1);

//            // 设置最终伤害
//            modifiers.FinalDamage.Base = (int)forcedDamage;
//        }

//        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//        {
//            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒
//        }
//    }
//}
