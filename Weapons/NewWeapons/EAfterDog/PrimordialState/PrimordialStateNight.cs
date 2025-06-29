using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimordialState
{
    public class PrimordialStateNight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public bool ableToHit = true;
        public NPC target;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 4;
            Projectile.timeLeft = 750;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            Projectile.DamageType = DamageClass.Melee;
        }
        public ref float Time => ref Projectile.ai[1];
        public override bool? CanDamage() => Time >= 20f; // 初始的时候不会造成伤害，直到x为止


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.localAI[0] += 1f / (Projectile.extraUpdates + 1);

            // 逐渐线性加速
            Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.1f;

            // 追踪 PrimordialStatePROJ
            int closestProjIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile otherProj = Main.projectile[i];
                if (otherProj.active && otherProj.type == ModContent.ProjectileType<PrimordialStatePROJ>())
                {
                    float distance = Vector2.Distance(Projectile.Center, otherProj.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestProjIndex = i;
                    }
                }
            }

            if (closestProjIndex != -1)
            {
                Projectile targetProj = Main.projectile[closestProjIndex];
                float distance = Vector2.Distance(Projectile.Center, targetProj.Center);

                if (distance > 5 * 16)
                {
                    // 继续追踪
                    Vector2 direction = (targetProj.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 10f, 0.08f);
                }
                else
                {
                    // 围绕目标旋转
                    float rotationSpeed = 0.1f;
                    Projectile.velocity = Projectile.velocity.RotatedBy(rotationSpeed);
                }
            }
            //if (Projectile.penetrate < 200) // 如果弹幕已经击中敌人，停止追踪能力
            //{
            //    if (Projectile.timeLeft > 60) { Projectile.timeLeft = 60; } // 弹幕开始缩小并减速
            //    Projectile.velocity *= 0.88f;
            //}

            //if (Projectile.timeLeft <= 20) // 弹幕即将消失时停止造成伤害
            //{
            //    ableToHit = false;
            //}

            Time++;

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.DarkGray, Color.Black, colorInterpolation) * 0.8f;  // **调整颜色渐变**
                color.A = 255;  // **确保透明度不会丢失**

                Vector2 drawPosition = Projectile.oldPos[i] + lightTexture.Size() * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(-28f, -28f);

                Color outerColor = color;
                Color innerColor = color * 0.5f;

                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);

                if (Projectile.timeLeft <= 60) // 弹幕即将消失时缩小
                {
                    intensity *= Projectile.timeLeft / 60f;
                }

                Vector2 outerScale = new Vector2(1f) * intensity;
                Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;

                outerColor *= intensity;
                innerColor *= intensity;

                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }
            return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
    }
}
