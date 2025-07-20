using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavFlame : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.CPreMoodLord";
        public int frameX = 0;
        public int frameY = 0;
        public int currentFrame => frameY + frameX * 4;
        public override void SetDefaults()
        {
            Projectile.width = 81;
            Projectile.height = 322;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            //2-6
            Projectile.frameCounter += 1;
            if (Projectile.frameCounter % 7 == 6)
            {
                frameY += 1;
                if (frameY >= 4)
                {
                    frameX += 1;
                    frameY = 0;
                }
                if (frameX >= 3)
                {
                    Projectile.Kill();
                }
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.position.Y -= Projectile.height / 2; //position adjustments
                Projectile.localAI[0] = 1f;
            }

            {
                // 向下位移的偏移中心
                Vector2 emitCenter = Projectile.Center + new Vector2(0f, 144f); // 9×16 往下移动

                // Dust 火山熔岩喷射效果（受到重力影响）
                for (int i = 0; i < 28; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(12f, 12f);
                    velocity.Y -= Main.rand.NextFloat(16f, 22f);
                    velocity = velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(10f), MathHelper.ToRadians(15f)));

                    int dustID = Dust.NewDust(
                        emitCenter,
                        0, 0,
                        Main.rand.NextBool() ? DustID.Torch : DustID.InfernoFork,
                        velocity.X, velocity.Y,
                        100,
                        Color.Orange,
                        Main.rand.NextFloat(1.5f, 2.4f)
                    );
                    Main.dust[dustID].noGravity = false;
                }

                Color sparkColor = Color.Yellow;
                Vector2 sparkVelocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-14f, -7f));

                SparkParticle spark = new SparkParticle(
                    emitCenter,
                    sparkVelocity,
                    true,
                    Main.rand.Next(15, 25),
                    Main.rand.NextFloat(1.5f, 2.2f),
                    Color.Yellow
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = new Rectangle(frameX * Projectile.width, frameY * Projectile.height, Projectile.width, Projectile.height);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, Projectile.Size / 2, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            target.AddBuff(BuffID.Daybreak, 240);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire3, 300);
    }
}
