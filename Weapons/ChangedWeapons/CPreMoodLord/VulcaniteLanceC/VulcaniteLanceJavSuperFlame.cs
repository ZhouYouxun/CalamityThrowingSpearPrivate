using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavSuperFlame : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.CPreMoodLord";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.boss)
            {
                modifiers.FinalDamage *= 0.75f; // 如果目标是 Boss，造成 0.75 倍伤害
            }
            else
            {
                modifiers.FinalDamage *= 1.5f; // 如果目标不是 Boss，造成 1.5 倍伤害
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/火山音效") with { Volume = 2.0f, Pitch = 0.0f }, Projectile.Center);
        }
        public override void AI()
        {
            // 顺时针旋转的角度（基础旋转角）
            float rotationSpeed = MathHelper.ToRadians(7f);
            Projectile.ai[0] += rotationSpeed;

            // 定义扇形起始角度、跨度与间隔
            float baseAngle = Projectile.ai[0];
            float sectorAngle = MathHelper.ToRadians(60);
            float gapAngle = MathHelper.ToRadians(60);

            // === 🔥保留原始三旋粒子喷发（削弱版本）===
            for (int i = 0; i < 3; i++)
            {
                float startAngle = baseAngle + i * (sectorAngle + gapAngle);
                for (int j = 0; j < Main.rand.Next(7, 16); j++) // 数量减半
                {
                    float randomAngle = startAngle + Main.rand.NextFloat(-sectorAngle / 2f, sectorAngle / 2f);

                    // 均匀角速度：改成单位速度后乘随机长度，避免横纵不一致
                    Vector2 baseDir = randomAngle.ToRotationVector2().SafeNormalize(Vector2.UnitY);
                    float speed = Main.rand.NextFloat(4.5f, 10f);

                    // ⏺ 手动修正方向分量【改主意了，这个确实不合理】
                    baseDir.X *= 1.0f;  // 水平削弱
                    baseDir.Y *= 1.0f;  // 垂直增强

                    Vector2 velocity = baseDir * speed;


                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        Main.rand.Next(new int[] { 55, 35, 174 }),
                        velocity
                    );
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.85f, 2.05f);
                    d.alpha = 217;
                }
            }

            // === 🌐新增粒子圆环喷发（随机从边缘往外）===
            float circleRadius = 150f;
            int ringParticles = 8; // 每帧喷出几个边缘粒子

            for (int i = 0; i < ringParticles; i++)
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * circleRadius;

                // 往外喷
                Vector2 outward = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustPerfect(
                    edgePos,
                    Main.rand.Next(new int[] { 55, 35, 174 }),
                    outward
                );
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.85f, 2.05f);
                d.alpha = 217;
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300); // 给敌人添加燃烧减益效果
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
