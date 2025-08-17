using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC
{
    public class AmidiasTridentJavWaterWall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 使用完全透明贴图

        public override void SetDefaults()
        {
            Projectile.width = 80; 
            Projectile.height = 16 * 20; // 自由设定较高的高度
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 1 == 0) // 保持高密度生成
            {
                for (int i = 0; i < 20; i++)
                {
                    // 在水墙宽度范围内分布
                    float xOffset = Main.rand.NextFloat(-Projectile.width * 0.5f, Projectile.width * 0.5f);

                    // 使用抛物线分布高度：y = -a(x^2) + c
                    float normalizedX = xOffset / (Projectile.width * 0.5f); // -1 ~ 1
                    float a = Main.rand.NextFloat(0.8f, 1.2f); // 抛物线开口大小微扰
                    float c = Main.rand.NextFloat(-Projectile.height * 0.5f, Projectile.height * 0.5f); // 抛物线上移微扰
                    float yOffset = -a * normalizedX * normalizedX * (Projectile.height * 0.4f) + c;

                    Vector2 spawnPos = Projectile.Center + new Vector2(xOffset, yOffset);

                    // 形成向上微扩散抛物线速度：
                    float speedY = Main.rand.NextFloat(-10f, -14f); // 向上速度
                    float speedX = normalizedX * Main.rand.NextFloat(0.5f, 2.5f); // 左右微偏

                    Vector2 velocity = new Vector2(speedX, speedY);

                    // 混合使用 DustID.Water 和 DustID.BlueCrystalShard
                    int type = Main.rand.NextBool() ? DustID.Water : DustID.BlueCrystalShard;

                    Dust d = Dust.NewDustPerfect(spawnPos, type, velocity * 0.5f, 150, Color.Cyan, Main.rand.NextFloat(1.0f, 1.6f));
                    d.noGravity = true;
                }
            }

            // 柔和光效不变
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.4f);
        }


        public override bool PreDraw(ref Color lightColor)
        {
            // 完全透明不绘制本体
            return false;
        }
    }
}
