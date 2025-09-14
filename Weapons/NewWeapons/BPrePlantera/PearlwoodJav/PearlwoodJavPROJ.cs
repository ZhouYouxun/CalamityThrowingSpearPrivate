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
using Terraria.ModLoader.IO;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/PearlwoodJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 65;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        //private float PrimitiveWidthFunction(float completionRatio)
        //{
        //    float width = 14f;
        //    float minWidth = 0.03f;
        //    float maxWidth = width;

        //    if (completionRatio <= 0.3f) // 让拖尾前段变细
        //        width = MathHelper.Lerp(minWidth, maxWidth, Utils.GetLerpValue(0f, 0.3f, completionRatio, true));

        //    return width;
        //}

        private float PrimitiveWidthFunction(float completionRatio)
        {
            return 8f; // 始终保持 X 像素宽度
        }

        //// 长度彩虹色
        //private Color PrimitiveColorFunction(float completionRatio)
        //{
        //    float timeFactor = Main.GlobalTimeWrappedHourly * 4.0f;
        //    float flicker = (float)Math.Sin(completionRatio * 6f + timeFactor) * 0.5f + 0.5f;

        //    // 彩虹混合色
        //    Color[] rainbowColors = new Color[]
        //    {
        //        Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet
        //    };

        //    int index = (int)(completionRatio * rainbowColors.Length) % rainbowColors.Length;
        //    Color startColor = Color.Lerp(rainbowColors[index], rainbowColors[(index + 1) % rainbowColors.Length], flicker);
        //    return Color.Lerp(startColor, Color.Transparent, MathHelper.SmoothStep(0f, 1f, completionRatio));
        //}

        // 长度方向的粉红色系渐变
        private Color PrimitiveColorFunction(float completionRatio)
        {
            float timeFactor = Main.GlobalTimeWrappedHourly * 4.0f;
            float flicker = (float)Math.Sin(completionRatio * 6f + timeFactor) * 0.5f + 0.5f;

            // 定义粉红色系的三种颜色
            Color[] pinkColors = new Color[]
            {
        Color.Pink,      // 粉红色
        Color.LightPink, // 浅粉色
        Color.White      // 白色
            };

            // 计算当前颜色索引
            int index = (int)(completionRatio * pinkColors.Length) % pinkColors.Length;

            // 进行颜色混合
            Color startColor = Color.Lerp(pinkColors[index], pinkColors[(index + 1) % pinkColors.Length], flicker);

            // 返回颜色并逐渐透明
            return Color.Lerp(startColor, Color.Transparent, MathHelper.SmoothStep(0f, 1f, completionRatio));
        }


        // 宽度彩虹色 [似乎失败了，但先用这个吧]
        //private Color PrimitiveColorFunction(float completionRatio)
        //{
        //    float timeFactor = Main.GlobalTimeWrappedHourly * 4.0f;

        //    // **彩虹颜色数组**
        //    Color[] rainbowColors = new Color[]
        //    {
        //Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet
        //    };

        //    // **限制 completionRatio 的范围**
        //    completionRatio = MathHelper.Clamp(completionRatio, 0f, 1f);

        //    // **计算基于宽度的颜色索引**
        //    int colorIndex = (int)Math.Floor(completionRatio * rainbowColors.Length * 3) % rainbowColors.Length;

        //    // **确保索引合法**
        //    if (colorIndex < 0 || colorIndex >= rainbowColors.Length)
        //        colorIndex = 0;

        //    // **直接切换颜色**
        //    Color fixedColor = rainbowColors[colorIndex];

        //    // **仍然让颜色在拖尾后段透明**
        //    return Color.Lerp(fixedColor, Color.Transparent, MathHelper.SmoothStep(0f, 1f, completionRatio));
        //}

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用 Shader: FlameStreakShader ("CalamityMod:Flame")
            GameShaders.Misc["CalamityMod:Flame"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.3f; // 调整偏移以适配火焰拖尾效果
            int numPoints = 38; // 火焰拖尾推荐的采样点数量

            // 渲染火焰拖尾
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    PrimitiveWidthFunction,    // 保持原有的宽度函数
                    PrimitiveColorFunction,    // 保持原有的颜色函数
                    (_) => overallOffset,      // 偏移量
                    shader: GameShaders.Misc["CalamityMod:Flame"] // 替换为 Flame 着色器
                ),
                numPoints
            );


//            {
//                float fixedRotation = Projectile.rotation - MathHelper.PiOver4;
//                Vector2 gunTip = Projectile.Center + new Vector2(16f * 3f, 0).RotatedBy(fixedRotation); // 计算枪尖位置
//                Vector2 drawPositio1n = gunTip - Main.screenPosition;

//                // 🌸 粉色 Extra_89 梦幻环（叠加层）
//                Texture2D pinkRing = Terraria.GameContent.TextureAssets.Extra[89].Value;
//                float pulse = 1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);
//                Color pinkColor = new Color(255, 200, 240) * 0.8f;

//                for (int i = 0; i < 5; i++)
//                {
//                    float angle = MathHelper.TwoPi * i / 5f + Main.GlobalTimeWrappedHourly * 2.5f;
//                    float scale = (0.25f + 0.05f * i) * pulse * 2.5f;

//                    Main.EntitySpriteDraw(
//                        pinkRing,
//                        drawPositio1n,
//                        null,
//                        pinkColor,
//                        angle,
//                        pinkRing.Size() * 0.5f,
//                        scale,
//                        SpriteEffects.None,
//                        0
//                    );
//                }


//                // 🌟 魔法阵纹理绘制
//                string[] magicCircles = new[]
//                {
//    "CalamityThrowingSpear/Texture/KsTexture/magic_01",
//    "CalamityThrowingSpear/Texture/KsTexture/magic_02",
//    "CalamityThrowingSpear/Texture/KsTexture/magic_04"
//};

//                for (int i = 0; i < magicCircles.Length; i++)
//                {
//                    Texture2D tex = ModContent.Request<Texture2D>(magicCircles[i]).Value;

//                    float scale = (0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + i)) * 0.4f;
//                    float rotation = Main.GlobalTimeWrappedHourly * (0.6f + 0.1f * i);

//                    Color magicColor = Color.Pink * 0.55f;
//                    magicColor.A = 0;

//                    Main.EntitySpriteDraw(
//                        tex,
//                        drawPositio1n,
//                        null,
//                        magicColor,
//                        rotation,
//                        tex.Size() * 0.5f,
//                        scale,
//                        SpriteEffects.None,
//                        0
//                    );
//                }

//            }


            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 彩虹描边
            float chargeOffset = 3f;
            Color[] rainbowColors = new Color[]
            {
                Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet
            };

            for (int i = 0; i < 8; i++)
            {
                Color chargeColor = rainbowColors[i % rainbowColors.Length] * 0.6f;
                chargeColor.A = 0;

                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 渲染本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        // 定义7种颜色的粒子
        int[] torchDusts = new int[]
        {
                DustID.RedTorch, DustID.OrangeTorch, DustID.YellowTorch, DustID.GreenTorch, DustID.ShimmerTorch, DustID.BlueTorch, DustID.PurpleTorch
        };

        private bool hasStopped = false; // 控制弹幕是否停止
        private float distanceTraveled = 0f; // 记录弹幕飞行的总距离
        private float lastProjSpawn = 0f; // 记录上次生成 PearlwoodJavPROJINV 时的距离

        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.ai[0] = 1f;
        }
        private float storedRotation = 0f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 无限多次扎入伤害（靠本地无敌帧限制频率）
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // 每 X 帧伤害一次
        }
        private float lockedRotation;

        public override void AI()
        {
            if (Projectile.ai[0] == 2f)
            {
                // 命中后保持锁定角度
                Projectile.rotation = lockedRotation;
            }
            else
            {
                // 正常飞行才跟随速度旋转
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                // 圣洁飞行特效：粉红光雾 + 火花
                if (Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkTorch, 0, 0, 100, default, 1.3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.3f;
                }

                if (Main.rand.NextBool(6))
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Projectile.velocity.RotatedByRandom(0.2f) * 0.2f,
                        false,
                        30,
                        1.2f,
                        Color.LightPink
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
      
        }
        private bool hasStuck = false; // 在类里加一个字段

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 第一次命中时刷新存在时间
            if (!hasStuck)
            {
                Projectile.timeLeft = 150;
                hasStuck = true; // 标记只执行一次
            }

            hasStuck = true; // 标记为已触发

            // === 一次性逻辑 ===
            Projectile.ai[0] = 2f;
            lockedRotation = Projectile.rotation;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;


            // 圣洁爆发音效 & 特效
            SoundEngine.PlaySound(SoundID.Item132.WithVolumeScale(1.2f), Projectile.Center);
            CTSLightingBoltsSystem.Spawn_PinkHolyExplosion(Projectile.Center);

            // 召唤 X 个 INV 子弹
            for (int i = 0; i < 1; i++) // 保留循环，但数量固定为 1
            {
                // 敌人下方 32~40 格的区域
                Vector2 spawnPos = target.Center + new Vector2(
                    Main.rand.NextFloat(-20f, 20f) * 16f,
                    Main.rand.NextFloat(32f, 40f) * 16f
                );

                // 方向 = 从生成点指向本体
                Vector2 baseDir = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);

                // 加一个 ±5° 的随机偏移
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f));
                Vector2 velocity = baseDir.RotatedBy(angleOffset) * 13f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<PearlwoodJavPROJINV>(),
                    (int)(Projectile.damage * 0.6f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }


        }



        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 扎在地面也会停留
            Projectile.ai[0] = 2f;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 300; // 停留一会再消失
            return false;
        }











    }
}
