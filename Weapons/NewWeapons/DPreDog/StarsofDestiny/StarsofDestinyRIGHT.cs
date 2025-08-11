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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Terraria.Audio;
using Microsoft.Xna.Framework.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyRIGHT : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        internal Color ColorFunction(float completionRatio)
        {
            // 计算末端的淡化效果
            float fadeToEnd = MathHelper.Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            // 控制拖尾的不透明度，越接近末尾越透明
            float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;

            // 拖尾颜色以 HSL 渐变
            Color colorHue = Main.hslToRgb(0.1f, 1, 0.8f); // 色相设置为金色

            // 动态颜色效果
            Color endColor = Color.Lerp(colorHue, Color.PaleTurquoise, (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);

            return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio)
        {
            // 拖尾宽度随位置衰减，越靠近末端越窄
            float expansionCompletion = (float)Math.Pow(1 - completionRatio, 3); // 位置越远，衰减越快
            return MathHelper.Lerp(0f, 22 * Projectile.scale * Projectile.Opacity, expansionCompletion);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (stuck)
            {
                // 如果已触发粘附，仅绘制弹幕本体
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                // 🚩 关键替换：
                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), lockedRotation, origin, Projectile.scale, direction, 0);

                return false;
            }

            // 未触发粘附时保留原有的所有绘制效果
            // 背光效果部分 - 白色光晕
            Texture2D textureGlow = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 originGlow = textureGlow.Size() * 0.5f;
            Vector2 drawPositionGlow = Projectile.Center - Main.screenPosition;

            // 白色光晕
            float chargeOffset = 3f;
            Color chargeColorWhite = Color.White * 0.6f;
            chargeColorWhite.A = 0;
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(textureGlow, drawPositionGlow + drawOffset, null, chargeColorWhite, rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 金色光晕
            Color chargeColorGold = Color.Gold * 0.5f;
            chargeColorGold.A = 0;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset * 1.5f;
                Main.spriteBatch.Draw(textureGlow, drawPositionGlow + drawOffset, null, chargeColorGold, rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 渲染实际的投射物本体
            Main.EntitySpriteDraw(textureGlow, drawPositionGlow, null, Projectile.GetAlpha(lightColor), rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);

            // 拖尾特效
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);

            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用通用无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为14帧
        }
        private float lockedRotation;
        private bool stuck;
        private NPC stuckTarget;
        private bool hasCreatedStandField = false; // 确保星空立场仅生成一次
        private bool isAttachedToTarget = false; // 用于标记是否已触发粘附

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.55f);

            // 粘附逻辑
            {
                if (stuck && stuckTarget != null && stuckTarget.active)
                {
                    Projectile.Center = stuckTarget.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.rotation = lockedRotation;
                }
                else if (stuck)
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.rotation = lockedRotation;
                }
                else
                {
                    Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                    Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                    Projectile.rotation += Projectile.spriteDirection * MathHelper.ToRadians(45f);
                }
            }
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                stuck = true;
                lockedRotation = Projectile.rotation;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 900;
            }

            // 检查当前世界中同类型立场的数量
            int activeFields = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<StarsofDestinyRStandField>());

            if (!hasCreatedStandField && activeFields < 2) // 如果数量小于2，允许生成新的立场
            {
                hasCreatedStandField = true;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<StarsofDestinyRStandField>(),
                    (int)(Projectile.damage * 1.8),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }


            return false; // 防止弹幕被 Kill
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

            if (!stuck)
            {
                stuck = true;
                stuckTarget = target; // 🚩 正确记录目标 NPC

                lockedRotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                lockedRotation += Projectile.spriteDirection * MathHelper.ToRadians(45f);

                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 900; // 可选延长时间
            }



            // 检查当前世界中同类型立场的数量
            int activeFields = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<StarsofDestinyRStandField>());

            if (!hasCreatedStandField && activeFields < 2) // 如果数量小于2，允许生成新的立场
            {
                hasCreatedStandField = true;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<StarsofDestinyRStandField>(),
                    (int)(Projectile.damage * 0.96),
                    Projectile.knockBack,
                    Projectile.owner,
                    0f,  // ai[0]
                    -1f  // ai[1] = -1 表示不跟随 NPC
                );
            }


        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<StarsofDestinyEDebuff>(), 300); // 右键无论如何命中敌人都会导致标记
            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead)
                {
                    player.AddBuff(ModContent.BuffType<StarsofDestinyPBuff>(), 300); // 为所有玩家添加机动性加成Buff
                }
            }

            // 粒子特效逻辑
            int particleCount = 40; // 粒子数量
            float particleRadius = 60f; // 粒子扩散半径
            float particleSpeed = 3f; // 粒子初始速度
            float scaleMin = 1.2f; // 粒子最小缩放
            float scaleMax = 1.6f; // 粒子最大缩放

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi / particleCount * i; // 计算粒子的扩散角度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * particleSpeed; // 粒子速度方向
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(particleRadius * 0.3f, particleRadius); // 随机化初始位置

                int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond }); // 随机选择粒子类型
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 100, Color.White, Main.rand.NextFloat(scaleMin, scaleMax));
                dust.noGravity = true; // 粒子不受重力影响
            }

            SoundEngine.PlaySound(SoundID.Item24, Projectile.position);   
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            {
                // 缩小比例
                float scaleFactor = 0.7f;

                // 圆形部分
                float circleRadius = 12 * 16 * scaleFactor; // 圆的半径
                int circleParticles = 80; // 粒子数量
                for (int i = 0; i < circleParticles; i++)
                {
                    float angle = MathHelper.TwoPi / circleParticles * i; // 角度
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * circleRadius;
                    Vector2 position = Projectile.Center + offset;

                    // 使用时钟的白色粒子特效
                    int dustType = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                    Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 100, Color.White, Main.rand.NextFloat(1.65f, 1.95f));
                    dust.noGravity = true;
                }

                // 椭圆部分
                float ellipseMajorAxis1 = 16 * 16 * scaleFactor; // 第一个椭圆长轴
                float ellipseMinorAxis1 = 8 * 16 * scaleFactor;  // 第一个椭圆短轴
                float ellipseMajorAxis2 = 14 * 16 * scaleFactor; // 第二个椭圆长轴
                float ellipseMinorAxis2 = 7 * 16 * scaleFactor;  // 第二个椭圆短轴
                int ellipseParticles = 120; // 椭圆粒子数量

                // 随机角度
                float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(0, 360)); // 椭圆的整体随机旋转角度

                for (int i = 0; i < ellipseParticles; i++)
                {
                    float angle = MathHelper.TwoPi / ellipseParticles * i; // 角度

                    // 第一个椭圆
                    Vector2 offset1 = new Vector2(
                        (float)Math.Cos(angle) * ellipseMajorAxis1, // 长轴方向
                        (float)Math.Sin(angle) * ellipseMinorAxis1  // 短轴方向
                    ).RotatedBy(randomAngle); // 绑定的随机角度
                    Vector2 position1 = Projectile.Center + offset1;

                    // 使用时钟的白色粒子特效
                    int dustType1 = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                    Dust dust1 = Dust.NewDustPerfect(position1, dustType1, Vector2.Zero, 100, Color.White, Main.rand.NextFloat(1.65f, 1.95f));
                    dust1.noGravity = true;

                    // 第二个椭圆
                    Vector2 offset2 = new Vector2(
                        (float)Math.Cos(angle) * ellipseMajorAxis2, // 长轴方向
                        (float)Math.Sin(angle) * ellipseMinorAxis2  // 短轴方向
                    ).RotatedBy(randomAngle); // 同样的随机角度
                    Vector2 position2 = Projectile.Center + offset2;

                    // 使用时钟的白色粒子特效
                    int dustType2 = Main.rand.Next(new int[] { DustID.WhiteTorch, DustID.RainbowTorch, DustID.GemDiamond });
                    Dust dust2 = Dust.NewDustPerfect(position2, dustType2, Vector2.Zero, 100, Color.White, Main.rand.NextFloat(1.65f, 1.95f));
                    dust2.noGravity = true;
                }
            }




            SoundEngine.PlaySound(SoundID.Item79, Projectile.position);

            {
                // 搜索范围内的至多5个敌人
                List<NPC> targets = new List<NPC>();
                float searchRadius = 1500f; // 最大搜索半径
                float minDistance = 50f; // 最小距离阈值

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        // 仅选取距离在 minDistance 和 searchRadius 之间的敌人
                        if (distance > minDistance && distance <= searchRadius)
                        {
                            targets.Add(npc);
                        }
                    }
                }

                // 按距离排序，取最近的5个敌人
                targets = targets.OrderBy(npc => Vector2.Distance(Projectile.Center, npc.Center)).Take(5).ToList();

                // 向这些敌人发射弹幕
                foreach (NPC target in targets)
                {
                    Vector2 shootDirection = Vector2.Normalize(target.Center - Projectile.Center) * 19f; // 发射方向
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        shootDirection,
                        ModContent.ProjectileType<StarsofDestinyRLIGHT>(), // 替换为目标弹幕类型
                        (int)(Projectile.damage * 1.2f), // 伤害倍率
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }
            }


        }

    }
}