//using CalamityMod.Particles;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.Audio;
//using Terraria.GameContent;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
//{
//    public class TheLastLanceDASHSUPER : ModProjectile, ILocalizedModType
//    {
//        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TheLastLance";

//        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
//        private const int MaxChargeTime = 30; // 蓄力时间为30帧
//        private const float DashSpeed = 32.5f * 0.75f; // 冲刺速度为默认速度的0.75倍
//        private const int MaxDashTime = 60000; // 冲刺时间无限制（60000帧）

//        private Vector2 lockedDirection; // 存储锁定的冲刺方向
//        private int dashTime = 0; // 冲刺已进行的时间
//        private bool isDashing = false; // 标记是否处于冲刺状态

//        public override void SetDefaults()
//        {
//            Projectile.width = 140;
//            Projectile.height = 32;
//            Projectile.friendly = false;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = MaxChargeTime + 60000;
//            Projectile.tileCollide = false;
//            Projectile.netImportant = true;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = 60;
//        }

//        public override void AI()
//        {
//            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
//            Player owner = Main.player[Projectile.owner];
//            if (owner.dead || !owner.active)
//            {
//                Projectile.Kill();
//                return;
//            }

//            if (Projectile.velocity == Vector2.Zero) // 蓄力阶段
//            {
//                Projectile.tileCollide = false;
//                Projectile.friendly = false;
//                isDashing = false;

//                // 对准鼠标方向并进行蓄力
//                Projectile.rotation = Projectile.AngleTo(Main.MouseWorld) + MathHelper.PiOver4;
//                Projectile.Center = owner.MountedCenter;
//                owner.heldProj = Projectile.whoAmI;

//                if (Projectile.ai[0] >= MaxChargeTime)
//                {
//                    StartLunge(owner); // 开始冲刺
//                }
//                else
//                {
//                    Projectile.ai[0]++;
//                }
//            }
//            else // 冲刺阶段
//            {
//                Projectile.tileCollide = true;
//                Projectile.friendly = true;
//                isDashing = true;

//                Projectile.velocity = lockedDirection * Projectile.velocity.Length();
//                owner.velocity = Projectile.velocity;
//                owner.Center = Projectile.Center;

//                dashTime++;
//                if (dashTime >= MaxDashTime)
//                {
//                    Projectile.velocity *= 0.75f;
//                    return;
//                }

//                // 在枪头位置生成深蓝色烟雾粒子特效
//                AddSmokeParticles();

//                // 每帧释放深蓝色气泡特效
//                if (Projectile.ai[0] % 3 == 0)
//                {
//                    ReleaseBubbles();
//                }
//            }

//            // 检查是否接触到液体（水、岩浆、蜂蜜、微光）
//            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
//            {
//                Projectile.Kill(); // 接触液体时销毁弹幕
//            }
//        }

//        private void StartLunge(Player owner) // 冲刺的具体逻辑
//        {
//            owner = Main.player[Projectile.owner];

//            dashTime = 0;
//            lockedDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
//            Projectile.velocity = lockedDirection * DashSpeed;

//            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
//            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
//            GeneralParticleHandler.SpawnParticle(pulse);

//            owner.immune = true;
//            owner.immuneNoBlink = true;
//            owner.immuneTime = 60;

//            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
//            {
//                owner.hurtCooldowns[i] = owner.immuneTime;
//            }

//            Projectile.netUpdate = true;
//        }

//        private void AddSmokeParticles() // 在枪头位置生成深蓝色烟雾粒子特效
//        {
//            Vector2 offset = new Vector2(0, -Projectile.width * 0.5f).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2);
//            Vector2 spawnPosition = Projectile.Center + offset;
//            Color smokeColor = Color.DarkBlue; // 深蓝色
//            Particle smoke = new HeavySmokeParticle(spawnPosition, Vector2.Zero, smokeColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
//            GeneralParticleHandler.SpawnParticle(smoke);
//        }
//        private void ReleaseBubbles() // 每帧释放深蓝色气泡特效
//        {
//            Player player = Main.player[Projectile.owner]; // 获取玩家对象
//            Vector2 playerPosition = player.Center; // 获取玩家的中心位置

//            // 将位置参数修改为玩家的位置
//            Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), playerPosition, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
//            bubble.timeLeft = 8 + Main.rand.Next(6);
//            bubble.scale = Main.rand.NextFloat(0.6f, 1f) * (1 + Projectile.timeLeft / (float)Projectile.timeLeft);
//            bubble.type = Main.rand.NextBool(3) ? 412 : 411;
//        }


//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {
//            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
//            return false;
//        }
//    }
//}
