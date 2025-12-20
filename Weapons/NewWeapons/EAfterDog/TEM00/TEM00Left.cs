using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.ID;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Physics;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod.Physics; // ← 来自 Calamity 的 Rope 系统
using CalamityMod.Graphics.Primitives;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny; // ← 用于渲染 trail

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Left : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";
        public override void SetStaticDefaults()
        {
            // 设置弹幕拖尾长度和模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        // ===== 描边控制 =====
        private float outlineFlash = 0f;  // 单次激光的白色描边闪烁强度
        private bool rainbowOutline = false; // 是否进入彩虹常驻描边模式


        // 这一段是跟飘带相关【顶端继承IPixelatedPrimitiveRenderer】：==================================================================================================================================
        // ========== 新增字段 ==========
        private RopeHandle? leftRibbon;   // 左边飘带
        private RopeHandle? rightRibbon;  // 右边飘带
        private ref float Time => ref Projectile.ai[1]; // 用来作为波动的时间基准（不要复用 ai[0]，避免和你的状态机冲突）

        // 飘带长度（和 Sylvestaff 一样）
        private static float RibbonLength => 70f;

        // 飘带挂点：在武器顶端（随便定义一个合适的位置）
        private Vector2 RibbonAttachPoint => Projectile.Center + Projectile.velocity * Projectile.scale * Projectile.width * 0.34f;


        // 规定在哪个图层绘制飘带（前后都绘制）
        public PixelationPrimitiveLayer LayerToRenderTo =>
            PixelationPrimitiveLayer.BeforeProjectiles | PixelationPrimitiveLayer.AfterPlayers;

        // 真正的飘带绘制放这里，而不是 PreDraw
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, PixelationPrimitiveLayer layer)
        {
            bool backLayer = layer == PixelationPrimitiveLayer.BeforeProjectiles;

            // 开启这两句话就能启用了，现在暂时将机关闭------------------------------------------------------------------------------------------------------
            //RenderRibbon(leftRibbon, -1, backLayer);
            //RenderRibbon(rightRibbon, 1, backLayer);
        }


        // ai函数的加上这些:
        /*
                     // 初始化飘带（第一次进入时）
            if (leftRibbon is null && rightRibbon is null)
                InitializeRibbons();

            // 更新飘带逻辑
            UpdateRibbon(leftRibbon, Projectile.velocity.RotatedBy(-MathHelper.PiOver2));
            UpdateRibbon(rightRibbon, Projectile.velocity.RotatedBy(MathHelper.PiOver2));
         */



        // 初始化飘带
        private void InitializeRibbons()
        {
            int ribbonSegmentCount = 12; // 段数
            float distancePerSegment = RibbonLength / ribbonSegmentCount;

            RopeSettings ribbonSettings = new RopeSettings()
            {
                StartIsFixed = true,         // 起点固定在武器上
                Mass = 0.72f,                // 段的质量
                RespondToEntityMovement = true,
                RespondToWind = true
            };

            leftRibbon = ModContent.GetInstance<RopeManagerSystem>()
                .RequestNew(RibbonAttachPoint, Projectile.Center, ribbonSegmentCount, distancePerSegment, Vector2.Zero, ribbonSettings, 25);
            rightRibbon = ModContent.GetInstance<RopeManagerSystem>()
                .RequestNew(RibbonAttachPoint, Projectile.Center, ribbonSegmentCount, distancePerSegment, Vector2.Zero, ribbonSettings, 25);
        }

        // 更新单条飘带的受力（保持向后拖拽）
        private void UpdateRibbon(RopeHandle? ribbon, Vector2 gravityDirection)
        {
            if (ribbon is not RopeHandle rope)
                return;

            rope.Start = RibbonAttachPoint;
            rope.Gravity = gravityDirection * 0.15f - Projectile.velocity * 0.4f;
        }


        private void RenderRibbon(RopeHandle? ribbon, int direction, bool backLayer)
        {
            if (ribbon is not RopeHandle rope)
                return;

            Vector2 forwardDirection = Projectile.velocity;
            Vector2 sideDirection = forwardDirection.RotatedBy(MathHelper.PiOver2 * direction);
            Vector2 attachmentPoint = RibbonAttachPoint;

            Vector2[] ribbonPositions = [.. rope.Positions];
            int positionCount = ribbonPositions.Length;

            for (int i = 0; i < ribbonPositions.Length; i++)
            {
                float completionRatio = i / (float)positionCount;
                float wave = MathF.Cos(MathHelper.Pi * completionRatio * 1.5f - MathHelper.TwoPi * Time / 97f) * completionRatio;

                Vector2 backwardsOffset = forwardDirection * i * -RibbonLength / positionCount;
                Vector2 sideWavyOffset = sideDirection * wave * RibbonLength * 0.5f;
                Vector2 rigidPosition = attachmentPoint + backwardsOffset + sideWavyOffset;

                ribbonPositions[i] = Vector2.Lerp(ribbonPositions[i], rigidPosition, 0.76f);
            }

            // 用 Calamity 的 shader
            MiscShaderData ribbonShader = GameShaders.Misc["CalamityMod:SylvestaffRibbon"];
            ribbonShader.UseShaderSpecificData(new Vector4(0f, 0f, sideDirection.X, sideDirection.Y));
            ribbonShader.UseSaturation(backLayer ? -1f : 1f);

            PrimitiveSettings primitiveSettings = new PrimitiveSettings(
                RibbonWidthFunction,
                RibbonColorFunction,
                pixelate: true,
                shader: ribbonShader
            );

            PrimitiveRenderer.RenderTrail(ribbonPositions, primitiveSettings, 33);
        }


        // 飘带宽度
        private float RibbonWidthFunction(float completionRatio) =>
            Projectile.scale * Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * 3.6f;

        // 飘带颜色（取光照）
        private Color RibbonColorFunction(float completionRatio)
        {
            Color light = Lighting.GetColor(RibbonAttachPoint.ToTileCoordinates());
            return Projectile.GetAlpha(light);
        }

        // 结束飘带相关的：==================================================================================================================================










        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            bool facingLeft = Projectile.velocity.X < 0;
            SpriteEffects effects = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float drawRotation = Projectile.rotation + (facingLeft ? MathHelper.PiOver2 : 0f);

            // =================================================
            // ① 立方体中心：玩家正中心（世界坐标）
            // =================================================
            Vector2 cubeCenter = Owner.Center;

            // =================================================
            // ② 立方体参数（边长 = X × 16）
            // =================================================
            float halfSize = 12.5f * 16f;
            float t = Main.GlobalTimeWrappedHourly;

            // 稳定、缓慢的三轴旋转
            float yaw = t * 0.8f;
            float pitch = t * 0.6f;
            float roll = t * 0.4f;

            Matrix rot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            // 近似正交的透视参数（保证可见、不中心漂移）
            float focal = 1200f;
            float zBias = 1200f;

            // =================================================
            // ③ 单位立方体顶点
            // =================================================
            Vector3[] v =
            {
                new(-1,-1,-1), new( 1,-1,-1), new( 1, 1,-1), new(-1, 1,-1),
                new(-1,-1, 1), new( 1,-1, 1), new( 1, 1, 1), new(-1, 1, 1)
            };

            int[,] e =
            {
                {0,1},{1,2},{2,3},{3,0},
                {4,5},{5,6},{6,7},{7,4},
                {0,4},{1,5},{2,6},{3,7}
            };

            // =================================================
            // ④ 3D → 2D 投影（世界坐标）
            // =================================================
            Vector2[] pv = new Vector2[8];

            for (int i = 0; i < 8; i++)
            {
                Vector3 p = Vector3.Transform(v[i] * halfSize, rot);
                float persp = focal / (focal + p.Z + zBias);
                pv[i] = cubeCenter + new Vector2(p.X, p.Y) * persp;
            }

            // =================================================
            // ⑤ 中心纠偏：确保立方体几何中心 = 玩家中心
            // =================================================
            Vector2 projectedCenter = Vector2.Zero;
            for (int i = 0; i < 8; i++)
                projectedCenter += pv[i];
            projectedCenter /= 8f;

            Vector2 fix = cubeCenter - projectedCenter;
            for (int i = 0; i < 8; i++)
                pv[i] += fix;

            // =================================================
            // ⑥ 蓝色线框立方体绘制
            // =================================================
            Color lineColor = Color.Lerp(
                new Color(80, 200, 255),
                Color.White,
                0.5f + 0.5f * (float)Math.Sin(t * 3f)
            );

            for (int i = 0; i < 12; i++)
            {
                Vector2 a = pv[e[i, 0]];
                Vector2 b = pv[e[i, 1]];
                Main.spriteBatch.DrawLineBetter(a, b, lineColor, 7f);
            }

            // =================================================
            // ⑦ 原有描边（完全不动）
            // =================================================
            float chargeOffset = 6f;
            int segments = 16;

            for (int i = 0; i < segments; i++)
            {
                Vector2 offset =
                    (MathHelper.TwoPi * i / segments).ToRotationVector2() * chargeOffset;

                Color edgeColor;
                if (rainbowOutline)
                {
                    float hue =
                        (Main.GlobalTimeWrappedHourly * 0.5f + i / (float)segments) % 1f;
                    edgeColor = Main.hslToRgb(hue, 0.8f, 0.6f) * 0.7f;
                }
                else
                {
                    edgeColor = Color.White * outlineFlash;
                }

                edgeColor.A = 0;
                Main.spriteBatch.Draw(
                    tex,
                    drawPos + offset,
                    null,
                    edgeColor,
                    drawRotation,
                    origin,
                    Projectile.scale,
                    effects,
                    0f
                );
            }

            // =================================================
            // ⑧ 本体绘制
            // =================================================
            Main.EntitySpriteDraw(
                tex,
                drawPos,
                null,
                Projectile.GetAlpha(lightColor),
                drawRotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }





        //    public override void DrawBehind(int index, List<int> behindNPCsAndTiles,
        //List<int> behindNPCs, List<int> behindProjectiles,
        //List<int> overPlayers, List<int> overWiresUI)
        //    {
        //        // 碎片始终算作“上层”，压在魔法阵之上
        //        overPlayers.Add(index);
        //    }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1; // 调高这个值可以让弹幕更加顺滑的跟随鼠标
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

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

        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            // 初始化飘带（第一次进入时）
            if (leftRibbon is null && rightRibbon is null)
                InitializeRibbons();

            // 更新飘带逻辑
            UpdateRibbon(leftRibbon, Projectile.velocity.RotatedBy(-MathHelper.PiOver2));
            UpdateRibbon(rightRibbon, Projectile.velocity.RotatedBy(MathHelper.PiOver2));


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






        private int chargeTimer = 0; // 在类里新建字段
        private int chargeCount = 0; // 已经触发几次（最多 8）


                                     // ===== 攻击控制 =====
        private int attackPhase = 0;    // 当前第几轮攻击 (0~4, 共5轮)
        private int shotsThisPhase = 0; // 本轮需要发射多少发
        private int shotsFired = 0;     // 本轮已发射多少发
        private int fireCooldown = 0;   // 单发冷却
        private int phaseCooldown = 0;  // 轮与轮之间的间隔
        private bool specialAttack = false; // 是否进入特殊攻击阶段
        
        // 记录最后生成的超级激光弹幕ID；-1 表示未生成/已失效
        private int superLaserId = -1;
        // 扇形喷发的节流计时（例如每帧都喷，或每2帧喷一次）
        private int backBurstTicker = 0;

        private void DoBehavior_Aim() // 瞄准阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width);
            Owner.heldProj = Projectile.whoAmI;

            // 枪头位置 [这很重要，因为许多特效都需要和他相关]
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            // 如果武器自身会高速旋转 [比如巨龙之怒] ，那么枪头需要改成这个来适配
            float fixedRotation = Projectile.rotation; // 可根据需求加减角度偏移
            headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(fixedRotation);



            {
                // ====== 攻击流程逻辑 ======
                if (!specialAttack)
                {
                    if (phaseCooldown > 0)
                    {
                        phaseCooldown--;
                    }
                    else
                    {
                        if (shotsThisPhase == 0)
                        {
                            // 初始化本轮射击
                            attackPhase++;
                            if (attackPhase <= 5)
                            {
                                shotsThisPhase = Main.rand.Next(10, 16); // 10~15发
                                shotsFired = 0;
                            }
                            else
                            {
                                // 五轮打完 → 进入特殊攻击阶段（暂时留空）
                                // ===== 当五轮结束，进入特殊攻击（只在此刻生成一次超级激光） =====
                                specialAttack = true;

                                // 生成超级激光（只生成一次）
                                // 把生成位置放在枪口 headPosition，上方发射方向以当前朝向为准
                                int laserProj = Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition, // 激光出生点：弹幕顶端（你已经算好了 headPosition）
                                    Projectile.velocity.SafeNormalize(Vector2.UnitY), // 方向（单位向量）——激光类会根据 owner 同步方向
                                    ModContent.ProjectileType<TEM00LeftSuperLazer>(),
                                    (int)(Projectile.damage * 5), // 可以按需调整伤害
                                    0f,
                                    Projectile.owner
                                );

                                // 控制彩虹描边
                                rainbowOutline = true;

                                // 绑定父弹幕索引：把 ai[0] 设为当前父弹幕索引（this.whoAmI）
                                if (laserProj >= 0 && laserProj < Main.maxProjectiles)
                                {
                                    Main.projectile[laserProj].ai[0] = Projectile.whoAmI; // 告诉激光它的“父弹幕”是谁
                                    Main.projectile[laserProj].netUpdate = true; // 多人时同步
                                                                                 // 可选：立即把激光的朝向和速度与父弹幕匹配（便于首帧视觉一致）
                                    Main.projectile[laserProj].rotation = Projectile.rotation - MathHelper.PiOver4;
                                    Main.projectile[laserProj].velocity = (Main.projectile[laserProj].rotation).ToRotationVector2();
                                }

                                superLaserId = laserProj;

                                // 屏幕震动
                                float shakePower = 95f;
                                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                                    Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


                                // === 超级魔法阵·一次性后扇形喷发（只在本地拥有者触发，避免多人重复）===
                                if (Main.myPlayer == Projectile.owner)
                                {
                                    // 枪口与方向（与你上方一致）
                                    Vector2 muzzle = headPosition;
                                    Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                                    Vector2 back = -forward;

                                    // 只在后方 180°：以 back 为轴，±90° 的扇形
                                    float coneHalf = MathHelper.PiOver2; // 90°

                                    // 调色板（科技蓝家族）
                                    Color[] techBlue =
                                    {
                                        new Color( 80, 200, 255),
                                        new Color(120, 220, 255),
                                        Color.Cyan,
                                        new Color(180, 220, 255),
                                        Color.WhiteSmoke
                                    };

                                    // ========== “1.5×宏伟度”的一次性强喷（数量、速度全面抬升） ==========
                                    // SquishyLight（EXO高亮喷焰）：粗壮主束感
                                    int exoCount = 32 + Main.rand.Next(10); // 32~41（持续喷发版的 ~1.5×）
                                                                            // Spark（线性火花）：锐利刀锋
                                    int sparkCount = 56 + Main.rand.Next(16); // 56~71
                                                                              // GlowOrb（柔性辉光）：对数螺旋点缀两臂，提供数学骨架
                                    int spiralArms = 2;
                                    int orbPerArm = 18 + Main.rand.Next(6);  // 18~23

                                    // 速度全面提升到“持续背喷”的 ≥1.5×
                                    // （持续背喷示例：EXO 14~26、Spark 18~34、Orb 10~18）
                                    Func<float, float, float> R = (a, b) => Main.rand.NextFloat(a, b);
                                    // 1) EXO：强亮扇形主喷
                                    for (int i = 0; i < exoCount; i++)
                                    {
                                        // 为了“半规则”的美感：分层+分段取角，既均匀又有随机
                                        float u = (i + Main.rand.NextFloat()) / exoCount;            // 0~1
                                        float ang = MathHelper.Lerp(-coneHalf, coneHalf, u);         // 均匀覆盖 180°
                                        Vector2 dir = back.RotatedBy(ang + Main.rand.NextFloat(-0.08f, 0.08f));

                                        var exo = new SquishyLightParticle(
                                            muzzle,
                                            dir * R(21f, 39f),               // ★ 比持续背喷快 ~1.5×
                                            R(0.30f, 0.46f),                 // 体积略大
                                            techBlue[Main.rand.Next(techBlue.Length)],
                                            Main.rand.Next(18, 28),          // 寿命中等
                                            opacity: 1f,
                                            squishStrenght: 1f,
                                            maxSquish: R(2.6f, 3.6f),
                                            hueShift: 0f
                                        );
                                        GeneralParticleHandler.SpawnParticle(exo);
                                    }

                                    // 2) Spark：刀锋形火花（极快、偏直线）
                                    for (int i = 0; i < sparkCount; i++)
                                    {
                                        float u = (i + 0.5f * (i % 2)) / sparkCount;                 // 轻微锯齿排列
                                        float ang = MathHelper.Lerp(-coneHalf, coneHalf, u);
                                        Vector2 baseDir = back.RotatedBy(ang);
                                        Vector2 jitter = baseDir.RotatedBy(Main.rand.NextFloat(-0.17f, 0.17f)); // 细小抖动

                                        var sp = new SparkParticle(
                                            muzzle,
                                            jitter * R(27f, 51f),            // ★ 速度更快
                                            false,
                                            Main.rand.Next(16, 26),
                                            R(0.9f, 1.5f),
                                            Color.Lerp(techBlue[Main.rand.Next(techBlue.Length)], Color.White, 0.35f)
                                        );
                                        GeneralParticleHandler.SpawnParticle(sp);
                                    }

                                    // 3) GlowOrb：两臂对数螺旋（数学美学骨架）
                                    // r = r0 * e^(k * t), theta 从 0 -> ±90°，两臂对称
                                    float r0 = 14f;
                                    float k = 0.035f; // 增长系数（更优雅，别太大）
                                    for (int arm = 0; arm < spiralArms; arm++)
                                    {
                                        float sign = (arm == 0) ? 1f : -1f;
                                        for (int j = 0; j < orbPerArm; j++)
                                        {
                                            float t = j / (float)(orbPerArm - 1);
                                            float theta = sign * MathHelper.Lerp(0f, coneHalf, t) + Main.rand.NextFloat(-0.05f, 0.05f);
                                            float r = r0 * (float)Math.Exp(k * t * 90f); // 半径缓慢外扩
                                            Vector2 dir = back.RotatedBy(theta);
                                            Vector2 pos = muzzle + dir * r;

                                            var orb = new GlowOrbParticle(
                                                pos,
                                                dir * R(15f, 27f),           // ★ 速度抬高到 ≥1.5×
                                                false,
                                                Main.rand.Next(10, 16),
                                                R(0.9f, 1.5f),
                                                techBlue[(arm + j) % techBlue.Length],
                                                true, false, true
                                            );
                                            GeneralParticleHandler.SpawnParticle(orb);
                                        }
                                    }

                                    // （可选）给一次性爆发一个更厚重的音色
                                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, muzzle);
                                }



                            }
                        }

                        if (shotsThisPhase > 0)
                        {
                            if (fireCooldown > 0)
                                fireCooldown--;
                            else
                            {
                                // 发射一发激光
                                headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48f;

                                // 基础方向：正前方
                                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                                // 在 ±10° (即 20°范围内) 随机偏移
                                dir = dir.RotatedByRandom(MathHelper.ToRadians(10f));

                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition,
                                    dir,
                                    ModContent.ProjectileType<TEM00LeftLazer>(),
                                    (int)(Projectile.damage * 0.5),
                                    0f,
                                    Projectile.owner
                                );

                                // 瞬间开启白色描边
                                outlineFlash = 1f;

                                // 屏幕震动
                                float shakePower = 5f;
                                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                                    Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

                                shotsFired++;
                                fireCooldown = 6; // 单发之间的间隔（你可以调大或调小）

                                if (shotsFired >= shotsThisPhase)
                                {
                                    // 本轮结束 → 设置间隔时间
                                    shotsThisPhase = 0;
                                    phaseCooldown = 30; // 两轮之间的间隔（可以调整）
                                }
                            }
                        }
                    }
                }
                else
                {
                    // === 超级激光持续期间，从枪口向左后/右后两个扇形喷发高速粒子 ===


                    //SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/拉链闪电") with { Volume = 0.95f, Pitch = -0.2f }, Projectile.Center);

                    // ① 确认超级激光仍然存活（存在且类型匹配），否则不喷发
                    bool laserAlive = superLaserId >= 0 && superLaserId < Main.maxProjectiles
                                      && Main.projectile[superLaserId].active
                                      && Main.projectile[superLaserId].type == ModContent.ProjectileType<TEM00LeftSuperLazer>();
                    if (!laserAlive)
                        return;

                    // ② 计算枪口位置（与你上方一致）
                    headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (16f * 3f);
                    //fixedRotation = Projectile.rotation;
                    //headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(fixedRotation);

                    // ③ 基方向：前=dir，后= -dir；左后/右后锥基向量（±45°）
                    Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Vector2 back = -dir;
                    Vector2 leftBackBase = back.RotatedBy(MathHelper.PiOver4);   // 后方 + 左 45°
                    Vector2 rightBackBase = back.RotatedBy(-MathHelper.PiOver4);   // 后方 + 右 45°
                    float coneHalf = MathHelper.ToRadians(24f); // 扇形半角（可调：20°~30°）

                    // ④ 颜色池（科技蓝家族）
                    Color[] techBlue =
                    {
        new Color( 80, 200, 255),
        new Color(120, 220, 255),
        Color.Cyan,
        new Color(180, 220, 255),
        Color.WhiteSmoke
    };

                    // ⑤ 节流（例如每帧都喷；若太炸可改为 %2==0）
                    backBurstTicker++;

                    // 单帧内：每个扇形各喷一组（“疯狂版”数量；若担心性能可把 *Count 降低）
                    void BurstCone(Vector2 baseDir)
                    {
                        // a) EXO（SquishyLightParticle）：强亮、速度极快
                        int exoCount = 4 + Main.rand.Next(3); // 4~6
                        for (int i = 0; i < exoCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(14f, 26f); // 非常快
                            var exo = new SquishyLightParticle(
                                headPosition,
                                v,
                                Main.rand.NextFloat(0.28f, 0.42f),
                                techBlue[Main.rand.Next(techBlue.Length)],
                                Main.rand.Next(18, 26),
                                opacity: 1f,
                                squishStrenght: 1f,
                                maxSquish: Main.rand.NextFloat(2.4f, 3.4f),
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }

                        // b) SparkParticle：线性火花，刀锋感强
                        int sparkCount = 8 + Main.rand.Next(6); // 8~13
                        for (int i = 0; i < sparkCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(18f, 34f);
                            var sp = new SparkParticle(
                                headPosition,
                                v,
                                false,
                                Main.rand.Next(14, 22),
                                Main.rand.NextFloat(0.7f, 1.2f),
                                Color.Lerp(techBlue[Main.rand.Next(techBlue.Length)], Color.White, 0.35f)
                            );
                            GeneralParticleHandler.SpawnParticle(sp);
                        }

                        // c) GlowOrb：柔性辉光，补充层次（数量略少）
                        int orbCount = 4 + Main.rand.Next(3); // 4~6
                        for (int i = 0; i < orbCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(10f, 18f);
                            var orb = new GlowOrbParticle(
                                headPosition + v.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(6f, 18f),
                                v * Main.rand.NextFloat(0.85f, 1.15f),
                                false,
                                Main.rand.Next(8, 14),
                                Main.rand.NextFloat(0.9f, 1.5f),
                                techBlue[Main.rand.Next(techBlue.Length)],
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // 左后扇形 + 右后扇形
                    BurstCone(leftBackBase);
                    BurstCone(rightBackBase);
                }



            }


            // 快速衰减白色描边
            if (outlineFlash > 0f)
                outlineFlash *= 0.9f; // 每帧衰减，逐渐消失


            // 松手后进入 Dash
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 80;
                Projectile.penetrate = -1; // 可调穿透次数
                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/新机炮") with { Volume = 0.95f, Pitch = -0.2f }, Projectile.Center);


                {
                    // 屏幕震动
                    float shakePower = 95f;
                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                        Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
                }

                CurrentState = BehaviorState.Dash;
            }
        }






        private int dashFrameCounter = 0; // 在类里新建计数器字段

        private void DoBehavior_Dash() // 冲刺阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 设置冲刺速度
            float initialSpeed = 55f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * initialSpeed;

            // 每帧计数
            dashFrameCounter++;

            if (dashFrameCounter % 2 == 0 && Main.myPlayer == Projectile.owner)
            {
                // ====== 1. 在自己正下方随机位置生成一发激光 ======
                float xOffset = Main.rand.NextFloat(-120f, 120f); // 左右随机
                float yOffset = Main.rand.NextFloat(35 * 16f, 35 * 16f);  // 在正下方一定范围
                Vector2 spawnPos = Projectile.Center + new Vector2(xOffset, yOffset);

                // 方向：指向本体  
                Vector2 dir = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dir,
                    ModContent.ProjectileType<TEM00LeftLazer>(),
                    (int)(Projectile.damage * 5),
                    0f,
                    Projectile.owner
                );

                SoundEngine.PlaySound(SoundID.Item33, spawnPos);

                // ====== 2. 疯狂的飞行特效 ======
                for (int i = 0; i < 6; i++) // 比平时多，显得更夸张
                {
                    // Square 方块能量片
                    SquareParticle sq = new SquareParticle(
                        spawnPos,
                        dir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f),
                        false,
                        18 + Main.rand.Next(12),
                        1.5f + Main.rand.NextFloat(0.8f),
                        new Color(90, 200, 255) * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(sq);

                    // GlowOrb 光点
                    GlowOrbParticle orb = new GlowOrbParticle(
                        spawnPos,
                        dir * Main.rand.NextFloat(1f, 3f),
                        false,
                        6,
                        0.9f + Main.rand.NextFloat(0.5f),
                        new Color(120, 220, 255),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {
            leftRibbon?.Dispose();
            rightRibbon?.Dispose();
        }



    }
}
