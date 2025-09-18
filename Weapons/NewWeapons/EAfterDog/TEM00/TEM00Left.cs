using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.ID;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Left : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";
        public override void SetStaticDefaults()
        {
            // 设置弹幕拖尾长度和模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制控制函数，可用于绘制自定义贴图、添加发光效果、叠加特效等
            // 若不需要可返回 true 使用默认绘制【很不推荐】
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1; // 调高这个值可以让弹幕更加顺滑的跟随鼠标
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }
        private int chargeTimer = 0; // 在类里新建字段
        private int chargeCount = 0; // 已经触发几次（最多 8）

        private void DoBehavior_Aim() // 瞄准阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width);
            Owner.heldProj = Projectile.whoAmI;

            // 枪头位置 [这很重要，因为许多特效都需要和他相关]
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            // 如果武器自身会高速旋转 [比如巨龙之怒] ，那么枪头需要改成这个来适配
            float fixedRotation = Projectile.rotation; // 可根据需求加减角度偏移
            headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(fixedRotation);



            {
                chargeTimer++;
                if (chargeTimer >= 30 && chargeCount < 8)
                {
                    chargeTimer = 0;
                    chargeCount++;

                    // ① 发射大量散射激光
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 dir = (MathHelper.TwoPi * i / 12f).ToRotationVector2();
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            headPosition,
                            dir * 12f,
                            ModContent.ProjectileType<TEM00LeftLazer>(),
                            Projectile.damage / 2,
                            0f,
                            Projectile.owner
                        );
                    }

                    // ② 屏幕震动
                    float shakePower = 5f;
                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                    // ③ 收缩冲击波
                    Particle shrinkingpulse = new DirectionalPulseRing(
                        headPosition,
                        Vector2.Zero,
                        Color.Cyan, // 改为科技蓝
                        new Vector2(1f, 1f),
                        Main.rand.NextFloat(6f, 10f),
                        0.15f,
                        3f,
                        10
                    );
                    GeneralParticleHandler.SpawnParticle(shrinkingpulse);

                    // ④ 额外光能特效（你交给我发挥）
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                        GlowOrbParticle orb = new GlowOrbParticle(
                            headPosition,
                            vel,
                            false,
                            15,
                            0.8f + Main.rand.NextFloat(0.3f),
                            Color.Lerp(Color.White, Color.Cyan, 0.5f),
                            true,
                            false,
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }

            }


            // 松手后进入 Dash
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 300;
                Projectile.penetrate = 10; // 可调穿透次数

                CurrentState = BehaviorState.Dash;
            }
        }

        private void DoBehavior_Dash() // 冲刺阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 设置冲刺速度
            float initialSpeed = 15f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * initialSpeed;







        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {

        }



    }
}
