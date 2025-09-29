using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SawBladeForkHornJav/SawBladeForkHornJav";

        // ====== 自建计时器（遵守禁止使用 localAI 计数器的规则） ======
        private int fxTick;          // 全局特效节拍
        private int stateTick;       // 当前状态内帧数
        private int fireTraceTick;   // 划空轨迹节拍

        public enum BehaviorState
        {
            Aim,
            Fire
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set { Projectile.ai[0] = (int)value; stateTick = 0; }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        // ====== 轨迹渲染函数（保持、并稍作强化） ======
        internal Color ColorFunction(float completionRatio)
        {
            float fadeOpacity = Utils.GetLerpValue(0.94f, 0.54f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.Black, Color.LightGray, 0.4f) * fadeOpacity;
        }
        internal float WidthFunction(float completionRatio)
        {
            float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
            return MathHelper.Lerp(0f, 12f * Projectile.Opacity, expansionCompletion);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 左右翻转 & 角度补偿
            bool facingLeft = Projectile.velocity.X < 0;
            SpriteEffects direction = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float rotation = Projectile.rotation + (facingLeft ? MathHelper.PiOver2 : 0f);

            if (CurrentState == BehaviorState.Fire)
            {
                // 冲刺时拖尾
                GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(
                    ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(
                    Projectile.oldPos,
                    new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]),
                    60);

                {
                    // 冲刺时：单体绘制 + 黑色描边
                    Color outlineColor = Color.Black * 0.8f;
                    float outlineOffset = 2f;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 offset = Vector2.Zero;
                        if (i == 0) offset = new Vector2(outlineOffset, 0f);
                        if (i == 1) offset = new Vector2(-outlineOffset, 0f);
                        if (i == 2) offset = new Vector2(0f, outlineOffset);
                        if (i == 3) offset = new Vector2(0f, -outlineOffset);

                        Main.EntitySpriteDraw(
                            texture,
                            drawPosition + offset,
                            frame,
                            outlineColor,
                            rotation,
                            origin,
                            Projectile.scale,
                            direction,
                            0
                        );
                    }

                    // 本体
                    Main.EntitySpriteDraw(
                        texture,
                        drawPosition,
                        frame,
                        Projectile.GetAlpha(lightColor),
                        rotation,
                        origin,
                        Projectile.scale,
                        direction,
                        0
                    );
                }
            }
            else
            {
                // 非冲刺时正常绘制
                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    frame,
                    Projectile.GetAlpha(lightColor),
                    rotation,
                    origin,
                    Projectile.scale,
                    direction,
                    0
                );
            }

            return false;
        }





        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public Player Owner => Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 出生时清理“右侧构件”（保留原逻辑）
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<SawBladeForkHornJavRIGHT>())
                    proj.Kill();
            }
        }

        public override void AI()
        {
            // 过远自杀（保留）
            float distanceToPlayer = Vector2.Distance(Projectile.Center, Owner.Center);
            if (distanceToPlayer > 5000f)
            {
                Projectile.Kill();
                return;
            }

            // 旋转与朝向（通用）
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            //Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();

            // 状态机
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Fire:
                    DoBehavior_Fire();
                    break;
            }

            // 全局计时
            Time++;
            fxTick++;
            stateTick++;
            fireTraceTick++;
        }

        // ====== 蓄力（持续性释放沉重浓烟 + 科学感脉冲/骨灵 + 微火花） ======
        public void DoBehavior_Aim()
        {
            // 维持存在与设定（保留）
            Projectile.timeLeft = 480;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 平滑瞄准鼠标（保留）
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.12f);
            }
            // 持握在手：向前顶一点，避免完全重叠
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Projectile.width * 0.5f);
            Owner.heldProj = Projectile.whoAmI;



            // === 污染感 Dust 图形特效（仅前 150 帧存在） ===
            if (stateTick < 150 && fxTick % 12 == 0)
            {
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 center = Projectile.Center + forward * (2f * 16f); // 前方核心
                float baseRadius = 24f; // 圆环半径

                // 1) 圆环 Dust —— 黑烟组成的脉冲污染环
                int ringPoints = 24;
                for (int i = 0; i < ringPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringPoints;
                    Vector2 offset = angle.ToRotationVector2() * baseRadius;
                    Vector2 pos = center + offset;
                    Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f); // 向内吸收
                    int d = Dust.NewDust(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.2f, 1.8f));
                    Main.dust[d].noGravity = true;
                }

                // 2) 内吸漩涡 —— 少量棕灰尘，形成螺旋往中心卷
                int swirl = 12;
                for (int i = 0; i < swirl; i++)
                {
                    float angle = (MathHelper.TwoPi * i / swirl) + (fxTick * 0.1f); // 随时间旋转
                    Vector2 offset = angle.ToRotationVector2() * (baseRadius * 0.6f);
                    Vector2 pos = center + offset;
                    Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                    int d = Dust.NewDust(pos, 0, 0, DustID.Ash, vel.X, vel.Y, 0, new Color(70, 45, 25), Main.rand.NextFloat(1.0f, 1.4f));
                    Main.dust[d].noGravity = true;
                }

                // 3) 不完整环（缺口圆弧） —— 像被撕开的污染波
                int arcPoints = 16;
                float arcSpread = MathHelper.ToRadians(220f); // 220° 弧
                float arcStart = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < arcPoints; i++)
                {
                    float angle = arcStart + arcSpread * (i / (float)(arcPoints - 1));
                    Vector2 offset = angle.ToRotationVector2() * (baseRadius * 1.3f);
                    Vector2 pos = center + offset;
                    Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 4f);
                    int d = Dust.NewDust(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.3f, 1.9f));
                    Main.dust[d].noGravity = true;
                }
            }



            // 松手 → 发射
            if (!Owner.channel)
            {
                if (stateTick < 150)
                {
                    // 未满 150 帧，直接销毁自己
                    Projectile.Kill();
                    return;
                }
                else
                {
                    // 满 150 帧，进入发射阶段
                    CurrentState = BehaviorState.Fire;
                    Projectile.netUpdate = true;

                    // ====== 在到达 150 帧瞬间，释放一次大型法阵 Dust 特效 ======
                    float radius = 60f;
                    int points = 50;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points;
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        int d = Dust.NewDust(Projectile.Center + offset, 0, 0, DustID.Smoke,
                                             -offset.X * 0.1f, -offset.Y * 0.1f, 0,
                                             Color.Black, Main.rand.NextFloat(1.8f, 2.5f));
                        Main.dust[d].noGravity = true;
                    }
                }
            }
        }

        // ====== 发射（强烈划空 + 数学/粗暴混合轨迹特效） ======
        public void DoBehavior_Fire()
        {
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;

            // 统一初速（保留）
            float initialSpeed = 60f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            {
                // === Dust-only 划空“刀锋涡迹” ===
                {
                    Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                    int streak = 3 + Main.rand.Next(2); // 3~4 条更浓烈
                    for (int i = 0; i < streak; i++)
                    {
                        float side = (i % 2 == 0) ? 1f : -1f;
                        Vector2 p = Projectile.Center + perp * side * Main.rand.NextFloat(6f, 18f);

                        Vector2 vel = perp * side * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(1f, 1f);
                        int idx = Dust.NewDust(p, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.2f, 1.8f));
                        Main.dust[idx].noGravity = true;

                        if (Main.rand.NextBool(2))
                        {
                            int idx2 = Dust.NewDust(p, 0, 0, DustID.Ash,
                                Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f,
                                0, new Color(80, 50, 30), Main.rand.NextFloat(1.0f, 1.4f));
                            Main.dust[idx2].noGravity = true;
                        }
                    }
                }

                // === Dust-only 正前方锥形“震荡喷洒” ===
                if ((fireTraceTick % 2) == 0)
                {
                    float baseRot = Projectile.velocity.ToRotation();
                    int rays = 12;
                    float spread = MathHelper.ToRadians(28f);
                    for (int r = 0; r < rays; r++)
                    {
                        float t = (stateTick * 0.12f + r * 0.31f);
                        float ang = baseRot + MathHelper.Lerp(-spread, spread, r / (float)(rays - 1));
                        float dist = 20f + 12f * (float)Math.Sin(t * 1.7f);
                        Vector2 pos = Projectile.Center + ang.ToRotationVector2() * dist;
                        Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);

                        int idx = Dust.NewDust(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.3f, 2.0f));
                        Main.dust[idx].noGravity = true;

                        if (Main.rand.NextBool(3))
                        {
                            int idx2 = Dust.NewDust(pos, 0, 0, DustID.Ash, vel.X * 0.8f, vel.Y * 0.8f, 0, new Color(60, 40, 20), Main.rand.NextFloat(1.0f, 1.6f));
                            Main.dust[idx2].noGravity = true;
                        }
                    }
                }

                // === Dust-only 脉冲环（用环形散点代替） ===
                if ((fireTraceTick % 6) == 0)
                {
                    int ringPoints = 30;
                    float radius = 12f;
                    for (int i = 0; i < ringPoints; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringPoints;
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        Vector2 pos = Projectile.Center + offset;

                        Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                        int idx = Dust.NewDust(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.0f, 1.6f));
                        Main.dust[idx].noGravity = true;

                        if (Main.rand.NextBool(4))
                        {
                            int idx2 = Dust.NewDust(pos, 0, 0, DustID.Ash, vel.X * 0.6f, vel.Y * 0.6f, 0, new Color(70, 45, 25), Main.rand.NextFloat(1.2f, 1.8f));
                            Main.dust[idx2].noGravity = true;
                        }
                    }
                }

            }
        }

        public override bool? CanDamage()
        {
            // 仅发射阶段能造成伤害（保留）
            return CurrentState == BehaviorState.Fire ? true : false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 原有三 Debuff（保留）
            target.AddBuff(ModContent.BuffType<SawBladeForkHornEDebuff>(), 300);
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 300);
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300);

            // 给予玩家堆叠（保留）
            var player = Main.player[Projectile.owner].GetModPlayer<SawBladeForkHornPlayer>();
            player.IncreaseStackCount();

            // 爆炸音效（保留）
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);


            // —— Dust-only 黑色爆裂：有序 + 无序 —— //
            {
                // 1) 有序：圆环 Dust —— 从中心均匀喷出
                int ringPoints = 40;
                float ringRadius = 12f;
                for (int i = 0; i < ringPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringPoints;
                    Vector2 pos = target.Center + angle.ToRotationVector2() * ringRadius;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);

                    int idx = Dust.NewDust(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.4f, 2.2f));
                    Main.dust[idx].noGravity = true;
                }

                // 2) 有序：前向锥形喷射 —— 黑色能量箭雨
                int coneCount = 60;
                float baseRot = Projectile.velocity.ToRotation();
                float spread = MathHelper.ToRadians(40f);
                for (int i = 0; i < coneCount; i++)
                {
                    float ang = baseRot + Main.rand.NextFloat(-spread, spread);
                    Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(10f, 20f);

                    int idx = Dust.NewDust(target.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.6f, 2.3f));
                    Main.dust[idx].noGravity = true;
                }

                // 3) 无序：大爆散 —— 随机全方向喷射
                int scatter = 120;
                for (int i = 0; i < scatter; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 28f);
                    int idx = Dust.NewDust(target.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.8f, 2.5f));
                    Main.dust[idx].noGravity = true;
                    Main.dust[idx].velocity *= 1.8f;
                }

                // 4) 无序：内吸回旋 —— 部分 Dust 反向回卷
                int inward = 40;
                for (int i = 0; i < inward; i++)
                {
                    Vector2 vel = -Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f);
                    int idx = Dust.NewDust(target.Center + Main.rand.NextVector2Circular(20f, 20f), 0, 0, DustID.Smoke, vel.X, vel.Y, 0, Color.Black, Main.rand.NextFloat(1.2f, 1.8f));
                    Main.dust[idx].noGravity = true;
                }
            }



            // 屏幕震动（保留）
            float shakePower = 10f;
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡强化屏震（保留）
            float pulseCompletionRatio = 1f;
            float screenShakePower = CalamityUtils.Convert01To010(pulseCompletionRatio) * 16f;
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakePower)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;

            // 补一轮“收束”暗色烟雾（呼应主题）
            for (int i = 0; i < 20; i++)
            {
                var hs = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(1.2f, 1.2f) * -1f, // 向内回卷
                    Color.Lerp(Color.Black, Color.Gray, 0.25f),
                    Main.rand.Next(22, 30),
                    Main.rand.NextFloat(0.9f, 1.3f),
                    0.9f,
                    Main.rand.NextFloat(-0.25f, 0.25f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(hs);
            }
        }
    }
}
