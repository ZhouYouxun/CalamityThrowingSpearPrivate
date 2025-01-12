using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC
{
    public class TyphonsGreedJavBubble : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";

        private bool hasLockedOn = false;  // 是否已经开始追踪
        private NPC target;  // 被追踪的目标
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false; // 不允许与方块碰撞
            Projectile.ignoreWater = true; // 无视水
            Projectile.aiStyle = 0; // 自定义AI
        }

        public override void AI()
        {
            // 增加计时器，用于开始追踪的延迟判断
            Projectile.ai[0]++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // 动画处理
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }

            // 追踪逻辑：30帧后开始寻找目标
            if (!hasLockedOn)
            {
                NPC closestNPC = FindClosestNPC(2000f);
                if (closestNPC != null && Projectile.ai[0] > 30f)
                {
                    hasLockedOn = true;
                    target = closestNPC;
                }
            }

            // 如果找到目标，则追踪
            if (hasLockedOn && target != null && target.active)
            {
                Projectile.velocity = Projectile.SuperhomeTowardsTarget(target, 12f, 15f);
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // 保持视觉效果
            Lighting.AddLight(Projectile.Center, 0f, 0.1f, 0.7f); // 蓝色光效
        }

        // Method to find the closest NPC within a specified range
        private NPC FindClosestNPC(float maxRange)
        {
            NPC closestNPC = null;
            float closestDistance = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile))
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            return closestNPC;
        }

        // Drawing effects remain unchanged
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)),
                Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }

        // On death, maintain original effects
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 64;
            Projectile.position.X -= (float)(Projectile.width / 2);
            Projectile.position.Y -= (float)(Projectile.height / 2);

            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
            }

            for (int j = 0; j < 6; j++)
            {
                int bubblyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedsWingsRun, 0f, 0f, 0, new Color(0, 255, 255), 2.5f);
                Main.dust[bubblyDust].noGravity = true;
                Main.dust[bubblyDust].velocity *= 3f;
                bubblyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedsWingsRun, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
                Main.dust[bubblyDust].velocity *= 2f;
                Main.dust[bubblyDust].noGravity = true;
            }
        }










    }
}
