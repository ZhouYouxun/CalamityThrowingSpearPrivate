//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch.FTDragon
//{
//    public class FinishingTouchDragon3Tail : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";

//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 70;
//            Projectile.height = 70;
//            Projectile.friendly = true;
//            Projectile.ignoreWater = true;
//            Projectile.netImportant = true;
//            Projectile.tileCollide = false;
//            Projectile.alpha = 255;
//            Projectile.penetrate = -1;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = 50;
//            //Projectile.ArmorPenetration = 15;
//        }
//        private Queue<Vector2> positionHistory = new Queue<Vector2>(); // 存储历史位置

//        public override void AI()
//        {
//            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.5f);

//            int prevIndex = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[0]);
//            if (prevIndex < 0 || !Main.projectile[prevIndex].active)
//            {
//                Projectile.Kill();
//                return;
//            }

//            Projectile prev = Main.projectile[prevIndex];

//            Vector2 offset = prev.Center - Projectile.Center;

//            // 平滑旋转追随前一节
//            float desiredRotation = offset.ToRotation();
//            float angleDiff = MathHelper.WrapAngle(desiredRotation - Projectile.rotation);
//            Projectile.rotation += angleDiff * 0.25f + MathHelper.PiOver2; // 平滑追随 + 调整 sprite 对齐

//            // 保持 scale 和宽高与前一节一致
//            float scale = MathHelper.Clamp(prev.scale, 0.5f, 3f);
//            Projectile.scale = scale;
//            Projectile.width = Projectile.height = (int)(10f * scale);

//            // 适合的节距
//            float followDistance = 70f * scale;

//            // 精准跟随前一节位置
//            if (offset != Vector2.Zero)
//                Projectile.Center = prev.Center - Vector2.Normalize(offset) * followDistance;

//            Projectile.spriteDirection = (offset.X > 0f) ? 1 : -1;

//            // 淡入
//            if (Projectile.alpha > 0)
//                Projectile.alpha -= 40;
//            if (Projectile.alpha < 0)
//                Projectile.alpha = 0;
//        }

//        /// <summary>
//        /// 释放紫色粒子特效
//        /// </summary>
//        private void SpawnDustEffect()
//        {
//            for (int i = 0; i < 8; i++)
//            {
//                int dustType = Main.rand.NextBool() ? 37 : 173;
//                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
//                dust.velocity *= 0.3f;
//                dust.noGravity = true;
//            }
//        }
//    }
//}
