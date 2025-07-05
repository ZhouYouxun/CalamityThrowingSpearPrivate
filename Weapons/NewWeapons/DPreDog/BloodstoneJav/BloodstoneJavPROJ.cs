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
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Items.Weapons.Magic;
using Terraria.GameContent;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav
{
    public class BloodstoneJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/BloodstoneJav/BloodstoneJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        private int chargeLevel = 0; // 当前蓄力等级
        private int hitCounter = 0; // 命中计数器
        private const int MaxChargeLevel = 15;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            // 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            // 每45帧提升蓄力等级
            if (Projectile.localAI[0] % 45 == 0 && chargeLevel < MaxChargeLevel)
            {
                chargeLevel++;
                CreateChargeEffect();
                InflictChargePenalty();
            }

            // 检测松手
            if (!Owner.channel)
            {
                CurrentState = BehaviorState.Dash;
                Projectile.netUpdate = true;
                Projectile.penetrate = 1 + chargeLevel; // 根据等级设置穿透次数
                float finalDamage =  chargeLevel; //伤害提升
                float speedBoost = 14f + chargeLevel * 0.2f; // 飞行速度提升
                Projectile.velocity *= speedBoost;
                Projectile.damage *= (int)(0.5f + finalDamage * 0.3f);
            }

            Projectile.localAI[0]++;
        }

        private void CreateChargeEffect()
        {
            Vector2 center = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 2f;

            // === 1️⃣ 血色闪电线性粒子 (SparkParticle) ★重制宏伟版===
            int sparkLayers = 3;
            int sparksPerLayer = 16;
            for (int layer = 0; layer < sparkLayers; layer++)
            {
                float radius = 12f + layer * 6f;
                float speedMultiplier = 1f + layer * 0.5f;
                float angleOffset = Main.GameUpdateCount * 0.05f * (layer % 2 == 0 ? 1 : -1); // 层间反向旋转

                for (int i = 0; i < sparksPerLayer; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparksPerLayer + angleOffset;
                    Vector2 direction = angle.ToRotationVector2();
                    Vector2 velocity = direction * Main.rand.NextFloat(4f, 8f) * speedMultiplier;

                    Particle spark = new SparkParticle(
                        center + direction * radius,
                        velocity,
                        false,
                        60,
                        Main.rand.NextFloat(1.5f, 2.4f),
                        Color.Lerp(Color.DarkRed, Color.Maroon, Main.rand.NextFloat(0.3f, 0.7f)) * 0.9f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // === 2️⃣ 血色 Dust 爆散 (血雾微粒) ★重制宏伟版===
            int dustLayers = 5;
            int dustPerLayer = 30;
            float baseRadius = 8f;
            float radiusStep = 10f;
            for (int layer = 0; layer < dustLayers; layer++)
            {
                float currentRadius = baseRadius + layer * radiusStep;
                float angleOffset = Main.GameUpdateCount * 0.1f + layer * 0.5f;

                for (int i = 0; i < dustPerLayer; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustPerLayer + angleOffset + Main.rand.NextFloat(-0.05f, 0.05f);
                    Vector2 direction = angle.ToRotationVector2();
                    Vector2 spawnPos = center + direction * currentRadius;
                    Vector2 velocity = direction * Main.rand.NextFloat(3f, 9f) + Main.rand.NextVector2Circular(1f, 1f);

                    Dust dust = Dust.NewDustPerfect(
                        spawnPos,
                        DustID.Blood,
                        velocity,
                        0,
                        Color.DarkRed * 0.9f,
                        Main.rand.NextFloat(1.3f, 2.2f)
                    );
                    dust.noGravity = true;
                }
            }

            // === 3️⃣ 血阵冲击波收缩 (DirectionalPulseRing) ===
            Particle pulse = new DirectionalPulseRing(
                center,
                Vector2.Zero,
                Color.DarkRed * 0.8f,
                new Vector2(1.0f, 1.0f),
                4f,
                0.05f,
                3f,
                40
            );
            GeneralParticleHandler.SpawnParticle(pulse);

            // === 4️⃣ 红色血雾 HeavySmokeParticle ===
            int smokeCount = 12;
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Particle smoke = new HeavySmokeParticle(
                    center,
                    velocity,
                    Color.DarkRed * 0.6f,
                    40,
                    Main.rand.NextFloat(1.0f, 1.6f),
                    0.3f,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // === 5️⃣ 播放血色献祭音效 ===
            SoundEngine.PlaySound(SoundID.Item30, Projectile.position);
        }


        private void InflictChargePenalty()
        {
            int damagePenalty = 10; // 每级扣除X点血量
            Owner.statLife -= damagePenalty;
            CombatText.NewText(Owner.getRect(), Color.Lime, -damagePenalty); // 显示绿色负值

            if (Owner.statLife <= 0)
            {
                Owner.KillMe(PlayerDeathReason.ByCustomReason($"{Owner.name} 把自己榨干了"), damagePenalty, 0);
            }
        }

        private void DoBehavior_Dash()
        {

            // 重置速度的逻辑
            {
                float initialSpeed = 31f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }


            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // === 🚩🚩🚩 飞行期间特效【强化版】 ===
            {
                Color bloodColor = Color.Red;
                float scaleBoost = MathHelper.Clamp(chargeLevel * 0.005f, 0f, 2f);
                float outerSparkScale = 1.5f + scaleBoost;

                // === 1️⃣ 大型血色 SparkParticle（保留） ===
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(spark);

                // === 2️⃣ 血红冲击波（保留） ===
                if (Projectile.localAI[0] % 5 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Particle pulse = new DirectionalPulseRing(
                            Projectile.Center,
                            Projectile.velocity * 0.75f,
                            bloodColor,
                            new Vector2(1f, 2.5f),
                            Projectile.rotation - MathHelper.PiOver4,
                            0.2f,
                            0.03f,
                            20
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                }

                // === 3️⃣ 血线拖尾 LineParticle（保留） ===
                if (Projectile.localAI[0] % 5 == 0)
                {
                    Vector2 particleVelocity = Projectile.velocity * 0.8f;
                    Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                    LineParticle bloodTrail = new LineParticle(
                        particlePosition,
                        particleVelocity,
                        false,
                        30,
                        0.5f,
                        Color.DarkRed
                    );
                    GeneralParticleHandler.SpawnParticle(bloodTrail);
                }

                // === 4️⃣ 新增：侧向血雾 Dust 环绕射出 ===
                if (Main.rand.NextBool(2))
                {
                    int dustPoints = 6;
                    float radius = 12f;
                    float angleOffset = Main.GameUpdateCount * 0.15f;
                    for (int i = 0; i < dustPoints; i++)
                    {
                        float angle = MathHelper.TwoPi * i / dustPoints + angleOffset;
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        Vector2 spawnPos = Projectile.Center + offset;
                        Vector2 velocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(Math.Sin(angle + Main.GameUpdateCount * 0.1f) * 0.2f) * Main.rand.NextFloat(2f, 5f);

                        Dust dust = Dust.NewDustPerfect(
                            spawnPos,
                            DustID.Blood,
                            velocity,
                            0,
                            Color.DarkRed * 0.8f,
                            Main.rand.NextFloat(1.0f, 1.4f)
                        );
                        dust.noGravity = true;
                    }
                }

                // === 5️⃣ 新增：滞后型暗血 LineParticle（点缀残影） ===
                if (Projectile.localAI[0] % 8 == 0)
                {
                    Vector2 lagVelocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, 3f);
                    Vector2 lagPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                    LineParticle darkBloodTrail = new LineParticle(
                        lagPos,
                        lagVelocity,
                        false,
                        40,
                        0.4f,
                        Color.Maroon * 0.7f
                    );
                    GeneralParticleHandler.SpawnParticle(darkBloodTrail);
                }

                Projectile.localAI[0]++;
            }


            Projectile.localAI[0]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 恢复玩家生命值
            Player player = Main.player[Projectile.owner];
            float healMultiplier = 0.01f + chargeLevel * 0.001f; // 每级多增加0.1%回复
            int healAmount = (int)(damageDone * healMultiplier);
            player.statLife += healAmount;
            player.HealEffect(healAmount);

            // 每命中4次触发特效
            hitCounter++;
            if (hitCounter % 4 == 0 && chargeLevel > 0)
            {
                CreateImpactEffects();
            }
        }

        private void CreateImpactEffects()
        {
            // 血雾
            for (int i = 0; i < 3; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                Particle bloodFog = new HeavySmokeParticle(Projectile.Center, velocity, Color.Red, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(bloodFog);
            }

            // Visceral爆炸
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VisceraBoom>(),
                (int)(Projectile.damage * 0.75f),
                Projectile.knockBack * 4,
                Projectile.owner
            );

            // 血液爆炸冲击波
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, 40, false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);
            Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, 40);
            GeneralParticleHandler.SpawnParticle(bloodsplosion2);

            // === 🩸 高级血腥爆炸特效：微分方程场 + 螺旋 + 玫瑰曲线 ===

            int particleCount = 50; // 高密度
            float goldenAngle = MathHelper.ToRadians(137.5f);

            for (int i = 0; i < particleCount; i++)
            {
                float t = i / (float)particleCount * 6f * MathHelper.Pi;
                float r = 2f + 0.3f * t; // 阿基米德螺旋递增
                Vector2 spiralVelocity = t.ToRotationVector2() * r * Main.rand.NextFloat(2f, 4f); // 两倍速度范围

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Blood,
                    spiralVelocity,
                    0,
                    Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f)),
                    Main.rand.NextFloat(1.2f, 1.8f)
                );
                d.noGravity = true;
            }

            // 玫瑰曲线爆裂血雾
            int rosePetals = 60;
            for (int i = 0; i < rosePetals; i++)
            {
                float theta = MathHelper.TwoPi * i / rosePetals;
                float roseRadius = 8f * (1 + 0.4f * (float)Math.Sin(6 * theta)); // 六瓣花
                Vector2 velocity = theta.ToRotationVector2() * roseRadius * Main.rand.NextFloat(1.5f, 3f);

                Dust roseDust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Blood,
                    velocity,
                    0,
                    Color.Red,
                    Main.rand.NextFloat(1.4f, 2.0f)
                );
                roseDust.noGravity = false;
            }

            // 黄金角螺旋微血滴点缀（点状扩散，增加动态）
            int drops = 30;
            for (int i = 0; i < drops; i++)
            {
                float angle = i * goldenAngle;
                Vector2 direction = angle.ToRotationVector2();
                Vector2 velocity = direction * Main.rand.NextFloat(3f, 6f);

                Dust dropDust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Blood,
                    velocity,
                    0,
                    Color.Lerp(Color.Red, Color.DarkRed, 0.5f),
                    1.0f
                );
                dropDust.noGravity = true;
            }

            // === 🩸 离谱血色 SparkParticle 数学爆裂特效 ===

            int totalSparks = 72; // 高密度
            goldenAngle = MathHelper.ToRadians(137.5f);
            Vector2 center = Projectile.Center;

            for (int i = 0; i < totalSparks; i++)
            {
                // === 🌺 玫瑰曲线半径计算（五瓣） ===
                float theta = MathHelper.TwoPi * i / totalSparks;
                float roseRadius = 8f * (1 + 0.4f * (float)Math.Sin(5 * theta));

                // === 🌀 阿基米德螺旋递增爆散速度 ===
                float spiralT = i * 0.3f;
                float spiralRadius = 3f + 0.25f * spiralT;

                // === 黄金角偏移分层旋转 ===
                float angle = i * goldenAngle + Main.GameUpdateCount * 0.05f;

                Vector2 velocity = angle.ToRotationVector2() * (roseRadius + spiralRadius) * Main.rand.NextFloat(0.8f, 1.6f);

                // 主导红色 SparkParticle 爆裂
                SparkParticle spark = new SparkParticle(
                    center,
                    velocity,
                    false,
                    Main.rand.Next(40, 60), // 生命周期拉长，延续残影感
                    Main.rand.NextFloat(1.8f, 2.8f), // 大型可见
                    Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.2f, 0.6f)) * 0.9f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // === 点缀极小血色 Spark 星屑（提升闪烁感） ===
            int starSparks = 40;
            for (int i = 0; i < starSparks; i++)
            {
                float angle = i * goldenAngle * 0.5f + Main.GameUpdateCount * 0.1f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);

                SparkParticle starSpark = new SparkParticle(
                    center,
                    velocity,
                    false,
                    Main.rand.Next(20, 35),
                    Main.rand.NextFloat(0.6f, 1.0f),
                    Color.Red * 0.7f
                );
                GeneralParticleHandler.SpawnParticle(starSpark);
            }


        }

        /*public override bool? CanDamage()
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
        }*/

        public override void OnKill(int timeLeft)
        {
            CreateImpactEffects();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (CurrentState == BehaviorState.Dash)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            else
            {
                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            }
            return false;
        }
      
    }
}

