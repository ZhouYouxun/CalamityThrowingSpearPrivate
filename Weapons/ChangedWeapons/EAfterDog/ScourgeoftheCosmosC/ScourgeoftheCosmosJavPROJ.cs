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
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.DataStructures;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/ScourgeoftheCosmosC/ScourgeoftheCosmosJav";
        private static Color ShaderColorOne = Color.LightGray; // 浅灰色
        private static Color ShaderColorTwo = Color.Purple; // 紫色
        private static Color ShaderEndColor = Color.LightPink; // 结束颜色，浅粉色

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8; // 保持原有拖尾缓存
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;    // 保持原有拖尾模式
        }
        private int shaderDisabledFrames = 0; // 记录传送后关闭着色器的帧数

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取玩家的 X 值
            int x = Main.player[Projectile.owner].GetModPlayer<ScourgeoftheCosmosJavPlayer>().X;

            // 使用 Shader: ImpFlameTrailShader ("CalamityMod:ImpFlameTrail")
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
            );

            // 计算拖尾长度
            int numPoints = 55 + x * 8; // 默认长度 ?，每个 X 增加 ?

            // 偏移设置
            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.2f;
             
            // **如果处于传送后的 12 帧内，不渲染着色器** 
            if (shaderDisabledFrames >= 12)
            {
                // 使用 Shader: ImpFlameTrailShader ("CalamityMod:ImpFlameTrail")
                GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
                    ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
                );

                // 定义一个额外的偏移量，让拖尾往后移
                Vector2 trailOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f; // 方向为弹幕的反方向，距离为 X 像素

                // 使用拖尾渲染器
                PrimitiveRenderer.RenderTrail(
                    Projectile.oldPos.Select(pos => pos + trailOffset).ToArray(), // 对每个旧位置增加偏移量
                    new(
                        CosmicWidthFunction,  // 拖尾宽度函数
                        CosmicColorFunction,  // 拖尾颜色函数
                        (completionRatio, vertexPos) => overallOffset,
                        shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]
                    ),
                    numPoints
                );
            }
            else
            {
                shaderDisabledFrames++;
            }

            // 绘制弹幕本体
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? frame = null;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects direction = SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);

            return false;
        }

        // 拖尾宽度函数
        private float CosmicWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float baseWidth = 45f;
            float flicker = (float)Math.Sin(completionRatio * 6f + Main.GlobalTimeWrappedHourly * 3f) * 2f;
            return baseWidth + flicker; // 拖尾末端波动
        }

        // 拖尾颜色函数
        private Color CosmicColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float intensity = (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f;
            Color baseColor = Color.Lerp(Color.MediumPurple, Color.HotPink, intensity); // 紫色到粉色渐变
            return Color.Lerp(baseColor, Color.Transparent, completionRatio); // 拖尾末端逐渐透明
        }

        private bool hasTeleported = false; // 判断是否已经完成屏幕穿越
        private bool trackingMode = false; // 判断是否启用四向追踪模式
        private bool hasConsumedApple = false; // **记录是否已经触发过苹果效果**
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public override void OnSpawn(IEntitySource source)
        {
            shaderDisabledFrames = 3; // 在刚开始时也一样禁用着色器
        }
        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加浅紫色光照，光照强度不变
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            // **在第一次反弹前，继续加速**
            if (!hasTeleported)
            {
                Projectile.velocity *= 1.01f;
            }

            // 计算屏幕范围
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Vector2 screenPosition = Projectile.Center - Main.screenPosition;

            // **检测是否超出屏幕边界**
            if (!screenRect.Contains(screenPosition.ToPoint()) && !hasTeleported)
            {
                TeleportToOtherSide(screenPosition);
                hasTeleported = true; // 仅允许一次反弹
            }

            // **四向追踪模式**
            if (trackingMode)
            {
                FollowEnemyInFourDirections();
            }

            // **检测与苹果弹幕的碰撞**
            foreach (Projectile otherProj in Main.projectile)
            {
                if (otherProj.active && otherProj.type == ModContent.ProjectileType<ScourgeoftheCosmosJavApple>() && otherProj.Hitbox.Intersects(Projectile.Hitbox))
                {
                    // **如果已经触发过苹果效果，后续的碰撞不会生效**
                    if (hasConsumedApple)
                        continue;

                    hasConsumedApple = true; // **记录第一次碰撞**
                    trackingMode = true; // 开启四向追踪模式
                    SoundEngine.PlaySound(SoundID.Item2, Projectile.Center); // 追踪模式开启音效
                    //CreateAppleExplosionEffect(); // 释放苹果形状粒子 在苹果的死亡特效里实现

                    Main.player[Projectile.owner].GetModPlayer<ScourgeoftheCosmosJavPlayer>().IncreaseX();
                    otherProj.Kill(); // **删除苹果**
                    break;
                }
            }

            // **双螺旋粒子：每两帧生成一次**
            particleTimer++;
            if (particleTimer >= 2)
            {
                GenerateDevourerHelixParticles();
                particleTimer = 0; // 重置计时器
            }
        }
        private int particleTimer = 0; // 用于控制粒子生成的计时器
        private List<SparkParticle> ownedSparkParticles = new();

        // **双螺旋粒子生成**

        private void GenerateDevourerHelixParticles()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 muzzlePos = Projectile.Center + forward * 24f + Main.rand.NextVector2Circular(4f, 4f);

            float phase = Main.GameUpdateCount * 0.25f;
            float baseAngle = Main.GameUpdateCount * 0.05f; // 时间扰动

            for (int i = 0; i < 4; i++)
            {
                // 基础四方向：0°, 90°, 180°, 270°，增加微扰动形成灵动感
                float angleOffset = MathHelper.PiOver2 * i + baseAngle + Main.rand.NextFloat(-0.2f, 0.2f);

                // 速度大小随机
                float speed = Main.rand.NextFloat(1.5f, 3.2f);
                Vector2 swirlVelocity = forward.RotatedBy(angleOffset) * speed;

                // 随机紫色系颜色
                Color swirlColor = Color.Lerp(Color.MediumPurple, Color.Violet, Main.rand.NextFloat(0f, 1f));

                SparkParticle swirlSpark = new SparkParticle(
                    muzzlePos + swirlVelocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 8f), // 起点随机偏移
                    swirlVelocity,
                    false,
                    40,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    swirlColor
                );
                GeneralParticleHandler.SpawnParticle(swirlSpark);
                ownedSparkParticles.Add(swirlSpark);
            }

            // ✦ 星空主题 Dust 复杂扩散 ✦
            for (int i = 0; i < 8; i++)
            {
                // 在弹幕周围 12px 圆环范围内随机生成
                Vector2 spawnOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 spawnPosition = Projectile.Center + spawnOffset;

                // 基础速度沿偏移方向扩散，增加小扰动
                Vector2 dustVelocity = spawnOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 1.5f);
                dustVelocity = dustVelocity.RotatedByRandom(MathHelper.ToRadians(15));

                Color particleColor = Main.rand.NextBool() ? Color.Purple : Color.Violet;

                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.FireworkFountain_Pink,
                    dustVelocity,
                    100,
                    particleColor,
                    0.65f * Main.rand.NextFloat(0.8f, 1.2f) // 大小微变
                );

                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat(0.4f, 0.8f);

                // 每 3 个粒子附带额外加速，制造深空灵息流动感
                if (i % 3 == 0)
                {
                    dust.velocity *= Main.rand.NextFloat(1.2f, 1.5f);
                }
            }


            for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
            {
                SparkParticle p = ownedSparkParticles[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedSparkParticles.RemoveAt(i);
                    continue;
                }

                // ✦ 离谱的“灵息蛇形拐弯”逻辑 ✦

                // 持续右拐形成螺旋
                p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(2.5f));

                // 每 15 帧倒退一次形成“撕裂回流”
                if (p.Time % 15 == 0)
                {
                    p.Velocity *= -0.8f; // 短暂倒退
                }

                // 呼吸式加速减速
                float cycle = 20f;
                float scaleFactor = 1f + 0.05f * (float)Math.Sin(MathHelper.TwoPi * p.Time / cycle);
                p.Velocity *= scaleFactor;
            }

        }







        // 传送到屏幕的对侧位置
        private void TeleportToOtherSide(Vector2 screenPosition)
        {
            // 计算新位置
            float newX = screenPosition.X <= 0 ? Main.screenWidth : (screenPosition.X >= Main.screenWidth ? 0 : screenPosition.X);
            float newY = screenPosition.Y <= 0 ? Main.screenHeight : (screenPosition.Y >= Main.screenHeight ? 0 : screenPosition.Y);

            // 计算弹幕在世界坐标中的新位置
            Projectile.position = new Vector2(newX, newY) + Main.screenPosition;

            shaderDisabledFrames = 0; // **在传送后 12 帧内禁用着色器**
        }
        private bool prioritizingX = true; // 初始时随机决定优先对齐哪个方向
        private void FollowEnemyInFourDirections()
        {
            NPC target = Projectile.Center.ClosestNPCAt(3800);
            if (target == null) return;

            Vector2 direction = Vector2.Zero;
            float distanceX = Math.Abs(target.Center.X - Projectile.Center.X);
            float distanceY = Math.Abs(target.Center.Y - Projectile.Center.Y);

            // 如果当前优先水平对齐（X 轴）
            if (prioritizingX)
            {
                if (distanceX > 8) // 还未完成水平对齐
                {
                    direction = target.Center.X > Projectile.Center.X ? Vector2.UnitX : -Vector2.UnitX;
                }
                else // 完成水平对齐，切换为垂直对齐
                {
                    prioritizingX = false; // 切换到优先垂直对齐
                }
            }

            // 如果当前优先垂直对齐（Y 轴）
            if (!prioritizingX)
            {
                if (distanceY > 8) // 还未完成垂直对齐
                {
                    direction = target.Center.Y > Projectile.Center.Y ? Vector2.UnitY : -Vector2.UnitY;
                }
                else // 完成垂直对齐，目标追踪完成
                {
                    direction = Vector2.Zero; // 停止移动或执行其他逻辑
                }
            }

            // 如果已经决定了方向，则更新弹幕速度
            if (direction != Vector2.Zero)
            {
                Projectile.velocity = direction * 10f; // 确保弹幕只朝当前对齐的方向前进
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            // 添加弑神者火焰buff，持续300帧
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);

            int x = Main.player[Projectile.owner].GetModPlayer<ScourgeoftheCosmosJavPlayer>().X;

            // **触发额外追踪弹幕**
            int numProjectiles = 3 + x / 2; // 调整数量为 3 + x/2
            for (int i = 0; i < numProjectiles; i++)
            {
                // **生成位置：目标周围 100 像素范围内**
                Vector2 spawnOffset = new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-50f, 50f));
                Vector2 spawnPosition = target.Center + spawnOffset;

                // **初始速度：只能是上、下、左、右**
                Vector2[] possibleVelocities = new Vector2[]
                {
            Vector2.UnitX * Projectile.velocity.Length(),  // 向右
            -Vector2.UnitX * Projectile.velocity.Length(), // 向左
            Vector2.UnitY * Projectile.velocity.Length(),  // 向下
            -Vector2.UnitY * Projectile.velocity.Length()  // 向上
                };
                Vector2 spawnVelocity = possibleVelocities[Main.rand.Next(4)]; // 随机选择一个方向

                // **生成小吞噬者**
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, spawnVelocity,
                    ModContent.ProjectileType<ScourgeoftheCosmosJavMini>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }

            // 不要回复生命，因为回复生命在吃苹果的时候就会有了，多回不合理
            //int healAmount = 1 + (x / 3);
            //Main.player[Projectile.owner].statLife += healAmount;
            //Main.player[Projectile.owner].HealEffect(healAmount);

            // **生成浅灰色菱形粒子**
            int numParticles = Main.rand.Next(x + 7, x + 10);
            for (int i = 0; i < numParticles; i++)
            {
                Vector2 randomDirection = Projectile.velocity.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f);
                float particleScale = Main.rand.NextFloat(0.6f, 1.0f);
                SparkParticle spark = new SparkParticle(Projectile.Center, randomDirection, false, Main.rand.Next(35, 50), particleScale, Color.LightGray);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // **生成 EssenceFlame2**
            int numEssence = 1 + x;
            for (int i = 0; i < numEssence; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<CTSEssenceFlame2>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }
        }
        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);

            // 生成紫色和浅灰色的烟雾特效
            int Dusts = 15; // 生成的粒子数量
            float radians = MathHelper.TwoPi / Dusts; // 每个粒子的旋转角度
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)); // 初始旋转方向
            for (int i = 0; i < Dusts; i++)
            {
                // 增大烟雾扩散幅度，调整速度
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * Main.rand.NextFloat(8f, 12f);

                // 随机选择紫色或浅灰色作为烟雾颜色
                Color smokeColor = Main.rand.NextBool() ? Color.LightGray : Color.Purple;

                // 生成烟雾特效
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity, smokeColor, 18, Main.rand.NextFloat(1.2f, 1.8f), 0.45f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }









    }
}