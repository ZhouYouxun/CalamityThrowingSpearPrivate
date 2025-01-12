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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Graphics.Metaballs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav
{
    public class SoulSeekerJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SoulSeekerJav/SoulSeekerJav";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; 
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);

            // 添加自定义烟雾效果，每隔x帧释放一个粒子
            if (Projectile.timeLeft % 1 == 0) // 每隔x帧触发一次
            {
                // 生成粒子的随机位置，相对于当前弹幕位置稍微向后偏移
                Vector2 randomOffset = Main.rand.NextVector2Circular(5f, 5f); // 控制偏移的范围，可以根据需要调整
                Vector2 spawnPosition = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * 10f + randomOffset;

                // 生成粒子
                float radius = Main.rand.NextFloat(24f, 48f); // 随机设置粒子的大小
                Vector2 particleVelocity = Main.rand.NextVector2Circular(3f, 3f); // 随机生成粒子的初始速度方向

                // 调用生成粒子的方法（异端僭越同款）
                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }

            // 前x帧不追踪，之后开始追踪敌人或与方块碰撞后开始追踪
            if (Projectile.ai[1] > 40 || !Projectile.tileCollide)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1200); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 15f, 0.08f); // 追踪速度为xf
                }
            }
            else
            {
                Projectile.velocity *= 1.001f;
                Projectile.ai[1]++;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 发生碰撞后禁用与方块的进一步碰撞
            Projectile.tileCollide = false;
            return false; // 不销毁弹幕
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 检查当前小鸟数量
            int currentBirdCount = Main.projectile.Count(p => p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<SoulSeekerJavBRID>());
            if (currentBirdCount >= 10)
            {
                // 小鸟数量达到上限时，增加x倍伤害
                modifiers.FinalDamage *= 2.22f;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                // 检查场上当前存在的小鸟数量
                int existingBirdCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<SoulSeekerJavBRID>())
                    {
                        existingBirdCount++;
                    }
                }

                // 生成等边三角形粒子效果
                for (int i = 0; i < 6; i++)
                {
                    float angle;
                    float nextAngle;

                    if (existingBirdCount < 10)
                    {
                        // 小鸟数量少于10，生成正向三角形
                        angle = MathHelper.PiOver2 + i * MathHelper.TwoPi / 3f; // 正向三角形角度
                        nextAngle = MathHelper.PiOver2 + (i + 1) * MathHelper.TwoPi / 3f;
                    }
                    else
                    {
                        // 小鸟数量达到或超过10，生成反向三角形
                        angle = -MathHelper.PiOver2 + i * MathHelper.TwoPi / 3f;
                        nextAngle = -MathHelper.PiOver2 + (i + 1) * MathHelper.TwoPi / 3f;

                    }

                    Vector2 start = angle.ToRotationVector2();
                    Vector2 end = nextAngle.ToRotationVector2();

                    for (int j = 0; j < 40; j++)
                    {
                        Dust triangleDust = Dust.NewDustPerfect(Projectile.Center, 267);
                        triangleDust.scale = 2.5f;
                        triangleDust.velocity = Vector2.Lerp(start, end, j / 40f) * 16f;
                        triangleDust.color = Color.Crimson;
                        triangleDust.noGravity = true;
                    }
                }

                // 如果数量达到10个，额外生成独特的自定义烟雾粒子
                if (existingBirdCount >= 10)
                {
                    // 额外生成10到20个粒子，以当前弹幕为中心向外发射
                    int extraParticles = Main.rand.Next(10, 21); // 随机生成10到20个粒子
                    for (int i = 0; i < extraParticles; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机生成角度
                        Vector2 particleVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f); // 随机速度方向和大小
                        float radius = Main.rand.NextFloat(24f, 48f); // 粒子的大小范围
                        Vector2 spawnPosition = Projectile.Center; // 以当前弹幕为中心
                        GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
                    }
                }

                // 如果数量少于10个，则生成新的小鸟
                if (existingBirdCount < 10)
                {
                    // 在玩家周围随机位置生成 SoulSeekerJavBRID
                    Vector2 spawnPosition = Main.player[Projectile.owner].Center + Main.rand.NextVector2Circular(200f, 200f);
                    int birdDamage = (int)(Projectile.damage * 1.133f); // 将小鸟的伤害设置为本体的两倍
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<SoulSeekerJavBRID>(), birdDamage, 0f, Projectile.owner);

                    // 生成7到10个烟雾粒子，围绕小鸟生成位置
                    int smokeParticleCount = Main.rand.Next(7, 11); // 随机生成烟雾粒子数量（7到10个）
                    for (int i = 0; i < smokeParticleCount; i++)
                    {
                        Vector2 randomOffset = Main.rand.NextVector2Circular(10f, 10f); // 偏移范围
                        Vector2 smokePosition = spawnPosition + randomOffset;
                        float radius = Main.rand.NextFloat(24f, 48f); // 粒子的大小范围
                        Vector2 particleVelocity = Main.rand.NextVector2Circular(3f, 3f); // 随机速度方向
                        GruesomeMetaball.SpawnParticle(smokePosition, particleVelocity, radius);
                    }
                }
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 生成粒子射向正前方、左30度和右30度
            int particleCount = 30; // 每个方向发射的粒子数量
            float baseAngle = Projectile.velocity.ToRotation(); // 基础角度为弹幕的当前方向
            float offsetAngle = MathHelper.ToRadians(30f); // 角度偏移为30度

            // 发射正前方的粒子
            for (int i = 0; i < particleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(-offsetAngle / 2, offsetAngle / 2); // 在正前方略微随机偏移角度
                Vector2 particleVelocity = (baseAngle + randomAngle).ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                float radius = Main.rand.NextFloat(24f, 48f); // 粒子的大小范围
                Vector2 spawnPosition = Projectile.Center;
                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }

            // 发射左偏30度的粒子
            for (int i = 0; i < particleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(-offsetAngle / 2, offsetAngle / 2); // 在左30度略微随机偏移角度
                Vector2 particleVelocity = (baseAngle - offsetAngle + randomAngle).ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                float radius = Main.rand.NextFloat(24f, 48f); // 粒子的大小范围
                Vector2 spawnPosition = Projectile.Center;
                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }

            // 发射右偏30度的粒子
            for (int i = 0; i < particleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(-offsetAngle / 2, offsetAngle / 2); // 在右30度略微随机偏移角度
                Vector2 particleVelocity = (baseAngle + offsetAngle + randomAngle).ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                float radius = Main.rand.NextFloat(24f, 48f); // 粒子的大小范围
                Vector2 spawnPosition = Projectile.Center;
                GruesomeMetaball.SpawnParticle(spawnPosition, particleVelocity, radius);
            }
        }





    }
}