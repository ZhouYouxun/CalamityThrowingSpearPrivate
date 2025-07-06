//using System;
//using System.IO;
//using CalamityMod.Particles;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using CalamityMod;


//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav
//{
//    public class ChaosWindJavWind : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
//        public Vector2 SpinCenter { get; set; }
//        public float SpinDirection { get; set; }
//        public ref float SpinOffsetAngle => ref Projectile.ai[0];

//        public static int Lifetime => 60;
//        public static float SpinConvergencePower => 3.7f;

//        public override void SetStaticDefaults()
//        {
//            Main.projFrames[Type] = 6;
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = Projectile.height = 42;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = 3;
//            Projectile.timeLeft = 120;
//            Projectile.light = 0.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = true; // 允许与方块碰撞
//            Projectile.extraUpdates = 1; // 额外更新次数
//            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
//            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
//            //Projectile.timeLeft = Lifetime;
//            //Projectile.Opacity = 0f;
//        }

//        public override void AI()
//        {
//            // 曲线飞行轨迹
//            float spinAngularVelocity = Utils.Remap(Time, 0f, 45f, MathHelper.Pi / 75f, MathHelper.Pi / 359f);
//            float spinRadius = (1f - MathF.Pow(Utils.GetLerpValue(0f, Lifetime - 4f, Time, true), SpinConvergencePower)) * 600f;
//            SpinOffsetAngle += spinAngularVelocity * SpinDirection;
//            Projectile.Center = SpinCenter + SpinOffsetAngle.ToRotationVector2() * spinRadius;

//            // 根据位置差异旋转
//            Projectile.rotation = (Projectile.position - Projectile.oldPosition).X * 0.01f;

//            // 帧动画切换
//            Projectile.frameCounter++;
//            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

//            // 渐变透明度
//            //Projectile.Opacity = Utils.GetLerpValue(0f, 20f, Time, true) * Utils.GetLerpValue(0f, 20f, Projectile.timeLeft, true);

//            // 天蓝色粒子效果
//            if (Main.rand.NextBool(5))
//            {
//                int d = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 136, new Color(172, 238, 255), 1.4f);
//                Main.dust[d].noGravity = true;
//                Main.dust[d].fadeIn = 1.5f;
//                Main.dust[d].velocity = -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * 0.2f;
//            }

//            Time++;
//        }
//        public ref float Time => ref Projectile.ai[1];

//        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到x为止

//        public override bool PreDraw(ref Color lightColor)
//        {
//            // 绘制拖尾效果
//            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
//            return true;
//        }
       


//    }
//}
