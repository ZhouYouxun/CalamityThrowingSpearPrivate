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
using CalamityMod.Particles;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC
{
    public class ViolenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/ViolenceC/ViolenceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        internal Player Owner => Main.player[Projectile.owner]; // 定义Owner，引用发射弹幕的玩家

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
            Projectile.penetrate = 2; // 允许两次伤害
            Projectile.timeLeft = 666;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3; // 额外更新次数改为2
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 将光效改为深红色
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            // 保留加速效果
            Projectile.velocity *= 1.01f;

            // 每隔一定时间产生轨迹
            if (Main.rand.NextBool(2))
            {
                float sideOffset = Main.rand.NextFloat(-1f, 1f);
                Vector2 trailPos = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * sideOffset;

                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                Color trailColor = Main.rand.NextBool() ? Color.Red : Color.DarkRed;

                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            if (target.Organic())
            {
                // 释放血液效果
                for (int i = 0; i < 15; i++) // 将数量增加到15
                {
                    // 随机选取喷射角度范围：正上方、左上30°或右上30°
                    float angleOffset = Main.rand.NextFloat(-30f, 30f);
                    float baseAngle = MathHelper.PiOver2; // 正上方为基础方向
                    float angle = baseAngle + MathHelper.ToRadians(angleOffset);

                    // 随机速度在一个范围内
                    float speed = Main.rand.NextFloat(6f, 12f);
                    Vector2 bloodVelocity = angle.ToRotationVector2() * speed;

                    // 创建粒子特效
                    var blood = new BloodParticle(target.Center, bloodVelocity, 30, 1f, Color.Red);
                    GeneralParticleHandler.SpawnParticle(blood);
                }
            }
            else
            {
                // 释放火花效果
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                    var spark = new SparkParticle(target.Center, sparkVelocity, true, 30, 1f, Color.Orange);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }




            //int bossCount = 0;

            //// 遍历所有 NPC，计算场上有效的 Boss 数量
            //for (int i = 0; i < Main.npc.Length; i++)
            //{
            //    NPC npc = Main.npc[i];

            //    // 检查 NPC 是否活跃且为 Boss
            //    if (npc.active && npc.boss)
            //    {
            //        // 如果 NPC 是蠕虫类 Boss 的一部分（除了头部），跳过计算
            //        if (npc.realLife != -1 && Main.npc[npc.realLife].type == npc.type && npc.whoAmI != npc.realLife)
            //        {
            //            continue; // 排除蠕虫 Boss 的身体部分，只有头部计算为一个 Boss
            //        }

            //        // 如果 NPC 是 阿瑞斯的身体，将其计作 5 个 Boss
            //        //if (npc.type == ModContent.NPCType<AresBody>())
            //        //{
            //        //    bossCount += 5; // 阿瑞斯 计作 5 个 Boss
            //        //}
            //        //else
            //        {
            //            bossCount++; // 其他普通 Boss 正常计数
            //        }
            //    }
            //}

            //// 测试型输出
            //Main.NewText($"场上目前有 {bossCount} 个 boss", Color.LightBlue);


            // 计算场上有效的 Boss 数量，排除蠕虫身体部分
            int bossCount = 2;

            foreach (NPC npc in Main.npc)
            {
                // 检查 NPC 是否是有效的 Boss
                if (npc.active && (npc.boss || npc.type == ModContent.NPCType<SupremeCataclysm>() ||
                                   npc.type == ModContent.NPCType<SupremeCatastrophe>() ||
                                   npc.type == ModContent.NPCType<SoulSeekerSupreme>() ||
                                   npc.type == ModContent.NPCType<BrimstoneHeart>()))
                {
                    // 对于蠕虫类 Boss，仅计入头部部分
                    if (npc.realLife != -1 && npc.whoAmI != npc.realLife)
                        continue;

                    bossCount++;

                    // 特殊处理：增加特定 NPC 计数权重
                    //if (npc.type == ModContent.NPCType<AresBody>())
                    //{
                        //bossCount += 3; // 将 AresBody 视作额外的多个 Boss
                    //}
                }
            }


            // 5% 概率翻倍 Boss 数量并触发五角星特效
            if (Main.rand.NextFloat() < 1f)
            {
                bossCount += 3;

                // 五角星粒子特效
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f;
                    float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f;
                    Vector2 start = angle.ToRotationVector2();
                    Vector2 end = nextAngle.ToRotationVector2();
                    for (int j = 0; j < 40; j++)
                    {
                        Dust starDust = Dust.NewDustPerfect(Projectile.Center, 267);
                        starDust.scale = 2.5f;
                        starDust.velocity = Vector2.Lerp(start, end, j / 40f) * 16f;
                        starDust.color = Color.Crimson;
                        starDust.noGravity = true;
                    }
                }
            }

            // 发射弹幕，随机方向，每个方向间隔 360 度 / bossCount
            for (int i = 0; i < bossCount; i++)
            {
                // 随机选择一个 360 度的方向
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 randomDirection = randomAngle.ToRotationVector2();

                // 从玩家位置发射弹幕
                Projectile.NewProjectile(
                    Owner.GetSource_FromThis(),
                    Owner.Center,
                    randomDirection * 12f, // 设置速度
                    ModContent.ProjectileType<ViolenceJavLight>(),
                    (int)(Projectile.damage * 0.333f),
                    0,
                    Main.myPlayer
                );
            }



            //// 获取主弹幕消失时的位置向量
            //Vector2 mainProjectileDirection = (Projectile.Center - Owner.Center).SafeNormalize(Vector2.Zero);

            //// 计算两个固定弹幕的方向：一个朝向主弹幕消失的方向，另一个是相反方向
            //Vector2 direction1 = mainProjectileDirection;
            //Vector2 direction2 = -mainProjectileDirection;

            //// 从玩家处发射两条固定弹幕
            //Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, direction1 * 12f, ModContent.ProjectileType<ViolenceJavLight>(), (int)(Projectile.damage * 0.75f), 0, Main.myPlayer);
            //Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, direction2 * 12f, ModContent.ProjectileType<ViolenceJavLight>(), (int)(Projectile.damage * 0.75f), 0, Main.myPlayer);

            //// 处理额外的追踪弹幕（仅针对头部）
            //if (bossCount > 0)
            //{
            //    for (int i = 0; i < bossCount; i++)
            //    {
            //        NPC targetBoss = Main.npc.FirstOrDefault(npc => npc.boss && npc.active && (npc.realLife == -1 || Main.npc[npc.realLife] == npc));
            //        if (targetBoss != null)
            //        {
            //            // 获取从玩家到BOSS头部的方向
            //            Vector2 bossDirection = (targetBoss.Center - Owner.Center).SafeNormalize(Vector2.Zero);
            //            Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, bossDirection * 12f, ModContent.ProjectileType<ViolenceJavLight>(), (int)(Projectile.damage * 0.75f), 0, Main.myPlayer);
            //        }
            //    }
            //}

            //// 50% 概率释放五角星粒子特效
            //if (Main.rand.NextFloat() < 0.5f)
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f;
            //        float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f;
            //        Vector2 start = angle.ToRotationVector2();
            //        Vector2 end = nextAngle.ToRotationVector2();
            //        for (int j = 0; j < 40; j++)
            //        {
            //            Dust starDust = Dust.NewDustPerfect(Projectile.Center, 267);
            //            starDust.scale = 2.5f;
            //            starDust.velocity = Vector2.Lerp(start, end, j / 40f) * 16f;
            //            starDust.color = Color.Crimson;
            //            starDust.noGravity = true;
            //        }
            //    }
            //}


        }

        public override void OnKill(int timeLeft)
        {

            // 生成亮红色和红橙色的烟雾粒子特效(这个不要了，这个留给灾影系列)
            for (int i = 0; i <= 20; i++)
            {
                // 随机方向和速度
                Vector2 velocity = new Vector2(2.5f, 2.5f).RotatedByRandom(MathHelper.ToRadians(360)) * Main.rand.NextFloat(0.2f, 1.5f);

                // 创建亮红色和红橙色的粒子
                Color particleColor = Main.rand.NextBool() ? Color.Red : Color.OrangeRed;
                Particle smoke = new DesertProwlerSkullParticle(Projectile.Center, velocity, particleColor * 0.8f, particleColor, Main.rand.NextFloat(0.5f, 1.0f), 150);

                // 生成粒子
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/SCalDash"));
        }




    }
}
