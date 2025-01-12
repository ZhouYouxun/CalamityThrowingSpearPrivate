using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using Terraria;
using Microsoft.Xna.Framework.Graphics;


namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJavRainbowTrail : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/TheRainbowFront";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Rainbow;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.light = 0.3f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 0.25f;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 通知玩家更新计数器
            if (Main.player[Projectile.owner].GetModPlayer<PearlwoodJavPLAYER>() is PearlwoodJavPLAYER player)
            {
                player.IncrementHitCounter();
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);

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
