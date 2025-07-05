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
using CalamityRangerExpansion.LightingBolts;

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
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 只允许X次伤害
            Projectile.timeLeft = 1000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

            // 记录初始位置
            //Projectile.localAI[0] = Projectile.Center.X;
            //Projectile.localAI[1] = Projectile.Center.Y;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.ai[0] = 1f;
        }
        private float storedRotation = 0f;

        public override void AI()
        {
            if (hasStopped)
            {
                // **停止后，速度归零**
                //Projectile.velocity = Vector2.Zero;
                Projectile.velocity *= 0f;
                Projectile.rotation = storedRotation;
                return;
            }

            // **保持弹幕旋转**
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // **初始化起始位置**
            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[0] = Projectile.Center.X;
                Projectile.localAI[1] = Projectile.Center.Y;
            }

            // **计算飞行距离**
            float distanceX = Projectile.Center.X - Projectile.localAI[0];
            float distanceY = Projectile.Center.Y - Projectile.localAI[1];
            distanceTraveled = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            // **达到 X tile 后停止**
            if (distanceTraveled >= 105 * 16f)
            {
                storedRotation = Projectile.rotation; // 记录进入静止模式前的旋转角度
                hasStopped = true;
                return;
            }

            // **每飞行 30 像素生成一个 PearlwoodJavPROJINV（限制最大数量 150）**
            if (distanceTraveled - lastProjSpawn >= 30f)
            {
                lastProjSpawn = distanceTraveled;

                // **统计当前已存在的 PearlwoodJavPROJINV 数量**
                int existingProjCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == ModContent.ProjectileType<PearlwoodJavPROJINV>())
                    {
                        existingProjCount++;
                        if (existingProjCount >= 150) // 达到 150 个时，不再生成新的
                            return;
                    }
                }

                // **数量未超限，正常生成**
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<PearlwoodJavPROJINV>(), Projectile.damage, 0f, Projectile.owner);
            }


            {
                // === 🌈 PearlwoodJavPROJ 飞行特效（AI 内） ===

                // 周期性彩虹圣光螺旋光点（已封装）
                if (Main.GameUpdateCount % 30 == 0)
                {
                    CTSLightingBoltsSystem.Spawn_RainbowHolySpirals(Projectile.Center);
                }

                // 持续外围粉红 Dust
                if (Main.rand.NextBool(3))
                {
                    int dustID = DustID.PinkTorch;
                    Vector2 dustVelocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, dustID, dustVelocity.X, dustVelocity.Y, 100, default, 1.2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].fadeIn = 0.5f;
                }

                // 粉红色 SparkParticle 轻线性拖尾
                if (Main.rand.NextBool(5))
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        false,
                        30,
                        1.0f,
                        Color.LightPink
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 🌈 七彩有序尘埃环绕（飞行期间的有序部分）
                if (Main.GameUpdateCount % 2 == 0) // 每 12 帧生成一圈
                {
                    Color[] rainbowColors = new Color[]
                    {
        Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet
                    };

                    float radius = 20f + Main.rand.NextFloat(-2f, 2f); // 微微抖动圆环大小
                    float baseAngle = Main.GlobalTimeWrappedHourly * 3f; // 随时间旋转

                    for (int i = 0; i < rainbowColors.Length; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / rainbowColors.Length;
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        Vector2 spawnPos = Projectile.Center + offset;
                        Vector2 velocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.5f, 0.5f); // 缓慢平移

                        int dustID = DustID.RainbowTorch;
                        int dust = Dust.NewDust(spawnPos, 0, 0, dustID, velocity.X, velocity.Y, 100, rainbowColors[i], 0.8f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].fadeIn = 0.6f;
                        Main.dust[dust].scale = 1.1f;
                    }
                }


            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            hasStopped = true;
            Projectile.timeLeft = 36000;
            return false;
        }


        public override void OnKill(int timeLeft)
        {
            // === 🌈 PearlwoodJavPROJ 死亡特效（OnKill 内调用） ===
            SoundEngine.PlaySound(SoundID.Item67, Projectile.Center); // 彩虹枪音效

            // 触发粉色圣洁光点爆发（柔和补充）
            CTSLightingBoltsSystem.Spawn_PinkHolyExplosion(Projectile.Center);


            {
                // 🌟 极限夸张的七彩虹尘爆发
                int totalBursts = 70; // 尘埃数量极大化
                for (int i = 0; i < totalBursts; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 36f); // 超高速爆炸
                    int dustID = DustID.RainbowTorch;
                    int dustIndex = Dust.NewDust(Projectile.Center, 0, 0, dustID, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(1.2f, 2.4f));
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].fadeIn = Main.rand.NextFloat(0.5f, 1.2f);
                }

                // 💖 粉色柔和尘雾环绕柔化视觉冲击
                for (int i = 0; i < 30; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(48f, 48f);
                    Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 4f);
                    int dustID = DustID.PinkTorch;
                    int dustIndex = Dust.NewDust(Projectile.Center + offset, 0, 0, dustID, vel.X, vel.Y, 150, default, Main.rand.NextFloat(1.5f, 2.5f));
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].fadeIn = Main.rand.NextFloat(0.3f, 0.8f);
                }

            }


        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 通知玩家更新计数器
            if (Main.player[Projectile.owner].GetModPlayer<PearlwoodJavPLAYER>() is PearlwoodJavPLAYER player)
            {
                player.IncrementHitCounter();
            }
            CTSLightingBoltsSystem.Spawn_RainbowHolySpirals(Projectile.Center);

        }












    }
}
