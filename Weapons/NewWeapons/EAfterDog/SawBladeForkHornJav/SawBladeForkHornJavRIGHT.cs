using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJavRIGHT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SawBladeForkHornJav/SawBladeForkHornJav";

        // ====== 状态机 ======
        private enum State : int
        {
            HoldInHand = 0,   // 手里蓄力（跟随手臂）
            SpinFlight = 1,   // 甩出后自转衰减
            Turret = 2        // 悬停炮台（限角速度转向并开火）
        }

        // ai/locai 快捷引用
        private ref float S => ref Projectile.ai[0];          // 状态
        private ref float T => ref Projectile.ai[1];          // 状态内计时
        private ref float SpinVel => ref Projectile.localAI[0]; // 自转角速度（弧度/帧）
        private ref float FireCD => ref Projectile.localAI[1];  // 炮台射击冷却计数

        // 常量参数（可按需调整）
        private float HoldArmLength = 32f;                  // 手臂前顶距离
        private float ThrowSpeed = 18f;                     // 甩出初速
        private float SpinStartVel = 0.35f;                 // 初始自转角速度（rad/frame）
        private float SpinDamp = 0.965f;                    // 自转衰减倍率
        private float MoveDamp = 0.985f;                    // 甩出阶段线速度衰减
        private float StopSpeedThreshold = 0.7f;            // 进入炮台的线速阈值
        private float StopSpinThreshold = 0.03f;            // 进入炮台的角速阈值
        private float TurretMaxTurnPerFrame = 0.06f;        // 炮台阶段每帧最大转角（弧度）
        private int TurretFirePeriod = 45;               // 炮台总周期（发射节拍）
        private int TurretBurstCountMin = 3;              // 炮台每轮最少连发
        private int TurretBurstCountMax = 4;              // 炮台每轮最多连发
        private float BulletSpeed = 20f;                    // 炮台发射子弹速度
        private float BulletSpreadDeg = 10f;                // 炮台发射散布角度（度）

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60000;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;          // 需要“悬停在空中”，关闭地形碰撞
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 11;
        }

        private Player Owner => Main.player[Projectile.owner];

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 初始：手里蓄力阶段
            S = (int)State.HoldInHand;
            T = 0f;
            SpinVel = 0f;
            FireCD = 0f;
        }

        public override void AI()
        {
            T++;

            switch ((State)(int)S)
            {
                // =========================
                // 0) 手里蓄力（跟随手臂）
                // =========================
                case State.HoldInHand:
                    {
                        Projectile.timeLeft = 60000;      // 常驻
                        Projectile.velocity = Vector2.Zero;

                        // 跟随手臂（简单版）：方向对准鼠标，中心置于玩家前方
                        Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
                        Owner.ChangeDir(Math.Sign(toMouse.X));
                        Owner.heldProj = Projectile.whoAmI;

                        // 让弹幕在手前一点（32px），稍微右侧偏移，使持握更自然
                        Vector2 handPos = Owner.MountedCenter + toMouse * HoldArmLength + new Vector2(Owner.direction == 1 ? 8 : -8, -2);
                        Projectile.Center = handPos;

                        // 旋转与朝向修正（贴图朝右，+Pi/4）
                        Projectile.rotation = toMouse.ToRotation() + MathHelper.PiOver4;

                        // 松手 → 甩出去
                        if (!Owner.channel)
                        {
                            S = (int)State.SpinFlight;
                            T = 0f;

                            // 赋予初速与初始自转角速度（方向随机顺/逆时针）
                            Vector2 launchDir = toMouse;
                            Projectile.velocity = launchDir * ThrowSpeed;
                            SpinVel = (Main.rand.NextBool() ? 1f : -1f) * SpinStartVel;

                            Projectile.netUpdate = true;
                        }
                        break;
                    }

                // =========================
                // 1) 自转衰减（黑烟污染 + 慢慢停下）
                // =========================
                case State.SpinFlight:
                    {
                        // 线速度和角速度衰减
                        Projectile.velocity *= MoveDamp;
                        SpinVel *= SpinDamp;

                        // 累计自转（注意这里 rotation 完全由自转主导，不再跟随速度）
                        Projectile.rotation += SpinVel;

                        // —— 旋转期间：四散甩黑色 Dust（污染感）——
                        DoSpinBlackDust();

                        {
                            // —— 旋转期间：周期性发射 RPP，频率逐渐加快 —— //
                            FireCD++;
                            int interval = (int)MathHelper.Max(2f, 10f - T * 0.02f); // 起始 10 帧，随时间缩短，最低 2 帧
                            if (FireCD % interval == 0)
                            {
                                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                                Vector2 dir = angle.ToRotationVector2();

                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    Projectile.Center,
                                    dir * 16f, // 初速
                                    ModContent.ProjectileType<SawBladeForkHornJavRPP>(),
                                    Projectile.damage / 2, // 自转期间火力稍弱
                                    Projectile.knockBack,
                                    Projectile.owner
                                );
                            }
                        }
                        // 满足停止阈值 → 进入炮台（悬停）
                        if (Projectile.velocity.Length() <= StopSpeedThreshold && Math.Abs(SpinVel) <= StopSpinThreshold)
                        {
                            Projectile.velocity = Vector2.Zero;
                            SpinVel = 0f;
                            S = (int)State.Turret;
                            T = 0f;
                            FireCD = 0f;
                            Projectile.netUpdate = true;
                        }
                        break;
                    }

                // =========================
                // 2) 炮台（限角速度转向 + 开火）
                // =========================
                case State.Turret:
                    {
                        Projectile.velocity = Vector2.Zero;

                        // 找目标
                        NPC target = FindClosestNPC(99900f);
                        float desiredRot = Projectile.rotation; // 以当前朝向为基准
                        if (target != null && target.active && !target.friendly && !target.dontTakeDamage)
                        {
                            // 贴图右向 +Pi/4，因此目标方向也要加 Pi/4 才是“贴图角”
                            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                            desiredRot = dir.ToRotation() + MathHelper.PiOver4;
                        }
                        else
                        {
                            // 没目标就缓慢巡航转动，别死板
                            desiredRot = Projectile.rotation + 0.012f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.3f);
                        }

                        // 限制每帧最大转角（灵动不突兀）
                        Projectile.rotation = Projectile.rotation.AngleTowards(desiredRot, TurretMaxTurnPerFrame);

                        // 炮台连发节奏
                        FireCD++;
                        if (FireCD % TurretFirePeriod == 0 && target != null && target.CanBeChasedBy())
                        {
                            int burst = Main.rand.Next(TurretBurstCountMin, TurretBurstCountMax + 1);
                            for (int i = 0; i < burst; i++)
                            {
                                float spread = MathHelper.ToRadians(Main.rand.NextFloat(-BulletSpreadDeg, BulletSpreadDeg));
                                Vector2 shootDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedBy(spread);
                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    Projectile.Center,
                                    shootDir * BulletSpeed,
                                    ModContent.ProjectileType<SawBladeForkHornJavRPP>(),
                                    Projectile.damage,
                                    Projectile.knockBack,
                                    Projectile.owner
                                );
                            }
                        }

                        break;
                    }
            }
        }

        // ========== 自转阶段：黑色污染 Dust ==========
        private void DoSpinBlackDust()
        {
            // 前方 2×16 的圆心，三个扇形：前方/左后/右后
            Vector2 forward = new Vector2((float)Math.Cos(Projectile.rotation - MathHelper.PiOver4),
                                          (float)Math.Sin(Projectile.rotation - MathHelper.PiOver4)); // 反推“真实前向”
            Vector2 center = Projectile.Center + forward * (2f * 16f);
            float radiusPx = 32f;

            // 前方主喷
            for (int i = 0; i < 5; i++)
            {
                float ang = forward.ToRotation() + Main.rand.NextFloat(-MathHelper.ToRadians(25f), MathHelper.ToRadians(25f));
                Vector2 dir = ang.ToRotationVector2();
                Vector2 spawn = center + Main.rand.NextVector2Circular(radiusPx, radiusPx) * 0.25f;
                float speed = Main.rand.NextFloat(6f, 12f);
                float scale = Main.rand.NextFloat(1.4f, 2.0f);

                int d = Dust.NewDust(spawn, 0, 0, DustID.Smoke, dir.X * speed, dir.Y * speed, 0, Color.Black, scale);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.2f;
            }
            // 棕尘点缀
            for (int i = 0; i < 2; i++)
            {
                float ang = forward.ToRotation() + Main.rand.NextFloat(-MathHelper.ToRadians(20f), MathHelper.ToRadians(20f));
                Vector2 dir = ang.ToRotationVector2();
                Vector2 spawn = center + Main.rand.NextVector2Circular(radiusPx, radiusPx) * 0.2f;
                float speed = Main.rand.NextFloat(5f, 10f);
                float scale = Main.rand.NextFloat(1.2f, 1.6f);

                int d = Dust.NewDust(spawn, 0, 0, DustID.Ash, dir.X * speed, dir.Y * speed, 0, new Color(70, 45, 25), scale);
                Main.dust[d].noGravity = true;
            }

            // 左后、右后侧喷
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float baseAng = forward.ToRotation() + side * MathHelper.ToRadians(150f);
                    float ang = baseAng + Main.rand.NextFloat(-MathHelper.ToRadians(18f), MathHelper.ToRadians(18f));
                    Vector2 dir = ang.ToRotationVector2();
                    Vector2 spawn = center + Main.rand.NextVector2Circular(radiusPx, radiusPx) * 0.25f;
                    float speed = Main.rand.NextFloat(4f, 9f);
                    float scale = Main.rand.NextFloat(1.3f, 1.8f);

                    int d = Dust.NewDust(spawn, 0, 0, DustID.Smoke, dir.X * speed, dir.Y * speed, 0, Color.Black, scale);
                    Main.dust[d].noGravity = true;
                }
            }
        }

        // ========== 目标搜索 ==========
        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float min = maxRange;
            foreach (var npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < min)
                {
                    min = d;
                    closest = npc;
                }
            }
            return closest;
        }

        // ========== 贴图绘制 ==========
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texMain = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texMain.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 统一角度（AI 内已维护好）：贴图右向 + Pi/4
            float rot = Projectile.rotation;
            float scale = Projectile.scale;

            // 自转阶段：单体 + “脉动描边”（不再画残影）
            if ((State)(int)S == State.SpinFlight)
            {
                // 脉动幅度
                float pulse = 1.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9f);
                Color outline = Color.Black * 0.9f;

                // 4 向描边
                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = i switch
                    {
                        0 => new Vector2(pulse, 0),
                        1 => new Vector2(-pulse, 0),
                        2 => new Vector2(0, pulse),
                        _ => new Vector2(0, -pulse),
                    };
                    Main.EntitySpriteDraw(texMain, drawPos + off, frame, outline, rot, origin, scale, SpriteEffects.None, 0);
                }
                // 本体
                Main.EntitySpriteDraw(texMain, drawPos, frame, Projectile.GetAlpha(lightColor), rot, origin, scale, SpriteEffects.None, 0);
                return false;
            }

            // 炮台阶段：本体 + 棕色魔法叠层（2~3 张反向旋转）
            if ((State)(int)S == State.Turret)
            {
                // 先画本体（稍微偏棕）
                Color baseCol = Color.SaddleBrown * 0.9f;
                baseCol.A = 255;
                Main.EntitySpriteDraw(texMain, drawPos, frame, baseCol, rot, origin, scale, SpriteEffects.None, 0);

                // —— 棕色魔法叠层（反向旋转）——
                // 选 3 张纹理：两张 twirl + 一张 magic
                Texture2D twirl1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
                Texture2D twirl2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_02").Value;
                Texture2D magic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;

                float gtime = Main.GlobalTimeWrappedHourly;
                float spinA = rot * 0.6f + gtime * 0.9f;   // 正转
                float spinB = -rot * 0.7f + gtime * 1.2f;  // 反转
                float spinC = rot * 0.9f - gtime * 1.5f;   // 再次反向

                Color brownA = new Color(140, 90, 55) * 0.65f;
                Color brownB = new Color(120, 75, 40) * 0.55f;
                Color brownC = new Color(100, 60, 30) * 0.5f;

                float sclA = 0.9f, sclB = 1.15f, sclC = 0.75f;

                Main.EntitySpriteDraw(twirl1, drawPos, null, brownA, spinA, twirl1.Size() * 0.5f, sclA, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(twirl2, drawPos, null, brownB, spinB, twirl2.Size() * 0.5f, sclB, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(magic1, drawPos, null, brownC, spinC, magic1.Size() * 0.5f, sclC, SpriteEffects.None, 0);

                return false;
            }

            // 手里阶段：普通绘制
            Main.EntitySpriteDraw(texMain, drawPos, frame, Projectile.GetAlpha(lightColor), rot, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
