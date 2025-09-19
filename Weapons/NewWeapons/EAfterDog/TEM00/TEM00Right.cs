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
            maxLaserShots = 30;
            laserShotsLeft = maxLaserShots;
            kamikazeMode = false;
        }


        public override void AI()
        {
            // =====================【飞行逻辑：激光战舰AI】=====================
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 持续保活；本体不造成接触伤害，避免误伤/卡位
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.friendly = false;

            // 初始化：随机确定一个顺/逆时针旋向（稳定飞行气质）
            if (!inited)
            {
                inited = true;
                // 用 ai[1] 记录旋向（±1），避免用 localAI（遵循你的规则）
                if (Projectile.ai[1] == 0f)
                    Projectile.ai[1] = Main.rand.NextBool() ? 1f : -1f;
                orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            int orbitDir = Projectile.ai[1] > 0 ? 1 : -1;

            // 目标获取/校验
            NPC target = null;
            if (targetId >= 0 && targetId < Main.maxNPCs)
            {
                NPC cand = Main.npc[targetId];
                if (cand.active && !cand.friendly && cand.CanBeChasedBy(this))
                    target = cand;
            }
            // 定期重寻
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

            // 轨道中心与参数：无敌人绕玩家，有敌人绕敌人
            Vector2 pivot = (target == null) ? owner.Center : target.Center;
            float desiredRadius = (target == null) ? orbitRadiusIdle : orbitRadiusCombat;

            // 呼吸半径（更优雅）：随时间轻微起伏
            float breath = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (target == null ? 2.0f : 2.8f) + Projectile.whoAmI) * 6f;
            float actualRadius = desiredRadius + breath;

            // 角速度（战斗态更快）
            float omega = (target == null ? orbitSpeedIdle : orbitSpeedCombat) * orbitDir;
            orbitAngle += omega;

            // 理想轨道位置
            Vector2 desiredPos = pivot + orbitAngle.ToRotationVector2() * actualRadius;

            // 用“期望速度 + 惯性插值”让飞行丝滑（避免瞬间折线）
            Vector2 toGoal = desiredPos - Projectile.Center;
            float speed = MathHelper.Clamp(toGoal.Length(), 0f, maxSpeed);
            Vector2 desiredVel = (speed <= 0.001f) ? Vector2.Zero : toGoal.SafeNormalize(Vector2.UnitY) * speed;
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVel) / inertia;

            // 姿态：沿速度切线轻轻“机翼倾斜”，既不过度也不钝
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
                    Projectile.timeLeft = 80;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

                    laserShotsLeft--; // 消耗一次
                    shootTimer = Main.rand.Next(fireCdMin, fireCdMax);

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
            // 只在第一次命中时触发
            if (!Projectile.localAI[0].Equals(1f))
            {
                // 关闭后续伤害
                Projectile.friendly = false;

                // 标记已命中过
                Projectile.localAI[0] = 1f;

                // 随机决定转向方向（-1 = 左转，+1 = 右转）
                Projectile.ai[0] = Main.rand.NextBool() ? -1f : 1f;

                // ====== 召唤 3 条激光 ======
                Player owner = Main.player[Projectile.owner];
                Vector2 center = Projectile.Center;

                //// 中心激光：正对自身 → 命中点回收
                //Vector2 dirMain = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                //SpawnMathLaser(center, dirMain, owner);

                //// 左右两条激光：±45° 对称
                //Vector2 dirLeft = dirMain.RotatedBy(MathHelper.ToRadians(45));
                //Vector2 dirRight = dirMain.RotatedBy(MathHelper.ToRadians(-45));
                //SpawnMathLaser(center, dirLeft, owner);
                //SpawnMathLaser(center, dirRight, owner);

                // 音效 / 特效
                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                for (int i = 0; i < 12; i++)
                {
                    Dust d = Dust.NewDustPerfect(center, DustID.Electric,
                        Main.rand.NextVector2Circular(4f, 4f));
                    d.noGravity = true;
                    d.scale = 1.2f;
                    d.color = Color.Lerp(Color.White, Color.Cyan, 0.6f);
                }


                // ====== 在敌人身上生成 TEM00RightAIM ======
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

        ////生成数学感激光（浅蓝→白）
        //private void SpawnMathLaser(Vector2 start, Vector2 direction, Player owner)
        //{
        //    int damage = (int)(Projectile.damage * 0.8f); // 略低伤害
        //    float kb = 2f;
        //    Projectile.NewProjectile(
        //        Projectile.GetSource_FromThis(),
        //        start,
        //        direction,
        //        ModContent.ProjectileType<TEM00LeftLazer>(), // 直接调用之前的激光弹幕
        //        damage,
        //        kb,
        //        owner.whoAmI
        //    );
        //}






        public override void OnKill(int timeLeft)
        {
            // 弹幕死亡（时间到或碰撞）时执行，可用于生成碎裂粒子、播放破碎音效
        }


    }
}
