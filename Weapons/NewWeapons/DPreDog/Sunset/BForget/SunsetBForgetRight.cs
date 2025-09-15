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
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
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
                    float spawnDistance = 25 * 16f;

                    // 计算对立面位置
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 oppositeOffset1 = directionToTarget.RotatedBy(angle1) * spawnDistance;
                    Vector2 oppositeOffset2 = directionToTarget.RotatedBy(angle2) * spawnDistance;

                    Vector2 spawnPos1 = target.Center + oppositeOffset1;
                    Vector2 spawnPos2 = target.Center + oppositeOffset2;

                    Vector2 velocity1 = (target.Center - spawnPos1).SafeNormalize(Vector2.Zero) * 20f;
                    Vector2 velocity2 = (target.Center - spawnPos2).SafeNormalize(Vector2.Zero) * 20f;

                    // 发射弹幕
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos1, velocity1, ModContent.ProjectileType<SunsetBForgetRightCut>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos2, velocity2, ModContent.ProjectileType<SunsetBForgetRightCut>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);

                    // 播放音效
                    SoundEngine.PlaySound(SoundID.Item103, Projectile.position);
                }
            }

            // 计算枪头位置
            Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 5f;

            // 在枪头处生成粒子
            bool useYellow = Main.rand.NextBool(); // 随机选择黄色或蓝色阵营
            int dustType = useYellow ? Main.rand.Next(SunsetBForgetParticleManager.YellowDusts)
                                     : Main.rand.Next(SunsetBForgetParticleManager.BlueDusts);

            Dust dust = Dust.NewDustPerfect(
                gunHeadPosition + Main.rand.NextVector2Circular(16f, 16f), // 以枪头为中心的半径 16 圆圈
                dustType,
                -Vector2.UnitY * Main.rand.NextFloat(2f, 5f), // 让粒子向上喷射
                100,
                Color.White,
                Main.rand.NextFloat(1f, 1.8f)
            );
            dust.noGravity = true;


            // 检测松手，直接删除自身
            Player player = Main.player[Projectile.owner];
            if (!player.Calamity().mouseRight)
            {
                Projectile.Kill();
            }
        }

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