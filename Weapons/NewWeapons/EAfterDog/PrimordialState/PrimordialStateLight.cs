using CalamityMod;
using CalamityThrowingSpear.Global;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimordialState
{
    public class PrimordialStateLight : ModProjectile, ILocalizedModType
    {
        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectile.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:ExobladeSlash"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.5f - Projectile.velocity.SafeNormalize(Vector2.Zero) * 35f; // 最后的数字调整它的偏移量
            int numPoints = 42;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    ExoSlashWidthFunction,
                    ExoSlashColorFunction,
                    (_) => overallOffset,
                    shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]
                ),
                numPoints
            );

            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 背光颜色渐变数组
            Color[] glowColors = { Color.White, Color.GhostWhite, Color.Cyan, Color.LightBlue, Color.LightYellow };
            float glowPhase = (Main.GlobalTimeWrappedHourly * 2f) % 1f;
            Color glowColor = Color.Lerp(glowColors[(int)(glowPhase * glowColors.Length) % glowColors.Length],
                                         glowColors[((int)(glowPhase * glowColors.Length) + 1) % glowColors.Length],
                                         glowPhase * glowColors.Length % 1f) * 0.6f;
            glowColor.A = 0;

            // 呼吸灯膨胀感
            float chargeOffset = 5f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 2f;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, glowColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }


        // 调整拖尾宽度
        private float ExoSlashWidthFunction(float completionRatio)
        {
            return 28f + (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 3f) * 6f; // 让拖尾更宽
        }

        // 调整颜色，使其更亮
        private Color ExoSlashColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.White, Color.Cyan, completionRatio) * 1.5f; // 增强亮度
        }

        public override void SetDefaults()
        {
            // 设置弹幕的基础属性
            Projectile.width = 11; // 弹幕宽度
            Projectile.height = 11; // 弹幕高度
            Projectile.friendly = true; // 对敌人有效
            Projectile.DamageType = DamageClass.Melee; // 伤害类型
            Projectile.penetrate = 3; // 穿透力为1，击中一个敌人就消失
            Projectile.timeLeft = 1200; // 弹幕存在时间为x帧
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.ignoreWater = true; // 弹幕不受水影响
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.45f;


            // 生成光明主题粒子特效
            if (Main.rand.NextBool(2)) // 50% 概率生成粒子
            {
                int[] dustTypes = { DustID.AncientLight, DustID.RainbowMk2, DustID.SilverFlame, DustID.SteampunkSteam };
                int dustType = Main.rand.Next(dustTypes);

                float offsetAmount = Main.rand.NextFloat(-Projectile.width / 2f, Projectile.width / 2f);
                Vector2 dustPosition = Projectile.Center + new Vector2(offsetAmount, 0f);
                Dust dust = Dust.NewDustPerfect(dustPosition, dustType, Projectile.velocity * 0.1f, 150, default, Main.rand.NextFloat(1.55f, 2.0f));
                dust.noGravity = true;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 瞄准最近的敌人并调整弹幕方向
            NPC closestNPC = Main.npc
                .Where(npc => npc.active && !npc.friendly && npc.life > 0 && npc.whoAmI != target.whoAmI)
                .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                .FirstOrDefault();

            if (closestNPC != null)
            {
                Vector2 direction = closestNPC.Center - Projectile.Center;
                Projectile.velocity = Vector2.Normalize(direction) * Projectile.velocity.Length();
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 随机角度释放X个太极纹理
            for (int i = 0; i < 1; i++)
            {
                Particle blastRing = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.White,
                    "CalamityThrowingSpear/Texture/YingYang",
                    Vector2.One * 0.33f,
                    Main.rand.NextFloat(-10f, 10f),
                    0.17f,
                    0.43f,
                    30
                );
                GeneralParticleHandler.SpawnParticle(blastRing);
            }
        }
    }
}