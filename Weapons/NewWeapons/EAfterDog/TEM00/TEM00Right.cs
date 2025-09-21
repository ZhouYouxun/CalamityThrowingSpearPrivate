using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;
using System.ComponentModel;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Right : ModProjectile, ILocalizedModType
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
            Projectile.width = Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 可击中次数
            Projectile.timeLeft = 80;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0; // 可调节飞行平滑度
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        // ======== AI参数（集中可调）========
        private float searchRange;       // 寻敌范围
        private float orbitRadiusIdle;   // 玩家轨道半径
        private float orbitRadiusCombat; // 敌人轨道半径
        private float orbitSpeedIdle;    // 空闲角速度
        private float orbitSpeedCombat;  // 战斗角速度
        private float maxSpeed;          // 最大机动速度
        private float inertia;           // 速度惯性（越大越平滑）

        private int fireCdMin, fireCdMax; // 开火冷却范围（帧）
        private float leadTime;           // 预测提前量（帧）

        // ======== 运行态（不使用 localAI）========
        private int targetId;      // 目标NPC索引（-1=无）
        private int retargetTimer; // 寻敌冷却
        private int shootTimer;    // 开火冷却
        private float orbitAngle;  // 当前轨道角度
        private bool inited;       // 初始化（确定轨道旋向）

        // ======== 激光计数器 ========
        private int maxLaserShots;   // 最大可发射的激光数量
        private int laserShotsLeft;  // 剩余激光数量
        private bool kamikazeMode;   // 是否进入撞击模式

        // ======== 射击频率控制 ========
        private int startFireCd;     // 初始冷却（帧）
        private int endFireCd;       // 最终冷却（帧）

        public override void OnSpawn(IEntitySource source)
        {
            // ========== 初始化AI参数 ==========
            searchRange = 1200f;

            orbitRadiusIdle = 160f;
            orbitRadiusCombat = 140f;
            orbitSpeedIdle = 0.035f;
            orbitSpeedCombat = 0.060f;

            maxSpeed = 12f;
            inertia = 14f;

            fireCdMin = 20;
            fireCdMax = 36;
            leadTime = 12f;

            // ========== 初始化运行态 ==========
            targetId = -1;
            retargetTimer = 0;
            shootTimer = 0;
            orbitAngle = 0f;
            inited = false;

            // ========== 初始化激光计数器 ==========
            maxLaserShots = 12;
            laserShotsLeft = maxLaserShots;
            kamikazeMode = false;

            // ========== 初始化射击频率 ==========
            startFireCd = 20; // 从20帧起步
            endFireCd = 4;    // 最快降到4帧
        }
        // ======== 无敌人自杀计时 ========
        private int noTargetTimer = 0; // 连续无敌人帧数
        // ======== 狂野冲刺计时 ========
        private int dashTimer = 0;

        public override void AI()
        {
            // =====================【飞行逻辑：激光战舰AI（圆滑·灵动·椭圆）】=====================
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 保活与碰撞设定（发射期不接触伤害，撞击期的friendly在下方状态机中切换）
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.friendly = false;

            // 初始化：记录顺/逆时针旋向（用 ai[1] 持久化）
            if (!inited)
            {
                inited = true;
                if (Projectile.ai[1] == 0f)
                    Projectile.ai[1] = Main.rand.NextBool() ? 1f : -1f;
                orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            int orbitDir = Projectile.ai[1] > 0 ? 1 : -1;

            // ---------- 目标获取 / 校验 ----------
            NPC target = null;
            if (targetId >= 0 && targetId < Main.maxNPCs)
            {
                NPC cand = Main.npc[targetId];
                if (cand.active && !cand.friendly && cand.CanBeChasedBy(this))
                    target = cand;
            }
            if (retargetTimer-- <= 0)
            {
                retargetTimer = 15;
                float minDist = searchRange;
                int found = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.active || n.friendly || !n.CanBeChasedBy(this)) continue;
                    float d = Vector2.Distance(n.Center, owner.Center);
                    if (d < minDist)
                    {
                        minDist = d;
                        found = i;
                    }
                }
                targetId = found;
                target = (found >= 0) ? Main.npc[found] : null;
            }

            // ---------- 椭圆轨道参数（半径/角速度动态波动 + 椭圆缓慢自转） ----------
            float t = Main.GlobalTimeWrappedHourly;
            bool inCombat = target != null;

            // 基础半径（平均值）
            float baseR = inCombat ? orbitRadiusCombat : orbitRadiusIdle;
            if (inCombat)
                baseR *= 1.5f; // 你的原始逻辑：战斗时半径整体扩大

            // 取一个 0~1 的周期因子（随时间变化）
            float osc = (float)(Math.Sin(t * 2.1f + Projectile.whoAmI * 0.37f) * 0.5 + 0.5f);

            // 半径范围：最小 baseR，最大 5 × baseR
            float minR = baseR;
            float maxR = baseR * 5f;
            float currentR = MathHelper.Lerp(minR, maxR, osc);

            // 椭圆长短轴（可以保持一点比例差异）
            float rx = currentR * 1.0f; // 横向半轴
            float ry = currentR * 1.2f; // 纵向半轴（稍长一点）

            // 椭圆自身还做一个缓慢的整体旋转（非固定朝向的椭圆，显得更“活”）
            float ellipseRot = (inCombat ? 0.6f : 0.3f) * (float)Math.Sin(t * (inCombat ? 0.7f : 0.45f) + Projectile.whoAmI * 0.15f);

            // 角速度：在原有基础上叠加噪声；半径更大时角速度略增（“大的时候更快一点”）
            float omegaBase = inCombat ? orbitSpeedCombat : orbitSpeedIdle;
            float omegaNoise = 0.9f + 0.25f * (float)Math.Sin(t * 1.1f + Projectile.whoAmI * 0.3f); // 0.65~1.15倍左右
                                                                                                    // 用当前椭圆“平均半径”驱动角速度微提升
            float avgR = (rx + ry) * 0.5f;
            float boost = MathHelper.Lerp(0.95f, 1.25f, Utils.GetLerpValue(baseR, baseR * (inCombat ? 1.5f : 1.25f), avgR, true));
            float omega = omegaBase * omegaNoise * boost;

            // 相位推进（顺/逆时针）
            orbitAngle += omega * orbitDir;

            // 轨道中心
            Vector2 pivot = inCombat ? target.Center : owner.Center;

            // 计算椭圆上的期望位置（先做长短轴分离，再整体旋转椭圆）
            Vector2 ellipse = new Vector2((float)Math.Cos(orbitAngle) * rx, (float)Math.Sin(orbitAngle) * ry).RotatedBy(ellipseRot);
            Vector2 desiredPos = pivot + ellipse;

            // ---------- 速度规划：最大速度也做轻微波动，惯性插值确保丝滑 ----------
            float dynamicMaxSpeed = maxSpeed * (inCombat ? 1.25f : 1.00f) * (1f + 0.12f * (float)Math.Sin(t * 1.85f + Projectile.whoAmI));
            Vector2 toGoal = desiredPos - Projectile.Center;
            float speed = Math.Min(dynamicMaxSpeed, toGoal.Length());
            Vector2 desiredVel = (speed <= 0.001f) ? Vector2.Zero : toGoal.SafeNormalize(Vector2.UnitY) * speed;

            // 惯性插值（越大越平滑）
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVel) / inertia;

            // 姿态：沿速度方向微倾，保持“机翼”既不呆板也不过度摇摆
            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;








            // —— 持续开火（仅战斗态） ——
            if (!kamikazeMode && target != null)
            {
                if (shootTimer > 0) shootTimer--;
                if (shootTimer <= 0 && laserShotsLeft > 0)
                {
                    // 发射激光
                    Vector2 aimPoint = target.Center + target.velocity * leadTime;
                    Vector2 dir = (aimPoint - Projectile.Center).SafeNormalize(Vector2.UnitY);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        dir,
                        ModContent.ProjectileType<TEM00LeftLazer>(),
                        Projectile.damage,
                        0f,
                        Projectile.owner
                    );

                    // ===== 在这里加入“反方向特效喷发” =====
                    Vector2 backDir = -dir; // 反方向
                    Color[] techBlue =
                    {
    new Color(80, 200, 255),
    new Color(120, 220, 255),
    Color.Cyan,
    new Color(180, 220, 255),
    Color.WhiteSmoke
};

                    // SquishyLight：亮度核心
                    for (int i = 0; i < 6; i++)
                    {
                        var exo = new SquishyLightParticle(
                            Projectile.Center,
                            backDir.RotatedByRandom(0.3f) * Main.rand.NextFloat(12f, 20f),
                            Main.rand.NextFloat(0.25f, 0.4f),
                            techBlue[Main.rand.Next(techBlue.Length)],
                            Main.rand.Next(14, 22),
                            opacity: 1f,
                            squishStrenght: 1f,
                            maxSquish: Main.rand.NextFloat(2.4f, 3.4f),
                            hueShift: 0f
                        );
                        GeneralParticleHandler.SpawnParticle(exo);
                    }

                    // Spark：锐利喷射
                    for (int i = 0; i < 10; i++)
                    {
                        var sp = new SparkParticle(
                            Projectile.Center,
                            backDir.RotatedByRandom(0.45f) * Main.rand.NextFloat(16f, 28f),
                            false,
                            Main.rand.Next(12, 18),
                            Main.rand.NextFloat(0.8f, 1.4f),
                            Color.Lerp(techBlue[Main.rand.Next(techBlue.Length)], Color.White, 0.4f)
                        );
                        GeneralParticleHandler.SpawnParticle(sp);
                    }

                    // GlowOrb：柔和点缀
                    for (int i = 0; i < 5; i++)
                    {
                        var orb = new GlowOrbParticle(
                            Projectile.Center,
                            backDir.RotatedByRandom(0.5f) * Main.rand.NextFloat(8f, 14f),
                            false,
                            Main.rand.Next(6, 10),
                            Main.rand.NextFloat(0.9f, 1.3f),
                            techBlue[Main.rand.Next(techBlue.Length)],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }




                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

                    laserShotsLeft--; // 消耗一次

                    // 只有还剩下激光时，才重置寿命
                    if (laserShotsLeft > 0)
                        Projectile.timeLeft = 80;

                    // 动态计算冷却：随发射次数逐渐减少
                    float progress = 1f - (laserShotsLeft / (float)maxLaserShots);
                    int cd = (int)MathHelper.Lerp(startFireCd, endFireCd, progress);
                    shootTimer = cd;

                    // 激光发射期间：本体不造成伤害
                    Projectile.friendly = false;
                }

                // 激光用光 → 进入撞击模式
                if (laserShotsLeft <= 0)
                {
                    kamikazeMode = true;
                    Projectile.friendly = true;
                }
            }






            if (kamikazeMode)
            {
                Projectile.friendly = true;
                // 找到最近敌人（重新锁定）
                NPC targetNpc = null;
                float minDist = searchRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.active || n.friendly || !n.CanBeChasedBy(this)) continue;
                    float d = Vector2.Distance(n.Center, Projectile.Center);
                    if (d < minDist)
                    {
                        minDist = d;
                        targetNpc = n;
                    }
                }

                if (targetNpc != null)
                {
                    // 指数级加速 → 像导弹一样砸过去
                    Vector2 dir = (targetNpc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * (maxSpeed * 3f), 0.1f);
                }
            }

            if (target == null)
            {
                noTargetTimer++;

                if (noTargetTimer >= 240) // 进入狂野冲刺模式
                {
                    // === 狂野前冲模式 ===
                    if (Projectile.velocity.LengthSquared() < 0.01f)
                    {
                        // 如果初速太小，就用当前朝向给个初速度
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * 8f;
                    }

                    // 每帧速度乘以 1.1，快速加速
                    Projectile.velocity *= 1.1f;

                    // 确保碰撞有效
                    Projectile.tileCollide = true;

                    // ====== 冲刺计时器 ======
                    dashTimer++;
                    if (dashTimer >= 180) // 冲刺超过 180 帧（3 秒）就自毁
                    {
                        Projectile.Kill();
                        return;
                    }

                    return; // 不再执行后续盘旋逻辑
                }

                return; // 还没到进入冲刺的条件
            }
            else
            {
                // 一旦有敌人，重置两个计时器
                noTargetTimer = 0;
                dashTimer = 0;
            }






            // =====================【特效：优雅飞行外观】=====================
            // 少量光照（科技蓝）
            Lighting.AddLight(Projectile.Center, 0.15f, 0.35f, 0.75f);

            // 2) 细长线性粒子（能量丝流）——几乎静止的轨迹光
            if (Main.rand.NextBool(2))
            {
                AltSparkParticle spark5 = new AltSparkParticle(
                    Projectile.Center - Projectile.velocity * 1.5f,
                    Projectile.velocity * 0.01f,
                    false,
                    8,
                    1.3f,
                    Color.Cyan * 0.135f
                );
                GeneralParticleHandler.SpawnParticle(spark5);
            }

            // 5) 点刺型粒子（橙色针刺，做能量脉冲点）
            if (Main.rand.NextBool(8))
            {
                PointParticle leftSpark = new PointParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.5f,
                    false,
                    15,
                    1.1f,
                    Color.Orange
                );
                GeneralParticleHandler.SpawnParticle(leftSpark);
            }

            // 小型 GlowOrb（清爽的快消光珠）
            if (Main.rand.NextBool(5))
            {
                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center,
                    Vector2.Zero,
                    false,
                    5,
                    0.9f,
                    Color.Lerp(Color.White, Color.Cyan, 0.45f),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // 11) 四方粒子（赛博几何感）
            if (Main.rand.NextBool(6))
            {
                SquareParticle squareParticle = new SquareParticle(
                    Projectile.Center,
                    Projectile.velocity * 0.5f,
                    false,
                    30,
                    1.7f + Main.rand.NextFloat(0.6f),
                    Color.Cyan * 1.5f
                );
                GeneralParticleHandler.SpawnParticle(squareParticle);
            }

            // 5+) 水雾粒子（柔化能量尾烟）
            if (Main.rand.NextBool(10))
            {
                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.3f,
                    false,
                    Main.rand.Next(18, 26),
                    0.9f + Main.rand.NextFloat(0.3f),
                    Color.LightBlue * 0.9f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            if (kamikazeMode)
            {
                // 自杀冲锋命中 → 造成一次伤害并立刻消亡
                Projectile.Kill();
                return;
            }



        }






        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

            // 找到最近目标
            NPC target = null;
            float minDist = 1200f; // 可调的范围
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || !n.CanBeChasedBy(this)) continue;
                float d = Vector2.Distance(n.Center, Projectile.Center);
                if (d < minDist)
                {
                    minDist = d;
                    target = n;
                }
            }

            // 如果找到敌人 → 生成 TEM00RightAIM
            if (target != null)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero, // 没有初速度，固定在敌人身上
                    ModContent.ProjectileType<TEM00RightAIM>(),
                    Projectile.damage,
                    0f,
                    owner.whoAmI
                );
            }




        }



    }
}
