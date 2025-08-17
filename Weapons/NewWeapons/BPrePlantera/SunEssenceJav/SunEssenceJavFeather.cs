using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取羽毛贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            SpriteEffects effects = SpriteEffects.None;

            // === 🚩 1️⃣ 绘制拖尾（原 Calamity 拖尾）===
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, (int)1.5f);

            // === 🚩 2️⃣ 外层光脉动绘制 ===
            // 计算呼吸脉动缩放（1.6x ~ 2.0x）
            float pulsate = 1.6f + 0.4f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f); // 呼吸周期

            // 设置蓝绿色淡光色
            Color glowColor = Color.Cyan * 0.3f;

            // 多层叠加轻微位移绘制，形成柔和蓝光晕
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(
                    texture,
                    drawPosition + offset,
                    null,
                    glowColor,
                    rotation,
                    origin,
                    pulsate,
                    effects,
                    0f
                );
            }

            // === 🚩 3️⃣ 绘制本体（1.5x 大小）===
            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                lightColor,
                rotation,
                origin,
                1.5f,
                effects,
                0f
            );

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 10;
            Projectile.DamageType = DamageClass.Melee;
            //Projectile.aiStyle = ProjAIStyleID.Nail;
            //AIType = ProjectileID.NailFriendly;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.aiStyle = -1;      // 禁止使用 Nail AI，完全跑自定义逻辑
        }
        // 在类里新增一个字段
        private int flyDuration;
        // 在类里新增一个字段来存储锁定的角度
        private float lockedRotation = 0f;

        // 在 OnSpawn 里生成随机值
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            flyDuration = Main.rand.Next(5, 26); // 5~25 随机
        }
        // 在类里增加变量
        private Projectile? parentProj;
        private int orbitTimer = 0;
        private const int orbitTimeMax = 120; // 转两圈，约120帧
        private const float orbitRadius = 96f; // 公转半径
        private const float orbitAngularSpeed = MathHelper.TwoPi / 60f; // 每60帧一圈
        public override void AI()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);



            Player player = Main.player[Projectile.owner];

            ref float state = ref Projectile.ai[0];  // 状态阶段
            ref float timer = ref Projectile.ai[1];  // 阶段内计时器

            switch ((int)state)
            {
                case 0: // 🚀 阶段 1：直线飞行
                    timer++;
                    if (timer > flyDuration) // ✅ 使用随机值替代固定 20
                    {
                        state = 1f;
                        timer = 0f;
                        Projectile.velocity = Vector2.Zero;
                    }
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 2;
                    break;

                case 1: // 🛑 阶段 2：停滞悬停
                    timer++;

                    // 目标角度：默认还是“朝右”
                    float targetRot = 0f; // MathHelper.ToRadians(0)，即水平朝右
                                          // 或者你也可以让它以某个固定速度转动：
                                          // targetRot = Projectile.rotation + 0.05f; // 每帧缓慢旋转

                    // 旋转惯性：逐渐靠近目标角度
                    Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, 0.05f);

                    // 位置不动
                    Projectile.velocity = Vector2.Zero;

                    if (timer > 55f)
                    {
                        state = 2f;
                        timer = 0f;
                    }
                    break;

                case 2: // 🌀 阶段 3：缓慢转向 -> 修改为固定角度
                    timer++;

                    // 第一次进入该阶段时，记录当前角度
                    if (timer == 1)
                    {
                        lockedRotation = Projectile.rotation;
                    }

                    // 强制保持这个固定角度
                    Projectile.rotation = lockedRotation;

                    NPC targetNPC = Projectile.Center.ClosestNPCAt(900f);
                    if (targetNPC != null && targetNPC.active && !targetNPC.friendly && !targetNPC.dontTakeDamage)
                    {
                        if (timer > 20f)
                        {
                            state = 3f;
                            timer = 0f;
                            Projectile.velocity = Vector2.Zero; // 冲刺前保持停顿
                        }
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                    break;

                case 3: // 💥 阶段 4：弹性冲刺
                    timer++;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver4;
                    float t = MathHelper.Clamp(timer / 20f, 0f, 1f);
                    float dashSpeed = EaseInOutElastic(t) * 24f;

                    NPC dashTarget = Projectile.Center.ClosestNPCAt(900f);
                    if (dashTarget != null && dashTarget.active && !dashTarget.friendly && !dashTarget.dontTakeDamage)
                    {
                        Vector2 dashDir = (dashTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = dashDir * dashSpeed;
                    }
                    else
                    {
                        Projectile.velocity = Vector2.Zero;
                    }

                    if (timer > 20f)
                    {
                        //Projectile.Kill(); // 冲刺完毕直接消失
                    }
                    break;
            }




            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 2;

            //{
            //    // ① 找到最近的 SunEssenceJavPROJ 作为父弹幕
            //    if (parentProj == null || !parentProj.active || parentProj.type != ModContent.ProjectileType<SunEssenceJavPROJ>())
            //    {
            //        parentProj = FindClosestParent();
            //    }

            //    if (parentProj != null && parentProj.active)
            //    {
            //        if (orbitTimer < orbitTimeMax)
            //        {
            //            orbitTimer++;

            //            float angle = orbitTimer * orbitAngularSpeed;

            //            // 半径小范围波动（避免跑太远）
            //            float dynamicRadius = orbitRadius * (1f + 0.15f * (float)Math.Sin(orbitTimer * 0.1f + Projectile.identity));

            //            // 计算目标点
            //            Vector2 orbitPos = parentProj.Center + angle.ToRotationVector2() * dynamicRadius;

            //            // 加快跟随，避免“掉队”
            //            Projectile.Center = Vector2.Lerp(Projectile.Center, orbitPos, 0.25f);

            //            // 保持朝向中心
            //            Projectile.rotation = (parentProj.Center - Projectile.Center).ToRotation();
            //        }
            //        else
            //        {
            //            // === ③ 追踪逻辑 ===
            //            NPC? target = FindParentClosestNPC(parentProj);
            //            if (target != null)
            //            {
            //                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            //                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.1f);
            //                Projectile.rotation = Projectile.velocity.ToRotation();
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // 没有找到父弹幕 → 自行消失
            //        Projectile.Kill();
            //    }
            //}


            // 💡 一定保留视觉效果
            Lighting.AddLight(Projectile.Center, 0.2f, 0.9f, 0.3f); // 绿色光

            // 若仍在飞行状态（未碰地形），保留所有特效
            if (Projectile.tileCollide && (int)Projectile.ai[0] != 1)
            {
                // === 🚩 1️⃣ 高密度 Dust 蓝绿色流动 ===
                int dustCount = 2;
                for (int i = 0; i < dustCount; i++)
                {
                    float spread = MathHelper.ToRadians(45f);
                    float angle = Main.rand.NextFloat(-spread, spread);
                    Vector2 velocity = forward.RotatedBy(angle) * Main.rand.NextFloat(4f, 10f);
                    int type = Main.rand.NextFloat() < 0.7f ? DustID.BlueTorch : DustID.GreenTorch;

                    Dust d = Dust.NewDustDirect(
                        Projectile.Center,
                        0, 0,
                        type,
                        velocity.X,
                        velocity.Y,
                        0,
                        default, // 使用默认色以匹配 Dust 自身蓝绿
                        Main.rand.NextFloat(1.2f, 2.5f)
                    );
                    d.noGravity = true;
                }

                // === 🚩 2️⃣ DirectionalPulseRing 蓝绿色脉冲波 ===
                if (Main.GameUpdateCount % 3 == 0)
                {
                    int pulseLayers = 1;
                    for (int i = 0; i < pulseLayers; i++)
                    {
                        Particle pulse = new DirectionalPulseRing(
                            Projectile.Center + forward * (10f + i * 6f),
                            forward * (2f + i * 0.8f),
                            Color.Lerp(Color.DeepSkyBlue, Color.Cyan, i / (float)pulseLayers),
                            new Vector2(0.8f, 1.8f + i * 0.3f),
                            Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver4,
                            0.15f + i * 0.03f,
                            0.025f,
                            8
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                }

                // === 🚩 3️⃣ SparkParticle（偏外侧，随机发散）===
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(10f, 10f); // ⬅️ 比原来更远
                    Vector2 velocity = forward.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(4f, 10f); // 速度范围拉大

                    Particle spark = new SparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        10,
                        Main.rand.NextFloat(0.8f, 1.0f),
                        Color.Lerp(Color.Aqua, Color.Cyan, Main.rand.NextFloat(0.3f, 0.7f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // === 🚩 4️⃣ AltSparkParticle（更靠近，集中）===
                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(2f, 2f); // ⬅️ 比原来更小，贴近轨迹
                    Vector2 velocity = forward * 0.02f + Main.rand.NextVector2Circular(0.1f, 0.1f); // 更低速、轻扰动

                    AltSparkParticle altSpark = new AltSparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        16,
                        1.2f,
                        Color.Cyan * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(altSpark);
                }

            }


        }

        // 工具函数：找到最近的 SunEssenceJavPROJ
        private Projectile? FindClosestParent()
        {
            Projectile? result = null;
            float minDist = 2000f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<SunEssenceJavPROJ>())
                {
                    float dist = Vector2.Distance(Projectile.Center, proj.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = proj;
                    }
                }
            }
            return result;
        }

        // 工具函数：找到父弹幕最近的敌人
        private NPC? FindParentClosestNPC(Projectile parent)
        {
            NPC? result = null;
            float minDist = 900f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(parent.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = npc;
                    }
                }
            }
            return result;
        }

        // 示例：弹性曲线函数（EaseInOutElastic），可调整为你喜欢的速度感
        private float EaseInOutElastic(float t)
        {
            if (t < 0.5f)
                return 0.5f * (float)(Math.Sin(13 * Math.PI / 2 * (2 * t)) * Math.Pow(2, 10 * ((2 * t) - 1)));
            else
                return 0.5f * ((float)(Math.Sin(-13 * Math.PI / 2 * ((2 * t - 1) + 1)) * Math.Pow(2, -10 * (2 * t - 1))) + 2);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            for (int i = 0; i < 15; i++)
            {
                int greenDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.2f);
                Main.dust[greenDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[greenDust].scale = 0.5f;
                    Main.dust[greenDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 30; j++)
            {
                int greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.7f);
                Main.dust[greenDust2].noGravity = true;
                Main.dust[greenDust2].velocity *= 5f;
                greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1f);
                Main.dust[greenDust2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果
        }

    }
}
