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
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM9Cat : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_3063"; // 使用原版的贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 55;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:PrismaticStreak"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f;
            int numPoints = 45;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    SoftPinkWidthFunction,
                    SoftPinkColorFunction,
                    (_) => overallOffset,
                    shader: GameShaders.Misc["CalamityMod:PrismaticStreak"]
                ),
                numPoints
            );

            // 读取武器贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 渲染武器本体
            Main.spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);

            return false;
        }

        private float SoftPinkWidthFunction(float completionRatio)
        {
            return 9f + (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 2f) * 1.5f;
        }

        private Color SoftPinkColorFunction(float completionRatio)
        {
            float shift = (completionRatio + Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
            return Color.Lerp(Color.White, Color.LightPink, shift); // 粉白渐变
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

            Vector2 dustVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.6f);
            Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity, Color.WhiteSmoke, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
            GeneralParticleHandler.SpawnParticle(smoke);


            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 130)
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

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 arrowCenter = Projectile.Center + new Vector2(0, 20 * 16);
            int projectileCount = 5;
            float baseSpeed = Projectile.velocity.Length() * 1.25f;
            float damageMultiplier = 0.25f;

            for (int i = 0; i < projectileCount; i++)
            {
                float xOffset = -10 * 16 + i * 5 * 16 / (projectileCount - 1); // 从 -10 到 0 的斜线
                float yOffset = xOffset + 10 * 16; // 形成 45° 斜线

                Vector2 spawnPos1 = arrowCenter + new Vector2(xOffset, yOffset);
                Vector2 spawnPos2 = arrowCenter + new Vector2(-xOffset, yOffset);

                // **生成 Meowmere 猫猫头弹幕**
                int proj1 = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos1,
                    -Vector2.UnitY * baseSpeed,
                    ProjectileID.Meowmere,
                    (int)(Projectile.damage * damageMultiplier),
                    Projectile.knockBack,
                    Projectile.owner
                );

                int proj2 = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos2,
                    -Vector2.UnitY * baseSpeed,
                    ProjectileID.Meowmere,
                    (int)(Projectile.damage * damageMultiplier),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // **设置 Meowmere 弹幕的 timeLeft = 90**
                if (Main.projectile.IndexInRange(proj1))
                    Main.projectile[proj1].timeLeft = 90;

                if (Main.projectile.IndexInRange(proj2))
                    Main.projectile[proj2].timeLeft = 90;
            }
        }

    }
}
