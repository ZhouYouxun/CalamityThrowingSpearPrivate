using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyLEFT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //if (CurrentState == BehaviorState.Aim) // 蓄力阶段
            {
                // 绘制 HalfStar 特效
                Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
                Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
                shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

                Vector2 lensFlareWorldPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 2.95f);
                Color lensFlareColor = Color.Lerp(Color.White, Color.LightGray, 0.23f) with { A = 0 };

                // 绘制 HalfStar 特效
                Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
                Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);

                //// 绘制 Sparkle 特效
                //Texture2D sparkleTex = ModContent.Request<Texture2D>("CalamityMod/Particles/Sparkle").Value;
                //float rotationSpeed = MathHelper.ToRadians(24f); // 每帧旋转的角度
                //float sparkleRadius = Projectile.width * 2.75f; // 半径与尖端位置一致

                //for (int i = 0; i < 5; i++) // 绘制 5 个 Sparkle
                //{
                //    float sparkleAngle = MathHelper.ToRadians(72 * i) + Main.GlobalTimeWrappedHourly * rotationSpeed; // 顺时针旋转
                //    Vector2 sparkleOffset = new Vector2((float)Math.Cos(sparkleAngle), (float)Math.Sin(sparkleAngle)) * sparkleRadius;
                //    Vector2 sparklePosition = lensFlareWorldPosition + sparkleOffset; // 根据角度计算 Sparkle 位置

                //    Main.EntitySpriteDraw(sparkleTex, sparklePosition - Main.screenPosition, null, lensFlareColor, sparkleAngle, sparkleTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
                //}

                // 绘制本体
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
                //return false;
            }
            //else if (CurrentState == BehaviorState.Dash) // 冲刺阶段
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 穿透次数
            Projectile.timeLeft = 24; // 设置持续时间
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public Player Owner => Main.player[Projectile.owner];



        // ======================================================================
        //  重写后的逻辑 —— 只有“发射”，没有“瞄准”
        //  创建后立刻进入 Dash，保持所有原本 Dash 特效与行为
        // ======================================================================

        public override void AI()
        {
            // 直接进入 Dash

            // 保留你的旋转方式
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 强制初速度（保持你原来 Dash 的速度）
            float initialSpeed = 25f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;

            // ====== 原 Dash 效果完整保留 ======
            DoEmit_DashParticles();
        }

        // ======================================================================
        //   把原本 Dash 粒子特效拆成独立函数（你要求的）
        // ======================================================================
        private void DoEmit_DashParticles()
        {
            // 你原本 Dash 中的全部粒子逻辑，100% 保留
            // ------------------------------------------------------

            // 双螺旋混合颜色粒子效果（复杂化并提升数量）
            for (int i = 0; i < 3; i++)
            {
                Vector2 particleOffset =
                    Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) *
                    Main.rand.NextFloat(0f, 6f);

                Vector2 position = Projectile.Center + particleOffset;

                Color startColor, endColor;
                if (Main.rand.NextBool())
                {
                    startColor = Color.White;
                    endColor = Color.LightGray;
                }
                else
                {
                    startColor = Color.Yellow;
                    endColor = Color.LightYellow;
                }

                CritSpark spark = new CritSpark(
                    position,
                    Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) *
                    Main.rand.NextFloat(0.8f, 1.2f),
                    startColor,
                    endColor,
                    Main.rand.NextFloat(1f, 1.5f),
                    Main.rand.Next(15, 25)
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 冲刺期间亮白色粒子特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color brightWhite = Color.WhiteSmoke;
                float outerSparkScale = 1.8f;
                SparkParticle spark = new SparkParticle(
                    Projectile.Center,
                    Projectile.velocity,
                    false,
                    7,
                    outerSparkScale,
                    brightWhite
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        // ======================================================================
        //                           OnHit NPC（不动）
        // ======================================================================
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            //// 保留你原本的全部逻辑（Debuff 触发 + Buff + CLK50）
            //if (target.HasBuff(ModContent.BuffType<StarsofDestinyEDebuff>()))
            //{
            //    Vector2 shootDirection = Projectile.velocity;
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        shootDirection,
            //        ModContent.ProjectileType<StarsofDestinyLSTAR>(),
            //        (int)(Projectile.damage * 0.6f),
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}

            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead)
                    player.AddBuff(ModContent.BuffType<StarsofDestinyPBuff>(), 300);
            }

            {
                Player owner = Main.player[Projectile.owner];

                int clkCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active &&
                        p.owner == owner.whoAmI &&
                        p.type == ModContent.ProjectileType<SODCLK50>())
                    {
                        clkCount++;
                        if (clkCount >= 12)
                            break;
                    }
                }

                if (clkCount < 12)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        owner.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SODCLK50>(),
                        Projectile.damage,
                        0f,
                        owner.whoAmI
                    );
                }
            }
        }




        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            string[] soundPaths = new string[]
            {
    "CalamityThrowingSpear/Sound/StarsofDestinyClock1",
    "CalamityThrowingSpear/Sound/StarsofDestinyClock2",
    "CalamityThrowingSpear/Sound/StarsofDestinyClock3",
    "CalamityThrowingSpear/Sound/StarsofDestinyClock4"
            };

            string selectedSoundPath = soundPaths[Main.rand.Next(soundPaths.Length)];

            SoundStyle boostedSound = new SoundStyle(selectedSoundPath).WithVolumeScale(3f);

            SoundEngine.PlaySound(boostedSound, Projectile.Center);






            //// 在死亡时发射5发弹幕，正前方散射
            //for (int i = -2; i <= 2; i++) // 扩散角度 -2 到 +2
            //{
            //    Vector2 shootDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(6f * i));
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        shootDirection,
            //        ModContent.ProjectileType<StarsofDestinyLSTAR>(),
            //        (int)(Projectile.damage * 0.36f), // 伤害倍率0.4倍
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}



            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<StarsofDestinyLEFTCLK>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            //{
            //    // 时钟圆环粒子
            //    for (int i = 0; i < 150; i++) // 内环
            //    {
            //        float angle = MathHelper.TwoPi / 150 * i;
            //        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10 * 16;
            //        Vector2 position = Projectile.Center + offset;

            //        int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
            //        Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.White, 1.5f);
            //        dust.noGravity = true;
            //    }

            //    for (int i = 0; i < 215; i++) // 外环（三倍粒子数量）
            //    {
            //        float angle = MathHelper.TwoPi / 215 * i;
            //        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12 * 16;
            //        Vector2 position = Projectile.Center + offset;

            //        int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
            //        Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.LightYellow, 1.2f);
            //        dust.noGravity = true;
            //    }

            //    // 时钟刻度线
            //    int numTicks = 12; // 刻度数量
            //    float innerRadius = 10 * 16; // 内环半径
            //    float outerRadius = 12 * 16; // 外环半径
            //    float tickLength = outerRadius - innerRadius; // 刻度线的长度

            //    for (int i = 0; i < numTicks; i++)
            //    {
            //        float angle = MathHelper.TwoPi / numTicks * i; // 每条刻度的角度
            //        Vector2 start = Projectile.Center + new Vector2(
            //            (float)Math.Cos(angle) * innerRadius,
            //            (float)Math.Sin(angle) * innerRadius
            //        ); // 起点在内环
            //        Vector2 end = Projectile.Center + new Vector2(
            //            (float)Math.Cos(angle) * outerRadius,
            //            (float)Math.Sin(angle) * outerRadius
            //        ); // 终点在外环

            //        // 计算沿线分布的粒子位置
            //        int numParticlesPerTick = 10; // 每条线的粒子数量
            //        for (int j = 0; j < numParticlesPerTick; j++)
            //        {
            //            float lerpFactor = j / (float)(numParticlesPerTick - 1); // 线性插值因子
            //            Vector2 position = Vector2.Lerp(start, end, lerpFactor); // 计算粒子位置

            //            int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
            //            Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.LightYellow, 1.2f);
            //            dust.noGravity = true;
            //        }
            //    }

            //    // 动态时针和分针 + 伤害透明弹幕
            //    float shortHandLength = 10 * 16 * 0.5f; // 短指针长度
            //    float longHandLength = 10 * 16 * 0.9f;  // 长指针长度

            //    for (int i = 0; i < 2; i++) // 绘制两根指针（i==0时针，i==1分针）
            //    {
            //        float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
            //        float length = i == 0 ? shortHandLength : longHandLength;
            //        Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;

            //        for (int j = 0; j < Main.rand.Next(1, 3); j++) // 1~2 层宽度
            //        {
            //            for (float k = 0f; k < 1f; k += 0.1f) // 沿指针方向放置
            //            {
            //                Vector2 particlePosition = Projectile.Center + direction * k + Main.rand.NextVector2Circular(4f, 4f);

            //                // 粒子
            //                Dust dust = Dust.NewDustPerfect(particlePosition, DustID.RainbowTorch, Vector2.Zero, 100, Color.Yellow, 2f);
            //                dust.noGravity = true;

            //                // 同步生成透明弹幕 StarsofDestinyL
            //                if (Main.myPlayer == Projectile.owner) // 防止多人重复生成
            //                {
            //                    Projectile.NewProjectile(
            //                        Projectile.GetSource_FromThis(),
            //                        particlePosition,
            //                        Vector2.Zero, // 静止不动
            //                        ModContent.ProjectileType<StarsofDestinyINV>(),
            //                        (int)(Projectile.damage * 0.3f), // 伤害倍率 0.2
            //                        0f,
            //                        Projectile.owner
            //                    );
            //                }
            //            }
            //        }
            //    }
            //}








        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 与 StarsofDestinyRStandField 弹幕的碰撞检测
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile otherProj = Main.projectile[i];
                if (otherProj.active && otherProj.type == ModContent.ProjectileType<StarsofDestinyRStandField>())
                {
                    if (projHitbox.Intersects(otherProj.Hitbox))
                    {
                        otherProj.Kill(); // 碰撞时直接消失
                        return false; // 自身不受影响
                    }
                }
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override bool? CanDamage()
        {
            //// 蓄力状态下不造成伤害
            //if (CurrentState == BehaviorState.Aim)
            //{
            //    return false;
            //}

            // 如果当前状态是冲刺状态，允许造成伤害
            return true;
        }
    }
}
