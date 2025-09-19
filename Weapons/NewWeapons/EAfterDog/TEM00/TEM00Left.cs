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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Left : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";
        public override void SetStaticDefaults()
        {
            // 设置弹幕拖尾长度和模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
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


                                     // ===== 攻击控制 =====
        private int attackPhase = 0;    // 当前第几轮攻击 (0~4, 共5轮)
        private int shotsThisPhase = 0; // 本轮需要发射多少发
        private int shotsFired = 0;     // 本轮已发射多少发
        private int fireCooldown = 0;   // 单发冷却
        private int phaseCooldown = 0;  // 轮与轮之间的间隔
        private bool specialAttack = false; // 是否进入特殊攻击阶段

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
                // ====== 攻击流程逻辑 ======
                if (!specialAttack)
                {
                    if (phaseCooldown > 0)
                    {
                        phaseCooldown--;
                    }
                    else
                    {
                        if (shotsThisPhase == 0)
                        {
                            // 初始化本轮射击
                            attackPhase++;
                            if (attackPhase <= 5)
                            {
                                shotsThisPhase = Main.rand.Next(10, 16); // 10~15发
                                shotsFired = 0;
                            }
                            else
                            {
                                // 五轮打完 → 进入特殊攻击阶段（暂时留空）
                                // ===== 当五轮结束，进入特殊攻击（只在此刻生成一次超级激光） =====
                                specialAttack = true;

                                // 生成超级激光（只生成一次）
                                // 把生成位置放在枪口 headPosition，上方发射方向以当前朝向为准
                                int laserProj = Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition, // 激光出生点：弹幕顶端（你已经算好了 headPosition）
                                    Projectile.velocity.SafeNormalize(Vector2.UnitY), // 方向（单位向量）——激光类会根据 owner 同步方向
                                    ModContent.ProjectileType<TEM00LeftSuperLazer>(),
                                    Projectile.damage, // 可以按需调整伤害
                                    0f,
                                    Projectile.owner
                                );

                                // 绑定父弹幕索引：把 ai[0] 设为当前父弹幕索引（this.whoAmI）
                                if (laserProj >= 0 && laserProj < Main.maxProjectiles)
                                {
                                    Main.projectile[laserProj].ai[0] = Projectile.whoAmI; // 告诉激光它的“父弹幕”是谁
                                    Main.projectile[laserProj].netUpdate = true; // 多人时同步
                                                                                 // 可选：立即把激光的朝向和速度与父弹幕匹配（便于首帧视觉一致）
                                    Main.projectile[laserProj].rotation = Projectile.rotation - MathHelper.PiOver4;
                                    Main.projectile[laserProj].velocity = (Main.projectile[laserProj].rotation).ToRotationVector2();
                                }
                            }
                        }

                        if (shotsThisPhase > 0)
                        {
                            if (fireCooldown > 0)
                                fireCooldown--;
                            else
                            {
                                // 发射一发激光
                                headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48f;

                                // 基础方向：正前方
                                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                                // 在 ±10° (即 20°范围内) 随机偏移
                                dir = dir.RotatedByRandom(MathHelper.ToRadians(10f));

                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition,
                                    dir,
                                    ModContent.ProjectileType<TEM00LeftLazer>(),
                                    Projectile.damage,
                                    0f,
                                    Projectile.owner
                                );

                                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

                                shotsFired++;
                                fireCooldown = 6; // 单发之间的间隔（你可以调大或调小）

                                if (shotsFired >= shotsThisPhase)
                                {
                                    // 本轮结束 → 设置间隔时间
                                    shotsThisPhase = 0;
                                    phaseCooldown = 30; // 两轮之间的间隔（可以调整）
                                }
                            }
                        }
                    }
                }
                else
                {
                    // ====== 特殊攻击阶段（留空） ======
                }


            }


            // 松手后进入 Dash
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 300;
                Projectile.penetrate = -1; // 可调穿透次数

                CurrentState = BehaviorState.Dash;
            }
        }






        private int dashFrameCounter = 0; // 在类里新建计数器字段

        private void DoBehavior_Dash() // 冲刺阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 设置冲刺速度
            float initialSpeed = 35f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * initialSpeed;

            // 每帧计数
            dashFrameCounter++;

            if (dashFrameCounter % 3 == 0 && Main.myPlayer == Projectile.owner)
            {
                // ====== 1. 在自己正下方随机位置生成一发激光 ======
                float xOffset = Main.rand.NextFloat(-120f, 120f); // 左右随机
                float yOffset = Main.rand.NextFloat(35 * 16f, 35 * 16f);  // 在正下方一定范围
                Vector2 spawnPos = Projectile.Center + new Vector2(xOffset, yOffset);

                // 方向：指向本体  
                Vector2 dir = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dir,
                    ModContent.ProjectileType<TEM00LeftLazer>(),
                    Projectile.damage,
                    0f,
                    Projectile.owner
                );

                SoundEngine.PlaySound(SoundID.Item33, spawnPos);

                // ====== 2. 疯狂的飞行特效 ======
                for (int i = 0; i < 6; i++) // 比平时多，显得更夸张
                {
                    // Square 方块能量片
                    SquareParticle sq = new SquareParticle(
                        spawnPos,
                        dir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f),
                        false,
                        18 + Main.rand.Next(12),
                        1.5f + Main.rand.NextFloat(0.8f),
                        new Color(90, 200, 255) * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(sq);

                    // GlowOrb 光点
                    GlowOrbParticle orb = new GlowOrbParticle(
                        spawnPos,
                        dir * Main.rand.NextFloat(1f, 3f),
                        false,
                        6,
                        0.9f + Main.rand.NextFloat(0.5f),
                        new Color(120, 220, 255),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {

        }



    }
}
