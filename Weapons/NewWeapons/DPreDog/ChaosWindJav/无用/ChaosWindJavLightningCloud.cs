//using CalamityMod.Sounds;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.Audio;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav
//{
//    public class ChaosWindJavLightningCloud : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
//        public override void SetStaticDefaults()
//        {
//            Main.projFrames[Projectile.type] = 6;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 54;
//            Projectile.height = 28;
//            Projectile.tileCollide = false;
//            Projectile.ignoreWater = true;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.timeLeft = 120;
//            Projectile.Opacity = 0f;
//            Projectile.penetrate = -1;
//        }

//        private bool hasStruckLightning = false; // 记录是否已生成闪电

//        public override void AI()
//        {
//            Projectile.frameCounter++;
//            if (Projectile.frameCounter > 6)
//            {
//                Projectile.frameCounter = 0;
//                Projectile.frame++;

//                int maxFrame = Projectile.timeLeft < 60 ? 6 : 3;
//                if (Projectile.frame >= maxFrame)
//                    Projectile.frame = 0;

//                // 在最后 60 帧时且尚未生成闪电时劈下一道闪电
//                if (Projectile.frame == 5 && Main.myPlayer == Projectile.owner && Projectile.timeLeft <= 60 && !hasStruckLightning)
//                {
//                    SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, Projectile.Center);
//                    float ai = Main.rand.Next(100);
//                    Vector2 velocity = Vector2.UnitY * 7f;

//                    // 生成我方激光弹幕，并设置穿透次数
//                    int lightningProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Bottom, velocity, ProjectileID.CultistBossLightningOrbArc, Projectile.damage, 0f, Projectile.owner, MathHelper.PiOver2, ai);

//                    // 设置为友方并增加穿透次数
//                    Projectile proj = Main.projectile[lightningProjectile];
//                    proj.friendly = true;
//                    proj.hostile = false;
//                    proj.penetrate = 10; // 设置穿透次数
//                    proj.localNPCHitCooldown = 50; // 无敌帧冷却时间
//                    proj.usesLocalNPCImmunity = true;

//                    // 标记已生成闪电
//                    hasStruckLightning = true;
//                }
//            }

//            if (Projectile.timeLeft < 30)
//                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 0.14f);
//            else
//                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.33f);
//        }

















//    }
//}
