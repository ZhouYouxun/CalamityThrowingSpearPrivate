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
            Projectile.width = 32;
            Projectile.height = 32;
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


        private void GenerateParticles()
        {
            // **计算枪头位置，方向固定为
            Vector2 gunHeadDirection = (InitialDirection).ToRotationVector2();
            Vector2 gunHeadPosition = Projectile.Center + gunHeadDirection * 16f * 5f;

            // **每帧生成 2~3 个粒子**
            int particleCount = Main.rand.Next(2, 4);

            for (int i = 0; i < particleCount; i++)
            {
                // **在半径 2.5×16 的小圆范围内随机生成**
                Vector2 randomOffset = Main.rand.NextVector2Circular(2.5f * 16f, 2.5f * 16f);
                Vector2 particlePosition = gunHeadPosition + randomOffset;

                Color[] possibleColors = {
            Color.Black, Color.White, Color.Green, new Color(255, 105, 180), // 蓝粉
            Color.Blue, Color.Gold, new Color(50, 0, 50), // 紫黑
            Color.Red, Color.Gray, Color.Silver
        };

                // **增加粒子大小和速度的随机性**
                float randomSpeed = Main.rand.NextFloat(0.3f, 2.2f); // **速度范围更大**
                float randomSize = 1.7f + Main.rand.NextFloat(0.8f, 1.2f); // **大小变化更明显**

                // **粒子方向也固定为 `InitialDirection + MathHelper.PiOver4`**
                Vector2 particleVelocity = Vector2.UnitY * -randomSpeed; // **固定向上运动**

                SquareParticle squareParticle = new SquareParticle(
                    particlePosition,
                    particleVelocity,
                    false,
                    30,
                    randomSize,
                    possibleColors[Main.rand.Next(possibleColors.Length)] * 1.5f
                );

                GeneralParticleHandler.SpawnParticle(squareParticle);
            }
        }

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
