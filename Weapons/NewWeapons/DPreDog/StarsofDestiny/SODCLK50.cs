using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    // “时刻”发光弹幕：围绕玩家旋转、造成接触伤害
    public class SODCLK50 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        // 显示用贴图列表（你提供的素材）
        private static readonly string TexStar07 = "CalamityThrowingSpear/Texture/KsTexture/star_07";
        private static readonly string TexStar08 = "CalamityThrowingSpear/Texture/KsTexture/star_08";
        private static readonly string TexFlare = "CalamityThrowingSpear/Texture/SuperTexturePack/flare2_002";

        public override string Texture => TexStar07; // 主体贴图

        // 时刻编号（0~n），用于平分角度，由外部生成时传入 ai[0]
        // ai[1] = 当前总时刻数量（用于自动平分 360°）
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 持续刷新，不会过期（外部保证刷新）
            Projectile.DamageType = DamageClass.Melee;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        private float smoothAngle = 0f;   // 平滑过渡后的角度
        private float selfRotation = 0f;  // 自身旋转累积角度


        // 放在类字段区域
        private float clkOrbitAngle;
        private bool clkAngleInitialized;
        private bool decaySet = false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner == null || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // 永不超时，由自己维持
            Projectile.timeLeft = 10;
            Projectile.friendly = true;

            // =========================
            // 1. 收集本玩家全部 SODCLK50
            // =========================
            List<Projectile> list = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active &&
                    p.owner == Projectile.owner &&
                    p.type == Projectile.type)
                {
                    list.Add(p);
                }
            }

            if (list.Count <= 0)
            {
                Projectile.Kill();
                return;
            }

            // 用 identity 排序，确保顺序稳定
            list.Sort((a, b) => a.identity.CompareTo(b.identity));

            int selfIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].whoAmI == Projectile.whoAmI)
                {
                    selfIndex = i;
                    break;
                }
            }

            int total = list.Count;

            // 记录一下索引和总数（如果外部需要的话）
            Projectile.ai[0] = selfIndex;
            Projectile.ai[1] = total;

            // =========================
            // 2. 目标角度（实时平分 + 全局自转）
            // =========================
            float angleStep = MathHelper.TwoPi / total;
            float spinSpeed = 1.4f; // 全局自转速度
            float globalSpin = Main.GlobalTimeWrappedHourly * spinSpeed;

            // 理想角度 = 全局自转 + 均分角度
            float desiredAngle = globalSpin + angleStep * selfIndex;

            // 当前角度存放在自定义字段
            float currentAngle = clkOrbitAngle;

            // 第一次初始化 / 异常保护
            if (!clkAngleInitialized || float.IsNaN(currentAngle) || float.IsInfinity(currentAngle))
            {
                currentAngle = desiredAngle;
                clkAngleInitialized = true; // 标记已初始化
            }

            currentAngle = MathHelper.WrapAngle(currentAngle);
            desiredAngle = MathHelper.WrapAngle(desiredAngle);

            // =========================
            // 3. 平滑逼近目标角度（避免角度跳变）
            // =========================
            float maxStep = 0.12f; // 每帧最大转动弧度
            float diff = MathHelper.WrapAngle(desiredAngle - currentAngle);

            if (Math.Abs(diff) <= maxStep)
                currentAngle = desiredAngle;
            else
                currentAngle += Math.Sign(diff) * maxStep;

            clkOrbitAngle = currentAngle;

            // =========================
            // 4. 圆形轨道（固定半径）
            // =========================
            float radius = 80f;
            Vector2 orbitCenter = owner.Center;
            Vector2 desiredPos = orbitCenter + currentAngle.ToRotationVector2() * radius;

            // 平滑追随目标位置（避免瞬移）
            Vector2 toDesired = desiredPos - Projectile.Center;
            float followLerp = 0.4f; // 越大越“跟手”，越小越迟钝
            Projectile.Center += toDesired * followLerp;

            // =========================
            // 5. 自转 + 伤害设置
            // =========================
            selfRotation += 0.08f; // 自己转起来
            Projectile.rotation = (Projectile.Center - orbitCenter).ToRotation() + selfRotation;

            Projectile.damage = Projectile.originalDamage;
            Projectile.DamageType = DamageClass.Melee;

            // =========================
            // 6. 特效 & 光照
            // =========================
            EmitCLK50FX();

            // 柔和金青色光
            Lighting.AddLight(Projectile.Center, 0.9f, 0.95f, 0.3f);
        }


        // ======================================================
        // 独立的“时刻”特效函数
        // ======================================================
        private void EmitCLK50FX()
        {
            if (Main.dedServ)
                return;

            Player owner = Main.player[Projectile.owner];
            Vector2 center = Projectile.Center;

            // 以“从玩家指向自身”的方向作为前方
            Vector2 forward = Vector2.UnitY;
            if (owner != null && owner.active)
                forward = (center - owner.Center).SafeNormalize(Vector2.UnitY);
            Vector2 right = forward.RotatedBy(MathHelper.PiOver2);

            // 主色：青绿 + 金色
            Color mainCyan = new Color(140, 255, 230);
            Color mainGold = new Color(255, 235, 140);
            float time = Main.GlobalTimeWrappedHourly;

            // Opacity 参与整体强度
            float alpha = Projectile.Opacity;
            if (alpha <= 0f)
                return;

            // ================================
            // 1）静态锐角三角形 GlowOrb 点阵
            // ================================
            // 尖端朝外，三层：1 点、3 点、5 点（总共 9 点 / 次）
            if (Main.GameUpdateCount % 4 == 0)
            {
                float triLength = 32f; // 三角形前后长度
                float triWidth = 26f;  // 最后排宽度

                int rows = 3;
                for (int row = 0; row < rows; row++)
                {
                    float rowT = rows == 1 ? 0f : row / (rows - 1f); // 0 → 1
                    float along = MathHelper.Lerp(triLength, 8f, rowT);

                    int cols = 1 + row * 2; // 1, 3, 5   宏伟一点
                    for (int k = 0; k < cols; k++)
                    {
                        float offsetT = cols == 1 ? 0f : (k / (cols - 1f) - 0.5f);
                        Vector2 pos =
                            center +
                            forward * along +
                            right * (offsetT * triWidth * (1f - rowT)); // 越靠前越尖

                        float lerpC = 1f - rowT;
                        Color c = Color.Lerp(mainCyan, mainGold, lerpC) * (0.85f * alpha);
                        c.A = 0;

                        GlowOrbParticle orb = new GlowOrbParticle(
                            pos,
                            Vector2.Zero,
                            false,
                            12,
                            0.9f + 0.2f * (1f - rowT),
                            c,
                            true,
                            false,
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }

            // ================================
            // 2）EXO 拉伸光：沿切线轻微喷射
            // ================================
            if (Main.rand.NextBool(3))
            {
                float phase = time * 2.1f + Projectile.identity * 0.37f;
                float sideSign = Math.Sign(Math.Sin(phase));
                Vector2 tangentDir = right * sideSign;

                Color exoColor = Color.Lerp(mainGold, mainCyan, (float)Math.Sin(phase) * 0.5f + 0.5f);

                SquishyLightParticle exo = new SquishyLightParticle(
                    center + Main.rand.NextVector2Circular(6f, 6f),
                    (forward * 0.6f + tangentDir * 0.8f).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.2f, 2.4f),
                    Main.rand.NextFloat(0.22f, 0.30f),
                    exoColor,
                    Main.rand.Next(20, 32),
                    opacity: 1f * alpha,
                    squishStrenght: 1f,
                    maxSquish: Main.rand.NextFloat(2.2f, 3.3f),
                    hueShift: 0f
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }

            // ================================
            // 3）椭圆冲击波：小尺度内敛的“心跳圈”
            // ================================
            if (Main.GameUpdateCount % 12 == 0)
            {
                float squishPhase = (float)Math.Sin(time * 3.0f + Projectile.identity * 0.5f);
                Vector2 squish = new Vector2(1f, 1.7f + 0.4f * squishPhase);

                Particle pulse = new DirectionalPulseRing(
                    center,
                    forward * 1.5f,
                    Color.Lerp(mainGold, mainCyan, 0.4f) * (0.9f * alpha),
                    squish,
                    Projectile.rotation - MathHelper.PiOver4,
                    0.16f,
                    0.04f,
                    22
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ================================
            // 4）水雾：中心轻微散逸，填补层次
            // ================================
            if (Main.rand.NextBool(4))
            {
                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    center + Main.rand.NextVector2Circular(4f, 4f),
                    -forward * Main.rand.NextFloat(0.3f, 0.9f),
                    false,
                    Main.rand.Next(18, 26),
                    0.9f + Main.rand.NextFloat(0.3f),
                    Color.LightBlue * (0.7f * alpha)
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // ================================
            // 5）Bloom + 数字方块：偶尔闪一下
            // ================================
            //if (Main.GameUpdateCount % 1 == 0)
            //{
            //    BloomRing ring = new BloomRing(
            //        center,
            //        Vector2.Zero,
            //        Color.Lerp(mainGold, mainCyan, 0.5f) * (0.9f * alpha),
            //        0.09f,
            //        30
            //    );
            //    GeneralParticleHandler.SpawnParticle(ring);
            //}

            if (Main.rand.NextBool(6))
            {
                Vector2 sqVel = forward.RotatedByRandom(0.9f) * Main.rand.NextFloat(1.2f, 3.2f);
                SquareParticle square = new SquareParticle(
                    center,
                    sqVel,
                    false,
                    28,
                    1.3f + Main.rand.NextFloat(0.4f),
                    Color.Lerp(mainCyan, mainGold, Main.rand.NextFloat()) * (1.1f * alpha)
                );
                GeneralParticleHandler.SpawnParticle(square);
            }
        }



        // ---------------------
        // 自行绘制魔法特效层
        // ---------------------
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            Texture2D tex1 = ModContent.Request<Texture2D>(TexStar07).Value;
            Texture2D tex2 = ModContent.Request<Texture2D>(TexStar08).Value;
            Texture2D tex3 = ModContent.Request<Texture2D>(TexFlare).Value;

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin1 = tex1.Size() / 2f;
            Vector2 origin2 = tex2.Size() / 2f;
            Vector2 origin3 = tex3.Size() / 2f;

            Color col = Color.Lerp(Color.Yellow, Color.White, 0.4f) * 0.85f;

            float scale = 0.07f + 0.01f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + Projectile.ai[0] * 0.12f);

            // 柔和发光背板（flare）
            sb.Draw(
                tex3,
                pos,
                null,
                Color.Gold * 0.45f,
                Projectile.rotation * 0.4f,
                origin3,
                scale * 1.4f,
                SpriteEffects.None,
                0f
            );

            // 主体星形
            sb.Draw(
                tex1,
                pos,
                null,
                col,
                Projectile.rotation,
                origin1,
                scale,
                SpriteEffects.None,
                0f
            );

            // 外层交叉星芒
            sb.Draw(
                tex2,
                pos,
                null,
                col * 0.8f,
                -Projectile.rotation * 1.2f,
                origin2,
                scale * 0.85f,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 简单 AABB，覆盖：星体大小
            return projHitbox.Intersects(targetHitbox);
        }
    }
}
