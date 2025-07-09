using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityRangerExpansion.LightingBolts;
using CalamityMod;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    /// <summary>
    /// Stardust 投掷弹幕，命中后贴附敌人并逐次提升伤害。
    /// </summary>
    public class ElementalLanceJavPROJStardust : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";

        private int targetWhoAmI = -1;
        private float damageMultiplier = 1f;
        private bool stuckToEnemy = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!stuckToEnemy)
            {
                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                Color trailColor = new Color(0, 0, 139);
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            }
            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 若已经贴附敌人，则跟随敌人位置，完全不可见
            if (stuckToEnemy && targetWhoAmI != -1 && Main.npc[targetWhoAmI].active)
            {
                NPC target = Main.npc[targetWhoAmI];
                Projectile.Center = target.Center;
                Projectile.Opacity = 0f;
                Projectile.hide = true;
                return;
            }

            // 未命中前持续播放飞行粒子
            if (!stuckToEnemy && Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 0, 139);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //modifiers.SourceDamage *= damageMultiplier;
            //damageMultiplier *= 1.1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item44, Projectile.Center);

            // ✨ 星尘主题的光点爆发（使用 CTSLightingBoltsSystem）
            for (int i = 0; i < 3; i++)
            {
                Vector2 positionOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 spawnPos = Projectile.Center + positionOffset;

                Color c = Color.Lerp(Color.LightBlue, Color.White, Main.rand.NextFloat(0.3f, 0.8f));
                float rot = MathHelper.ToRadians(Main.rand.Next(-45, 45));

                CTSLightingBoltsSystem.Spawn_StardustNova_Simple(spawnPos, c, rot);
            }

            // 第一次命中时触发光点 + 粉尘爆炸
            if (!stuckToEnemy)
            {
                stuckToEnemy = true;
                targetWhoAmI = target.whoAmI;
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;
            }


            // 更复杂的华丽尘埃 + 方形组合
            {
                float globalRotationOffset = Main.rand.NextFloat(0, MathHelper.TwoPi); // 整体随机旋转角度

                for (int i = 0; i < 12; i++)
                {
                    float baseAngle = MathHelper.TwoPi * i / 12f;
                    float finalAngle = baseAngle + globalRotationOffset;

                    float speed = 4f + Main.rand.NextFloat(2f); // 4~6f 的速度
                    Vector2 dir = finalAngle.ToRotationVector2() * speed;

                    Dust d = Dust.NewDustPerfect(Projectile.Center + dir.SafeNormalize(Vector2.UnitY) * 6f, DustID.Electric);
                    d.velocity = dir;
                    d.noGravity = true;
                    d.scale = 1.5f;
                }

                for (int i = 0; i < 6; i++)
                {
                    float baseAngle = MathHelper.TwoPi * i / 6f;
                    float finalAngle = baseAngle + globalRotationOffset;

                    float speed = 3f + Main.rand.NextFloat(2f); // 3~5f 的速度
                    Vector2 vel = finalAngle.ToRotationVector2() * speed;

                    SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 30, 1.4f, Color.Cyan);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }

        }

        public override void OnKill(int timeLeft)
        {
            // 死亡特效：再次触发光点 + 粉尘爆炸
            for (int i = 0; i < 3; i++)
            {
                Vector2 positionOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 spawnPos = Projectile.Center + positionOffset;

                Color c = Color.Lerp(Color.LightBlue, Color.White, Main.rand.NextFloat(0.3f, 0.8f));
                float rot = MathHelper.ToRadians(Main.rand.Next(-45, 45));

                CTSLightingBoltsSystem.Spawn_StardustNova_Simple(spawnPos, c, rot);
            }

            // 更复杂的尘埃 + 方形组合
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 dir = angle.ToRotationVector2() * 4.5f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + dir * 6f, DustID.Electric);
                d.velocity = dir * 2.5f;
                d.noGravity = true;
                d.scale = 1.6f;
            }
            //for (int i = 0; i < 20; i++)
            //{
            //    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Electric);
            //    d.velocity = Main.rand.NextVector2Circular(4f, 4f);
            //    d.scale = Main.rand.NextFloat(1.2f, 2.0f);
            //    d.fadeIn = 0.4f;
            //    d.noGravity = true;
            //}
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3.5f;
                SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 28, 1.5f, Color.LightBlue);
                GeneralParticleHandler.SpawnParticle(p);
            }


            int count = Main.rand.Next(3, 5); // 3~4 枚

            for (int i = 0; i < count; i++)
            {
                // 随机从自身下方半径 100 的圆环中取点
                Vector2 basePos = Projectile.Center + new Vector2(0f, 200f);
                Vector2 offset = Main.rand.NextVector2Circular(100f, 100f);
                Vector2 spawnPos = basePos + offset;

                // 向自身方向射击，但带偏移
                Vector2 shootDir = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                shootDir = shootDir.RotatedByRandom(MathHelper.ToRadians(18f)); // 加入 ±18° 偏移
                Vector2 velocity = shootDir * 9f;

                int projID = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<ElementalLanceJavPROJS>(),
                    (int)(Projectile.damage * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                if (Main.projectile.IndexInRange(projID))
                {
                    Projectile p = Main.projectile[projID];
                    p.friendly = true;
                    p.hostile = false;
                }
            }

        }
    }
}
