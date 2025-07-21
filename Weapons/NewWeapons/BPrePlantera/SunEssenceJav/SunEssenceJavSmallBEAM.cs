//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod.Projectiles.BaseProjectiles;
//using CalamityMod.Sounds;
//using CalamityMod;
//using ReLogic.Content;
//using System.IO;
//using Terraria.Audio;
//using Terraria.ID;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
//{
//    public class SunEssenceJavSmallBEAM : BaseLaserbeamProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
//        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

//        private Color startingColor = new Color(255, 255, 224); // 浅黄色
//        private Color secondColor = new Color(255, 255, 255);   // 白色

//        public override float MaxScale => 0.7f;
//        public override float MaxLaserLength => 799.999999f; // 最大长度
//        public override float Lifetime => 20f;
//        public override Color LaserOverlayColor => CalamityUtils.ColorSwap(startingColor, secondColor, 0.9f);
//        public override Color LightCastColor => LaserOverlayColor;
//        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavSmallBEAM").Value;
//        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavSmallBEAMMiddle").Value;
//        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJavSmallBEAMEnd").Value;

//        public override void SetDefaults()
//        {
//            Projectile.width = Projectile.height = 22;
//            Projectile.friendly = true;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1;
//            Projectile.alpha = 255;
//            Projectile.localNPCHitCooldown = -1;
//            Projectile.usesLocalNPCImmunity = true;
//        }

//        public override bool PreAI()
//        {
//            // Initialization. Using the AI hook would override the base laser's code, and we don't want that.
//            if (Projectile.localAI[0] == 0f)
//            {
//                if (Main.rand.NextBool())
//                {
//                    secondColor = new Color(255, 255, 224);
//                    startingColor = new Color(255, 255, 255);
//                }
//                // 在生成时创建粒子效果
//                for (int i = 0; i < 10; i++)
//                {
//                    Vector2 offset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f) * 10f;
//                    Vector2 particlePosition = Projectile.Center + offset;
//                    Dust dust = Dust.NewDustPerfect(particlePosition, DustID.RedStarfish, Projectile.velocity * 0.5f, 0, new Color(255, 165, 0));
//                    dust.noGravity = true;
//                    dust.scale = 1.2f;
//                }
//            }
//            // 生成装饰粒子效果的代码
//            for (float offsetMultiplier = 0.2f; offsetMultiplier <= 0.8f; offsetMultiplier += 0.2f) // 设置多个生成点，间隔较小
//            {
//                Vector2 middlePosition = Projectile.Center + Projectile.velocity * (MaxLaserLength * offsetMultiplier);
//                if (Main.rand.NextBool(3)) // 增加生成频率
//                {
//                    for (int i = -1; i <= 1; i += 2) // 生成两个方向的粒子
//                    {
//                        Vector2 particleOffset = middlePosition + Projectile.velocity.RotatedBy(MathHelper.PiOver2 * i) * Main.rand.NextFloat(1f, 5f);
//                        Dust dust = Dust.NewDustPerfect(particleOffset, DustID.GoldFlame, new Vector2(Projectile.velocity.Y * i, Projectile.velocity.X * i) * 0.1f, 100, Color.Gold);
//                        dust.noGravity = true;
//                        dust.scale = 1.0f;
//                    }
//                }
//            }

//            return true;
//        }

//        public override bool ShouldUpdatePosition() => false;


//    }
//}
