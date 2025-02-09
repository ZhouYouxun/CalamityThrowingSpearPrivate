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
using CalamityMod;
using Terraria.ID;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM10Terra : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_757"; // 使用原版的贴图

        // 使用 Shader: PrismaticStreakShader ("CalamityMod:PrismaticStreak")
        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:PrismaticStreak"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
            int numPoints = 45;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    PrismaticWidthFunction,
                    PrismaticColorFunction,
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
            Main.spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale * 2f, SpriteEffects.None, 0f);

            return false;
        }

        private float PrismaticWidthFunction(float completionRatio)
        {
            return 18f + (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 2f) * 1f;
        }

        private Color PrismaticColorFunction(float completionRatio)
        {
            Color[] spectrum = { Color.ForestGreen, Color.LimeGreen, Color.MediumSeaGreen };
            float shift = (completionRatio + Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
            return Color.Lerp(spectrum[(int)(shift * spectrum.Length)], Color.Transparent, completionRatio);
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


            if (Main.rand.NextBool(3)) // 适当控制频率
            {
                int dustType = Main.rand.NextBool() ? DustID.Terragrim : DustID.Terra;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
            }

            //if (Projectile.timeLeft % 15 == 0)
            //{
            //    float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            //    for (int i = 0; i < 3; i++)
            //    {
            //        float angle = baseAngle + MathHelper.TwoPi / 3f * i;
            //        Vector2 shootVelocity = angle.ToRotationVector2() * Projectile.velocity.Length();

            //        Projectile.NewProjectile(
            //            Projectile.GetSource_FromThis(),
            //            Projectile.Center,
            //            shootVelocity,
            //            ProjectileID.TerraBeam,
            //            (int)(Projectile.damage * 0.25f),
            //            Projectile.knockBack,
            //            Projectile.owner
            //        );
            //    }
            //}


            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 140)
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
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<TerratomereExplosion>(), (int)(Projectile.damage * 1.05f), Projectile.knockBack, Projectile.owner);

            int[] terraProjectiles = { ProjectileID.TerraBeam, ProjectileID.LightBeam, ProjectileID.NightBeam };
            float baseSpeed = 8f;
            float radius = 16 * 16; // 16x16 的半径
            Vector2 center = target.Center;

            for (int corner = 0; corner < 3; corner++) // **三个角落**
            {
                float angle = MathHelper.ToRadians(120 * corner); // **每个角落相隔 120°**
                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                for (int j = 0; j < 2; j++) // **每个角落生成两颗弹幕**
                {
                    float speedMultiplier = (j == 0) ? 1.0f : 1.5f; // **分别 1.0 倍 和 1.5 倍速度**
                    Vector2 shootVelocity = (center - spawnPos).SafeNormalize(Vector2.Zero) * baseSpeed * speedMultiplier;

                    int selectedType = Main.rand.Next(terraProjectiles); // **随机选一个 Terra 系列弹幕**

                    Projectile beam = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        shootVelocity,
                        selectedType,
                        (int)(Projectile.damage * 0.75f),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    if (beam.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        beam.DamageType = DamageClass.Melee;
                        beam.usesLocalNPCImmunity = true;
                        beam.localNPCHitCooldown = -1;
                        beam.timeLeft = 300 * beam.MaxUpdates;
                        beam.penetrate = 4;
                    }
                }
            }
        }
    }
}
