using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC
{
    public class DragonRageJavFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/Magic/RancorFog"; // 透明烟雾贴图

        private Player owner;
        private bool isTracking = true;
        private float orbitRadius;
        private float orbitAngle;
        private Vector2 targetPosition;
        private bool isDashing = false;

        private Color FireColor = new Color(255, 80, 30); // 更加强烈的红橙色
        private const float MaxScale = 0.75f; // 参考 PristineSecondary 的最大缩放值

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
        }
        private bool isEnteringOrbit = true; // 是否处于前进阶段
        private int enterOrbitTimer = 30; // 多少帧后进入旋转模式
        //private int rotationDuration = 0; // 旋转多少帧后进入冲刺
        private int Time = 0; // 旋转多少帧后进入冲刺


        private int flightStage = 0; // 0 = 初始漂浮, 1 = 旋转, 2 = 冲刺
        private int flightTimer = 0; // 控制不同阶段的时间
        private int rotationDuration = 120; // 旋转阶段的持续时间
        private Vector2 initialRandomVelocity; // 初始随机速度

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // **检测主弹幕状态**
            bool shouldEnterDash = true;
            bool shouldDestroy = true; // 只要没有 `Mode.Attract` 的主弹幕，就销毁自己
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                {
                    DragonRageJavPROJ mainProjectile = proj.ModProjectile as DragonRageJavPROJ;
                    if (mainProjectile != null && mainProjectile.currentMode == DragonRageJavPROJ.Mode.Attract)
                    {
                        shouldEnterDash = false;
                        shouldDestroy = false;
                        break;
                    }
                }
            }

            // **如果主弹幕不再处于 `Mode.Attract`，则销毁自己**
            if (shouldDestroy)
            {
                Projectile.Kill();
                return;
            }

            // **如果主弹幕不再处于 `Mode.Attract`，则强制进入冲刺**
            if (shouldEnterDash && flightStage < 2)
            {
                flightStage = 2;
                flightTimer = 0;
            }

            // **🔥 阶段 1：初始漂浮（前 120 帧）**
            if (flightStage == 0)
            {
                if (flightTimer == 0)
                {
                    // **初始随机漂浮方向**
                    initialRandomVelocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                }

                // **随机漂浮 + 慢慢向玩家靠近**
                Vector2 moveDirection = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = (Projectile.velocity * 0.95f + moveDirection * 0.05f) + initialRandomVelocity * 0.2f;

                if (flightTimer >= 120)
                {
                    flightStage = 1; // 进入旋转阶段
                    flightTimer = 0;
                }
            }

            // **🌀 阶段 2：圆周运动（120 - 240 帧）**
            else if (flightStage == 1)
            {
                // **围绕 `DragonRageJavPROJ` 旋转**
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation += 0.05f;
                Projectile.Center = FindDragonRageJavPROJCenter() + new Vector2(15 * 16, 0).RotatedBy(flightTimer * 0.05f);

                if (flightTimer >= rotationDuration)
                {
                    flightStage = 2; // 进入冲刺阶段
                    flightTimer = 0;
                }
            }

            // **⚡ 阶段 3：冲刺（240 帧后 或 `主弹幕` 退出 `Attract`）**
            else if (flightStage == 2)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1800);
                if (target != null)
                {
                    Vector2 targetDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    targetDirection = targetDirection.RotatedByRandom(MathHelper.ToRadians(10f)); // 偏移 ±10°
                    Projectile.velocity = targetDirection * 10f;
                }
                else
                {
                    Projectile.velocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 15f;
                }

                Projectile.timeLeft = 30; // **冲刺后 60 帧内消失**
            }

            flightTimer++;

            // 🔥 生成橙色烟雾特效
            int dustCount = 3;
            float radians = MathHelper.TwoPi / dustCount;
            Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVelocity = smokePoint.RotatedBy(radians * i).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.6f);
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    dustVelocity * Main.rand.NextFloat(1f, 2.6f),
                    Color.Orange, // 橙色烟雾
                    18, // 生命周期
                    Main.rand.NextFloat(0.9f, 1.6f), // 缩放
                    0.35f, // 透明度
                    Main.rand.NextFloat(-1, 1), // 旋转速度
                    false // 关闭发光
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 🔥 生成橙色火花（1×16 半径范围）
            if (Time % 5 == 0)
            {
                float sparkAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + new Vector2(16, 0).RotatedBy(sparkAngle);
                Vector2 sparkVelocity = (sparkPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * 4f;

                CritSpark spark = new CritSpark(
                    sparkPos,
                    sparkVelocity,
                    Color.OrangeRed, // 初始颜色
                    Color.DarkOrange, // 结束颜色
                    1.2f, // 缩放
                    20 // 生命时间
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        // **查找最近的 `DragonRageJavPROJ`**
        private Vector2 FindDragonRageJavPROJCenter()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                    return proj.Center;
            }
            return Main.player[Projectile.owner].Center; // 如果找不到，默认返回玩家中心
        }






        // **进入冲刺模式**
        private void EnterDashMode(Vector2 dashTarget)
        {
            if (isTracking)
            {
                isTracking = false;
                isDashing = true;
                targetPosition = dashTarget;
                Projectile.timeLeft = 90; // 进入冲刺后不再重置
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 让火焰特效微小随机旋转，避免太过平滑
            float randomRotation = Projectile.rotation + Main.rand.NextFloat(-0.35f, 0.35f);
            float opacity = Projectile.Opacity * 0.6f;
            Color drawColor = FireColor * opacity;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, randomRotation, texture.Size() / 2, MaxScale, SpriteEffects.None);

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰
        }
    }
}
