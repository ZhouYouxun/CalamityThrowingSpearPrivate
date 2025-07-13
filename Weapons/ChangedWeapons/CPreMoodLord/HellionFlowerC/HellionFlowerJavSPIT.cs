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
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC
{
    public class HellionFlowerJavSPIT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        private string selectedTexture; // 存储随机选择的贴图路径
        public ref float Time => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            // 在弹幕生成时随机选择一次贴图
            string[] textures = new[]
            {
                "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJavSPIT",                
                "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJavSPIT2",
                "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJavSPIT3"
            };

            selectedTexture = textures[Main.rand.Next(textures.Length)];

            Time = 0f; // 初始化计时器

        }

        //public override bool PreDraw(ref Color lightColor)
        //{
        //    SpriteBatch spriteBatch = Main.spriteBatch;

        //    // 加载固定的贴图
        //    Texture2D texture = ModContent.Request<Texture2D>(selectedTexture).Value;

        //    // 计算绘制的原点和位置
        //    Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
        //    Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

        //    // 绘制贴图
        //    spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        //    return false; // 禁用默认绘制
        //}

        public override void SetDefaults()
        {
            // 设置弹幕的基础属性
            Projectile.width = 11; // 弹幕宽度
            Projectile.height = 24; // 弹幕高度
            Projectile.friendly = true; // 对敌人有效
            Projectile.DamageType = DamageClass.Melee; // 远程伤害类型
            Projectile.penetrate = 1; // 穿透力为1，击中一个敌人就消失
            Projectile.timeLeft = 150; // 弹幕存在时间为x帧
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.ignoreWater = true; // 弹幕不受水影响
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            // 让弹幕随着时间逐渐减速
            Projectile.velocity *= 0.99f;

            // 实时调整透明度 (阿尔法值)
            float alphaFactor = 1f - (Projectile.timeLeft / 150f);
            Projectile.alpha = (int)(alphaFactor * 235);

            // 飞行期间生成粒子 DustID.145
            if (Main.rand.NextFloat() < 0.3f) // 控制粒子生成概率
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.JungleSpore, Vector2.Zero, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 300); // 为目标添加毒液效果

            // 命中敌人时生成粒子效果：硬币扩散
            Vector2 hitPosition = target.Center;
            for (int i = 0; i < 8; i++) // 多层粒子
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-20f, 20f));
                Dust.NewDustPerfect(hitPosition + offset, DustID.JungleSpore, null, 150, default, 1.5f).noGravity = true;
                Dust.NewDustPerfect(hitPosition + offset, DustID.JungleSpore, null, 150, default, 1.5f).noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // 加载随机选择的贴图
            Texture2D texture = ModContent.Request<Texture2D>(selectedTexture).Value;

            // 计算绘制的原点和位置
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 渲染充能光晕 - 背光 Lime 描边
            float chargeOffset = 3f; // 控制光晕的扩散
            Color limeOutline = Color.Lime * 0.6f;
            limeOutline.A = 0; // 设置透明度

            // 循环绘制外部光晕
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                spriteBatch.Draw(texture, drawPosition + drawOffset, null, limeOutline, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 额外挑战部分：前半部分为粉色光晕，后半部分为 Lime 光晕
            float gradientFactor = MathHelper.Clamp(Projectile.velocity.Length() / 10f, 0f, 1f); // 根据速度长度控制渐变效果
            Color pinkGlow = Color.HotPink * gradientFactor;
            Color limeGlow = Color.Lime * (1f - gradientFactor);

            spriteBatch.Draw(texture, drawPosition, null, pinkGlow, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPosition, null, limeGlow, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            // 渲染实际的弹幕本体
            spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false; // 禁用默认绘制
        }

        public override void OnKill(int timeLeft)
        {
            // 在消失时生成粒子效果：扩散硬币样式
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * (i / 16f); // 平均分布角度
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 20f;
                Dust.NewDustPerfect(Projectile.Center + offset, 145, null, 150, default, 1.5f).noGravity = true;
                Dust.NewDustPerfect(Projectile.Center + offset, DustID.JungleSpore, null, 150, default, 1.5f).noGravity = true;
            }
        }

    }
}