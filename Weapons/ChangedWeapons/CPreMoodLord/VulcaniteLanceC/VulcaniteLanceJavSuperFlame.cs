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
            // 顺时针旋转的角度
            float rotationSpeed = MathHelper.ToRadians(7f); // 每帧旋转 X 度
            Projectile.ai[0] += rotationSpeed;

            // 定义扇形的起始角度和间隔
            float baseAngle = Projectile.ai[0];
            float sectorAngle = MathHelper.ToRadians(60); // 每个扇形 60 度
            float gapAngle = MathHelper.ToRadians(60); // 扇形之间的间隔

            // 循环生成三个扇形区域的粒子
            for (int i = 0; i < 3; i++)
            {
                float startAngle = baseAngle + i * (sectorAngle + gapAngle);
                for (int j = 0; j < Main.rand.Next(15, 31); j++) // 每帧生成 X 个粒子
                {
                    float randomAngle = startAngle + Main.rand.NextFloat(-sectorAngle / 2, sectorAngle / 2); // 随机角度偏移
                    Vector2 velocity = randomAngle.ToRotationVector2() * Main.rand.NextFloat(5.5f, 24.1f); // 粒子速度

                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        Main.rand.Next(new int[] { 55, 35, 174 }), // 混合使用粒子类型
                        velocity
                    );
                    dust.noGravity = true; // 不受重力影响
                    dust.scale = Main.rand.NextFloat(1.85f, 2.05f); // 大小随机
                    dust.alpha = 217; // 透明度设置为 0.85
                }
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
