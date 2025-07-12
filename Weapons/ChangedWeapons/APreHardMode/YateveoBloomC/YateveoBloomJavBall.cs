//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod;

//namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC
//{
//    internal class YateveoBloomJavBall : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
//        }
//        private bool stuck = false; // 是否已固定在墙上

//        public override bool PreDraw(ref Color lightColor)
//        {
//            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
//            return false;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 16;
//            Projectile.height = 16;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1; // 可击中次数
//            Projectile.timeLeft = 300;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = true;
//            Projectile.extraUpdates = 0; // 可调节飞行平滑度
//            Projectile.aiStyle = ProjAIStyleID.Arrow;

//        }

//        public override void OnSpawn(IEntitySource source)
//        {
//            // 弹幕生成时执行，用于初始化粒子或播放生成音效
//        }

//        public override void AI()
//        {
//            // 主循环，每帧执行

//            // === 垂直贴图对齐飞行方向 ===
//            // Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

//            // === 45度倾斜贴图对齐飞行方向 ===
//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

//            if (Projectile.velocity.X > 0)
//                Projectile.rotation += 0.15f;
//            else
//                Projectile.rotation -= 0.15f;

//            // 飞行期间生成收敛绿色花粉尘
//            if (Main.rand.NextBool(3)) // 控制密度
//            {
//                Dust d = Dust.NewDustPerfect(
//                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
//                    DustID.GrassBlades,
//                    Projectile.velocity * 0.1f,
//                    100,
//                    Color.ForestGreen,
//                    Main.rand.NextFloat(0.6f, 0.9f)
//                );
//                d.noGravity = true;
//            }

//            // 可在此添加不同摆动方式、额外特效、速度变化、粒子拖尾等
//        }



//        public override bool OnTileCollide(Vector2 oldVelocity)
//        {
//            if (!stuck)
//            {
//                stuck = true;
//                Projectile.velocity = Vector2.Zero;
//                Projectile.timeLeft = 200; // 停留 200 帧后自动消失
//                Projectile.tileCollide = false; // 防止再触发
//            }
//            return false; // 防止立即消失
//        }

//        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//        {
//            // 播放击中草音效
//            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

//            // 给目标添加中毒效果
//            target.AddBuff(BuffID.Poisoned, 300);

//            // 播放环形绿色 Dust 特效
//            for (int i = 0; i < 12; i++)
//            {
//                float angle = MathHelper.TwoPi * i / 12f;
//                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
//                Dust d = Dust.NewDustPerfect(
//                    target.Center,
//                    DustID.Grass,
//                    velocity,
//                    100,
//                    Color.ForestGreen,
//                    Main.rand.NextFloat(0.8f, 1.2f)
//                );
//                d.noGravity = true;
//            }
//        }

//        public override void OnKill(int timeLeft)
//        {
//            // 结束时生成淡绿色花瓣散开特效
//            for (int i = 0; i < 16; i++)
//            {
//                float angle = MathHelper.TwoPi * i / 16f;
//                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
//                Dust d = Dust.NewDustPerfect(
//                    Projectile.Center,
//                    DustID.GrassBlades,
//                    velocity,
//                    100,
//                    Color.ForestGreen,
//                    Main.rand.NextFloat(0.8f, 1.2f)
//                );
//                d.noGravity = true;
//            }
//        }


//    }
//}
