using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationSTAR : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private int noTileHitCounter = 90;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public ref float Time => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 50;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 🚩【速度保持恒定】
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 22f;

            // 🚩【飞行特效：AstralOrange Dust（轻度增强）】
            if (!Main.dedServ && Time > 5f)
            {
                int dustCount = Main.rand.Next(3, 5); // 每帧生成3~4个

                for (int i = 0; i < dustCount; i++)
                {
                    // 使用圆环分布而非线性插值以分散
                    Vector2 offset = Main.rand.NextVector2CircularEdge(Projectile.width * 0.5f, Projectile.height * 0.5f);
                    Vector2 spawnPos = Projectile.Center + offset;

                    Dust dust = Dust.NewDustPerfect(spawnPos, ModContent.DustType<AstralOrange>());

                    //// 🚩 保持亮度稳定的蓝白流动
                    //float hue = (0.55f + Main.rand.NextFloat(-0.05f, 0.05f)) % 1f; // 蓝白范围
                    //dust.color = Main.hslToRgb(hue, 0.85f, 0.8f); // 提高亮度到0.8f

                    dust.scale = Main.rand.NextFloat(1.0f, 1.4f); // 保持原本体积感
                    dust.fadeIn = 0.8f;
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f); // 较小扰动
                    dust.noGravity = true;
                }
            }


            // 🚩【生成 Gore：频率略增强】
            if (Main.rand.NextBool(32) && Main.netMode != NetmodeID.Server)
            {
                int gore = Gore.NewGore(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    Projectile.velocity * 0.25f,
                    16,
                    1f
                );
                Main.gore[gore].velocity *= 0.7f;
                Main.gore[gore].velocity += Projectile.velocity * 0.3f;
            }

            // 🚩【透明度和光效】
            Projectile.alpha = Math.Max(Projectile.alpha - 15, 0);
            int minAlpha = 50;
            if (Projectile.alpha < minAlpha)
                Projectile.alpha = minAlpha;

            if (Projectile.ai[1] == 1f)
            {
                Projectile.light = 0.9f;
            }

            // 🚩【旋转同步】
            Projectile.rotation += Projectile.velocity.Length() * 0.01f * Projectile.direction;

            // 🚩【飞行路径控制】
            if (Time <= 30f)
            {
                // 前30帧：sin震荡+随机扰动（已改进飞行）
                float wave = (float)Math.Sin(Time * 0.3f) * MathHelper.ToRadians(2f);
                float randomNudge = Main.rand.NextFloat(-0.01f, 0.01f);
                Projectile.velocity = Projectile.velocity.RotatedBy(wave + randomNudge);
            }
            else
            {
                // 之后进行柔和追踪
                NPC target = Projectile.Center.ClosestNPCAt(3600);
                if (target != null)
                {
                    // 🚩 线性提升追踪速度
                    Projectile.localAI[0] += 0.0025f; // 每帧线性加速，可根据需要微调

                    float currentSpeed = 18f + Projectile.localAI[0] * 60f; // 可封顶防止过快，可改为 Math.Min(currentSpeed, maxSpeed)
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 desiredVelocity = toTarget.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f)) * currentSpeed;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.06f);
                }
            }


            Time++;
        }



        public override Color? GetAlpha(Color lightColor) => new Color(200, 100, 250, Projectile.alpha);

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] == 1f)
                return;

            Projectile.position.X = Projectile.position.X + Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y + Projectile.height / 2;
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.position.X = Projectile.position.X - Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y - Projectile.height / 2;
            for (int i = 0; i < 5; i++)
            {
                int starryDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.2f);
                Main.dust[starryDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[starryDust].scale = 0.5f;
                    Main.dust[starryDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 5; j++)
            {
                int starryDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.7f);
                Main.dust[starryDust2].noGravity = true;
                Main.dust[starryDust2].velocity *= 5f;
                starryDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1f);
                Main.dust[starryDust2].velocity *= 2f;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k < 3; k++)
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawStarTrail(Color.Coral, Color.White);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);
            return false;
        }
        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到12为止

    }
}
