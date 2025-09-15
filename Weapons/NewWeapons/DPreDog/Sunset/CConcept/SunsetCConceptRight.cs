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

        private int shootTimer = 0; // 计时器
        private int holdTime = 0; // 握持时间（帧）

        private int postBigShotCooldown = 0; // 大招释放后进入冷却的计时器

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

            // 如果大招释放过，则进入冷却状态
            if (postBigShotCooldown > 0)
            {
                postBigShotCooldown--;
                return; // 直接跳过小弹幕的释放逻辑
            }

            // 计时器增加
            shootTimer++;
            holdTime++;

            if (shootTimer >= 15) // 每 15 帧执行一次
            {
                shootTimer = 0;

                NPC target = FindClosestTarget();
                if (target != null)
                {
                    float smallShotDistance = 12 * 16f;
                    float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    Vector2 spawnOffset = randomAngle.ToRotationVector2() * smallShotDistance;
                    Vector2 spawnPos = target.Center + spawnOffset;
                    Vector2 velocity = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 20f;

                    // **生成小型弹幕**
                    int smallProjIndex = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        velocity,
                        ModContent.ProjectileType<SunsetCConceptRightCut>(),
                        Projectile.damage / 2,
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    // **调整小弹幕的属性**
                    if (Main.projectile[smallProjIndex].active)
                    {
                        Projectile smallProj = Main.projectile[smallProjIndex];
                        smallProj.penetrate = 1;  // **具备 1 次穿透**
                        smallProj.tileCollide = true; // **小弹幕会碰撞地形**
                        smallProj.usesLocalNPCImmunity = true;
                        smallProj.localNPCHitCooldown = 10;
                        smallProj.netUpdate = true;
                    }

                    SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                }
            }

            // **释放大弹幕**
            if (holdTime >= 600) // 600 帧 = 10 秒
            {
                Projectile.Kill();
                NPC target = FindClosestTarget();
                if (Main.myPlayer == Projectile.owner && target != null)
                {
                    Vector2 explosionPos = target.Center + new Vector2(0, -30 * 16);
                    Vector2 explosionVelocity = Vector2.UnitY * 30f; // 向下冲击

                    int bigProjIndex = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        explosionPos,
                        explosionVelocity,
                        ModContent.ProjectileType<SunsetCConceptRightCut>(),
                        Projectile.damage * 10,
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    // **调整大弹幕的属性**
                    if (Main.projectile[bigProjIndex].active)
                    {
                        Projectile bigProj = Main.projectile[bigProjIndex];
                        bigProj.penetrate = 6;  // **具备 6 次穿透**
                        bigProj.tileCollide = false; // **不会碰撞地形**
                        bigProj.usesLocalNPCImmunity = true;
                        bigProj.localNPCHitCooldown = 1; // **无敌帧为 1**
                        bigProj.netUpdate = true;
                    }

                    Main.projectile[bigProjIndex].scale *= 5f; // 放大 5 倍

                    postBigShotCooldown = 150; // 进入 `2.5 秒` 冷却

                    // 屏幕震动效果
                    float shakePower = 5f;
                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
                }
            }

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
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒
        }
    }
}