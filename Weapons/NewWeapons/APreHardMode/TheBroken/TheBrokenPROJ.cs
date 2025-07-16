using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    public class TheBrokenPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/TheBroken/TheBroken";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        private bool EnableStickMode => _enableStickMode;
        private bool _enableStickMode = false;
        private bool stuckToTarget = false;
        private NPC stuckTarget = null;
        private int spawnCounter = 0;

        public override bool PreDraw(ref Color lightColor)
        {
            if (stuckToTarget)
                return false; // 完全隐形
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 20 * 3;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1; // 默认值，之后会在AI中调整
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 初始化攻击模式
            if (!Projectile.localAI[1].Equals(1f))
            {
                _enableStickMode = Projectile.localAI[0] == 1f;
                Projectile.localAI[1] = 1f;

                // 根据模式设置 extraUpdates
                Projectile.extraUpdates = _enableStickMode ? 1 : 2; // _enableStickMode = 5
                Projectile.timeLeft = _enableStickMode ? 60 : 30;

                // 如果是右键模式则设置穿透为 2
                if (_enableStickMode)
                    Projectile.penetrate = 2;
            }

            // 左键模式不受重力影响
            if (_enableStickMode)
            {
                // 右键扎入模式
                if (stuckToTarget && stuckTarget != null && stuckTarget.active)
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, stuckTarget.Center + new Vector2(0, -16f), 0.2f);
                }
            }
            else
            {
                // 左键：不受重力影响
                // 不再释放飞刀雨逻辑
                Projectile.tileCollide = true;
            }

            // 自身旋转和粒子保持通用
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            GenerateSilverDustAndSparks();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            // 扎入逻辑
            if (EnableStickMode && !stuckToTarget && target.CanBeChasedBy())
            {
                stuckToTarget = true;
                stuckTarget = target;
                Projectile.timeLeft = 250;
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;
            }
        }

        public override void OnKill(int timeLeft)
        {
            CreateSilverDeathEffect();
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

            if (!_enableStickMode)
            {
                int count = 5;
                Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                for (int i = 0; i < count; i++)
                {
                    // 在 ±45° 内随机角度偏转
                    float randomRotation = MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f));
                    Vector2 direction = baseDirection.RotatedBy(randomRotation);

                    // 随机速度 6 ~ 10f
                    float speed = Main.rand.NextFloat(6f, 10f);
                    Vector2 velocity = direction * speed;

                    // 随机伤害倍率 0.2 ~ 0.4
                    float damageMultiplier = Main.rand.NextFloat(0.2f, 0.4f);

                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<TheBrokenSpit>(),
                        (int)(Projectile.damage * damageMultiplier),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    if (proj.WithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[proj].friendly = true;
                        Main.projectile[proj].hostile = false;
                    }

                    //// 随机角度偏移（±45° 扇形）
                    //float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f));
                    //Vector2 offsetDir = direction.RotatedBy(randomAngle);

                    //// 在 direction 前方 ±45° 范围内，半径在 0 到 3×长度 之间随机
                    //float radius = Main.rand.NextFloat(0f, 3f * Projectile.Size.Length());
                    //Vector2 glowPos = Projectile.Center + offsetDir * radius;

                    //// 生成光点（旋转由内部粒子系统控制）
                    //CTSLightingBoltsSystem.Spawn_SilverSpearGlow(glowPos);

                }
            }
        }

        private void GenerateSilverDustAndSparks()
        {
            if (Main.rand.NextBool(1))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Silver,
                    -Projectile.velocity * 0.2f,
                    150,
                    Color.White,
                    Main.rand.NextFloat(0.9f, 1.2f)
                );
                d.noGravity = true;
            }

            if (Main.rand.NextBool(1))
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 sparkVelocity = direction.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * 5f;
                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    sparkVelocity,
                    Color.White,
                    Color.LightBlue,
                    0.8f,
                    18
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1f, 1f) * 3f;
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    sparkVel,
                    false,
                    20,
                    0.7f,
                    Color.LightSteelBlue
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        private void CreateSilverDeathEffect()
        {
            int particles = 30;
            for (int i = 0; i < particles; i++)
            {
                float angle = MathHelper.TwoPi * i / particles + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Silver,
                    velocity,
                    120,
                    Color.White,
                    Main.rand.NextFloat(1.0f, 1.3f)
                );
                d.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    velocity,
                    Color.White,
                    Color.LightBlue,
                    0.8f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
