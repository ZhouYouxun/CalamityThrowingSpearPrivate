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

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.StarsofDestiny
{
    internal class StarsofDestinyLEFT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (CurrentState == BehaviorState.Aim) // 蓄力阶段
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
                return false;
            }
            else if (CurrentState == BehaviorState.Dash) // 冲刺阶段
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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 穿透次数
            Projectile.timeLeft = 30; // 设置持续时间为1500帧
            Projectile.extraUpdates = 2; // 额外更新次数为1
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }
        private int shootTimer = 0; // 添加一个计时器
        public ref float Time => ref Projectile.ai[1];
        private int chargeDurationFrames = 0; // 用于跟踪蓄力时间（帧数）

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;


            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;


            // 逐渐增加蓄力时间
            chargeDurationFrames++;

            // 如果蓄力时间超过 20 帧，播放水晶球音效（仅播放一次）
            if (chargeDurationFrames == 20)
            {
                //SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
            }

            // 检测松手
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 30; // 冲刺阶段持续时间
                Projectile.penetrate = -1; // 设置冲刺阶段的穿透次数

                CurrentState = BehaviorState.Dash;
            }
        }
        private bool damageBoostApplied = false; // 新增变量，记录是否已应用伤害奖励
        private void DoBehavior_Dash()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            // 重置速度的逻辑
            {
                float initialSpeed = 25f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }

            // 确保伤害奖励只计算一次
            if (!damageBoostApplied)
            {
                damageBoostApplied = true; // 标记已应用
                float damageMultiplier = 1f + MathHelper.Clamp(chargeDurationFrames * 0.01f, 0f, 0.2f); // 每帧增加 1%，最多 20%
                Projectile.damage = (int)(Projectile.damage * damageMultiplier); // 应用一次性奖励
            }

            // 双螺旋混合颜色粒子效果（复杂化并提升数量）
            for (int i = 0; i < 3; i++) // 每帧生成 3 组粒子
            {
                // 粒子生成位置（稍微偏离弹幕中心以形成动态感）
                Vector2 particleOffset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) * Main.rand.NextFloat(0f, 6f);
                Vector2 position = Projectile.Center + particleOffset;

                // 随机选择粒子颜色
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

                // 创建粒子并加入轻微旋转运动
                CritSpark spark = new CritSpark(
                    position,
                    Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(0.8f, 1.2f),
                    startColor,
                    endColor,
                    Main.rand.NextFloat(1f, 1.5f), // 随机放大粒子
                    Main.rand.Next(15, 25) // 粒子寿命
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 冲刺期间亮白色粒子特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color brightWhite = Color.WhiteSmoke; // 纯白色
                float outerSparkScale = 1.8f; // 放大？%
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, brightWhite);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            // 检查敌人是否携带 StarsofDestinyEDebuff
            if (target.HasBuff(ModContent.BuffType<StarsofDestinyEDebuff>()))
            {
                // 计算正前方方向
                Vector2 shootDirection = Projectile.velocity; // 保持当前弹幕的飞行方向
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shootDirection,
                    ModContent.ProjectileType<StarsofDestinyLSTAR>(), // 替换为目标弹幕类型
                    (int)(Projectile.damage * 0.95f), // 伤害倍率0.95倍
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead)
                {
                    player.AddBuff(ModContent.BuffType<StarsofDestinyPBuff>(), 300); // 为所有玩家添加机动性加成Buff
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

            SoundStyle boostedSound = new SoundStyle(selectedSoundPath).WithVolumeScale(3f); // ✅ 放大300%音量【这个没用，别看这玩意儿，这失效了，这tmd绝对是来捣乱的是吧】

            SoundEngine.PlaySound(boostedSound, Projectile.Center);






            // 在死亡时发射5发弹幕，正前方散射
            for (int i = -2; i <= 2; i++) // 扩散角度 -2 到 +2
            {
                Vector2 shootDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(6f * i));
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shootDirection,
                    ModContent.ProjectileType<StarsofDestinyLSTAR>(),
                    (int)(Projectile.damage * 0.75f), // 伤害倍率0.75倍
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            {
                // 时钟圆环粒子
                for (int i = 0; i < 150; i++) // 内环
                {
                    float angle = MathHelper.TwoPi / 150 * i;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10 * 16;
                    Vector2 position = Projectile.Center + offset;

                    int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                    Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.White, 1.5f);
                    dust.noGravity = true;
                }

                for (int i = 0; i < 215; i++) // 外环（三倍粒子数量）
                {
                    float angle = MathHelper.TwoPi / 215 * i;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12 * 16;
                    Vector2 position = Projectile.Center + offset;

                    int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                    Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.LightYellow, 1.2f);
                    dust.noGravity = true;
                }

                // 时钟刻度线
                int numTicks = 12; // 刻度数量
                float innerRadius = 10 * 16; // 内环半径
                float outerRadius = 12 * 16; // 外环半径
                float tickLength = outerRadius - innerRadius; // 刻度线的长度

                for (int i = 0; i < numTicks; i++)
                {
                    float angle = MathHelper.TwoPi / numTicks * i; // 每条刻度的角度
                    Vector2 start = Projectile.Center + new Vector2(
                        (float)Math.Cos(angle) * innerRadius,
                        (float)Math.Sin(angle) * innerRadius
                    ); // 起点在内环
                    Vector2 end = Projectile.Center + new Vector2(
                        (float)Math.Cos(angle) * outerRadius,
                        (float)Math.Sin(angle) * outerRadius
                    ); // 终点在外环

                    // 计算沿线分布的粒子位置
                    int numParticlesPerTick = 10; // 每条线的粒子数量
                    for (int j = 0; j < numParticlesPerTick; j++)
                    {
                        float lerpFactor = j / (float)(numParticlesPerTick - 1); // 线性插值因子
                        Vector2 position = Vector2.Lerp(start, end, lerpFactor); // 计算粒子位置

                        int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                        Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.LightYellow, 1.2f);
                        dust.noGravity = true;
                    }
                }

                // 动态时针和分针
                float shortHandLength = 10 * 16 * 0.5f; // 短指针长度
                float longHandLength = 10 * 16 * 0.9f; // 长指针长度

                for (int i = 0; i < 2; i++) // 绘制两根指针
                {
                    // 随机方向
                    float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    float length = i == 0 ? shortHandLength : longHandLength; // 短指针或长指针
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;

                    // 指针由粒子组成
                    for (int j = 0; j < Main.rand.Next(1, 3); j++) // 1 到 2 层宽度
                    {
                        for (float k = 0f; k < 1f; k += 0.1f) // 沿指针方向放置粒子
                        {
                            Vector2 particlePosition = Projectile.Center + direction * k + Main.rand.NextVector2Circular(4f, 4f); // 增加轻微随机性
                            Dust dust = Dust.NewDustPerfect(particlePosition, DustID.RainbowTorch, Vector2.Zero, 100, Color.Yellow, 2f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
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
            // 蓄力状态下不造成伤害
            if (CurrentState == BehaviorState.Aim)
            {
                return false;
            }

            // 如果当前状态是冲刺状态，允许造成伤害
            return true;
        }
    }
}
