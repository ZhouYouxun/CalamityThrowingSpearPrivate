//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    public class FinishingTouchDASHLocater : ModProjectile
//    {
//        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 完全透明贴图

//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 0;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = Projectile.height = 8;
//            Projectile.friendly = false;
//            Projectile.hostile = false;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 5; // 持续时间仅为5
//            Projectile.extraUpdates = 0;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = false;
//            Projectile.alpha = 255; // 完全透明
//            Projectile.DamageType = DamageClass.Default;
//        }

//        public override void AI()
//        {
//            // 不做任何操作，仅用于生成时在枪头位置做范围检测定位
//        }

//        public override bool? CanDamage() => false; // 不造成任何伤害

//        public override void OnKill(int timeLeft)
//        {
//            // 消失时不执行任何操作
//        }
//    }
//}
