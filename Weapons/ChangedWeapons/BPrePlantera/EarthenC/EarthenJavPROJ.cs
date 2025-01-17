using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Projectiles.Ranged;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC
{
    public class EarthenJavPROJ : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/BPrePlantera/EarthenC/EarthenJav";
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
            Projectile.penetrate = 7; // 设置为7次穿透
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 模拟重力效果
            if (Projectile.velocity.Y < 24f)
            {
                Projectile.velocity.Y += 0.1f; // Y 轴速度逐渐增加
            }

            // 飞行时留下卡其色的烟雾特效
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 6f)
            {
                for (int d = 0; d < 5; d++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f)];
                    dust.velocity = Vector2.Zero;
                    dust.position -= Projectile.velocity / 5f * d;
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.noLight = true;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--; // 每次反弹减少一次穿透次数
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y; // 反弹效果
                }

                // 发射 FossilShard
                for (int i = 0; i < Main.rand.Next(5, 7); i++)
                {
                    float rotation = MathHelper.ToRadians(45);
                    Vector2 velocity = Projectile.velocity.RotatedByRandom(rotation) * Main.rand.NextFloat(0.8f, 1.2f) * 1.4f; // 增加速度到 1.4 倍
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, velocity,
                        ModContent.ProjectileType<FossilShard>(), (int)(Projectile.damage * 0.5), Projectile.knockBack, Projectile.owner);
                }


                // 生成卡其色和泥土颜色粒子
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0, 0, 150, default, 1.2f);
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0, 0, 150, default, 1.2f);
                }

                // 播放音效
                SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 粉碎
        }

    }
}
