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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM8Crazy : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_3065"; // 使用原版的贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.2f - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f;
            int numPoints = 36;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    StarNebulaWidthFunction,
                    StarNebulaColorFunction,
                    (_) => overallOffset,
                    shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]
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

        private float StarNebulaWidthFunction(float completionRatio)
        {
            float baseWidth = 6f;
            float flicker = (float)Math.Sin(completionRatio * 8f + Main.GlobalTimeWrappedHourly * 4f) * 1.5f;
            return baseWidth + flicker;
        }

        private Color StarNebulaColorFunction(float completionRatio)
        {
            float oscillation = (float)Math.Sin(completionRatio * 12f + Main.GlobalTimeWrappedHourly * 6f) * 0.5f + 0.5f;
            Color startColor = Color.Lerp(Color.Magenta, Color.HotPink, oscillation);
            return Color.Lerp(startColor, Color.Transparent, completionRatio);
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
            if (Projectile.ai[1] > 120)
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
            SoundEngine.PlaySound(SoundID.Item105, Projectile.Center); // 播放音效

            {
                int starCount = Main.rand.Next(3, 7);
                Vector2 center = target.Center;
                float innerRadius = 15 * 15;
                float outerRadius = 25 * 15;

                int[] starProjectiles =
                {
                    ModContent.ProjectileType<PlasmaBlast>(),
                    ModContent.ProjectileType<AstralStar>(),
                    ModContent.ProjectileType<GalacticaComet>(),
                    ProjectileID.StarCannonStar,
                    ProjectileID.Starfury,
                    ProjectileID.StarWrath,
                    ProjectileID.SuperStar
                };

                for (int i = 0; i < starCount; i++)
                {
                    Vector2 spawnPos = center + Main.rand.NextVector2CircularEdge(outerRadius, outerRadius);
                    while (spawnPos.LengthSquared() < innerRadius * innerRadius)
                    {
                        spawnPos = center + Main.rand.NextVector2CircularEdge(outerRadius, outerRadius);
                    }

                    Vector2 velocity = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length() * 3f;
                    int selectedStar = Main.rand.Next(starProjectiles);

                    int projID = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        velocity,
                        selectedStar,
                        (int)(Projectile.damage * 0.55f),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    // **设置新弹幕的属性**
                    if (Main.projectile.IndexInRange(projID))
                    {
                        Projectile proj = Main.projectile[projID];
                        proj.timeLeft = 60; // **持续时间 60 帧**
                        proj.usesLocalNPCImmunity = true;
                        proj.localNPCHitCooldown = -1; // **无敌帧**
                        proj.penetrate = 1; // **穿透次数 1**
                    }
                }
            }


            int starPentagramCount = Main.rand.Next(2, 4);
            for (int n = 0; n < starPentagramCount; n++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 pentagramCenter = target.Center + offset;
                float sizeFactor = Main.rand.NextFloat(0.7f, 0.9f); // 稍微缩小一点

                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f;
                    float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f;
                    Vector2 start = angle.ToRotationVector2() * sizeFactor;
                    Vector2 end = nextAngle.ToRotationVector2() * sizeFactor;

                    for (int j = 0; j < 50; j++) // 增加粒子数让五角星更复杂
                    {
                        Dust starDust = Dust.NewDustPerfect(pentagramCenter, DustID.PinkTorch);
                        starDust.scale = 2f;
                        starDust.velocity = Vector2.Lerp(start, end, j / 50f) * 14f;
                        starDust.noGravity = true;
                    }
                }
            }

        }
    }
}
