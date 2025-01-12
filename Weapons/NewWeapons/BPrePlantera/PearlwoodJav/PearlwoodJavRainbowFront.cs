using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJavRainbowFront : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/TheRainbowFront";


        // 根据是否是特殊情况来启用对应的贴图
        //public override string Texture
        //{
        //    get
        //    {
        //        // 判断 Main.zenithWorld 状态并选择对应的贴图路径
        //        if (Main.zenithWorld)
        //        {
        //            return "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/StarSpangledBanner"; // 星条旗
        //        }
        //        else
        //        {
        //            return "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/TheRainbowFront"; // 正常的彩虹
        //        }
        //    }
        //}

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.scale = 0.25f;
            Projectile.usesIDStaticNPCImmunity = true;            
            Projectile.idStaticNPCHitCooldown = 10;
        }


        // 这一段是本来的，因为我们把scale从1.25强行改成0.25，所以用一段新的
        //public override void AI()
        //{
        //    if (Projectile.owner == Main.myPlayer)
        //    {
        //        Projectile.localAI[0] += 1f;
        //        if (Projectile.localAI[0] > 4f)
        //        {
        //            Projectile.localAI[0] = 3f;
        //            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, Projectile.velocity.X * (1f / 1000f), Projectile.velocity.Y * (1f / 1000f), ModContent.ProjectileType<PearlwoodJavRainbowTrail>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
        //        }
        //        if (Projectile.timeLeft > 1200)
        //            Projectile.timeLeft = 1200;
        //    }
        //    float gravityControl = 1f;
        //    if (Projectile.velocity.Y < 0f)
        //        gravityControl -= Projectile.velocity.Y / 3f;
        //    Projectile.ai[0] += gravityControl;
        //    if (Projectile.ai[0] > 30f)
        //    {
        //        Projectile.velocity.Y += 0.5f;
        //        if (Projectile.velocity.Y > 0f)
        //        {
        //            Projectile.velocity.X *= 0.95f;
        //        }
        //        else
        //        {
        //            Projectile.velocity.X *= 1.05f;
        //        }
        //    }
        //    float x = Projectile.velocity.X;
        //    float y = Projectile.velocity.Y;
        //    float velocityMult = 15.95f * Projectile.scale / (float)Math.Sqrt((double)x * (double)x + (double)y * (double)y);
        //    float xVel = x * velocityMult;
        //    float yVel = y * velocityMult;
        //    Projectile.velocity.X = xVel;
        //    Projectile.velocity.Y = yVel;
        //    Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        //}

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.localAI[0] += 1f;
                if (Projectile.localAI[0] > 8f) // 调整尾迹生成间隔
                {
                    Projectile.localAI[0] = 7f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center.X,
                        Projectile.Center.Y,
                        Projectile.velocity.X * (1f / 1000f),
                        Projectile.velocity.Y * (1f / 1000f),
                        ModContent.ProjectileType<PearlwoodJavRainbowTrail>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        0f,
                        0f
                    );
                }
                if (Projectile.timeLeft > 800) // 调整存活时间
                    Projectile.timeLeft = 800;
            }

            float gravityControl = 1f;
            if (Projectile.velocity.Y < 0f)
                gravityControl -= Projectile.velocity.Y / 3f;
            Projectile.ai[0] += gravityControl;

            if (Projectile.ai[0] > 20f) // 调整重力生效时间
            {
                Projectile.velocity.Y += 0.3f; // 减小引力效果
                if (Projectile.velocity.Y > 0f)
                {
                    Projectile.velocity.X *= 0.98f; // 减小阻尼幅度
                }
                else
                {
                    Projectile.velocity.X *= 1.02f; // 减小加速幅度
                }
            }

            float x = Projectile.velocity.X;
            float y = Projectile.velocity.Y;
            float velocityMult = 15.95f / (float)Math.Sqrt((double)x * (double)x + (double)y * (double)y); // 移除对scale的依赖
            float xVel = x * velocityMult;
            float yVel = y * velocityMult;
            Projectile.velocity.X = xVel;
            Projectile.velocity.Y = yVel;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 通知玩家更新计数器
            if (Main.player[Projectile.owner].GetModPlayer<PearlwoodJavPLAYER>() is PearlwoodJavPLAYER player)
            {
                player.IncrementHitCounter();
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;

        public override bool PreDraw(ref Color lightColor)
        {
            // 根据 Main.zenithWorld 状态选择贴图
            Texture2D texture = Main.zenithWorld
                ? ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/StarSpangledBanner").Value
                : ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/TheRainbowFront").Value;

            // 计算绘制位置和原点
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            // 使用 EntitySpriteDraw 绘制弹幕
            Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0);

            return false; // 阻止默认绘制
        }

    }
}
