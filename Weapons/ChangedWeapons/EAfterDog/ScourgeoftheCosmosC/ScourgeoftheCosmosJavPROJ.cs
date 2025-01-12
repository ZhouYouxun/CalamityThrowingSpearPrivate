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
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/ScourgeoftheCosmosC/ScourgeoftheCosmosJav";
        private static Color ShaderColorOne = Color.LightGray; // 浅灰色
        private static Color ShaderColorTwo = Color.Purple; // 紫色
        private static Color ShaderEndColor = Color.LightPink; // 结束颜色，浅粉色

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 保留现有的拖尾效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 添加新的拖尾着色器效果
            //GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            //Vector2 overallOffset = Projectile.Size * 0.5f;
            //overallOffset += Projectile.velocity * 1.6f; // 使长度更长
            //int numPoints = 100; // 增加拖尾的长度
            //PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);
            return false;
        }

        private float PrimitiveWidthFunction(float completionRatio)
        {
            return 24f; // 保持固定宽度
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            float endFadeRatio = 0.41f;
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, completionRatio);
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, completionRatio));
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加浅紫色光照，光照强度不变
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            // 弹幕逐渐加速
            Projectile.velocity *= 1.01f;

            // 每5帧添加尖刺型粒子特效
            if (Projectile.timeLeft % 5 == 0)
            {
                PointParticle spark = new PointParticle(Projectile.Center - Projectile.velocity + Projectile.velocity.RotatedBy(2.3f), Projectile.velocity.RotatedBy(2.3f) * 0.5f, false, 15, 1.1f, Color.Purple);
                GeneralParticleHandler.SpawnParticle(spark);
                PointParticle spark2 = new PointParticle(Projectile.Center - Projectile.velocity + Projectile.velocity.RotatedBy(-2.3f), Projectile.velocity.RotatedBy(-2.3f) * 0.5f, false, 15, 1.1f, Color.Purple);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 添加弑神者火焰buff，持续300帧
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);

            int numProjectiles = Main.rand.Next(4, 9); // 生成3到5个迷你小吞噬者弹幕
            for (int i = 0; i < numProjectiles; i++)
            {
                Vector2 spawnPosition = Projectile.Center;
                Vector2 spawnVelocity = Projectile.velocity.RotatedByRandom(MathHelper.TwoPi) * 0.85f * 0.5f; // 随机方向，伤害倍率0.7倍
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, spawnVelocity, ModContent.ProjectileType<ScourgeoftheCosmosJavMini>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }

            // 生成浅紫色冲击波
            //Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Purple, new Vector2(1.5f, 1.5f), Main.rand.NextFloat(8f, 12f), 0.15f, 3f, 10);
            //GeneralParticleHandler.SpawnParticle(pulse);

            // 生成浅灰色菱形粒子，数量7到9个
            int numParticles = Main.rand.Next(7, 10);
            for (int i = 0; i < numParticles; i++)
            {
                Vector2 randomDirection = Projectile.velocity.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f); // 随机方向和速度
                float particleScale = Main.rand.NextFloat(0.6f, 1.0f); // 随机大小
                SparkParticle spark = new SparkParticle(Projectile.Center, randomDirection, false, Main.rand.Next(35, 50), particleScale, Color.LightGray);
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // 生成EssenceFlame2
            int numProjectiles2 = Main.rand.Next(7, 12); // 生成7到12个投射物
            for (int i = 0; i < numProjectiles2; i++)
            {
                Vector2 spawnPosition = Projectile.Center; // 弹幕消失位置为生成位置
                Vector2 spawnVelocity = Vector2.Zero; // 初始速度为0
                int essenceDamage = (int)(Projectile.damage * 0.5f); // 伤害倍率为0.75
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, spawnVelocity, ModContent.ProjectileType<EssenceFlame2>(), essenceDamage, Projectile.knockBack, Projectile.owner);
            }

            // 为玩家回复生命
            Main.player[Projectile.owner].statLife += 3;
            Main.player[Projectile.owner].HealEffect(3);
        }

        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);

            // 生成紫色和浅灰色的烟雾特效
            int Dusts = 15; // 生成的粒子数量
            float radians = MathHelper.TwoPi / Dusts; // 每个粒子的旋转角度
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)); // 初始旋转方向
            for (int i = 0; i < Dusts; i++)
            {
                // 增大烟雾扩散幅度，调整速度
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * Main.rand.NextFloat(8f, 12f);

                // 随机选择紫色或浅灰色作为烟雾颜色
                Color smokeColor = Main.rand.NextBool() ? Color.LightGray : Color.Purple;

                // 生成烟雾特效
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity, smokeColor, 18, Main.rand.NextFloat(1.2f, 1.8f), 0.45f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }


        }









    }
}