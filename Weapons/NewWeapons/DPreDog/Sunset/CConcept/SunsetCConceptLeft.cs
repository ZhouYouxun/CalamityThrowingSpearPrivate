//using CalamityMod;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using CalamityMod.Particles;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
//{
//    internal class SunsetCConceptLeft : ModProjectile
//    {
//        public override string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
//        private const int AttackInterval = 90; // 每 1.5 秒发动一次攻击
//        private const float IdleDistance = 5 * 16f; // 5×16 距离
//        private Vector2 idlePosition; // 记录悬停位置
//        private bool isAttacking = false; // 是否处于攻击状态
//        private Vector2 attackTarget = Vector2.Zero; // 目标点（右上角）

//        public override void SetDefaults()
//        {
//            Projectile.width = 32;
//            Projectile.height = 32;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = 1;
//            Projectile.timeLeft = 300; // 永不消失
//            Projectile.light = 0.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = false;
//            Projectile.extraUpdates = 1;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = 30;
//        }

//        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
//        {
//            base.OnSpawn(source);

//            // **检查当前已有的 `SunsetCConceptLeft` 数量**
//            int existingCount = 0;
//            foreach (Projectile proj in Main.projectile)
//            {
//                if (proj.active && proj.type == Type && proj.owner == Projectile.owner)
//                {
//                    existingCount++;
//                    if (existingCount >= 4) // **如果已经有 4 个，删除自己**
//                    {
//                        Projectile.Kill();
//                        return;
//                    }
//                }
//            }

//            // **分配空闲位置**
//            AssignIdlePosition();
//            attackTarget = Main.screenPosition + new Vector2(Main.screenWidth - 50, 50);
//        }

//        private bool isReturning = false; // 是否正在返回初始位置
//        private float attackSpeedMultiplier = 2.0f; // 初始攻击速度倍率
//        private float returnSpeedMultiplier = 3.0f; // 初始返回速度倍率

//        public override void AI()
//        {
//            Projectile.timeLeft = 300; // **确保不会消失**
//            Projectile.rotation = (idlePosition - Main.player[Projectile.owner].Center).ToRotation() + MathHelper.PiOver4;

//            if (isReturning)
//            {
//                ReturnToIdle();
//            }
//            else if (isAttacking)
//            {
//                ExecuteAttack();
//            }
//            else
//            {
//                MaintainIdlePosition();

//                // **只有当完全回到 Idle 位置，才进行下一次攻击**
//                if (Projectile.ai[0] % AttackInterval == 0 && Vector2.Distance(Projectile.Center, idlePosition) < 5f)
//                {
//                    isAttacking = true;
//                }
//            }

//            // **特效：在飞行过程中留下方形粒子**
//            if (Main.rand.NextBool(2))
//            {
//                SquareParticle squareParticle = new SquareParticle(
//                    Projectile.Center,
//                    Projectile.velocity * 0.5f,
//                    false,
//                    30,
//                    1.7f + Main.rand.NextFloat(0.6f),
//                    Color.White
//                );

//                GeneralParticleHandler.SpawnParticle(squareParticle);
//            }

//            Projectile.ai[0]++;
//        }

//        // **攻击执行**
//        private void ExecuteAttack()
//        {
//            // **冲刺向目标点**
//            float distanceToTarget = Vector2.Distance(Projectile.Center, attackTarget);
//            float speedFactor = MathHelper.Clamp(distanceToTarget / 100f, 0.5f, 1.0f); // 远处加速，近处减速
//            Vector2 direction = (attackTarget - Projectile.Center).SafeNormalize(Vector2.Zero);
//            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * (20f * attackSpeedMultiplier * speedFactor), 0.1f);

//            // **检测是否到达目标点**
//            if (distanceToTarget < 10f)
//            {
//                OnReachTarget();
//            }
//        }

//        // **攻击到达目标点后，立即返回玩家身边**
//        private void OnReachTarget()
//        {
//            isAttacking = false; // 结束攻击
//            isReturning = true; // **进入返回状态**
//            Projectile.velocity = Vector2.Zero;

//            // **在玩家中心释放 2.0 倍伤害的 `SunsetCConceptLeftEXP`**
//            Player owner = Main.player[Projectile.owner];
//            Projectile.NewProjectile(
//                Projectile.GetSource_FromThis(),
//                owner.Center,
//                Vector2.Zero,
//                ModContent.ProjectileType<SunsetCConceptLeftEXP>(),
//                (int)(Projectile.damage * 2.0),
//                Projectile.knockBack,
//                Projectile.owner
//            );
//        }

//        // **返回到 Idle 位置**
//        private void ReturnToIdle()
//        {
//            float distanceToIdle = Vector2.Distance(Projectile.Center, idlePosition);
//            float speedFactor = MathHelper.Clamp(distanceToIdle / 100f, 0.5f, 1.0f); // 远处加速，近处减速
//            Vector2 direction = (idlePosition - Projectile.Center).SafeNormalize(Vector2.Zero);
//            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * (30f * returnSpeedMultiplier * speedFactor), 0.1f);

//            // **检测是否回到 Idle 位置**
//            if (distanceToIdle < 5f)
//            {
//                isReturning = false; // **返回完成，等待下一次攻击**
//            }
//        }

//        private void MaintainIdlePosition()
//        {
//            Player owner = Main.player[Projectile.owner];

//            // **向 Idle 位置平滑移动**
//            Projectile.velocity = Vector2.Lerp(Projectile.velocity, (idlePosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f, 0.1f);
//        }


//        private void AssignIdlePosition()
//        {
//            Player owner = Main.player[Projectile.owner];

//            Vector2[] possiblePositions = {
//                owner.Center + new Vector2(-IdleDistance, -IdleDistance),
//                owner.Center + new Vector2(IdleDistance, -IdleDistance),
//                owner.Center + new Vector2(-IdleDistance, IdleDistance),
//                owner.Center + new Vector2(IdleDistance, IdleDistance)
//            };

//            // **寻找一个未占用的位置**
//            foreach (Vector2 pos in possiblePositions)
//            {
//                bool isOccupied = false;
//                foreach (Projectile proj in Main.projectile)
//                {
//                    if (proj.active && proj.type == Type && proj.owner == Projectile.owner && proj.whoAmI != Projectile.whoAmI)
//                    {
//                        if (Vector2.Distance(proj.Center, pos) < 16f)
//                        {
//                            isOccupied = true;
//                            break;
//                        }
//                    }
//                }

//                if (!isOccupied)
//                {
//                    idlePosition = pos;
//                    return;
//                }
//            }

//            // **如果所有位置都被占用，选择第一个**
//            idlePosition = possiblePositions[0];
//        }
//    }
//}
