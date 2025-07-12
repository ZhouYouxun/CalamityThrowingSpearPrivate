using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav
{
    internal class WulfrimJavPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 检测玩家是否手持 WulfrimJav 武器
            if (Player.HeldItem != null && Player.HeldItem.type == ModContent.ItemType<WulfrimJav>())
            {
                // 检测场上是否有存活的 WulfrimJavExtraPROJ
                bool exists = Main.projectile.Any(proj => proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<WulfrimJavExtraPROJ>());

                // 检测是否右键按下
                if (exists && Main.mouseRight && Player.controlUseTile)
                {
                    foreach (Projectile proj in Main.projectile.Where(proj => proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<WulfrimJavExtraPROJ>()))
                    {
                        // 在死亡前生成伤害为1.0倍的 WulfrimJavExtraExtraEXP
                        Projectile.NewProjectile(
                            proj.GetSource_FromThis(),
                            proj.Center,
                            Vector2.Zero, // 可改成方向向量以便飞出
                            ModContent.ProjectileType<WulfrimJavExtraExtraEXP>(),
                            proj.damage,
                            proj.knockBack,
                            proj.owner
                        );

                        // 立即杀死原弹幕
                        proj.Kill();
                    }
                }



                // 🌿 持续按住右键时在玩家周围生成同款特效并朝玩家飞行
                if (Main.mouseRight && Player.controlUseTile)
                {
                    for (int i = 0; i < 2; i++) // 控制生成数量
                    {
                        // 在玩家周围圆环随机位置生成
                        float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                        float radius = Main.rand.NextFloat(48f, 96f);
                        Vector2 spawnPos = Player.Center + angle.ToRotationVector2() * radius;

                        // 朝玩家飞行方向
                        Vector2 velocity = (Player.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);

                        // 生成 LimeGreen SparkParticle
                        Particle spark = new SparkParticle(
                            spawnPos,
                            velocity,
                            false,
                            30,
                            0.7f,
                            Color.LimeGreen * 0.9f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // 生成少量 Dust
                        if (Main.rand.NextBool(2))
                        {
                            Dust d = Dust.NewDustPerfect(
                                spawnPos,
                                DustID.GemEmerald,
                                velocity * 0.5f,
                                100,
                                Color.LimeGreen * 0.9f,
                                Main.rand.NextFloat(0.8f, 1.2f)
                            );
                            d.noGravity = true;
                        }
                    }
                }
            }


       

        }
    }
}