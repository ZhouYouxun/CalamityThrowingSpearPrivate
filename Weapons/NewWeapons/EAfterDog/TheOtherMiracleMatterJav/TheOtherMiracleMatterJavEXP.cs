using CalamityMod.Buffs.DamageOverTime;
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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TheOtherMiracleMatterJav
{
    public class TheOtherMiracleMatterJavEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            if (Main.getGoodWorld)
            {
                Projectile.width = 5500;
                Projectile.height = 5500;
            }
            else
            {
                Projectile.width = 175;
                Projectile.height = 175;
            }
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            // 播放斩击音效
            //SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
            //SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBigHit"));

            // 新增闪电召唤逻辑
            int lightningDamage = (int)(Projectile.damage * 0.42f); // 可调整的伤害倍率

            // 判断是否为 getGoodWorld 世界
            int lightningCount = Main.getGoodWorld ? 5 : 1; // 如果是 getGoodWorld，则生成 5 道闪电，否则生成 1 道

            for (int i = 0; i < lightningCount; i++) // 根据 lightningCount 动态生成闪电数量
            {
                // 计算闪电的生成位置和速度
                Vector2 lightningSpawnPosition = target.Center - Vector2.UnitY.RotatedByRandom(0.36f) * Main.rand.NextFloat(960f, 1020f);
                Vector2 lightningShootVelocity = (target.Center - lightningSpawnPosition).SafeNormalize(Vector2.UnitY) * 14f;

                // 生成闪电投射物
                int lightning = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    lightningSpawnPosition,
                    lightningShootVelocity,
                    ModContent.ProjectileType<TheOtherMiracleMatterJavExoLightningBolt>(), // 闪电弹幕
                    lightningDamage,
                    0f,
                    Projectile.owner
                );

                // 配置闪电的自定义 AI 参数
                if (Main.projectile.IndexInRange(lightning))
                {
                    Main.projectile[lightning].ai[0] = lightningShootVelocity.ToRotation(); // 方向
                    Main.projectile[lightning].ai[1] = Main.rand.Next(100); // 随机化其他参数
                }
            }

        }

        public override void AI()
        {

        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
