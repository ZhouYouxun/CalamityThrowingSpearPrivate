using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using System;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJS : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = 2;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 2.2f;
        }

        public override void AI()
        {
            Projectile.rotation += 2.5f;
            Projectile.alpha -= 5;

            if (Projectile.alpha < 50)
            {
                Projectile.alpha = 50;

                NPC target = FindClosestNPC(8000f);
                if (target != null && target.active)
                {
                    float maxSpeed = 10f;
                    float acceleration = 0.2f;
                    if (Projectile.velocity.Length() < maxSpeed)
                    {
                        Projectile.velocity += Projectile.DirectionTo(target.Center) * acceleration;
                        if (Projectile.velocity.Length() > maxSpeed)
                        {
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
                        }
                    }
                }

                {
                    // 飞行粒子效果：双螺旋结构（寿命更短、粒子更小、混合ID 217/202）
                    float spiralRadius = 6f;
                    float spiralSpeed = 0.25f;
                    float spiralAngle1 = Projectile.ai[1] * spiralSpeed;
                    float spiralAngle2 = spiralAngle1 + MathHelper.Pi;

                    Vector2 offset1 = new Vector2((float)Math.Cos(spiralAngle1), (float)Math.Sin(spiralAngle1)) * spiralRadius;
                    Vector2 offset2 = new Vector2((float)Math.Cos(spiralAngle2), (float)Math.Sin(spiralAngle2)) * spiralRadius;

                    int dustType1 = Main.rand.NextBool() ? 217 : 202;
                    int dustType2 = Main.rand.NextBool() ? 217 : 202;

                    Dust d1 = Dust.NewDustPerfect(Projectile.Center + offset1, dustType1, Vector2.Zero, 100, Color.LightGray, 0.5f);
                    d1.noGravity = true;
                    d1.scale = 0.5f;
                    d1.velocity *= 0.1f;
                    d1.fadeIn = 0.1f;

                    Dust d2 = Dust.NewDustPerfect(Projectile.Center + offset2, dustType2, Vector2.Zero, 100, Color.LightGray, 0.5f);
                    d2.noGravity = true;
                    d2.scale = 0.5f;
                    d2.velocity *= 0.1f;
                    d2.fadeIn = 0.1f;
                }
             
                if (Main.rand.NextBool(5))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    SquareParticle sq = new SquareParticle(Projectile.Center, vel, false, 20, 1f, Color.LightGray);
                    GeneralParticleHandler.SpawnParticle(sq);
                }

                if (Projectile.ai[1] >= 15)
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        Vector2 dustspeed = new Vector2(3f, 3f).RotatedBy(MathHelper.ToRadians(60 * i));
                        int d = Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, 31, dustspeed.X, dustspeed.Y, 200, Color.LightGray, 1.3f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity = dustspeed;
                    }
                    Projectile.ai[1] = 0;
                }
            }
            Projectile.ai[1]++;
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closestNPC = null;
            float closestDistance = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile))
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }
            return closestNPC;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;

            float time = Main.GlobalTimeWrappedHourly;
            float scale = 1.0f * Projectile.Opacity;

            // 本体小层交错叠加（互相反转）
            DrawPortalLayer(texture, drawPos, origin, scale * 1.9f, time * 1.2f, Color.White * 0.6f);
            DrawPortalLayer(texture, drawPos, origin, scale * 1.9f, -time * 1.2f, Color.White * 0.6f);

            // 外层更大、更快旋转，两层反转 + 更低透明度
            DrawPortalLayer(texture, drawPos, origin, scale * 2.3f, time * 2.5f, Color.Cyan * 0.3f);
            DrawPortalLayer(texture, drawPos, origin, scale * 2.3f, -time * 2.5f, Color.Cyan * 0.3f);

            return false;
        }

        private void DrawPortalLayer(Texture2D texture, Vector2 pos, Vector2 origin, float scale, float rotation, Color color)
        {
            Main.spriteBatch.Draw(texture, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 360; i += 15)
            {
                Vector2 dustspeed = new Vector2(3f, 3f).RotatedBy(MathHelper.ToRadians(i));
                int d = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, 31, dustspeed.X, dustspeed.Y, 200, Color.LightGray, 1.2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].position = Projectile.Center;
                Main.dust[d].velocity = dustspeed;
            }

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat(1.5f));
                SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 28, 1.3f, Color.LightGray);
                GeneralParticleHandler.SpawnParticle(p);
            }
        }
    }
}
