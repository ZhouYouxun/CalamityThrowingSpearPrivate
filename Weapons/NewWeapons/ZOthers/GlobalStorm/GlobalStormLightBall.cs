using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.ZOthers.GlobalStorm
{
    internal class GlobalStormLightBall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
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

        private Vector2 initialPosition;
        private bool enteredSecondPhase = false;

        public override void OnSpawn(IEntitySource source)
        {
            // 记录生成时的位置
            Player player = Main.player[Projectile.owner];
            initialPosition = player.Center + new Vector2(0, -15 * 16);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 检查是否已经进入第二阶段
            if (!enteredSecondPhase)
            {
                // 确保光球一直跟随初始位置
                initialPosition = player.Center + (initialPosition - player.oldPosition);

                // 检查 GlobalStormRightHoldOut 是否存在
                bool exists = Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<GlobalStormRightHoldOut>() && p.owner == player.whoAmI);
                if (!exists)
                {
                    enteredSecondPhase = true; // 进入第二阶段
                }
            }

            if (!enteredSecondPhase)
            {
                // 第一阶段：保持在初始位置
                Projectile.Center = initialPosition;
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                // 第二阶段：强力追踪最近敌人
                NPC target = Projectile.Center.ClosestNPCAt(2500);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 15f, 0.08f);
                }
            }
            Time++;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.Blue, Color.AliceBlue, colorInterpolation) * 0.8f;  // **调整颜色渐变**
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
