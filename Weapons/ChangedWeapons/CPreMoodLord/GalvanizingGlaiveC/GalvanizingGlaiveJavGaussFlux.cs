using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC
{
    public class GalvanizingGlaiveJavGaussFlux : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        //public float Time
        //{
        //    get => Projectile.ai[0];
        //    set => Projectile.ai[0] = value;
        //}
        //public NPC Target
        //{
        //    get => Main.npc[(int)Projectile.ai[1]];
        //    set => Projectile.ai[1] = value.whoAmI;
        //}
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Lime.ToVector3());

            // 查找范围内最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(1800);
            if (target != null)
            {
                // 追踪敌人
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为12f

                // 在敌人位置生成粒子效果
                for (int i = 0; i < 7; i++)
                {
                    for (int arcIndex = 0; arcIndex < 6; arcIndex++)
                    {
                        float offsetAngle = MathHelper.ToRadians(1080f) * i / 18f;
                        offsetAngle += Projectile.ai[0] / 10f;
                        float scale = 1.4f + (float)Math.Cos(i / 7f * MathHelper.TwoPi + Projectile.ai[0] / 30f) * 0.3f;
                        scale *= MathHelper.Lerp(1f, 0.4f, arcIndex / 6f);
                        Vector2 offset = target.Size.RotatedBy(offsetAngle) * 0.5f;
                        offset += (arcIndex * MathHelper.TwoPi / 6f + Projectile.ai[0] / 20f).ToRotationVector2() * 6f * arcIndex;

                        Dust dust = Dust.NewDustPerfect(target.Center + offset, 261);
                        dust.color = Utils.SelectRandom(Main.rand, Color.AliceBlue, Color.CornflowerBlue);
                        dust.velocity = Vector2.Zero;
                        dust.scale = scale;
                        dust.noGravity = true;
                    }
                }
            }

            // 时间累加
            Projectile.ai[0]++;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
        }
    }
}
