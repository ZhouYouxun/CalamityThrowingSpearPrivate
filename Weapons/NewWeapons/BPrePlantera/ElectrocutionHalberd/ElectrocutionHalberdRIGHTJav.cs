using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    internal class ElectrocutionHalberdRIGHTJav : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ElectrocutionHalberd/ElectrocutionHalberdJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50 * Projectile.extraUpdates;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 3; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //Projectile.scale = 0.7f; //
            Projectile.alpha = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0.7f;
        }
        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(25);

            // Lighting - 添加深蓝色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);

            // 添加渐变透明度
            Projectile.alpha = Math.Min(Projectile.alpha + 2, 255);

            // 头部特效
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.25f);
            CreateDustEffect(headPosition, Projectile.velocity, 146, 50);

            // 中心特效146【钛金】，50【精金】
            CreateCenterDust(Projectile.Center, 146, 50);
        }

        private void CreateDustEffect(Vector2 position, Vector2 velocity, int dustType1, int dustType2)
        {
            // 生成粒子特效（两侧散开）
            for (int i = 0; i < 2; i++)
            {
                int dustType = (i % 2 == 0) ? dustType1 : dustType2;
                Vector2 offset = (i == 0) ? -Vector2.UnitX * 3f : Vector2.UnitX * 3f;

                Dust dust = Dust.NewDustPerfect(position + offset, dustType, -velocity * 0.1f, 150, default, Main.rand.NextFloat(1.55f, 1.95f));
                dust.noGravity = true;
            }
        }

        private void CreateCenterDust(Vector2 position, int dustType1, int dustType2)
        {
            // 中心生成粒子特效（左右偏移）
            for (int i = 0; i < 2; i++)
            {
                int dustType = (i % 2 == 0) ? dustType1 : dustType2;
                Vector2 offset = (i == 0) ? -Vector2.UnitX * 5f : Vector2.UnitX * 5f;

                Dust dust = Dust.NewDustPerfect(position + offset, dustType, Vector2.Zero, 150, default, Main.rand.NextFloat(1.55f, 1.95f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.25f);

            // 生成三发子弹
            for (int i = 0; i < 3; i++)
            {
                // 随机角度偏移（-20°到20°之间）
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                Vector2 modifiedVelocity = Projectile.velocity.RotatedBy(angleOffset);

                // 随机初始速度调整（0.85倍到1.25倍之间）
                float speedMultiplier = Main.rand.NextFloat(0.85f, 1.25f);
                modifiedVelocity *= speedMultiplier;

                // 随机伤害倍率（0.95倍到1.5倍之间）
                float damageMultiplier = Main.rand.NextFloat(0.6f, 1.1f);

                // 创建散射弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    headPosition,
                    modifiedVelocity,
                    ModContent.ProjectileType<ElectrocutionHalberdRIGHT>(),
                    (int)(Projectile.damage * damageMultiplier),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
        }
    }
}
