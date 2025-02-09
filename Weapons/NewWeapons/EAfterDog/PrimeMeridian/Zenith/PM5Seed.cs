using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM5Seed : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_3018"; // 使用原版的贴图
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private float PrimitiveWidthFunction(float completionRatio)
        {
            float baseWidth = 20f;
            return baseWidth + (float)Math.Sin(completionRatio * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly * 3f) * 1f; // 添加动态波动
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            Color leafGreen = new Color(34, 139, 34); // 深绿色
            Color seedGreen = new Color(85, 107, 47); // 种子绿
            float oscillation = (float)Math.Sin(completionRatio * MathHelper.Pi + Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f; // 颜色动态变化
            return Color.Lerp(leafGreen, seedGreen, oscillation); // 颜色在两种绿色间动态变化
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
            int numPoints = 46;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]),
                numPoints
            );

            // 读取武器贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 渲染武器本体
            Main.spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0f);


            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 350;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧

        }
        public override void OnSpawn(IEntitySource source)
        {


        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.55f);



            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 90)
            {
                NPC target = Projectile.Center.ClosestNPCAt(2400); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float desiredRotation = direction.ToRotation(); // 目标方向
                    float currentRotation = Projectile.velocity.ToRotation(); // 当前方向
                    float rotationDifference = MathHelper.WrapAngle(desiredRotation - currentRotation); // 计算角度差
                    // 让 `maxRotation` 随时间增加，每 20 帧增加 `1°`，最大不超过 `90°`
                    float maxRotation = MathHelper.ToRadians(8f + (Projectile.ai[1] / 20f));
                    maxRotation = MathHelper.Clamp(maxRotation, 0f, MathHelper.ToRadians(90f)); // 限制最大角度为 90°

                    // 限制旋转角度
                    float rotationAmount = MathHelper.Clamp(rotationDifference, -maxRotation, maxRotation);
                    Projectile.velocity = Projectile.velocity.RotatedBy(rotationAmount).SafeNormalize(Vector2.Zero) * 18f; // 追踪但受限
                }
            }
            else
            {
                Projectile.ai[1]++;
            }

            Time++;
        }
        public ref float Time => ref Projectile.ai[1];
        public override bool? CanDamage() => Time >= 22f; // 初始的时候不会造成伤害，直到x为止



        public override void OnKill(int timeLeft)
        {
            int[] particleTypes = { 2, 3, 167, 157 }; // 粒子特效混用
            Vector2[] petalOffsets =
            {
                new Vector2(-16, -24), // 每个花瓣的相对位置
                new Vector2(16, -24),
                new Vector2(-24, 0),
                new Vector2(24, 0),
                new Vector2(0, 16)
            };

            foreach (Vector2 offset in petalOffsets)
            {
                Vector2 spawnPos = Projectile.Center + offset;
                for (int i = 0; i < 25; i++) // 每个花瓣生成 25 个粒子
                {
                    int particleType = Main.rand.Next(particleTypes);
                    Dust.NewDustPerfect(spawnPos, particleType, Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.2f).noGravity = true;
                }
            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 spawnArea = new Vector2(20 * 16, 1 * 16); // 宽 20×16，高 1×16 的区域
            Vector2 spawnCenter = Projectile.Center - new Vector2(0, 50 * 16); // 从自身中心上方 50×16 处随机生成

            int seedAmount = Main.rand.Next(1, 4); // 随机生成 1 到 3 个 SeedlerNut
            for (int i = 0; i < seedAmount; i++)
            {
                Vector2 spawnPos = spawnCenter + new Vector2(Main.rand.NextFloat(-spawnArea.X / 2, spawnArea.X / 2), Main.rand.NextFloat(-spawnArea.Y / 2, spawnArea.Y / 2));
                Vector2 shootDirection = new Vector2(Main.rand.NextFloat(-2f, 2f), 1f).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    shootDirection,
                    ProjectileID.SeedlerNut, // SeedlerNut
                    (int)(Projectile.damage * 1.0f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

        }
    }
}
