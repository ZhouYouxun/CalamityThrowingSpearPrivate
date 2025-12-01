using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using Terraria.DataStructures;
using static CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget.SunsetBForgetLeft;
using Terraria.GameContent.Drawing;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetRight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.PiOver2 : 0f);

            // ===== 动态包边颜色（黄绿交替，可随时间变化）=====
            // 利用 Main.GlobalTimeWrappedHourly 做动态旋转，使颜色条纹不断流动
            float time = Main.GlobalTimeWrappedHourly * 3f;
            float offsetRadius = 6f;     // 包边半径大小
            int segments = 18;           // 多少条边缘光

            for (int i = 0; i < segments; i++)
            {
                // 动态旋转，让颜色不断流动
                float ang = (MathHelper.TwoPi * i / segments) + time;

                Vector2 offset = ang.ToRotationVector2() * offsetRadius;

                // 经过 sin 波动决定颜色在黄 ↔ 绿之间变化
                float wave = (float)Math.Sin(ang + time * 0.7f);
                Color edgeColor = Color.Lerp(new Color(230, 255, 60), new Color(100, 255, 120), (wave + 1f) * 0.5f);

                edgeColor *= 0.55f; // 半透明辉光
                edgeColor.A = 0;

                Main.spriteBatch.Draw(
                    texture,
                    drawPos + offset,
                    null,
                    edgeColor,
                    rotation,
                    origin,
                    Projectile.scale,
                    spriteEffects,
                    0f
                );
            }

            // ===== 绘制本体 =====
            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                Projectile.GetAlpha(lightColor),
                rotation,
                origin,
                Projectile.scale,
                spriteEffects,
                0f
            );

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public enum BehaviorState
        {
            Aim
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 🚨 遍历所有投射物，检查是否已有 `Aim` 状态的 `SunsetASunsetRight`
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.whoAmI != Projectile.whoAmI)
                {
                    // 仅检测相同类型的 `Aim` 状态投射物
                    if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() && proj.ModProjectile is SunsetASunsetRight rightProj && rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill(); // ❌ 删除自己（新的投射物）
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetBForgetRight>() && proj.ModProjectile is SunsetBForgetRight forgetProj && forgetProj.CurrentState == SunsetBForgetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() && proj.ModProjectile is SunsetCConceptRight conceptProj && conceptProj.CurrentState == SunsetCConceptRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
            }
        }

        private int shootTimer = 0; // 计时器
        private float rotationAngle = 0f; // 用于记录当前旋转角度

        // 在类里新建字段（不要用 localAI）
        private int soundTimer = 0;
        private float currentPitch = 0f;

        private void DoBehavior_Aim()
        {
            // —— 音效逻辑 —— 
            soundTimer++;
            if (soundTimer >= 5) // 每 5 帧播放一次
            {
                soundTimer = 0;

                // 累加音调，但不超过上限
                currentPitch = Math.Min(currentPitch + 0.05f, 0.8f);

                // 播放音效
                SoundEngine.PlaySound(
                    SoundID.Item73 with
                    {
                        Volume = 0.7f,
                        Pitch = currentPitch
                    },
                    Projectile.Center
                );
            }


            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = false;


            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            // 计时器增加
            shootTimer++;
            if (shootTimer >= 15) // 每 15 帧执行一次
            {
                shootTimer = 0; // 重置计时器

                // 获取最近的目标，优先选择 Boss
                NPC target = FindClosestTarget();
                if (target != null)
                {
                    if (target == null) return;

                    // **每次随机选择两个对立面，而不是使用固定的旋转角度**
                    float angle1 = Main.rand.NextFloat(0, MathHelper.TwoPi); // 随机角度
                    float angle2 = angle1 + MathHelper.Pi; // 计算对立角度（180° 相反方向）

                    // 控制对立弹幕的生成距离（更远或更近）
                    float spawnDistance = 15 * 16f;

                    // 计算对立面位置
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 oppositeOffset1 = directionToTarget.RotatedBy(angle1) * spawnDistance;
                    Vector2 oppositeOffset2 = directionToTarget.RotatedBy(angle2) * spawnDistance;

                    Vector2 spawnPos1 = target.Center + oppositeOffset1;
                    Vector2 spawnPos2 = target.Center + oppositeOffset2;

                    Vector2 velocity1 = (target.Center - spawnPos1).SafeNormalize(Vector2.Zero) * 20f;
                    Vector2 velocity2 = (target.Center - spawnPos2).SafeNormalize(Vector2.Zero) * 20f;

                    // 计算玩家当下此伤害类别的“总暴击率”（包含饰品、Buff、Calamity 全局加成等）
                    int totalCrit = (int)Math.Round(Owner.GetTotalCritChance(Projectile.DamageType));

                    int proj1 = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos1,
                        velocity1,
                        ModContent.ProjectileType<SunsetBForgetRightCut>(),
                        Projectile.damage / 2,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    if (proj1.WithinBounds(Main.maxProjectiles))
                        Main.projectile[proj1].CritChance = totalCrit; // ✅ 直接写总暴击

                    int proj2 = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos2,
                        velocity2,
                        ModContent.ProjectileType<SunsetBForgetRightCut>(),
                        Projectile.damage / 2,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    if (proj2.WithinBounds(Main.maxProjectiles))
                        Main.projectile[proj2].CritChance = totalCrit; // ✅ 同上



                    // 播放音效
                    SoundEngine.PlaySound(SoundID.Item103, Projectile.position);
                }
            }





            // 计算枪头位置
            Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 4f;


            Vector2 gunHead = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 80f;

            //CTSLightingBoltsSystem.Spawn_BlueGoldFloaters(gunHead, 1f);


            {
                // 计算枪头位置（朝向 + 固定距离）
                Vector2 gunDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                gunHeadPosition = Projectile.Center + gunDir * 16f * 4f;

                // ===============================
                // 枪口复合特效（黄 + 绿主题）
                // ===============================
                Color mainYellow = new Color(255, 235, 80);
                Color mainGreen = new Color(120, 255, 140);
                float t = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f;
                Color mixedColor = Color.Lerp(mainYellow, mainGreen, t);


                // 1）椭圆冲击波（DirectionalPulseRing）
                if (Main.rand.NextBool(1))
                {
                    var pulse = new DirectionalPulseRing(
                        gunHeadPosition,
                        gunDir * 4f,
                        mixedColor,
                        new Vector2(0.7f, 1.6f),
                        Projectile.rotation - MathHelper.PiOver4,
                        0.12f,
                        0.04f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                    ownedPulses.Add(pulse);
                }


                // 2）SquishyLight EXO 光点
                if (Main.rand.NextBool(2))
                {
                    Color exoColor = Main.rand.NextBool() ? mainYellow : mainGreen;
                    var exo = new SquishyLightParticle(
                        gunHeadPosition + Main.rand.NextVector2Circular(6f, 6f),
                        (-Vector2.UnitY * 0.6f + gunDir * 0.4f).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 1.4f),
                        Main.rand.NextFloat(0.20f, 0.30f),
                        exoColor,
                        Main.rand.Next(20, 30),
                        opacity: 1f,
                        squishStrenght: 1f,
                        maxSquish: Main.rand.NextFloat(2.0f, 3.0f),
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(exo);
                    ownedExos.Add(exo);
                }


                // 3）GlowOrb 魔法阵小光球
                if (Main.rand.NextBool(2))
                {
                    int orbCount = 4;
                    float orbRadius = 10f;
                    for (int i = 0; i < orbCount; i++)
                    {
                        float ang = Main.GlobalTimeWrappedHourly * 2.2f + MathHelper.TwoPi * i / orbCount;
                        Vector2 orbOffset = ang.ToRotationVector2() * orbRadius;

                        Color orbColor = Color.Lerp(mainGreen, mainYellow, (float)i / orbCount);
                        var orb = new GlowOrbParticle(
                            gunHeadPosition + orbOffset,
                            Vector2.Zero,
                            false,
                            Main.rand.Next(6, 10),
                            Main.rand.NextFloat(0.7f, 1.0f),
                            orbColor,
                            true,
                            false,
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                        ownedOrbs.Add(orb);
                    }
                }


                // 4）SparkParticle 火花
                if (Main.rand.NextBool(1))
                {
                    float sparkPhase = Main.GlobalTimeWrappedHourly * 1.7f + Projectile.whoAmI * 0.23f;
                    float sparkAngleOffset = Main.rand.NextFloat(-0.35f, 0.35f)
                                             + 0.12f * (float)Math.Sin(sparkPhase);

                    float speedLerp = (float)Math.Sin(Main.rand.NextFloat() * MathHelper.Pi);
                    float speed = MathHelper.Lerp(7.5f, 13.5f, speedLerp);

                    Vector2 vel = gunDir.RotatedBy(sparkAngleOffset) * speed;

                    var spark = new SparkParticle(
                        gunHeadPosition + Main.rand.NextVector2Circular(8f, 8f),
                        vel,
                        false,
                        Main.rand.Next(18, 26),
                        Main.rand.NextFloat(0.9f, 1.2f),
                        mixedColor
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                    ownedSparks.Add(spark);
                }


                // 5）PointParticle 尖锐碎片
                if (Main.rand.NextBool(2))
                {
                    float baseAngle = MathHelper.TwoPi / 10f * Main.rand.Next(10);
                    float localT = Main.rand.NextFloat();
                    float angle = baseAngle + (localT - 0.5f) * 0.7f;

                    float speedFactor = (float)Math.Sin(localT * MathHelper.Pi) * 0.5f + 0.5f;
                    float speed = MathHelper.Lerp(8f, 16f, speedFactor);

                    Vector2 vel = gunDir.RotatedBy(angle * 0.08f) * speed;

                    var point = new PointParticle(
                        gunHeadPosition,
                        vel,
                        false,
                        Main.rand.Next(12, 18),
                        1.1f + Main.rand.NextFloat(0.3f),
                        Main.rand.NextBool() ? mainYellow : mainGreen
                    );
                    GeneralParticleHandler.SpawnParticle(point);
                    ownedPoints.Add(point);
                }


                // 6）BloomRing
                if (Main.GameUpdateCount % 2 == 0)
                {
                    var ring = new BloomRing(
                        gunHeadPosition,
                        Vector2.Zero,
                        mixedColor * 0.9f,
                        0.5f,
                        35
                    );
                    GeneralParticleHandler.SpawnParticle(ring);
                    ownedBloomRings.Add(ring);
                }


                // 7）GenericBloom
                if (Main.GameUpdateCount % 30 == 0)
                {
                    var bloom = new GenericBloom(
                        gunHeadPosition,
                        Vector2.Zero,
                        mixedColor,
                        1.1f,
                        30
                    );
                    GeneralParticleHandler.SpawnParticle(bloom);
                    ownedGenericBlooms.Add(bloom);
                }


                // 8）SquareParticle
                if (Main.rand.NextBool(4))
                {
                    Vector2 sqVel = gunDir.RotatedByRandom(0.7f) * Main.rand.NextFloat(2f, 5f);
                    var square = new SquareParticle(
                        gunHeadPosition,
                        sqVel,
                        false,
                        30,
                        1.4f + Main.rand.NextFloat(0.4f),
                        mixedColor * 1.3f
                    );
                    GeneralParticleHandler.SpawnParticle(square);
                    ownedSquares.Add(square);
                }
            }

            {
                // ========== 相对跟随模块（所有粒子统一处理） ==========
                if (!Main.dedServ)
                {
                    if (lastCenter == Vector2.Zero)
                        lastCenter = Projectile.Center;

                    Vector2 delta = Projectile.Center - lastCenter;
                    lastCenter = Projectile.Center;

                    // Pulse
                    foreach (var p in ownedPulses)
                        p.Position += delta;

                    // EXO
                    foreach (var p in ownedExos)
                        p.Position += delta;

                    // Orbs
                    foreach (var p in ownedOrbs)
                        p.Position += delta;

                    // Sparks
                    foreach (var p in ownedSparks)
                        p.Position += delta;

                    // Points
                    foreach (var p in ownedPoints)
                        p.Position += delta;

                    // BloomRing
                    foreach (var p in ownedBloomRings)
                        p.Position += delta;

                    // GenericBloom
                    foreach (var p in ownedGenericBlooms)
                        p.Position += delta;

                    // Squares
                    foreach (var p in ownedSquares)
                        p.Position += delta;
                }

            }


            // 检测松手，直接删除自身
            Player player = Main.player[Projectile.owner];
            if (!player.Calamity().mouseRight)
            {
                Projectile.Kill();
            }
        }
        // === 枪头特效的粒子池（全部相对跟随用） ===
        private List<DirectionalPulseRing> ownedPulses = new();
        private List<SquishyLightParticle> ownedExos = new();
        private List<GlowOrbParticle> ownedOrbs = new();
        private List<SparkParticle> ownedSparks = new();
        private List<PointParticle> ownedPoints = new();
        private List<BloomRing> ownedBloomRings = new();
        private List<GenericBloom> ownedGenericBlooms = new();
        private List<SquareParticle> ownedSquares = new();

        // 用于跟随的上一帧中心
        private Vector2 lastCenter;

        private NPC FindClosestTarget()
        {
            NPC closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy()) // 确保是合法目标
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);

                    // 优先选择 Boss，如果不存在 Boss 则选择最近的小怪
                    if (npc.boss && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                    else if (!closestTarget?.boss ?? true) // 如果当前目标不是 Boss，允许更新为最近的非 Boss 目标
                    {
                        if (distance < closestDistance)
                        {
                            closestTarget = npc;
                            closestDistance = distance;
                        }
                    }
                }
            }
            return closestTarget;
        }


        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒
        }
    }
}