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
                // 生成 20 个随机
                //for (int i = 0; i < 20; i++)
                //{
                //    Vector2 randomDirection = Main.rand.NextVector2Circular(3f, 3f) * 3.33f; // 生成随机方向的速度
                //    Vector2 particlePosition = Projectile.Center; // 粒子生成位置为弹幕中心
                //    GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, randomDirection, Color.White, 0.25f, Main.rand.Next(20) + 10));
                //}

                // 魔法阵设计
                int numRings = 3; // 圆环数量
                int pointsPerRing = 12; // 每个圆环的点数
                float radiusIncrement = 30f; // 每个圆环之间的半径增量
                int totalParticles = 0;

                for (int ring = 0; ring < numRings; ring++)
                {
                    if (totalParticles >= 36) break; // 限制总光球数量

                    float radius = (ring + 1) * radiusIncrement; // 当前圆环半径
                    for (int point = 0; point < pointsPerRing; point++)
                    {
                        if (totalParticles >= 36) break; // 再次检查光球总数限制

                        float angle = MathHelper.TwoPi * point / pointsPerRing; // 计算点的位置角度
                        Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius; // 光球位置
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f); // 光球初始速度

                        // 创建光球粒子
                        GeneralParticleHandler.SpawnParticle(new GenericBloom(position, velocity, Color.White, 0.25f, Main.rand.Next(20) + 10));

                        totalParticles++;
                    }
                }

                // 中心光球旋转特效
                for (int i = 0; i < 12; i++) // 12个中心点粒子
                {
                    if (totalParticles >= 36) break;

                    float angle = MathHelper.TwoPi * i / 12f; // 角度间隔
                    Vector2 position = Projectile.Center; // 粒子生成位置
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f); // 中心点旋转速度

                    // 创建光球粒子
                    GeneralParticleHandler.SpawnParticle(new GenericBloom(position, velocity, Color.LightBlue, 0.3f, Main.rand.Next(20) + 10));

                    totalParticles++;
                }
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
                // 在投射物飞行过程中生成特效
                Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2); // 生成粒子位置

                switch (Main.rand.Next(2)) // 随机选择生成哪种粒子
                // 这里选择两种鸿蒙方舟的光点特效
                {
                    case 0:
                        Vector2 strongBloomVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f) * 0.33f;
                        GeneralParticleHandler.SpawnParticle(new StrongBloom(particlePosition, strongBloomVelocity, Color.White, 0.25f, Main.rand.Next(20) + 10)); // 将粒子缩小到 1/4
                        break;
                    case 1:
                        Vector2 genericBloomVelocity = Main.rand.NextVector2Circular(3f, 3f) * 0.33f;
                        GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, genericBloomVelocity, Color.White, 0.25f, Main.rand.Next(20) + 10)); // 将粒子缩小到 1/4
                        break;
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

            // 零级及以上的时候会释放这种特效
            if (ProjectileLevel >= 0)
            {
                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.GhostWhite, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(Viscera.BoomLifetime * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
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