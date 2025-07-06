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
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 1 == 0) // 每2帧生成一次
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 spawnPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-Projectile.width * 0.5f, Projectile.width * 0.5f), Main.rand.NextFloat(-Projectile.height * 0.5f, Projectile.height * 0.5f));
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-8f, -12f)); // 几乎向上

                    // 混合使用 DustID.Water 和 DustID.BlueCrystalShard
                    int type = Main.rand.NextBool() ? DustID.Water : DustID.BlueCrystalShard;

                    Dust d = Dust.NewDustPerfect(spawnPos, type, velocity * 0.5f, 150, Color.Cyan, Main.rand.NextFloat(1.0f, 1.6f));
                    d.noGravity = true;
                }
            }

            // 加入柔和光效
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 完全透明不绘制本体
            return false;
        }
    }
}
