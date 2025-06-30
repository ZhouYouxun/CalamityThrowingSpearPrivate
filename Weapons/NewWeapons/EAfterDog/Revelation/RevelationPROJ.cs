using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using Terraria.Audio;
using System.IO;
using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.DraedonsArsenal;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public int ProjectileLevel { get; set; } // 只是创建开关，不进行初始化

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/Revelation/Revelation";
        public enum BehaviorState
        {
            Aim,
            Fire
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1; // 0级或1级的拖尾长度
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 如果是 0 级或 1 级，保留现状
            if (ProjectileLevel == 0 || ProjectileLevel == 1)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
                return false;
            }
            // 如果是 2 级、3 级或 4 级，使用背光特效（类似天堂之风的充能）
            else
            {
                // 获取纹理资源和位置
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = texture.Size() * 0.5f;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;

                // 背光效果部分 - 亮白色光晕
                float chargeOffset = 3f; // 控制充能效果扩散的偏移量
                Color chargeColor = Color.White * 0.6f; // 设置为亮白色
                chargeColor.A = 0; // 设置透明度

                // 修复旋转逻辑，确保与速度方向同步
                float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                SpriteEffects direction = SpriteEffects.None;

                // 绘制充能效果 - 圆周上绘制多个充能光效
                for (int i = 0; i < 8; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
                }

                // 渲染实际的投射物本体
                Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

                return false;
            }
        }


        public Player Owner => Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        private bool hasReleasedSparks = false; // 确保火花只释放一次
        private int chargeDurationFrames = 0; // 用于跟踪蓄力时间（帧数）

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim(); // 蓄力期间
                    break;
                case BehaviorState.Fire:
                    // 重置速度的逻辑
                    {
                        float initialSpeed = 20f; // 设定你的初始速度值，根据需求替换具体值
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
                    }
                    // 进入冲刺前释放散射火花，仅当蓄力层数 >= 2 且未释放过时
                    if (ProjectileLevel >= 2 && !hasReleasedSparks)
                    {
                        for (int i = 0; i < 4; i++) // 生成4个随机角度
                        {
                            // 在 -x 度到 x 度之间生成一个随机角度
                            float randomAngle = Main.rand.NextFloat(-90f, 90f);
                            Vector2 sparkDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(randomAngle)).SafeNormalize(Vector2.Zero); // 调整方向
                            Vector2 sparkVelocity = sparkDirection * 18f; // 固定速度18f
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                Projectile.Center,
                                sparkVelocity,
                                ModContent.ProjectileType<RevelationSpark>(),
                                (int)(Projectile.damage * 1.0f), // 1.0倍倍率
                                Projectile.knockBack,
                                Projectile.owner
                            );
                        }
                        hasReleasedSparks = true; // 设置标志，确保只释放一次
                    }
                    DoBehavior_Fire(); // 冲刺期间
                    break;
            }
            Time++;
        }

        public void DoBehavior_Aim()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加黑色光源
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 不断的重置剩余时间
            Projectile.timeLeft = 480;

            // 设置穿透次数为 -1
            Projectile.penetrate = -1;

            // 可以穿墙
            Projectile.tileCollide = false;

            // 逐渐增加蓄力时间
            chargeDurationFrames++;

            // 如果蓄力时间超过 60 帧，播放水晶球音效（仅播放一次）
            if (chargeDurationFrames == 60)
            {
                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);

                // === 确保中心对齐枪头位置 ===
                Vector2 gunTipCenter = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);

                int totalParticles = 0;

                // ============================== 1️⃣ 光球（GenericBloom） ==============================
                int spiralPoints = 72; // 狂野值 x3
                float spiralRadius = 120f;
                float spiralTurns = 3f;

                for (int i = 0; i < spiralPoints; i++)
                {
                    float progress = i / (float)spiralPoints;
                    float angle = MathHelper.TwoPi * spiralTurns * progress;
                    float radius = spiralRadius * progress;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Vector2 position = gunTipCenter + offset;
                    Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);

                    Color color = (i % 3 == 0) ? Color.White : Color.LightBlue;
                    float scale = Main.rand.NextFloat(0.25f, 0.4f);

                    GeneralParticleHandler.SpawnParticle(new GenericBloom(position, velocity, color, scale, Main.rand.Next(25, 40)));

                    totalParticles++;
                }

                // ============================== 2️⃣ Dust（蓝色系爆散，无序） ==============================
                for (int i = 0; i < 120; i++) // 狂野级数量
                {
                    int dustType = DustID.BlueCrystalShard;
                    Vector2 dustVelocity = Main.rand.NextVector2Circular(12f, 12f);
                    int dust = Dust.NewDust(gunTipCenter, 0, 0, dustType, dustVelocity.X, dustVelocity.Y, 100, default, Main.rand.NextFloat(1.2f, 2.0f));
                    Main.dust[dust].noGravity = true;
                }

                // ============================== 3️⃣ 线性粒子 SparkParticle（有序旋转爆发） ==============================
                int sparkCount = 36;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkCount;
                    Vector2 direction = angle.ToRotationVector2();
                    Vector2 velocity = direction * Main.rand.NextFloat(6f, 12f);

                    Color color = Color.Cyan;
                    float scale = Main.rand.NextFloat(0.8f, 1.4f);

                    Particle spark = new SparkParticle(
                        gunTipCenter,
                        velocity,
                        false,
                        50,
                        scale,
                        color
                    );

                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // ============================== 4️⃣ 冲击波收缩（DirectionalPulseRing 浅绿色） ==============================
                Particle shrinkingpulse = new DirectionalPulseRing(
                    gunTipCenter,
                    Vector2.Zero,
                    Color.LightGreen,
                    new Vector2(1.2f, 1.2f),
                    Main.rand.NextFloat(8f, 12f),
                    0.1f,
                    3.5f,
                    15
                );
                GeneralParticleHandler.SpawnParticle(shrinkingpulse);
            }









            // 如果是 Zenith World 天顶世界，那么让他无视敌人无敌帧
            if (Main.zenithWorld)
            {
                Projectile.localNPCHitCooldown = 1;
                //Projectile.damage = (int)(Projectile.damage * Main.rand.NextFloat(2f, 18f)); // 2倍到18倍的伤害
            }

            // 瞄准状态逻辑
            //int aimDuration = 54;
            //float aimCompletion = Utils.GetLerpValue(0f, aimDuration, Time, true);

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 将投射物位置与玩家中心对齐，模拟持握效果
            // Projectile.Center = Owner.Center;
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI; // 玩家持有此投射物

            // 检查玩家是否松开鼠标
            if (!Owner.channel)
            {
                // 切换至发射状态
                CurrentState = BehaviorState.Fire;
                Time = 0f; // 重置计时器
                Projectile.netUpdate = true; // 同步网络更新
            }
        }

        public void DoBehavior_Fire()
        {
            // 发射状态逻辑：弹幕直线运动

            // 如果蓄力时间少于 60 帧，计算并降低伤害
            if (chargeDurationFrames < 60)
            {
                int framesUnderCharged = 60 - chargeDurationFrames;
                float damageMultiplier = 1f - (framesUnderCharged * 0.01f); // 每少1帧降低1%伤害
                damageMultiplier = MathHelper.Clamp(damageMultiplier, 0f, 1f); // 确保伤害倍数不会低于0

                // 计算伤害并设置最低值
                Projectile.damage = Math.Max((int)(Projectile.damage * damageMultiplier), 5); // 确保伤害最低为5
            }


            // 设置穿透次数为 1
            Projectile.penetrate = 1;

            // 不再可以穿墙
            Projectile.tileCollide = true;

            // 调整速度变化
            float speedMultiplier = (ProjectileLevel >= 0) ? 1.022f : 1.032f; // 如果是一级，1.032，如果是零级，则是1.022
            Projectile.velocity *= speedMultiplier;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            // 添加黑色光源
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 零级时候产生的飞行过程中的特效
            if (ProjectileLevel >= 0)
            {
                //// 在投射物飞行过程中生成特效
                //Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2); // 生成粒子位置

                //switch (Main.rand.Next(2)) // 随机选择生成哪种粒子
                //// 这里选择两种鸿蒙方舟的光点特效
                //{
                //    case 0:
                //        Vector2 strongBloomVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f) * 0.33f;
                //        GeneralParticleHandler.SpawnParticle(new StrongBloom(particlePosition, strongBloomVelocity, Color.White, 0.25f, Main.rand.Next(20) + 10)); // 将粒子缩小到 1/4
                //        break;
                //    case 1:
                //        Vector2 genericBloomVelocity = Main.rand.NextVector2Circular(3f, 3f) * 0.33f;
                //        GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, genericBloomVelocity, Color.White, 0.25f, Main.rand.Next(20) + 10)); // 将粒子缩小到 1/4
                //        break;
                //}



                // ============================== 【启示录飞行特效】 ==============================
                if (ProjectileLevel >= 0)
                {
                    // === 🚩 1️⃣ 无序部分：光球特效（翻倍狂野） ===
                    for (int i = 0; i < 2; i++) // 翻倍执行两次
                    {
                        Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2);
                        switch (Main.rand.Next(2))
                        {
                            case 0:
                                Vector2 strongBloomVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f) * 0.66f; // 翻倍速度
                                GeneralParticleHandler.SpawnParticle(new StrongBloom(particlePosition, strongBloomVelocity, Color.White, 0.35f, Main.rand.Next(30, 45)));
                                break;
                            case 1:
                                Vector2 genericBloomVelocity = Main.rand.NextVector2Circular(3f, 3f) * 0.66f; // 翻倍速度
                                GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, genericBloomVelocity, Color.White, 0.35f, Main.rand.Next(30, 45)));
                                break;
                        }
                    }

                    // === 🚩 2️⃣ 有序部分：四螺旋 Dust（蓝、白，气势 ×3，复杂化） ===
                    float time = Main.GameUpdateCount / 6f; // 提高旋转速度以增强动态感
                    float baseRadius = 20f; // 扩大基础半径
                    float radiusVariation = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 10f; // sin 波动幅度加大
                    float actualRadius = baseRadius + radiusVariation;

                    // 使用蓝白四螺旋，相互对立构成高科技感结构
                    Color[] dustColors = { Color.Cyan, Color.LightBlue, Color.White, Color.SkyBlue };
                    int[] dustTypes = { DustID.BlueCrystalShard, DustID.WhiteTorch, DustID.WhiteTorch, DustID.BlueTorch };

                    for (int i = 0; i < 4; i++)
                    {
                        // 相邻螺旋以 Pi/2 间隔，形成四螺旋结构
                        float angle = time + i * MathHelper.PiOver2;
                        Vector2 offset = angle.ToRotationVector2() * actualRadius;

                        // 对称螺旋（相互对立）
                        Vector2 spawnPosition1 = Projectile.Center + offset;
                        Vector2 spawnPosition2 = Projectile.Center - offset;

                        Vector2 velocity1 = offset.SafeNormalize(Vector2.Zero) * 1.2f + Projectile.velocity * 0.2f;
                        Vector2 velocity2 = -offset.SafeNormalize(Vector2.Zero) * 1.2f + Projectile.velocity * 0.2f;

                        // 每条螺旋生成多个点以加大密度（气势 ×3）
                        for (int j = 0; j < 2; j++)
                        {
                            int dustType = dustTypes[i];
                            Color color = dustColors[i];
                            float scale = Main.rand.NextFloat(1.5f, 2.3f); // 大型粒子

                            Vector2 pos = (j == 0) ? spawnPosition1 : spawnPosition2;
                            Vector2 vel = (j == 0) ? velocity1 : velocity2;

                            int dust = Dust.NewDust(pos, 0, 0, dustType, vel.X, vel.Y, 80, color, scale);
                            Main.dust[dust].noGravity = true;
                        }
                    }


                    // === 🚩 3️⃣ 有序部分：周期性科技方块法阵释放（SquareParticle矩阵） ===
                    if (Main.GameUpdateCount % 12 == 0) // 每12帧触发一次
                    {
                        Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

                        int matrixCount = 8; // 矩阵中的小方块数量
                        float matrixRadius = 24f;
                        float rotationOffset = Main.GameUpdateCount * 0.05f;

                        for (int i = 0; i < matrixCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / matrixCount + rotationOffset;
                            Vector2 offset = angle.ToRotationVector2() * matrixRadius;
                            Vector2 spawnPos = gunTip + offset;
                            Vector2 particleVelocity = offset.SafeNormalize(Vector2.Zero) * 1.5f; // 缓慢向外扩散

                            SquareParticle squareParticle = new SquareParticle(
                                spawnPos,
                                particleVelocity,
                                false,
                                25,
                                1.5f + Main.rand.NextFloat(0.4f),
                                Color.Cyan * 1.3f // 冷色科技感
                            );

                            GeneralParticleHandler.SpawnParticle(squareParticle);
                        }
                    }
                }










            }

            // 如果是二级及以上，每 20 帧释放一个 火花
            if (ProjectileLevel >= 2)
            {
                if (Projectile.timeLeft % 20 == 0) // 每 20 帧触发一次
                {
                    // 生成 火花，使其初始速度与主弹幕一致
                    Vector2 sparkVelocity = Projectile.velocity; // 使用当前主弹幕的速度
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        sparkVelocity,
                        ModContent.ProjectileType<RevelationSpark>(),
                        (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }

            }

            // （这个东西有点问题，所以暂时让它的蓄力层数不要下降）
            // 检查是否接近结束且未触发 OnHitNPC
            if (Projectile.timeLeft == 160)
            {
                if (Main.player[Projectile.owner].TryGetModPlayer<RevelationPlayer>(out RevelationPlayer player))
                {
                    player.DecrementLevel(); // 减少一个等级
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 检查玩家是否有 RevelationPlayer 并减少等级
            if (Main.player[Projectile.owner].TryGetModPlayer<RevelationPlayer>(out RevelationPlayer player))
            {
                player.DecrementLevel(); // 减少一个等级
            }

            // 如果需要破坏弹幕，返回 true；否则返回 false
            return base.OnTileCollide(oldVelocity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //调用玩家类的方法
            if (Main.player[Projectile.owner].TryGetModPlayer<RevelationPlayer>(out RevelationPlayer player))
            {
                //Main.NewText($"击中次数: {player.HitCounterForLevelUp}, 当前等级: {player.RevelationLevel}");
                player.TrackHit(); // 调用以增加计时器
            }
       
            // 如果强化等级是三级及以上，触发元素紊乱效果
            if (ProjectileLevel >= 3)
            {
                target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 造成5秒钟的元素紊乱
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/TeslaCannonFire").WithVolumeScale(0.33f));
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/空中分裂").WithVolumeScale(0.33f));
            
            chargeDurationFrames = 0; // 重置蓄力计数

            // 012为小爆炸效果
            if (ProjectileLevel == 0 || ProjectileLevel == 1 || ProjectileLevel == 2)
            {
                // 设置伤害倍率
                float boomDamageMultiplier = (ProjectileLevel == 0) ? 0.8f : 1.25f;
                int boomDamage = (int)(Projectile.damage * boomDamageMultiplier);

                // 创建爆炸弹幕并设置属性
                Projectile explosion = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<RevelationBoom>(),
                    (int)(Projectile.damage * boomDamageMultiplier),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 设置爆炸弹幕的额外属性
                float maxRadius = Main.rand.NextFloat(110f, 200f); // 随机最大半径
                if (ProjectileLevel >= 1)
                {
                    maxRadius *= 1.5f; // 当 ProjectileLevel 为 1 时，最大半径扩大到 1.5 倍
                }
                explosion.ai[1] = maxRadius;
                explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // 插值步长
                explosion.netUpdate = true;
            }

            // 三级及以上强化时释放剧烈爆炸
            if (ProjectileLevel >= 3)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RevelationSUPERBoom>(), (int)(Projectile.damage * 2.25f), Projectile.knockBack, Projectile.owner);
            }

            // 处理四级强化时释放多个彗星，可以追踪
            if (ProjectileLevel >= 4)
            {
                for (int i = 0; i < 10; i++) // 数量10
                {
                    // 生成一个在 0 到 360 度之间的随机角度
                    float randomAngle = Main.rand.NextFloat(0f, 360f);
                    Vector2 baseDirection = Vector2.UnitX; // 使用标准的单位向量（例如 X 轴方向）
                    Vector2 rotatedDirection = baseDirection.RotatedBy(MathHelper.ToRadians(randomAngle)); // 在基础方向上旋转角度
                    float randomSpeedMultiplier = Main.rand.NextFloat(0.75f, 2f); // 生成0.5到1.5倍之间的随机倍率
                    Vector2 starVelocity = rotatedDirection * Projectile.velocity.Length() * randomSpeedMultiplier; // 使用随机速度大小
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        starVelocity,
                        ModContent.ProjectileType<RevelationSTAR>(),
                        (int)(Projectile.damage * 0.4f),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }
            }



            // 零级及以上的时候会释放这种特效
            if (ProjectileLevel >= 0)
            {
                Vector2 hitCenter = Projectile.Center;

                // 🚩 🚩 🚩 1️⃣ 有序：冲击波（强化）
                Particle bloodsplosion = new CustomPulse(
                    hitCenter,
                    Vector2.Zero,
                    Color.GhostWhite,
                    "CalamityMod/Particles/DetailedExplosion",
                    Vector2.One * 1.5f, // 放大尺寸
                    Main.rand.NextFloat(-25f, 25f),
                    0.08f,
                    0.88f,
                    30,
                    false
                );
                GeneralParticleHandler.SpawnParticle(bloodsplosion);

                // 🚩 🚩 🚩 2️⃣ 无序：Dust 蓝黑白狂野爆散
                int dustAmount = 300; // 500% 狂野度
                int[] dustTypes = { DustID.BlueCrystalShard, DustID.Smoke, DustID.WhiteTorch };
                Color[] dustColors = { Color.Cyan, Color.Black, Color.White };

                for (int i = 0; i < dustAmount; i++)
                {
                    int type = dustTypes[Main.rand.Next(dustTypes.Length)];
                    Color color = dustColors[Main.rand.Next(dustColors.Length)];
                    Vector2 velocity = Main.rand.NextVector2Circular(28f, 28f);

                    int dust = Dust.NewDust(hitCenter, 0, 0, type, velocity.X, velocity.Y, 100, color, Main.rand.NextFloat(1.5f, 3.0f));
                    Main.dust[dust].noGravity = true;
                }

                // 🚩 🚩 🚩 3️⃣ 屏幕震动
                if (Main.player[Projectile.owner] == Main.LocalPlayer)
                {
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, 15f);
                }

                // 🚩 🚩 🚩 4️⃣ 有序：复杂“魔法阵”线性粒子阵
                // 模拟三层六芒星展开
                int layers = 3;
                int pointsPerLayer = 36;
                float baseRadius = 24f;
                float radiusIncrement = 24f;

                for (int layer = 0; layer < layers; layer++)
                {
                    float radius = baseRadius + layer * radiusIncrement;
                    for (int point = 0; point < pointsPerLayer; point++)
                    {
                        float angle = MathHelper.TwoPi * point / pointsPerLayer + layer * 0.2f; // 每层略偏移形成旋转感
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        Vector2 position = hitCenter + offset;
                        Vector2 velocity = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 7f);

                        Particle spark = new SparkParticle(
                            position,
                            velocity,
                            false,
                            50,
                            1.2f,
                            Color.Cyan
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

                // 🚩 🚩 🚩 5️⃣ 中和：十字星粒子闪光
                int starAmount = 8; // 多个十字星
                for (int i = 0; i < starAmount; i++)
                {
                    Vector2 starOffset = Main.rand.NextVector2Circular(24f, 24f);
                    GenericSparkle sparker = new GenericSparkle(
                        hitCenter + starOffset,
                        Vector2.Zero,
                        Color.Cyan,
                        Color.White,
                        Main.rand.NextFloat(1.5f, 2.5f),
                        7,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        1.8f
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);
                }
            }


            {
                // 🚩🚩🚩 6️⃣ 新增：死亡时正前方扇形喷射三层特效（速度×2，数量×1.5）

                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                // === 有序喷射：SparkParticle 锥形喷射 ===
                int sparkAmount = 36; // 24 × 1.5
                for (int i = 0; i < sparkAmount; i++)
                {
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                    Vector2 direction = forward.RotatedBy(angleOffset);
                    Vector2 velocity = direction * Main.rand.NextFloat(24f, 40f); // 速度×2

                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        velocity,
                        false,
                        50,
                        1.2f,
                        Color.Cyan
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // === 无序喷射：Dust 扇形爆散 ===
                int dustAmount = 90; // 60 × 1.5
                for (int i = 0; i < dustAmount; i++)
                {
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                    Vector2 direction = forward.RotatedBy(angleOffset);
                    Vector2 velocity = direction * Main.rand.NextFloat(30f, 60f); // 速度×2

                    int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.BlueCrystalShard, velocity.X, velocity.Y, 100, Color.Cyan, Main.rand.NextFloat(1.3f, 1.8f));
                    Main.dust[dust].noGravity = true;
                }

                // === 中和闪光：GenericSparkle 十字星扇形闪光 ===
                int sparkleAmount = 18; // 12 × 1.5
                for (int i = 0; i < sparkleAmount; i++)
                {
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f));
                    Vector2 offset = forward.RotatedBy(angleOffset) * Main.rand.NextFloat(12f, 36f);
                    Vector2 spawnPos = Projectile.Center + offset;

                    GenericSparkle sparkle = new GenericSparkle(
                        spawnPos,
                        Vector2.Zero,
                        Color.Cyan,
                        Color.White,
                        Main.rand.NextFloat(1.5f, 2.2f),
                        8,
                        Main.rand.NextFloat(-0.03f, 0.03f),
                        1.6f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }

            }






        }

        public override bool? CanDamage()
        {
            // 如果是 Zenith World 天顶世界，无论何时都允许造成伤害
            if (Main.zenithWorld)
            {
                return true;
            }

            // 如果是正常世界，那么蓄力状态下不造成伤害
            if (CurrentState == BehaviorState.Aim)
            {
                return false;
            }

            // 如果当前状态是冲刺状态，允许造成伤害
            return true;
        }


    }
}