using System;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Graphics.Primitives;
using CalamityMod;
using Terraria.Graphics.Shaders;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC
{
    public class TenebreusTidesJavWaterSword : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public Color TrailColor(float completionRatio, Vector2 vertexPos)
        {
            float opacity = Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * Projectile.Opacity;
            return new Color(40, 120, 240) * opacity; // 深海蓝渐隐
        }

        public float TrailWidth(float completionRatio, Vector2 vertexPos)
        {
            return MathHelper.SmoothStep(16f, 26f, completionRatio);
        }



        public override bool PreDraw(ref Color lightColor)
        {
            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TenebreusTidesC/TenebreusTidesJavWaterSword").Value;

            //// === 新增：深海 Shader 拖尾层 ===
            //Main.spriteBatch.EnterShaderRegion();

            //GameShaders.Misc["ModNamespace:TrailWarpDistortion"]
            //    .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/spark_07"))
            //    .UseColor(new Color(40, 100, 200)) // 深海主色
            //    .UseSecondaryColor(new Color(100, 200, 255)) // 次光色
            //    .Apply();

            //PrimitiveRenderer.RenderTrail(
            //    Projectile.oldPos,
            //    new(TrailWidth, (completionRatio, vertexPos) => TrailColor(completionRatio), (completionRatio, vertexPos) => Projectile.Size * 0.5f, shader: GameShaders.Misc["ModNamespace:TrailWarpDistortion"]),
            //    10
            //);

            //Main.spriteBatch.ExitShaderRegion();


            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                .UseColor(new Color(40, 100, 200))
                .UseSecondaryColor(new Color(100, 200, 255))
                .Apply();

            // 2.1 大更新之后的修改:
            // 先说一下原有的版本，但是中间需要改成双参数
            //PrimitiveRenderer.RenderTrail(
            //    Projectile.oldPos,
            //    new(
            //        ratio => MathHelper.SmoothStep(16f, 4f, ratio) * 1.0f, // ← 这里 * Xf 可以控制梯形的宽窄程度
            //        TrailColor,
            //        (_) => Projectile.Size * 0.5f,
            //        shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
            //    ),
            //    10
            //);

            // 我们改成了这样的
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new PrimitiveSettings(
                    (completionRatio, vertexPos) => MathHelper.SmoothStep(16f, 4f, completionRatio) * 1.0f,
                    (completionRatio, vertexPos) => TrailColor(completionRatio, vertexPos),
                    (completionRatio, vertexPos) => Projectile.Size * 0.5f,
                    shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                ),
                10
            );

            //新版 PrimitiveSettings 委托签名变成：

            //float Width(float completionRatio, Vector2 vertexPos)
            //Color Color(float completionRatio, Vector2 vertexPos)
            //Vector2 Offset(float completionRatio, Vector2 vertexPos)

            //你原来只传 1 个参数的 lambda，现在必须传 2 个。

            //vertexPos 不用也要写出来。




            //ratio =>
            //替换成
            //(completionRatio, vertexPos) =>

            //ratio
            //替换成
            //completionRatio

            //(completionRatio, vertexPos) => TrailColor(completionRatio),
            //替换成
            //(completionRatio, vertexPos) => TrailColor(completionRatio),

            //(_) =>
            //替换成
            //(completionRatio, vertexPos) =>




            Main.spriteBatch.ExitShaderRegion();



            // === 原有：高质量贴图拖尾层 ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.DarkBlue, Color.MidnightBlue, colorInterpolation) * 0.4f;
                color.A = 0;

                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                Color outerColor = color;
                Color innerColor = color * 0.5f;

                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f;
                }

                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // === 本体绘制 ===
            Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            // === 保留：水能脉动圈绘制 ===
            float pulseTime = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.identity);
            float pulseScale = 1.1f + 0.15f * pulseTime;
            float pulseOpacity = 0.3f + 0.15f * pulseTime;

            Color pulseColor = Color.Lerp(Color.MediumBlue, Color.LightBlue, 0.5f + 0.5f * pulseTime) * pulseOpacity;
            pulseColor.A = 0;

            Texture2D pulseTex = lightTexture;
            Vector2 pulseOrigin = pulseTex.Size() * 0.5f;
            Vector2 pulsePos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            Main.EntitySpriteDraw(pulseTex, pulsePos, null, pulseColor, Projectile.rotation, pulseOrigin, Projectile.scale * pulseScale * 1.25f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pulseTex, pulsePos, null, pulseColor * 1.3f, Projectile.rotation, pulseOrigin, Projectile.scale * pulseScale * 0.7f, SpriteEffects.None, 0);

            return false;
        }


        private int penetrationAmt = 2;
        private bool dontDraw = false;
        private int drawInt = 0;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = penetrationAmt;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5 * Projectile.MaxUpdates;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(penetrationAmt);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            penetrationAmt = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 🌊 在出生时制造水流逆喷粒子特效（深海冲击感）
            Vector2 dir = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
            for (int i = 0; i < 14; i++)
            {
                float rot = MathHelper.Lerp(-0.8f, 0.8f, i / 13f); // 扇形角度
                Vector2 particleDir = dir.RotatedBy(rot) * Main.rand.NextFloat(2.5f, 4.5f);

                Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Water : DustID.BlueTorch, particleDir, 150,
                    Color.Lerp(Color.DarkBlue, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }
        int[] deepSeaDustTypes = { 29, 104, 186 };
        public override void AI()
        {
            // 弹幕的速度每帧乘以 1.01，逐渐加速
            Projectile.velocity *= 1.01f;

            {
                //// 添加小型烟雾粒子
                //Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 使用之前定义的颜色渐变
                //Particle smoke = new HeavySmokeParticle(
                //    Projectile.Center,
                //    Projectile.velocity * Main.rand.NextFloat(-0.2f, -0.6f),
                //    smokeColor,
                //    30, // 粒子存活时间
                //    Main.rand.NextFloat(0.45f, 0.6f), // 粒子缩放大小
                //    0.3f,
                //    Main.rand.NextFloat(-0.2f, 0.2f),
                //    false,
                //    required: true
                //);
                //GeneralParticleHandler.SpawnParticle(smoke);

                //// 添加双螺旋粒子特效
                //float progress = (Projectile.localAI[0] % 60) / 60f; // 粒子进度控制
                //float angle1 = MathHelper.TwoPi * progress; // 第一条螺旋
                //float angle2 = MathHelper.TwoPi * (progress + 0.5f); // 第二条螺旋，相差 180 度
                //Vector2 offset1 = new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * 10f; // 第一条螺旋的偏移
                //Vector2 offset2 = new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * 10f; // 第二条螺旋的偏移

                //// 第一条螺旋的粒子
                //Dust dust1 = Dust.NewDustPerfect(Projectile.Center + offset1, DustID.Water, Projectile.velocity * 0.2f, 0, Color.DarkBlue, 1.2f);
                //dust1.noGravity = true;

                //// 第二条螺旋的粒子
                //Dust dust2 = Dust.NewDustPerfect(Projectile.Center + offset2, DustID.Water, Projectile.velocity * 0.2f, 0, Color.CadetBlue, 1.2f);
                //dust2.noGravity = true;

            }


            // 🌊 深海前导粒子（前方生成 + 左右高速扩散）
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 lateral = forward.RotatedBy(MathHelper.PiOver2); // 左右扩散方向
            Vector2 frontPos = Projectile.Center + forward * 24f; // 前方生成点（可调）

            for (int i = 0; i < 2; i++) // 每帧生成 X 个前导粒子
            {
                Vector2 spawnPos = frontPos + Main.rand.NextVector2Circular(6f, 6f); // 稍微分散
                int dustType = deepSeaDustTypes[Main.rand.Next(deepSeaDustTypes.Length)];

                float sideSpeed = Main.rand.NextFloat(2.5f, 5.0f); // 高速扩散
                Vector2 velocity = lateral * Main.rand.NextFloat(-sideSpeed, sideSpeed); // 左右扩散
                velocity += Main.rand.NextVector2Circular(0.3f, 0.3f); // 加些扰动

                Dust d = Dust.NewDustPerfect(spawnPos, dustType, velocity, 100,
                    Color.Lerp(Color.LightBlue, Color.DarkBlue, Main.rand.NextFloat()), Main.rand.NextFloat(0.9f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 0.2f;
            }



            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            // 如果弹幕没有击中任何东西
            if (Projectile.ai[0] == 0f)
            {
                Projectile.localAI[0] += 1f;
                if (Projectile.localAI[0] > 7f)
                {
                    int water = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 0.4f);
                    Main.dust[water].noGravity = true;
                    Main.dust[water].velocity *= 0.5f;
                    Main.dust[water].velocity += Projectile.velocity * 0.1f;
                }

                float scalar = 0.01f;
                int alphaAmt = 5;
                int alphaCeiling = alphaAmt * 15;
                int alphaFloor = 0;

                if (Projectile.localAI[0] > 7f)
                {
                    if (Projectile.localAI[1] == 0f)
                    {
                        Projectile.scale -= scalar;
                        Projectile.alpha += alphaAmt;
                        if (Projectile.alpha > alphaCeiling)
                        {
                            Projectile.alpha = alphaCeiling;
                            Projectile.localAI[1] = 1f;
                        }
                    }
                    else if (Projectile.localAI[1] == 1f)
                    {
                        Projectile.scale += scalar;
                        Projectile.alpha -= alphaAmt;
                        if (Projectile.alpha <= alphaFloor)
                        {
                            Projectile.alpha = alphaFloor;
                            Projectile.localAI[1] = 0f;
                        }
                    }
                }
            }

            // 弹幕在命中敌人后会开始追踪，并返回攻击相同的敌人
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.alpha += 15;
                Projectile.velocity *= 0.98f;
                Projectile.localAI[0] = 0f;

                if (Projectile.alpha >= 255)
                {
                    // 寻找最近的敌人以追踪
                    int whoAmI = -1;
                    Vector2 targetSpot = Projectile.Center;
                    float detectRange = 700f;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.CanBeChasedBy(Projectile, false))
                        {
                            float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                            if (targetDist < detectRange)
                            {
                                detectRange = targetDist;
                                targetSpot = npc.Center;
                                whoAmI = npc.whoAmI;
                            }
                        }
                    }

                    // 如果找到敌人，则追踪返回
                    if (whoAmI >= 0)
                    {
                        Projectile.netUpdate = true;
                        Projectile.ai[0] = 2f; // 标记为第二次攻击
                        Projectile.position = targetSpot + ((float)Main.rand.NextDouble() * 6.28318548f).ToRotationVector2() * 100f - new Vector2(Projectile.width, Projectile.height) / 2f;
                        Projectile.velocity = Vector2.Normalize(targetSpot - Projectile.Center) * 18f; // 加速追踪敌人
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                // 第二次攻击逻辑，追踪敌人并继续攻击
                Projectile.scale = 0.9f;
                Projectile.ai[1] += 1f;

                if (Projectile.ai[1] >= 15f)
                {
                    Projectile.alpha += 51;
                    Projectile.velocity *= 0.8f;

                    if (Projectile.alpha >= 255)
                        Projectile.Kill();
                }
                else
                {
                    Projectile.alpha -= 125;
                    if (Projectile.alpha < 0)
                        Projectile.alpha = 0;

                    Projectile.velocity *= 0.98f;
                }

                Projectile.localAI[0] += 1f;

                int water = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 0.4f);
                Main.dust[water].noGravity = true;
                Main.dust[water].velocity *= 0.5f;
                Main.dust[water].velocity += Projectile.velocity * 0.1f;
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 0f, 0f, (255 - Projectile.alpha) * 1f / 255f);
        }

        public override Color? GetAlpha(Color lightColor) => new Color(50, 50, 255, Projectile.alpha);

     
        public override bool? CanDamage()
        {
            // Do not do damage if a tile is hit OR if projectile has 'split' and hasn't been live for more than 5 frames
            if (((int)(Projectile.ai[0] - 1f) / penetrationAmt == 0 && penetrationAmt < 3 || Projectile.ai[1] < 5f) && Projectile.ai[0] != 0f)
                return false;
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 弹幕击中敌人后，穿透一次并获得追踪能力，准备第二次攻击
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f; // 标记为第一次命中
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[0] = 2f; // 标记为第二次命中
            }

            Projectile.ai[1] = 0f;
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
