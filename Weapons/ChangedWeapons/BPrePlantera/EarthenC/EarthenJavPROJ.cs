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
using CalamityMod.Projectiles.Ranged;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.GameContent.Drawing;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

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
        private float outlineFlash = 0f;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float rotation = stuck ? lockedRotation : (Projectile.velocity.ToRotation() + MathHelper.PiOver4);
            SpriteEffects direction = SpriteEffects.None;

            if (stuck)
            {
                // ✨ 发光描边（黄光脉冲）
                float chargeOffset = 2f + 4f * outlineFlash; // 脉冲扩张
                Color edgeColor = Color.Gold * 0.5f * outlineFlash;
                edgeColor.A = 0;

                for (int i = 0; i < 6 + (int)(8 * outlineFlash); i++) // 脉冲数量随值变化
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + offset, null, edgeColor, rotation, origin, Projectile.scale, direction, 0f);
                }
            }
            else
            {
                // 非扎入状态使用普通拖影
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }

            // 主体本体绘制
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7; // 设置为7次穿透
            Projectile.timeLeft = 420;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        private bool stuck = false;
        private float lockedRotation;
        private int shardCooldown = 0;
        private int facingDirection = 1;
        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);


            if (!stuck)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                if (Math.Abs(Projectile.velocity.X) > 0.1f) // 有效速度才记录
                    facingDirection = Projectile.velocity.X < 0f ? -1 : 1;
            }
            {
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
                int smokeRate = 2; // 每X帧喷一次
                if (Projectile.timeLeft % smokeRate == 0)
                {
                    // 🌀 角度偏摆（雨刮器形式）
                    float time = Projectile.timeLeft / 50f; // 控制摆动频率
                    float swingAngle = MathF.Sin(time) * MathHelper.ToRadians(40f); // 左右摇动 ±?0°

                    Vector2 direction = -Vector2.UnitY.RotatedBy(swingAngle); // 往上摆动方向喷射
                    Vector2 velocity = direction * Main.rand.NextFloat(3.5f, 7f);

                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center + direction * 4f + Main.rand.NextVector2Circular(2f, 2f), // 轻微抖动
                        velocity,
                        Color.SandyBrown,
                        Main.rand.Next(30, 45),
                        Main.rand.NextFloat(0.5f, 0.7f),
                        0.5f,
                        Main.rand.NextFloat(-0.01f, 0.01f),
                        false
                    );

                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                // 🌋 飞行期间猛烈土石尘尾
                if (Main.rand.NextBool(1)) // 每帧
                {
                    int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType);
                    d.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                    d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    d.noGravity = Main.rand.NextBool(3); // 部分无重力
                }

                //// 深棕色 SparkParticle 模拟碎石飞溅
                //if (Main.rand.NextBool(6))
                //{
                //    Particle spark = new SparkParticle(
                //        Projectile.Center,
                //        Main.rand.NextVector2Circular(2f, 2f),
                //        false,
                //        30,
                //        1.0f,
                //        new Color(90, 60, 40) // 深棕色
                //    );
                //    GeneralParticleHandler.SpawnParticle(spark);
                //}
            }
            // 模拟重力效果
            if (Projectile.velocity.Y < 24f)
            {
                //Projectile.velocity.Y += 0.1f; // Y 轴速度逐渐增加
            }


            if (stuck)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = lockedRotation;

                shardCooldown++;
                if (shardCooldown % 12 == 0)
                {
                    int shardCount = 12;
                    float spacing = 4 * 16f;
                    int index = (shardCooldown / 12) - 1;

                    if (index < shardCount)
                    {
                        // 判断当前面朝方向（基于 rotation）
                        float rot = Projectile.rotation - MathHelper.PiOver4;
                        bool facingRight = Math.Abs(MathHelper.WrapAngle(rot)) < MathHelper.PiOver2;

                        // 向量方向：右或左
                        Vector2 direction = facingRight ? Vector2.UnitX : -Vector2.UnitX;

                        // 位置偏移：第一个在前面，其余依次扩展
                        Vector2 shardPos = Projectile.Center + direction * (index + 1) * spacing;

                        // 初始速度为 0
                        Vector2 shardVel = Vector2.Zero;

                        int shard = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            shardPos,
                            shardVel,
                            ModContent.ProjectileType<EarthenJavSHARD>(),
                            (int)(Projectile.damage * 1f),
                            0f,
                            Projectile.owner,
                            0f, // ai[0] 可用作其他功能
                            Projectile.velocity.X < 0f ? -1f : 1f // ai[1]：-1 向左，+1 向右
                        );

                        if (shard >= 0 && shard < Main.maxProjectiles && Main.projectile[shard].ModProjectile is EarthenJavSHARD shardProj)
                        {
                            shardProj.spawnDirection = facingDirection;
                        }

                        outlineFlash = 1f; // 每次发射碎片时高亮描边
                    }
                }




            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }


            {
                if (outlineFlash > 0f)
                    outlineFlash -= 0.05f;
            }

        }



    

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                stuck = true;
                lockedRotation = Projectile.rotation;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 160;

                {
                    {
                        // 粉碎土石尘暴爆发
                        for (int i = 0; i < 25; i++)
                        {
                            int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                            Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                            d.velocity = Main.rand.NextVector2Circular(6f, 6f);
                            d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                            d.noGravity = Main.rand.NextBool();
                        }

                        // 重型烟雾环绕
                        for (int i = 0; i < 8; i++)
                        {
                            Particle smoke = new HeavySmokeParticle(
                                Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                                Main.rand.NextVector2Circular(1.5f, 1.5f),
                                Color.SandyBrown,
                                Main.rand.Next(25, 40),
                                Main.rand.NextFloat(0.6f, 0.9f),
                                0.8f,
                                Main.rand.NextFloat(-0.02f, 0.02f),
                                false
                            );
                            GeneralParticleHandler.SpawnParticle(smoke);
                        }

                        // 强烈“咚”声
                        SoundEngine.PlaySound(SoundID.Item70, Projectile.position);

                        // 增强震屏
                        float shakePower = 5f; // 强烈震动
                        float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                    }
                }
            }
            return false;

            //if (!hasCollided)
            //{
            //    hasCollided = true;
            //    disableDraw = true;
            //    beginSpawning = true;

            //    Projectile.friendly = false;
            //    Projectile.velocity = Vector2.Zero;
            //    Projectile.tileCollide = false;
            //    Projectile.timeLeft = 70;
            //    //Projectile.alpha = 255;
            //    Projectile.velocity = new Vector2(0f, -6f); // 给一个向上的初速（可调）


            //    // 朝向判定（只允许水平两方向）
            //    spawnDirection = cachedDirection.X < 0 ? -Vector2.UnitX : Vector2.UnitX;

             
            //}
            //return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 保留原有debuff

            // === 岩石定向爆裂 ===
            int shardCount = 4;
            for (int i = 0; i < shardCount; i++)
            {
                // 均匀角度 + 随机扰动
                float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 9f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<FossilShard>(),
                    (int)(Projectile.damage * 0.6f),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            // 🌋 视觉效果：石屑冲击波
            for (int d = 0; d < 20; d++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    Main.rand.Next(new int[] { DustID.Stone, DustID.Sand, DustID.Dirt }),
                    Main.rand.NextVector2Circular(4f, 4f),
                    100,
                    default,
                    Main.rand.NextFloat(1.2f, 1.6f)
                );
                dust.noGravity = true;
            }

            // 💥 爆裂音效
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
        }






        public override void OnKill(int timeLeft)
        {

            // 爆炸弹幕：X个 EarthenJavSHARD 弹片
            //for (int i = 0; i < 3; i++)
            //{
            //    Vector2 shardVelocity = Main.rand.NextVector2Circular(5f, 5f); // 随机方向
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        shardVelocity,
            //        ModContent.ProjectileType<EarthenJavSHARD>(),
            //        (int)(Projectile.damage * 0.5f),
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}

            {
                // 大量泥土尘爆发
                for (int i = 0; i < 60; i++)
                {
                    int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.Sand });
                    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                    d.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    d.scale = Main.rand.NextFloat(1.3f, 2.2f);
                    d.noGravity = Main.rand.NextBool();
                }

                // 土石碎片飞散（SparkParticle）
                for (int i = 0; i < 12; i++)
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(10f, 10f),
                        false,
                        40,
                        1.2f,
                        new Color(120, 72, 40)
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 强爆炸声
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.8f }, Projectile.Center);

                // 强震动
                float shakePower = 6f;
                float distanceFactor = Utils.GetLerpValue(1200f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            }
            // 可选：爆炸声
            //SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/钻地武器进入地面的音效"), Projectile.Center);

        }



    }
}
