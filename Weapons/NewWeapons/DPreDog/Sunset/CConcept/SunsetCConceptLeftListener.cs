using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Collections.Generic;
using CalamityMod;
using Terraria.Audio;
using CalamityMod.Particles;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftListener : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 他需要有一个贴图贴图

        private List<int> subordinateProjectiles = new List<int>(); // 存储子弹幕的 ID
        public int Time;
        private const int SpinTime = 120; // 旋转阶段持续时间
        private const float SpinSpeed = 8f; // 旋转速度
        private bool HasTransitioned = false; // 标志是否已经进入固定状态

        public Player Owner => Main.player[Projectile.owner];
        public float SpinCompletion => Utils.GetLerpValue(0f, SpinTime, Time, true);
        public ref float InitialDirection => ref Projectile.ai[0];
        public ref float SpinDirection => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            base.OnSpawn(source);
            SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

        }


        public override void AI()
        {
            // **确保初始方向被正确记录**
            if (Time == 0)
            {
                InitialDirection = Owner.DirectionTo(Main.MouseWorld).ToRotation();
                SpinDirection = Main.rand.NextBool().ToDirectionInt();
                Projectile.netUpdate = true;
            }

            // **旋转阶段**
            if (Time < SpinTime)
            {
                // **计算旋转角度**
                Projectile.rotation = (float)Math.Pow(SpinCompletion, 0.82) * MathHelper.Pi * SpinDirection * 12f
                                      + InitialDirection + MathHelper.PiOver4;

                // 使投射物与玩家保持一致并瞄准鼠标位置
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
                }

                // 对齐到玩家中心
                Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
                Owner.heldProj = Projectile.whoAmI;
            }
            else if (!HasTransitioned)
            {
                // **立正阶段，进入渐进式移动**
                HasTransitioned = true;
            }

            // **如果已经进入立正阶段，则逐渐向前移动**
            if (HasTransitioned)
            {
                Vector2 targetPosition = Owner.Center + InitialDirection.ToRotationVector2() * (80f + 15f * 16f);
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.1f); // **平滑移动**
                Projectile.rotation = InitialDirection + MathHelper.PiOver4;
                SpawnTargetProjectiles();
            }

            // **固定阶段开始生成方形粒子**
            if (HasTransitioned)
            {
                GenerateParticles();
            }

            // **确保玩家手臂始终与长枪保持水平**
            ManipulatePlayerArmPositions();

            // **如果玩家松开武器，则销毁弹幕**
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.Kill();
                // **销毁 `Magic`（魔法阵）**
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() && proj.owner == Projectile.owner)
                    {
                        proj.Kill();
                    }
                }
            }
            Projectile.timeLeft = 180;
            Time++;
        }

        // ============ 概念形态·几何扩散特效 ============
        // 外部可调速度参数
        public static float CritSparkSpeed = 8f;   // CritSpark 圆环速度
        public static float SquishySpeed = 6f;   // Squishy 光束速度
        public static float OrbSpeed = 4f;   // GlowOrb 方形速度
        public static float TrailSpeed = 7f;   // SparkParticle 随机扩散速度

        // 外部控制旋转角度累积
        private float squishyRotationOffset = 0f;   // Squishy 扩散角度偏移
        private float orbRotationOffset = 0f;   // GlowOrb 方形角度偏移

        private void GenerateParticles()
        {
            Vector2 gunHeadDirection = (InitialDirection).ToRotationVector2();
            Vector2 gunHeadPosition = Projectile.Center + gunHeadDirection * 16f * 3.5f;

            // 银灰配色
            Color[] techMetal = {
        Color.Silver,
        new Color(200, 208, 216),
        new Color(150, 164, 180),
        new Color(176, 196, 222)
    };
            Color coreWhite = Color.WhiteSmoke;

            // 全局时间参数
            float t = (float)Main.GameUpdateCount * 0.05f;

            // =====================================================
            // 1. CritSpark —— 组成圆环并向外扩散
            // =====================================================
            int ringCount = 12;                           // 每个圆环的点数
            float ringRadius = 2f * 16f;                  // 初始圆环半径
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi / ringCount * i;
                Vector2 pos = gunHeadPosition + angle.ToRotationVector2() * ringRadius;
                Vector2 vel = angle.ToRotationVector2() * CritSparkSpeed;

                CritSpark spark = new CritSpark(
                    pos,
                    vel,
                    techMetal[Main.rand.Next(techMetal.Length)],
                    coreWhite,
                    1.4f,
                    22
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // =====================================================
            // 2. SquishyLightParticle —— 5个平均分布的稳定扩散，整体旋转
            // =====================================================
            int squishyCount = 5;
            for (int i = 0; i < squishyCount; i++)
            {
                float angle = MathHelper.TwoPi / squishyCount * i + squishyRotationOffset;
                Vector2 vel = angle.ToRotationVector2() * SquishySpeed;

                SquishyLightParticle exo = new SquishyLightParticle(
                    gunHeadPosition,
                    vel,
                    0.45f,
                    techMetal[Main.rand.Next(techMetal.Length)],
                    28,
                    opacity: 1f,
                    squishStrenght: 1.2f,
                    maxSquish: 3.5f
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }
            squishyRotationOffset += 0.15f; // 每次调用旋转偏移

            // =====================================================
            // 3. GlowOrbParticle —— 方形分布并持续旋转
            // =====================================================
            float halfSide = 16f; // 边长 2×16 → 半边 16
            Vector2[] squareCorners =
            {
        new Vector2(-halfSide, -halfSide),
        new Vector2( halfSide, -halfSide),
        new Vector2( halfSide,  halfSide),
        new Vector2(-halfSide,  halfSide)
    };

            for (int i = 0; i < squareCorners.Length; i++)
            {
                Vector2 offset = squareCorners[i].RotatedBy(orbRotationOffset);
                Vector2 pos = gunHeadPosition + offset;
                Vector2 vel = offset.SafeNormalize(Vector2.UnitX) * OrbSpeed;

                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    vel,
                    false,
                    20,
                    1.2f,
                    techMetal[Main.rand.Next(techMetal.Length)],
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
            orbRotationOffset += 0.05f; // 方形旋转角度累积

            // =====================================================
            // 4. SparkParticle —— 随机扩散填充
            // =====================================================
            if (Main.rand.NextBool(1))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * TrailSpeed;

                SparkParticle trail = new SparkParticle(
                    gunHeadPosition,
                    vel,
                    false,
                    50,
                    1.1f,
                    new Color(210, 218, 230)
                );
                GeneralParticleHandler.SpawnParticle(trail);
                ownedSparkParticles.Add(trail); // 保存引用
            }

            for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
            {
                SparkParticle p = ownedSparkParticles[i];

                if (p.Time >= p.Lifetime)
                {
                    ownedSparkParticles.RemoveAt(i); // 粒子死了就移除
                    continue;
                }

                // 每帧往左旋转 3°
                p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(-3f));
            }


            // =====================================================
            // （可选增强）—— 给整体增加一个随时间变化的脉冲波
            // =====================================================
            if (Main.GameUpdateCount % 20 == 0) // 每 20 帧一个环
            {
                int pulseCount = 8;
                float pulseRadius = 20f;
                for (int i = 0; i < pulseCount; i++)
                {
                    float angle = MathHelper.TwoPi / pulseCount * i + t;
                    Vector2 pos = gunHeadPosition + angle.ToRotationVector2() * pulseRadius;
                    Vector2 vel = angle.ToRotationVector2() * (TrailSpeed * 0.5f);

                    Particle pulse = new SparkParticle(
                        pos,
                        vel,
                        false,
                        30,
                        1.0f,
                        Color.LightGray
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
        }

        private List<SparkParticle> ownedSparkParticles = new List<SparkParticle>();








        private void SpawnTargetProjectiles()
        {
            NPC target = FindClosestTarget();
            if (target == null) return;

            // **检查是否已经存在 `SunsetCConceptLeftMagic`**
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() && proj.owner == Projectile.owner)
                {
                    return; // **如果已经存在 `Magic`，则不生成新的**
                }
            }

            // **生成 `SunsetCConceptLeftMagic` 造成伤害**
            int damageProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                target.Center, Vector2.Zero,
                ModContent.ProjectileType<SunsetCConceptLeftMagic>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                target.whoAmI);

            subordinateProjectiles.Add(damageProj);
        }

        private NPC FindClosestTarget()
        {
            NPC closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (npc.boss && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                    else if (!closestTarget?.boss ?? true && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                }
            }
            return closestTarget;
        }

        public void ManipulatePlayerArmPositions()
        {
            Vector2 gunHeadPosition = Owner.Center + InitialDirection.ToRotationVector2() * 80f;

            // **让玩家手臂方向始终指向枪头**
            float armRotation = (gunHeadPosition - Owner.Center).ToRotation();

            Owner.ChangeDir((Math.Cos(armRotation) > 0f).ToDirectionInt());
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = CalamityUtils.WrapAngle90Degrees(armRotation - MathHelper.PiOver2);

            Projectile.Center = Owner.Center; // 监听弹幕仍然依附于玩家
            if (Owner.CantUseHoldout())
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // **绘制刀盘特效**
            if (SpinCompletion >= 0f && SpinCompletion < 1f)
            {
                Texture2D smear = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmear").Value;

                float rotation = Projectile.rotation - MathHelper.Pi / 5f;
                if (SpinDirection == -1f)
                    rotation += MathHelper.Pi;

                Color smearColor = Color.GhostWhite * CalamityUtils.Convert01To010(SpinCompletion) * 0.9f;
                Vector2 smearOrigin = smear.Size() * 0.5f;

                Main.EntitySpriteDraw(smear, Owner.Center - Main.screenPosition, null, smearColor with { A = 0 }, rotation, smearOrigin, Projectile.scale * 1.45f, 0, 0);
            }

            // **计算监听弹幕的绘制方向**
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            // **修正旋转方向**
            float adjustedRotation = Projectile.rotation;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), adjustedRotation, origin, Projectile.scale, 0, 0);

            return false;
        }
    }
}
