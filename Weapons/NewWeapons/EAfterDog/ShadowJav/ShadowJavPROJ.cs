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
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.ShadowJav
{
    public class ShadowJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/ShadowJav/ShadowJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private bool hasSplit = false; // 是否已分裂
        private static readonly string[] SplitProjectiles = new string[]
        {
            "AmidiasTridentJavPROJ", "GoldplumeJavPROJ", "SausageMakerJavPROJ", "YateveoBloomJavPROJ",
            "BrimlanceJavPROJ", "EarthenJavPROJ", "StarnightLanceJavPROJ", 
            "AstralPikeJavPROJ", "BotanicPiercerJavPROJ", "DiseasedJavPROJ", "GalvanizingGlaiveJavPROJ", "HellionFlowerJavPROJ",
            "TenebreusTidesJavPROJ", "TyphonsGreedJavPROJ", "VulcaniteLanceJavPROJ", 
            "BansheeHookJavPROJ", "GildedProboscisJavPROJ",
            "ElementalLanceJavPROJNebula", "ElementalLanceJavPROJSolar", "ElementalLanceJavPROJStardust", "ElementalLanceJavPROJVortex", "ElementalLanceJavPROJEntropy",
            "DragonRageJavPROJ", "NadirJavPROJ", "ScourgeoftheCosmosJavPROJ", "StreamGougeJavPROJ", "ViolenceJavPROJ",

            "GraniteJavPROJ", "WulfrimJavPROJ", "RedtideJavPROJ", "BraisedPorkJavPROJ", "ElectrocoagulationTenmonJavPROJ",
            "ElectrocutionHalberdPROJ", "HeartSwordPROJ", "PearlwoodJavPROJ",
            "ChaosEssenceJavPROJ", "SunEssenceJavPROJ", "PolarEssenceJavPROJ",
            "SHPCKPROJ", "SHPCKFast", "FestiveHalberdPROJ", 
            "TerraLancePROJ", "BloodstoneJavPROJ",
            "EndlessDevourJavPROJ", "ChaosWindJavPROJ", "InfiniteDarknessJavPROJ", "SoulHunterJavPROJ",
            "AuricJavPROJ", "MiracleMatterJavPROJ", "TheOtherMiracleMatterJavPROJ",
            "SoulSeekerJavPROJ"
        }; 

        //             , "BonebreakerProjectile", "InsidiousHarpoon"

        //private static readonly int[] VanillaProjectiles = new int[]
        //{
        //    ProjectileID.Daybreak, // Daybreak（636号）
        //    ProjectileID.EatersBite // EatersBite（306号）
        //};

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //SpriteBatch spriteBatch = Main.spriteBatch;
            //Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            //// 获取弹幕绘制位置
            //Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            //Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            //// 设定 Shader 着色器（原本用于拖尾的，现在应用到弹幕本体）
            //GameShaders.Misc["CalamityMod:TrailStreak"].Apply();

            //// 直接绘制弹幕本体，Shader 现在会影响这个 Sprite
            //Main.EntitySpriteDraw(texture, drawPosition, null, lightColor,
            //    Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            //return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
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

            // 添加黑色光源
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 弹幕逐渐加速
            Projectile.velocity *= 1.01f;



            //if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 0, 0);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 前进一段距离后进行分裂
            if (Projectile.timeLeft < 193 && !hasSplit)
            {
                SplitProjectile();
                hasSplit = true;
            }
            // 添加黑色能量光效
            LineParticle energy = new LineParticle(Projectile.Center + Projectile.velocity * 4, Projectile.velocity * 4.95f, false, 9, 2.4f, Color.Black);
            GeneralParticleHandler.SpawnParticle(energy);
        }

        // 分裂逻辑
        private void SplitProjectile()
        {
            int splitCount = Main.rand.Next(2, 5); // 分裂出2到4个弹幕

            for (int i = 0; i < splitCount; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(-5, 6)); // 左右5度随机角度
                Vector2 velocity = Projectile.velocity.RotatedBy(angle) * 0.9f;

                // 始终选择模组中的弹幕
                string selectedProjectile = SplitProjectiles[Main.rand.Next(SplitProjectiles.Length)];
                float damageMultiplier = Main.zenithWorld ? 175.0f : 25.0f; // 判断是否为 ZenithWorld 模式并设置伤害倍率
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, Mod.Find<ModProjectile>(selectedProjectile).Type, (int)(Projectile.damage * damageMultiplier), 0f, Projectile.owner);

                // 生成复杂的黑色粒子特效
                for (int j = 0; j < 8; j++)
                {
                    float particleAngle = MathHelper.ToRadians(45) * (Main.rand.NextBool() ? 1 : -1);
                    Vector2 particleVelocity = velocity.RotatedBy(particleAngle) * Main.rand.NextFloat(2f, 4f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, particleVelocity, 0, Color.Black, Main.rand.NextFloat(1f, 1.5f));
                    dust.noGravity = true;
                }
            }

            // 额外的粒子特效
            for (int i = 0; i < 3; i++)
            {
                Vector2 scatterDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(15 * (i - 1))) * 0.5f;
                Particle pulse = new DirectionalPulseRing(Projectile.Center, scatterDirection, Color.Black, new Vector2(1f, 2.5f), Projectile.rotation, 0.2f, 0.1f, 30);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }



    }
}