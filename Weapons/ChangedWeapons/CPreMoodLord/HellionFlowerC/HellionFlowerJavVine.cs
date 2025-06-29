using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC
{
    public class HellionFlowerJavVine : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public static int TotalSegments = 10;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为30帧
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.ai[1] == 0f)
            {
                Projectile.alpha -= 150; // 这个值越低，生长速度越慢，反之亦然
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;
                    Projectile.ai[1] = 1f;

                    // 此射弹通常不会自行移动，因此这将仅手动移动一次
                    // 这仅适用于第一段
                    if (Projectile.ai[0] == 0f)
                    {
                        Projectile.ai[0]++;
                        Projectile.position += Projectile.velocity;
                    }

                    // 生成下一节
                    if (Main.myPlayer == Projectile.owner && Projectile.ai[0] < TotalSegments)
                    {
                        int nextSegment = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Projectile.velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0] + 1f);
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, nextSegment);
                    }
                }
            }
            else // 消失
            {
                int AlphaPerFrame = 8; // 提高这个值可以让它更快的消失
                Projectile.alpha += AlphaPerFrame;
                if (Projectile.alpha == AlphaPerFrame * 21)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Dust thorn = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, 200, default, 1.3f);
                        thorn.noGravity = true;
                        thorn.velocity *= 0.5f;
                    }
                }

                if (Projectile.alpha >= 255)
                    Projectile.Kill();
            }
        }


        public override bool ShouldUpdatePosition() => false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 300); // 原版的酸性毒液效果
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (Projectile.ai[0] == TotalSegments)
                texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/NettleTip").Value;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
