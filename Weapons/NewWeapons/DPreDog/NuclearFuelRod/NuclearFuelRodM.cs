using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    internal class NuclearFuelRodM : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";


        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * 1.0f);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 中等华丽飞行特效
            if (Main.rand.NextBool(2))
            {
                int dustID = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 107);
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                Main.dust[dustID].scale = Main.rand.NextFloat(1f, 1.5f);
                Main.dust[dustID].color = Color.LimeGreen;
            }

            // 速度加快 + 波动快慢
            float speedFactor = 1.01f + 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.2f);
            Projectile.velocity *= speedFactor;

            // 查找场上的唯一 NuclearFuelRodPROJ
            int targetIndex = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<NuclearFuelRodPROJ>())
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex == -1)
            {
                // 没有找到父弹幕，自杀
                Projectile.Kill();
                return;
            }

            // 延时20帧后开始追踪
            if (Projectile.timeLeft < 280)
            {
                Projectile targetProj = Main.projectile[targetIndex];
                Vector2 toTarget = targetProj.Center - Projectile.Center;
                float targetAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float maxTurn = MathHelper.ToRadians(1.5f); // 转向限制，保证追不准

                // 差值角度计算并限制
                float newAngle = currentAngle.AngleTowards(targetAngle, maxTurn);
                float speed = Projectile.velocity.Length();

                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }




        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                int idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
                idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
            }
        }


    }
}
