using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ELPEntropyEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        public override void AI()
        {
            float lights = (float)Main.rand.Next(90, 111) * 0.01f;
            lights *= Main.essScale;
            Lighting.AddLight(Projectile.Center, 5f * lights, 1f * lights, 4f * lights);

            float projTimer = 25f;
            if (Projectile.ai[0] > 180f)
            {
                projTimer -= (Projectile.ai[0] - 180f) / 2f;
            }
            if (projTimer <= 0f)
            {
                projTimer = 0f;
                Projectile.Kill();
            }
            projTimer *= 0.7f;
            Projectile.ai[0] += 4f;
            int timerCounter = 0;

            while ((float)timerCounter < projTimer)
            {
                float rando1 = (float)Main.rand.Next(-40, 41);
                float rando2 = (float)Main.rand.Next(-40, 41);
                float rando3 = (float)Main.rand.Next(12, 36);
                float randoAdjust = (float)Math.Sqrt((double)(rando1 * rando1 + rando2 * rando2));
                randoAdjust = rando3 / randoAdjust;
                rando1 *= randoAdjust;
                rando2 *= randoAdjust;


                // 更改粒子类型为 191 和 240
                int randomDust = Main.rand.Next(2);
                if (randomDust == 0)
                {
                    randomDust = 191; // 使用粒子类型 191
                }
                else
                {
                    randomDust = 240; // 使用粒子类型 240
                }

                // 创建粒子效果
                int EXPLODE = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, randomDust, 0f, 0f, 100, Color.Black, 1.5f); // 大小缩减25%，原来是2f，现在是1.5f
                Main.dust[EXPLODE].noGravity = true;
                Main.dust[EXPLODE].position.X = Projectile.Center.X;
                Main.dust[EXPLODE].position.Y = Projectile.Center.Y;
                // 将位置偏移范围调整为原来的3/5
                Main.dust[EXPLODE].position.X += (float)Main.rand.Next(-90, 91); // 假设原先为-150到150，现在缩小到-90到90
                Main.dust[EXPLODE].position.Y += (float)Main.rand.Next(-90, 91); // 同理，Y方向也缩小到-90到90
                Main.dust[EXPLODE].velocity.X *= 0.6f; // 缩减速度到60%以缩短射程
                Main.dust[EXPLODE].velocity.Y *= 0.6f; // 同理缩减Y方向速度

                timerCounter++;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
            //target.AddBuff(ModContent.BuffType<GalvanicCorrosion>(), 300); // 电偶腐蚀
        }
    }
}
