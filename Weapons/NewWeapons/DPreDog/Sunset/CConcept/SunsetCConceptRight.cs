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
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget;
using Terraria.DataStructures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Terraria.GameContent.Events;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
 
        public override bool PreDraw(ref Color lightColor)
        {
            // 先画弹幕本体（你原来的效果保持不变）
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // ============ 开始绘制能量条 ============

            if (Main.myPlayer == Projectile.owner)
            {
                var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
                var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

                // 玩家头顶 36 像素位置
                Vector2 drawPos = Owner.Center - Main.screenPosition + new Vector2(0, -48);

                // X 秒读条：下面是3000帧
                float p = holdTime / 3000f;
                p = MathHelper.Clamp(p, 0f, 1f);

                // 裁切前景条显示长度
                Rectangle frame = new Rectangle(0, 0, (int)(barFG.Width * p), barFG.Height);

                // 动态颜色：蓝 → 青 → 白
                float hue = (Main.GlobalTimeWrappedHourly * 0.25f) % 1f;
                Color barColor = Main.hslToRgb(hue, 1f, 0.75f);

                // 后景
                Main.spriteBatch.Draw(
                    barBG,
                    drawPos - barBG.Size() * 0.5f,
                    null,
                    barColor * 0.50f,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );

                // 前景
                Main.spriteBatch.Draw(
                    barFG,
                    drawPos - barFG.Size() * 0.5f,
                    frame,
                    barColor,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }

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
            Aim,
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];
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
        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
            }
        }

        private int holdTime = 0; // 握持时间（帧）

        private int vfxTimer = 0;
        // 科技蓝主色 + 辅色
        private static readonly Color[] TechBluePalette = new Color[]
        {
            new Color( 80, 200, 255),  // Electric Blue
            new Color(120, 220, 255),  // Light Tech Blue
            Color.Cyan,
            new Color(180, 220, 255),  // 冷白蓝
            Color.WhiteSmoke           // 高光
        };


        // 在类的字段区定义（和 shootTimer / holdTime 一样）
        private int shotIndex = 0;
        private int frameTimer = 0;

        // 记录上一次魔法阵触发时间（用 holdTime 单位计）
        private int lastMagicFireTime = 0;

        // 魔法阵 → 大弹幕之间的 1.5 秒延迟计数器（90 帧）
        private int magicDelayTimer = 0;
        private bool stopSmallShots = false;

        private void DoBehavior_Aim()
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

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            {
                // ==========================
                //   新时间线控制
                // ==========================

                frameTimer++;
                holdTime++;

                NPC target = FindClosestTargetInRange(12000f);

                // -------------------------------
                // 45 秒（2700 帧）：生成魔法阵
                // -------------------------------
                if (holdTime == 2700)
                {
                    Vector2 anchorBase;

                    if (target != null)
                    {
                        // 有敌人 → 敌人头顶
                        anchorBase = target.Center;
                    }
                    else
                    {
                        // 无敌人 → 玩家头顶
                        anchorBase = Owner.Center;
                    }

                    Vector2 magicSpawnPos = anchorBase + new Vector2(0, -30 * 16);
                    Vector2 magicVelocity = Vector2.UnitY * 30f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        magicSpawnPos,
                        magicVelocity,
                        ModContent.ProjectileType<SunsetCConceptRightMagic>(),
                        Projectile.damage * 8,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }


                // -------------------------------
                // 50~55 秒（3000–3300 帧）：禁止小弹幕
                // -------------------------------

                bool forbidSmallShots = (holdTime >= 3000 && holdTime < 3300);

                // -------------------------------
                // 0~50 秒：发射小弹幕（0~3000 帧）
                // -------------------------------
                if (!forbidSmallShots && holdTime < 3000)
                {

                    int minInterval = 5;
                    int startInterval = 18;
                    int interval = Math.Max(minInterval, startInterval - (shotIndex / 2));

                    if (frameTimer >= interval)
                    {
                        frameTimer = 0;

                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 40f * 16f * Main.rand.NextFloat(0.65f, 1.35f);
                        Vector2 spawnOffset = angle.ToRotationVector2() * radius;

                        Vector2 ringCenter = (target != null) ? target.Center : Projectile.Center;
                        Vector2 spawnPos = ringCenter + spawnOffset;

                        Vector2 shootDir = (target != null) ?
                            (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) :
                            (-spawnOffset).SafeNormalize(Vector2.UnitY);

                        Vector2 velocity = shootDir * 40f;

                        int[] projTypes = new int[]
                        {
                ModContent.ProjectileType<SunsetCConceptRightCut1Time>(),
                ModContent.ProjectileType<SunsetCConceptRightCut2Item>(),
                ModContent.ProjectileType<SunsetCConceptRightCut3Space>(),
                ModContent.ProjectileType<SunsetCConceptRightCut4Energy>()
                        };

                        int pickType = projTypes[shotIndex % 4];
                        int totalCrit = (int)Math.Round(Owner.GetTotalCritChance(Projectile.DamageType));

                        int proj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPos,
                            velocity,
                            pickType,
                            Projectile.damage / 2,
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        if (proj.WithinBounds(Main.maxProjectiles))
                        {
                            Projectile small = Main.projectile[proj];
                            small.CritChance = totalCrit;
                            small.scale = 1.5f;
                            small.penetrate = 1;
                            small.tileCollide = true;
                            small.usesLocalNPCImmunity = true;
                            small.localNPCHitCooldown = 10;
                            small.netUpdate = true;
                        }

                        shotIndex++;
                    }
                }

                // -------------------------------
                // 61 秒（3660 帧）：强制终止右键弹幕，并触发 10 秒全局冷却
                if (holdTime >= 3660)
                {
                    Player owner = Main.player[Projectile.owner];

                    // 只有真正坚持到 60 秒读条结束的那次，才进入概念形态右键冷却
                    var cooldownPlayer = owner.GetModPlayer<ConceptRightCooldown>();
                    cooldownPlayer.conceptRightCooldown = ConceptRightCooldown.ConceptCooldownMax; // 600 = 10 秒

                    Projectile.Kill();
                    return;
                }

            }






            {
                // ========= 枪口（枪头）空间点：指向速度方向 5 * 16 =========
                Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 10f;

                // ========= 科技蓝 VFX（瞄准阶段常驻）=========
                vfxTimer++;
                float phase = vfxTimer * 0.1f;

                // —— 环参数：以枪头为圆心的小环发射，观感更“顺滑”，不是硬贴中心 —— //
                int ringCount = 6;                  // 每圈数量
                float ringRadius = 18f;                // 初始半径
                float ringPulse = 4f * (float)Math.Sin(phase * 1.2f); // 轻微呼吸脉动
                float radiusNow = ringRadius + ringPulse;

                // 【1】EXO之光（SquishyLightParticle）：高亮柔光，密度适中（每2帧一圈）
                if ((vfxTimer % 2) == 0)
                {
                    for (int i = 0; i < ringCount; i++)
                    {
                        float a = MathHelper.TwoPi * i / ringCount + phase * 0.25f;  // 有序 + 缓慢相位
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * radiusNow;              // ★ 环上生成
                                                                                      // 径向外喷 + 少量切向（微旋）
                        Vector2 vel = dir * Main.rand.NextFloat(1.6f, 2.8f) + dir.RotatedBy(MathHelper.PiOver2) * 0.3f;

                        // 科技蓝主色
                        Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                        SquishyLightParticle exo = new SquishyLightParticle(
                            pos,
                            vel,
                            Main.rand.NextFloat(0.22f, 0.34f),  // 缩放更“亮点”
                            c,
                            Main.rand.Next(20, 28),             // 短寿命更干净
                            opacity: 1f,
                            squishStrenght: Main.rand.NextFloat(0.9f, 1.2f),
                            maxSquish: 3.2f,
                            hueShift: 0f
                        );
                        GeneralParticleHandler.SpawnParticle(exo);
                    }
                }

                // 【2】辉光球（GlowOrbParticle）：清爽的亮点，稀疏（每3帧，半圈）
                if ((vfxTimer % 3) == 0)
                {
                    int orbCount = ringCount / 2; // 半圈
                    for (int i = 0; i < orbCount; i++)
                    {
                        float a = MathHelper.TwoPi * i / orbCount + phase * 0.35f + 0.7f;
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * (radiusNow + 4f);
                        // 轻微向外
                        Vector2 vel = dir * Main.rand.NextFloat(0.6f, 1.2f);

                        Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                        GlowOrbParticle orb = new GlowOrbParticle(
                            pos,
                            vel,
                            false,
                            Main.rand.Next(6, 10),          // 6~9帧
                            Main.rand.NextFloat(0.75f, 0.95f),
                            c,
                            true,                           // 加法混合，提亮
                            false,
                            true                            // 中心叠白
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }

                // 【3】四方粒子（SquareParticle）：赛博能量片，适度点缀（每4帧 1~2 个）
                if ((vfxTimer % 4) == 0)
                {
                    int squares = Main.rand.Next(1, 3);
                    for (int s = 0; s < squares; s++)
                    {
                        float a = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = gunHeadPosition + dir * Main.rand.NextFloat(radiusNow - 6f, radiusNow + 6f); // 环附近“带宽”
                        Vector2 vel = dir * Main.rand.NextFloat(0.8f, 1.6f) + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.6f, 0.6f);

                        // 主色+少量白/浅金作高光
                        Color c = (Main.rand.NextBool(4) ? Color.Gold : TechBluePalette[Main.rand.Next(TechBluePalette.Length)]);
                        c *= 1.1f;

                        SquareParticle sq = new SquareParticle(
                            pos,
                            vel,
                            false,
                            Main.rand.Next(24, 36),                            // 24~35帧
                            1.2f + Main.rand.NextFloat(0.6f),                 // 1.2~1.8
                            c
                        );
                        GeneralParticleHandler.SpawnParticle(sq);
                    }
                }

            }


            // 检测松手，直接删除自身
            Player player = Main.player[Projectile.owner];
            if (!player.Calamity().mouseRight)
            {
                Projectile.Kill();
            }
        }


        private NPC FindClosestTargetInRange(float maxRange)
        {
            NPC closestTarget = null;
            float closestDistance = maxRange;

            Vector2 playerCenter = Owner.Center;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy())
                    continue;

                float distance = Vector2.Distance(playerCenter, npc.Center);
                if (distance > maxRange)
                    continue;

                // Boss 优先
                if (npc.boss)
                {
                    if (closestTarget == null || !closestTarget.boss || distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                }
                else if (closestTarget == null || !closestTarget.boss)
                {
                    if (distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
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
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒
        }
    }
}