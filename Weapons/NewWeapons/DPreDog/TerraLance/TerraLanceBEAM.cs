using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles;
using Terraria.ID;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Graphics.Shaders;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance
{
    public class TerraLanceBEAM : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 135;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.6f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1; // 无敌帧冷却时间
            // 使用与 TerratomereSwordBeam 相同的着色器效果
            //Projectile.GetGlobalProjectile<CalamityGlobalProjectile>().ExoBladePierce = true;
        }

        public override void AI()
        {
            // 直线飞行，不使用追踪
            //Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 24f;
            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.015f;

            // 生成 DNA 双链 Dust 特效
            if (Main.rand.NextBool(2))
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 offset = new Vector2(0, 8).RotatedBy(Projectile.rotation + MathHelper.PiOver4 * i);
                    Dust dnaDust = Dust.NewDustPerfect(Projectile.Center + offset, 107, null, 0, Color.Green, 1f);
                    dnaDust.noGravity = true;
                    dnaDust.velocity = Projectile.velocity * -0.3f;
                }
            }

            // 渐渐缩小的效果
            if (Projectile.timeLeft < 60)
            {
                Projectile.scale *= 0.98f;
            }

            // 创建类似 TerratomereSwordBeam 的尾迹效果
            if (Main.rand.NextBool(3))
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch);
                trailDust.noGravity = true;
                trailDust.scale = 1.5f;
                trailDust.velocity *= 0.1f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 发射大量的绿色粒子效果，角度偏移为左右各 1 度，速度较快
            //for (int i = 0; i < 50; i++) // 调整粒子数量
            //{
            //    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-1f, 1f)); // 左右各 1 度偏移
            //    Vector2 particleVelocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(5f, 10f); // 粒子速度较快
            //    Dust greenDust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, particleVelocity, 0, Color.Green, 1.5f); // 绿色粒子
            //    greenDust.noGravity = true; // 无重力
            //    greenDust.scale = Main.rand.NextFloat(1f, 1.5f); // 调整粒子大小
            //}
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 240); // 原版的破晓效果
            target.AddBuff(BuffID.Frostburn, 240); // 原版的霜火效果
            //target.AddBuff(ModContent.BuffType<GlacialState>(), 240); // 冰河时代

            if (Projectile.timeLeft > 12)
                Projectile.timeLeft = 12;
            Projectile.velocity *= 0.2f;
            Projectile.damage = 0;
            Projectile.netUpdate = true;
        }

        public float SlashWidthFunction(float _) => Projectile.width * Projectile.scale * Utils.GetLerpValue(0f, 0.1f, _, true);

        public Color SlashColorFunction(float _) => Color.Turquoise;

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise"));
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(Terratomere.TerraColor1);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(Terratomere.TerraColor2);

            // 17MAY2024: Ozzatron: remove Terratomere rendering its trails multiple times
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(SlashWidthFunction, SlashColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), 30);

            return false;
        }

    }
}
