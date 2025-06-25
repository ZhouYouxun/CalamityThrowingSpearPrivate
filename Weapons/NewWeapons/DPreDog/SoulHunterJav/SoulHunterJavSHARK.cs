using CalamityMod.Graphics.Primitives;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav
{
    internal class SoulHunterJavSHARK : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.DPreDog";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.Opacity = 0f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 300; 
        }
        private int time = 0; // 新增计时器
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = (Math.Cos(Projectile.rotation) > 0f).ToDirectionInt();
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.1f, 0f, 1f);


            // 自定义计时器，每帧递增
            time++;

            if (time <= 60)
            {
                if (time <= 20)
                {
                    // 前 20 帧直线飞行
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                }
                else if (time <= 60)
                {
                    // 接下来的 10 帧每帧右转 3 度
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(3));
                }
            }
            else
            {
                // 60 帧后开始追踪
                NPC target = Projectile.Center.ClosestNPCAt(5000);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float desiredRotation = direction.ToRotation();
                    float currentRotation = Projectile.velocity.ToRotation();
                    float rotationDifference = MathHelper.WrapAngle(desiredRotation - currentRotation);
                    float rotationAmount = MathHelper.ToRadians(Main.rand.Next(1, 10));

                    // 限制旋转幅度
                    if (Math.Abs(rotationDifference) < rotationAmount)
                    {
                        rotationAmount = rotationDifference;
                    }

                    // 调整速度方向
                    Projectile.velocity = Projectile.velocity.RotatedBy(rotationAmount);
                }
            }

            Time++;
        }
        public ref float Time => ref Projectile.ai[1];

        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到x为止

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        internal Color ColorFunction(float completionRatio)
        {
            float fadeOpacity = Utils.GetLerpValue(0.94f, 0.54f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.Cyan, Color.White, 0.4f) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio)
        {
            float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
            return MathHelper.Lerp(0f, 12f * Projectile.Opacity, expansionCompletion);
        }

        public override void OnKill(int timeLeft)
        {
            // 添加深海主题矩形粒子特效
            int numRectangles = 5; // 矩形的数量
            float maxRectangleSize = 150f; // 最大矩形尺寸
            for (int i = 0; i < numRectangles; i++)
            {
                // 计算矩形的随机尺寸
                float width = Main.rand.NextFloat(50f, maxRectangleSize);
                float height = Main.rand.NextFloat(30f, maxRectangleSize);
                float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

                // 生成矩形的四个角上的粒子
                for (int j = 0; j < 4; j++)
                {
                    float x = (j % 2 == 0 ? -1 : 1) * width / 2f;
                    float y = (j / 2 == 0 ? -1 : 1) * height / 2f;
                    Vector2 cornerOffset = new Vector2(x, y).RotatedBy(rotation);

                    Dust rectangleDust = Dust.NewDustPerfect(
                        Projectile.Center + cornerOffset,
                        Main.rand.NextBool() ? 80 : 172, // 深海主题粒子类型
                        cornerOffset * Main.rand.NextFloat(0.2f, 1.2f), // 粒子速度
                        0,
                        Color.DarkBlue, // 深蓝色粒子
                        Main.rand.NextFloat(1.2f, 1.8f) // 粒子缩放
                    );
                    rectangleDust.noGravity = true;
                }
            }

            // 添加额外随机扩散粒子效果
            for (int i = 0; i < 60; i++)
            {
                Vector2 randomOffset = Main.rand.NextVector2Circular(80f, 80f);
                Dust randomDust = Dust.NewDustPerfect(
                    Projectile.Center + randomOffset,
                    Main.rand.NextBool() ? 80 : 172, // 深海主题粒子类型
                    randomOffset * Main.rand.NextFloat(0.3f, 1.0f), // 粒子速度
                    0,
                    Color.Cyan, // 浅蓝色粒子
                    Main.rand.NextFloat(1f, 1.5f) // 粒子缩放
                );
                randomDust.noGravity = true;
            }

            // Create death effects for the shark, including a death sound, gore, and some blood.
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                //Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark1").Type, Projectile.scale);
                //Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark2").Type, Projectile.scale);
                //Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark3").Type, Projectile.scale);
            }
            for (int i = 0; i < 12; i++)
            {
                Dust blood = Dust.NewDustPerfect(Projectile.Center, 5);
                blood.velocity = Main.rand.NextVector2Circular(6f, 6f);
                blood.scale *= Main.rand.NextFloat(0.7f, 1.3f);
                blood.noGravity = true;
            }

            if (Main.myPlayer != Projectile.owner)
                return;


            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TheMaelstromExplosion>(), Projectile.damage, 0f, Projectile.owner);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 60);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            return false;
        }
    }
}
