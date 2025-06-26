using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Particles;
using CalamityMod;
using System;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    /// <summary>
    /// Elemental Lance 辅助弹幕：停留型 portal 弹幕，持续旋转并在死亡时释放闪电。
    /// </summary>
    public class ElementalLanceJavPROJV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        public override string Texture => "CalamityMod/Projectiles/Melee/StreamGougePortal";

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.Opacity = 0f;
        }

        public override void AI()
        {
            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.05f, 0f, 1f);
            Projectile.rotation += 0.03f;

            // 寻找最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(2400f);
            if (target != null)
            {
                Vector2 targetPos = target.Center + new Vector2(0, -400);
                Vector2 toTarget = targetPos - Projectile.Center;
                float dist = toTarget.Length();

                float t = 1f - MathHelper.Clamp(dist / 800f, 0f, 1f);
                float easing = (float)Math.Sin(t * MathHelper.Pi); // iOS节奏
                float maxSpeed = MathHelper.Lerp(4f, 48f, easing); // 速度

                Vector2 desiredVelocity = toTarget.SafeNormalize(Vector2.Zero) * maxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.4f); // 加速响应
            }
            else
            {
                Projectile.velocity = Vector2.Zero; // 没有敌人则停下
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects flip = SpriteEffects.None;

            float time = Main.GlobalTimeWrappedHourly;
            float scale = 1.0f * Projectile.Opacity;

            // 绘制三层 portal，不同颜色 + 旋转速度
            DrawPortalLayer(texture, drawPos, origin, scale * 1.2f, time * 1.4f, Color.DarkGreen * 0.6f);
            DrawPortalLayer(texture, drawPos, origin, scale * 1.1f, -time * 1.1f, Color.LightGreen * 0.5f);
            DrawPortalLayer(texture, drawPos, origin, scale * 1.0f, time * 0.6f, Color.LightBlue * 0.6f);

            // 叠加三层 twirl 光圈
            Texture2D twirl = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
            Vector2 originTwirl = twirl.Size() * 0.5f;
            DrawTwirl(twirl, drawPos, originTwirl, scale * 0.38f, time * 1.6f, Color.Cyan);
            DrawTwirl(twirl, drawPos, originTwirl, scale * 0.75f, -time * 1.2f, Color.Teal * 0.8f);
            DrawTwirl(twirl, drawPos, originTwirl, scale * 0.59f, time * 0.8f, Color.White * 0.5f);

            return false;
        }

        private void DrawPortalLayer(Texture2D tex, Vector2 pos, Vector2 origin, float scale, float rot, Color color)
        {
            Main.EntitySpriteDraw(tex, pos, null, color with { A = 0 }, rot, origin, scale, SpriteEffects.None, 0);
        }

        private void DrawTwirl(Texture2D tex, Vector2 pos, Vector2 origin, float scale, float rot, Color color)
        {
            Main.EntitySpriteDraw(tex, pos, null, color with { A = 0 }, rot, origin, scale, SpriteEffects.None, 0);
        }

        public override void OnKill(int timeLeft)
        {
            // 有序粒子：方形蓝绿爆散
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 30, 1.6f, Color.Cyan);
                GeneralParticleHandler.SpawnParticle(p);
            }

            // 无序粒子：混合 Dust 爆散
            for (int i = 0; i < 30; i++)
            {
                int dustType = Utils.SelectRandom(Main.rand, 99, 202, 229);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                d.scale = Main.rand.NextFloat(1.2f, 2.0f);
                d.fadeIn = 0.4f;
                d.noGravity = true;
            }

            // 放出六道朝下的闪电弹幕，带轻微偏转角度
            for (int i = 0; i < 6; i++)
            {
                Vector2 shootDir = Vector2.UnitY.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f)));
                Vector2 velocity = shootDir * 10f;

                int lightning = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ProjectileID.CultistBossLightningOrbArc, (int)(Projectile.damage * 2.1), 0f, Projectile.owner,
                    MathHelper.PiOver2, Main.rand.Next(100));

                if (Main.projectile.IndexInRange(lightning))
                {
                    Projectile p = Main.projectile[lightning];
                    p.friendly = true;
                    p.hostile = false;
                    p.penetrate = -1;
                    p.usesLocalNPCImmunity = true;
                    p.localNPCHitCooldown = 60;
                }
            }

            SoundStyle fire = new("CalamityMod/Sounds/Item/AuricBulletHit");
            SoundEngine.PlaySound(fire with { Volume = Projectile.ai[0] == 1 ? 1.0f : 0.4f, Pitch = 0f }, Projectile.Center);


            int missileCount = Main.rand.Next(4, 7); // 4 ~ 6 枚导弹

            for (int i = 0; i < missileCount; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY);
                Vector2 velocity = dir * Main.rand.NextFloat(7f, 12f); // 弹速

                int projID = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ProjectileID.VortexBeaterRocket, // 原版导弹
                    (int)(Projectile.damage * 0.2f), // 伤害为主弹幕的 20%
                    1f,
                    Projectile.owner
                );

                if (Main.projectile.IndexInRange(projID))
                {
                    Projectile missile = Main.projectile[projID];
                    missile.friendly = true;
                    missile.hostile = false;
                    missile.usesLocalNPCImmunity = true;
                    missile.localNPCHitCooldown = 10;
                }
            }

        }
    }
}
