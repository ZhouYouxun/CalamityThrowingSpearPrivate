using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Projectiles.Melee;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    public class GoldplumeJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/GoldplumeSpearC/GoldplumeJav";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
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
            Projectile.penetrate = 1; // 只允许一次伤害
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
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加白色光源
            Lighting.AddLight(Projectile.Center, Color.WhiteSmoke.ToVector3() * 0.55f);

            // 弹幕逐渐加速
            Projectile.velocity.X *= 1.005f;
            Projectile.velocity.Y -= 0.15f;

            // 释放天蓝色烟雾特效
            Projectile.ai[0] += 1f; // 主要用于烟雾效果的计时器
            if (Projectile.ai[0] > 6f)
            {
                for (int d = 0; d < 5; d++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarfish, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f)];
                    dust.velocity = Vector2.Zero;
                    dust.position -= Projectile.velocity / 5f * d;
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.noLight = true;
                }
            }

            // 新增单独的计时器用于召唤Feather弹幕
            if (Projectile.ai[1] % 20 == 0 && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    NPC target = null;
                    float maxDistance = 2400f; // 半径150个方块范围

                    // 寻找最近的敌人
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.CanBeChasedBy() && !npc.friendly)
                        {
                            float distanceToNPC = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distanceToNPC < maxDistance)
                            {
                                maxDistance = distanceToNPC;
                                target = npc;
                            }
                        }
                    }

                    // 发射Feather弹幕
                    Vector2 featherVelocity;
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        featherVelocity = direction * Projectile.velocity.Length() * 2.5f; // 速度为本体的两倍
                    }
                    else
                    {
                        featherVelocity = Projectile.velocity * 2f;
                    }

                    Vector2 spawnPosition = Projectile.Center;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, featherVelocity,
                        ModContent.ProjectileType<Feather>(), (int)(Projectile.damage * 0.275), Projectile.knockBack, Projectile.owner, Projectile.ArmorPenetration = 10);
                }
            }

            // 增加计时器，用于控制Feather弹幕生成频率
            Projectile.ai[1] += 1f;
        }


        public override void OnKill(int timeLeft)
        {
            // 在弹幕死亡时往上方和下方随机三个方向发射Feather弹幕
            for (int i = 0; i < 3; i++)
            {
                // 发射上方的羽毛弹幕，左右各45度范围内
                float angleUp = MathHelper.ToRadians(45) * (i - 1);
                Vector2 featherVelocityUp = new Vector2(0, -1).RotatedBy(angleUp) * Projectile.velocity.Length();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, featherVelocityUp,
                    ModContent.ProjectileType<Feather>(), (int)(Projectile.damage * 0.33), Projectile.knockBack, Projectile.owner, Projectile.ArmorPenetration = 10);

                // 发射下方的羽毛弹幕，左右各45度范围内
                float angleDown = MathHelper.ToRadians(45) * (i - 1);
                Vector2 featherVelocityDown = new Vector2(0, 1).RotatedBy(angleDown) * Projectile.velocity.Length();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, featherVelocityDown,
                    ModContent.ProjectileType<Feather>(), (int)(Projectile.damage * 0.33), Projectile.knockBack, Projectile.owner, Projectile.ArmorPenetration = 10);

                // 释放天蓝色的粒子特效（针对上方）
                for (int d = 0; d < 10; d++)
                {
                    Vector2 dustVelocityUp = featherVelocityUp * 0.2f; // 粒子速度较慢
                    Dust dustUp = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.StarRoyale, dustVelocityUp.X, dustVelocityUp.Y, 100, default, 1f)];
                    dustUp.noGravity = true;
                    dustUp.scale = 0.65f;
                    dustUp.noLight = true;
                }

                // 释放天蓝色的粒子特效（针对下方）
                for (int d = 0; d < 10; d++)
                {
                    Vector2 dustVelocityDown = featherVelocityDown * 0.2f; // 粒子速度较慢
                    Dust dustDown = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.StarRoyale, dustVelocityDown.X, dustVelocityDown.Y, 100, default, 1f)];
                    dustDown.noGravity = true;
                    dustDown.scale = 0.65f;
                    dustDown.noLight = true;
                }
            }
        }












    }
}
