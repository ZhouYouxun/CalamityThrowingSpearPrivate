using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRodM : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private int lifeTimer = 0; // 替代 timeLeft 延时

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9000;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 在飞行期间绘制黑色残影
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.LimeGreen, 1);
            return false;
        }
        private float lemniscateAngle = 0f;
        private float lemniscateSpeed = 0.04f;
        private float outwardMultiplier = 120f;
        private int lemniscateCycle = 0; // 当前完整循环计数
        private bool clockwise = true; // 当前是否顺时针


        private float orbitAngle = 0f;
        private float orbitSpeed = MathHelper.TwoPi / 180f; // 每 180 帧旋转一圈，可调整快慢
        private float orbitRadius = 240f; // 飞行半径，可调整呼吸扩张
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
            float speedFactor = 1.01f + 0.005f * (float)Math.Sin(Main.GameUpdateCount * 0.2f);
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

            lifeTimer++;
            if (lifeTimer >= 100) // X帧后开始追踪
            {
                //Projectile targetProj = Main.projectile[targetIndex];
                //Vector2 toTarget = targetProj.Center - Projectile.Center;
                //float targetAngle = toTarget.ToRotation();
                //float currentAngle = Projectile.velocity.ToRotation();
                //float maxTurn = MathHelper.ToRadians(0.5f); // 每帧仅转动？°

                //float newAngle = currentAngle.AngleTowards(targetAngle, maxTurn);
                //float speed = Projectile.velocity.Length();
                //Projectile.velocity = newAngle.ToRotationVector2() * speed;

                Projectile targetProj = Main.projectile[targetIndex];

                //// 每帧推进相位角
                //lemniscateAngle += lemniscateSpeed;

                //// 检测是否完成完整圈（2π）
                //if (Math.Abs(lemniscateAngle) >= MathHelper.TwoPi)
                //{
                //    lemniscateAngle = 0f;
                //    lemniscateCycle++;

                //    // 每次完整循环后重新随机大小（原大小 ~ 5x 大小）
                //    outwardMultiplier = Main.rand.NextFloat(120f, 600f);

                //    // 每次完整循环后重新随机转向
                //    clockwise = Main.rand.NextBool();
                //    lemniscateSpeed = 0.04f * (clockwise ? 1f : -1f);
                //}

                //// Lemniscate 计算
                //float scale = 2f / (3f - (float)Math.Cos(2 * lemniscateAngle));
                //Vector2 lemniscateOffset = scale * new Vector2((float)Math.Cos(lemniscateAngle), (float)Math.Sin(2f * lemniscateAngle) / 2f);
                //Vector2 targetPosition = targetProj.Center + lemniscateOffset * outwardMultiplier;

                //// 平滑移动过去，保持速度曲线自然
                //Vector2 toTarget = targetPosition - Projectile.Center;
                //Projectile.velocity = toTarget * 0.2f; // 平滑度可调

                orbitAngle += orbitSpeed;

                if (orbitAngle >= MathHelper.TwoPi)
                {
                    orbitAngle -= MathHelper.TwoPi;
                    // 可在此处随机化 orbitRadius 实现呼吸扩张
                    orbitRadius = Main.rand.NextFloat(180f, 360f);
                }

                // 计算圆周位置
                Vector2 offset = orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 targetPosition = targetProj.Center + offset;

                // 平滑移动过去
                Vector2 toTarget = targetPosition - Projectile.Center;
                Projectile.velocity = toTarget * 0.2f; // 保留平滑自然的跟随感
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
