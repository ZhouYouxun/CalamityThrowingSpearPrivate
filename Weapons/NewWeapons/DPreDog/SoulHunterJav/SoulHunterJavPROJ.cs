
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav
{
    public class SoulHunterJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/SoulHunterJav/SoulHunterJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        private int phase = 1; // 阶段控制
        private bool returning = false; // 是否处于回收状态
        private int effectCounter = 0; // 粒子效果计数器

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (phase == 1 || returning)
            {
                float chargeOffset = 3f;
                Color chargeColor = Color.DarkBlue * 0.6f;
                chargeColor.A = 0;

                for (int i = 0; i < 8; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            else if (phase == 3)
            {
                for (int i = 0; i < 7; i++)
                {
                    Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/SoulHunterJav/SoulHunterJav").Value;
                    Vector2 rotationalOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 8f).ToRotationVector2();
                    rotationalOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);
                    Main.EntitySpriteDraw(glowTexture, Projectile.Center - Main.screenPosition + rotationalOffset, null, Color.AliceBlue, rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
                }
            }
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 允许-1次伤害
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 5; // 无敌帧冷却时间
        }

        public override void AI()
        {
            // 调整武器的指向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 光照效果
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

            if (phase == 1)
            {
                // 阶段1：快速减速并释放粒子
                Projectile.velocity *= 0.96f;
                effectCounter++;
                if (effectCounter % 5 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * (i / 3f) + Main.GlobalTimeWrappedHourly * 2) * 8;
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? 80 : 172, offset * 0.1f, 0, Color.Cyan, 1.2f);
                        dust.noGravity = true;
                        if (dust.type == 80)
                            dust.alpha = 180;
                    }
                }
                if (effectCounter >= 38 && effectCounter < 40)
                {
                    for (int j = 0; j < 3; j++) // 连续产生三次特效
                    {
                        float baseRotation = Projectile.velocity.ToRotation(); // 基础旋转角度

                        // 冲击波特效，保持与弹幕垂直（不需要修改）
                        for (int i = 0; i < 3; i++)
                        {
                            Particle pulse = new DirectionalPulseRing(
                                Projectile.Center,
                                Vector2.Zero,
                                Color.DarkBlue, // 深蓝色
                                new Vector2(1.2f + 0.2f * j, 3f + 0.5f * j), // 每一波增大范围
                                baseRotation + i * MathHelper.PiOver2, // 垂直于弹幕方向
                                0.25f + 0.05f * j,
                                0.04f + 0.01f * j,
                                25
                            );
                            GeneralParticleHandler.SpawnParticle(pulse);
                        }

                        // 粒子特效，朝向弹幕正前方发射
                        for (j = 0; j < 3; j++) // 连续产生三次特效
                        {

                            // 粒子特效，固定发射 6 撮，沿弹幕面向方向扩散
                            for (int i = 0; i < 4; i++) // 固定x撮，沿方向扩散
                            {
                                float spreadAngle = MathHelper.PiOver4; // 每撮间隔 x 度
                                float angle = baseRotation - spreadAngle * 3 + spreadAngle * i; // 以弹幕面向方向为基准扩散
                                Vector2 offsetDirection = Vector2.UnitX.RotatedBy(angle); // 计算当前方向

                                for (int k = 0; k <= 6 + j * 2; k++) // 每撮粒子数量随波次增加
                                {
                                    Vector2 offset = offsetDirection * Main.rand.NextFloat(2f + j, 5f + j * 2f); // 偏移量随波次增加

                                    Dust dust = Dust.NewDustPerfect(
                                        Projectile.Center + offset,
                                        Main.rand.NextBool() ? 80 : 172, // 使用深蓝色哑光粒子类型
                                        offsetDirection * Main.rand.NextFloat(0.3f + 0.1f * j, 0.7f + 0.1f * j) // 粒子速度随波次增加
                                    );

                                    dust.scale = Main.rand.NextFloat(1.5f + 0.2f * j, 2.5f + 0.3f * j); // 粒子大小随波次增加
                                    dust.noGravity = true; // 无重力
                                }
                            }

                            // 模拟帧延迟以区分每次发射
                            Main.time += 2;
                        }


                        // 增加帧延迟以模拟两帧间隔
                        Main.time += 2;
                    }
                }


                if (Projectile.velocity.Length() < 0.1f || effectCounter >= 40)
                {
                    phase = 2;
                }
            }
            else if (phase == 2)
            {
                // 阶段2：寻找目标
                NPC target = FindTarget(1500f);
                if (target == null)
                {
                    // 未找到敌人时回收
                    returning = true;
                    ReturnToPlayer();
                }
                else
                {
                    // 找到敌人进入阶段3
                    phase = 3;
                    AttackTarget(target);
                }
            }
            if (phase == 3 && Projectile.penetrate != 6)
            {
                Projectile.penetrate = 6; // 第3阶段冲刺时设置为6次穿透
            }

        }

        private void ReturnToPlayer()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 directionToPlayer = Vector2.Normalize(player.Center - Projectile.Center) * 28f;
            Projectile.velocity = directionToPlayer;

            if (Projectile.Hitbox.Intersects(player.Hitbox))
            {
                player.statLife += 5;
                player.HealEffect(5);
                Projectile.Kill();
                return;
            }
        }

        private void AttackTarget(NPC target)
        {
            Vector2 direction = Vector2.Normalize(target.Center - Projectile.Center);
            Projectile.velocity = direction * 25f;
            // 传送并砸向目标
            if (Projectile.Hitbox.Intersects(target.Hitbox))
            {
                Projectile.damage = (int)(Projectile.damage * 1.05); // 增加伤害

                // 生成深蓝色冲击波特效
                Vector2 smallPulseScale = new Vector2(0.3f, 1.2f);
                Vector2 largePulseScale = new Vector2(0.6f, 1.6f);

                Particle smallPulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, smallPulseScale, MathHelper.PiOver2, 0.3f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(smallPulse);

                Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, largePulseScale, 0, 0.2f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(largePulse);

                // 随机传送到目标周围并再次砸向目标
                Vector2 randomOffset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 30 * 16;
                Projectile.Center = target.Center + randomOffset;
                Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * 25f;
            }
        }

        private NPC FindTarget(float range)
        {
            NPC closestNPC = null;
            float closestDistance = range;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(Projectile) && Projectile.Distance(npc.Center) < closestDistance)
                {
                    closestDistance = Projectile.Distance(npc.Center);
                    closestNPC = npc;
                }
            }

            return closestNPC;
        }

        public override void OnKill(int timeLeft)
        {
            // 屏幕震动效果
            float shakePower = 2f;
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


            // 释放三个SoulHunterJavSHARK
            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 3);
                Vector2 velocity = spawnOffset * Projectile.velocity.Length() * 1.5f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + spawnOffset * 50, velocity, ModContent.ProjectileType<SoulHunterJavSHARK>(), (int)(Projectile.damage * 1.0), 0, Projectile.owner);
            }


            // 生成规则扩散的深蓝色线性粒子特效
            int points = 30; // 生成更多的粒子
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                LineParticle subTrail = new LineParticle(Projectile.Center + velocity * 20.5f, velocity * 15, false, 30, 0.75f, Color.Blue); // 蓝色粒子
                GeneralParticleHandler.SpawnParticle(subTrail);
            }

            //{
            //    // 使用深蓝色泡泡 Gore 创建复杂魔法阵
            //    int numSpirals = 5; // 螺旋数量
            //    float spiralRadiusIncrement = 20f; // 每个螺旋之间的半径增量
            //    int pointsPerSpiral = 48; // 每个螺旋的粒子数量

            //    for (int spiral = 0; spiral < numSpirals; spiral++)
            //    {
            //        float radius = (spiral + 1) * spiralRadiusIncrement + Main.rand.NextFloat(-5f, 5f); // 每个螺旋的半径随机扰动
            //        for (int i = 0; i < pointsPerSpiral; i++)
            //        {
            //            float angle = MathHelper.TwoPi * i / pointsPerSpiral + Main.rand.NextFloat(-0.2f, 0.2f); // 随机扰动角度
            //            Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius;

            //            Gore bubble = Gore.NewGorePerfect(
            //                Projectile.GetSource_FromAI(),
            //                position,
            //                Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f), // 随机速度
            //                Main.rand.NextBool(3) ? 412 : 411, // 随机选择水泡类型
            //                Main.rand.NextFloat(0.8f, 1.3f) // 随机缩放
            //            );
            //            bubble.timeLeft = 12 + Main.rand.Next(6); // 调整时间让它存留得更久
            //            bubble.velocity *= 0.4f; // 减缓速度
            //        }
            //    }

            //    // 增加中心旋转效果
            //    int numCenterPoints = 72; // 中心点的粒子数量
            //    float centerRadius = 40f; // 中心旋转半径
            //    for (int i = 0; i < numCenterPoints; i++)
            //    {
            //        float angle = MathHelper.TwoPi * i / numCenterPoints + Main.rand.NextFloat(-0.1f, 0.1f); // 加入随机扰动
            //        Vector2 position = Projectile.Center + angle.ToRotationVector2() * centerRadius;

            //        Gore bubble = Gore.NewGorePerfect(
            //            Projectile.GetSource_FromAI(),
            //            position,
            //            angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2f), // 随机旋转速度
            //            Main.rand.NextBool(3) ? 412 : 411, // 随机选择水泡类型
            //            Main.rand.NextFloat(0.9f, 1.5f) // 随机缩放
            //        );
            //        bubble.timeLeft = 10 + Main.rand.Next(5); // 调整持续时间
            //        bubble.velocity *= 0.5f; // 减缓速度
            //    }
            //}


            {
                // 用粒子特效来完成之前气泡没有完成的魔法阵
                // 使用粒子特效 (80 和 172) 替代泡泡 Gore
                int numSpirals = 5; // 螺旋数量
                float spiralRadiusIncrement = 20f; // 每个螺旋之间的半径增量
                int pointsPerSpiral = 48; // 每个螺旋的粒子数量

                for (int spiral = 0; spiral < numSpirals; spiral++)
                {
                    float radius = (spiral + 1) * spiralRadiusIncrement + Main.rand.NextFloat(-5f, 5f); // 每个螺旋的半径随机扰动
                    for (int i = 0; i < pointsPerSpiral; i++)
                    {
                        float angle = MathHelper.TwoPi * i / pointsPerSpiral + Main.rand.NextFloat(-0.2f, 0.2f); // 随机扰动角度
                        Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius;

                        Dust particle = Dust.NewDustPerfect(
                            position,
                            Main.rand.NextBool() ? 80 : 172, // 粒子类型
                            angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f), // 初始速度为放射状
                            0,
                            Color.DarkBlue, // 深蓝色主题
                            Main.rand.NextFloat(1.0f, 1.5f) // 粒子大小随机化
                        );
                        particle.noGravity = true; // 无重力
                    }
                }

                // 增加中心旋转效果
                int numCenterPoints = 72; // 中心点的粒子数量
                float centerRadius = 40f; // 中心旋转半径
                for (int i = 0; i < numCenterPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / numCenterPoints + Main.rand.NextFloat(-0.1f, 0.1f); // 加入随机扰动
                    Vector2 position = Projectile.Center + angle.ToRotationVector2() * centerRadius;

                    Dust particle = Dust.NewDustPerfect(
                        position,
                        Main.rand.NextBool() ? 80 : 172, // 粒子类型
                        angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2f), // 初始速度为放射状
                        0,
                        Color.DarkBlue, // 深蓝色主题
                        Main.rand.NextFloat(0.9f, 1.5f) // 粒子大小随机化
                    );
                    particle.noGravity = true; // 无重力
                }
            }

            // 重型烟雾效果来实现爆炸
            {
                // 在弹幕消亡时释放一圈重型烟雾
                int numSmokeParticles = 30; // 烟雾粒子数量
                float smokeRadius = 60f; // 烟雾生成的半径

                for (int i = 0; i < numSmokeParticles; i++)
                {
                    Vector2 randomOffset = new Vector2(smokeRadius, 0).RotatedBy(MathHelper.TwoPi * i / numSmokeParticles); // 烟雾的圆形分布
                    Vector2 randVel = randomOffset * Main.rand.NextFloat(0.8f, 1.6f); // 随机速度

                    Color smokeColor = Main.rand.Next(3) switch
                    {
                        0 => new Color(57, 46, 115), // 深蓝色
                        1 => new Color(30, 60, 120), // 深海蓝
                        _ => new Color(20, 40, 80),  // 深邃蓝
                    };

                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center + randomOffset,
                        randVel,
                        smokeColor * 0.9f, // 颜色透明度调整
                        Main.rand.Next(25, 36), // 存在时间
                        Main.rand.NextFloat(0.9f, 2.3f), // 随机大小
                        0.4f // 透明度衰减速度
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }          



            // 增加额外的随机旋转法阵效果
            int numSpinningPoints = 72; // 粒子数量翻倍
            float spinningRadius = 100f; // 更大的半径
            for (int i = 0; i < numSpinningPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / numSpinningPoints;
                Vector2 position = Projectile.Center + angle.ToRotationVector2() * spinningRadius;

                Dust spinningBubble = Dust.NewDustPerfect(
                    position,
                    Main.rand.NextBool(3) ? 80 : 172, // 随机水泡
                    angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f), // 更高的旋转速度
                    0,
                    Color.DarkBlue, // 统一深蓝色
                    Main.rand.NextFloat(0.8f, 1.5f) // 更大水泡
                );
                spinningBubble.noGravity = true;
                spinningBubble.velocity *= 0.4f; // 轻微漂浮
            }
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.NPCDeath37, Projectile.Center);
        }

        // 添加命中计数器并销毁逻辑
        private int hitCounter = 0; // 命中计数器

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 屏幕震动效果
            float shakePower = 0.35f;
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


            hitCounter++; // 每次命中增加计数
            if (hitCounter >= 6)
            {
                Projectile.Kill(); // 达到6次后销毁自己
                return; // 停止后续逻辑
            }

            if (phase == 3) // 确保只在第三阶段触发逻辑
            {
                // 增加伤害
                Projectile.damage = (int)(Projectile.damage * 1.05);

                // 生成深蓝色冲击波特效
                Vector2 smallPulseScale = new Vector2(0.3f, 1.2f);
                Vector2 largePulseScale = new Vector2(0.6f, 1.6f);

                Particle smallPulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, smallPulseScale, MathHelper.PiOver2, 0.3f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(smallPulse);

                Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, largePulseScale, 0, 0.2f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(largePulse);

                // 随机传送到目标周围
                Vector2 randomOffset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 30 * 16;
                Projectile.Center = target.Center + randomOffset;

                // 重新计算砸向目标的方向
                Vector2 direction = Vector2.Normalize(target.Center - Projectile.Center);
                Projectile.velocity = direction * 25f;

                // 在敌人和传送点之间绘制粒子线
                Vector2 startPoint = target.Center; // 起点为敌人中心
                Vector2 endPoint = Projectile.Center; // 终点为传送后的弹幕位置
                int particleCount = 30; // 粒子数量决定线条的密度
                for (int i = 0; i < particleCount; i++)
                {
                    float progress = (float)i / particleCount; // 计算当前粒子的位置进度
                    Vector2 position = Vector2.Lerp(startPoint, endPoint, progress); // 线性插值计算粒子位置
                    for (int j = -1; j <= 1; j++) // 宽度为3的粒子线
                    {
                        Vector2 offset = Vector2.Normalize(endPoint - startPoint).RotatedBy(MathHelper.PiOver2) * j * 2f; // 计算偏移量
                        Dust dust = Dust.NewDustPerfect(position + offset, Main.rand.NextBool() ? 80 : 172); // 使用两种粒子类型
                        dust.color = Color.Lerp(Color.DarkBlue, Color.Cyan, Main.rand.NextFloat()); // 粒子颜色
                        dust.scale = Main.rand.NextFloat(1.2f, 1.8f); // 粒子大小
                        dust.noGravity = true; // 无重力
                        dust.velocity *= 0.1f; // 粒子速度
                    }
                }
            }


            // 创建抽象扩散粒子
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(10f, 50f);
                Dust dust = Dust.NewDustPerfect(target.Center + offset, Main.rand.Next(new int[] { DustID.Water, DustID.BlueTorch, DustID.BlueFlare }));
                dust.color = Color.Lerp(Color.DarkBlue, Color.Cyan, Main.rand.NextFloat());
                dust.scale = Main.rand.NextFloat(1.2f, 2f);
                dust.noGravity = true;
                dust.velocity = offset * 0.2f;
            }
            

            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
            SoundEngine.PlaySound(SoundID.Item21, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
        }
    }
}
