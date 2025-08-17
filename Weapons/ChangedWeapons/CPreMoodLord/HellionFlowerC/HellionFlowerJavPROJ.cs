using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC
{
    public class HellionFlowerJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private bool isStuck = false; // 标记弹幕是否已经粘附
        private int stuckTime = 0; // 计时器
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJav").Value;

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 使用红绿渐变
                Color color = Color.Lerp(Color.Lime, Color.LightGreen, colorInterpolation) * 0.4f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.5f;

                // 计算强度，使拖尾逐渐变弱
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 如果弹幕即将消失，则拖尾也逐渐消失
                }

                // 计算外部和内部的缩放比例，使拖尾具有渐变效果
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // 绘制默认的弹幕，并应用旋转
            Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 允许？次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 24; // 无敌帧冷却时间为14帧
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 遍历所有活跃的弹幕
            //for (int i = 0; i < Main.maxProjectiles; i++)
            //{
            //    Projectile proj = Main.projectile[i];
            //    if (proj.active && proj.type == ModContent.ProjectileType<HellionFlowerJavAbsorb>())
            //    {
            //        proj.Kill(); // 销毁所有活跃的弹幕
            //    }
            //}
        }

        public override void AI()
        {
            // 速度衰减
            Projectile.velocity *= 0.992f;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);




            // 每隔一定帧数生成 HellionFlowerJavAbsorb
            if (Projectile.localAI[0] <= 0)
            {
                // 检查场上 HellionFlowerJavAbsorb 的数量
                int absorbCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == ModContent.ProjectileType<HellionFlowerJavAbsorb>())
                    {
                        absorbCount++;
                    }
                }

                // 上限从 8 -> 20
                if (absorbCount < 20)
                {
                    float radius = 80 * 16f;

                    // ✨ 使用旋转角度，而不是完全随机
                    // 利用 localAI[1] 存储旋转角度累计值
                    if (Projectile.localAI[1] == 0)
                        Projectile.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi); // 初始随机起点

                    Projectile.localAI[1] += MathHelper.ToRadians(18f); // 每次旋转 18°（可调节旋转规律）
                    float angle = Projectile.localAI[1];

                    Vector2 spawnPosition = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                    // 朝向当前弹幕的速度
                    Vector2 velocityToSelf = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * 10f;

                    // 生成 HellionFlowerJavAbsorb 弹幕
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        velocityToSelf,
                        ModContent.ProjectileType<HellionFlowerJavAbsorb>(),
                        (int)(Projectile.damage * 0.6f),
                        Projectile.knockBack,
                        Main.myPlayer
                    );
                }

                // 触发时间从 5~15 帧 -> 1~8 帧
                Projectile.localAI[0] = Main.rand.Next(1, 9);
            }
            else
            {
                Projectile.localAI[0]--;
            }





            // 每 12 帧发射两侧弹幕
            if (Projectile.timeLeft % 12 == 0)
            {
                // 发射左侧和右侧弹幕
                Vector2 baseVelocity = Projectile.velocity * 0.6f; // FlowerPetal速度为60%本体速度
                Vector2 leftVelocity = baseVelocity.RotatedBy(-MathHelper.PiOver2); // 左侧
                Vector2 rightVelocity = baseVelocity.RotatedBy(MathHelper.PiOver2); // 右侧

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftVelocity, ModContent.ProjectileType<HellionFlowerJavSPIT>(), (int)(Projectile.damage * 0.4f), 0f, Main.myPlayer);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightVelocity, ModContent.ProjectileType<HellionFlowerJavSPIT>(), (int)(Projectile.damage * 0.4f), 0f, Main.myPlayer);
            }

            // 尖端生成粒子特效
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
            int dustType = Main.rand.Next(new int[] { 39, 40, DustID.JungleSpore, DustID.GemEmerald });

            // 增加粒子生成的数量
            for (int i = 0; i < 3; i++) // 每次生成更多粒子
            {
                Dust dust = Dust.NewDustPerfect(tipPosition, dustType, Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1.5f, 1.5f), 150, default, Main.rand.NextFloat(1.5f, 2.0f));
                dust.noGravity = true;

                // 让粒子有一定的向外扩散效果
                dust.velocity += new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 在 OnKill 中绘制一个抽象的花朵形状
            int petalCount = 12; // 花瓣数量
            float petalRadius = 50f; // 花瓣半径
            float centerRadius = 20f; // 花心半径
            int dustType = Main.rand.Next(new int[] { 39, 40, DustID.JungleSpore, DustID.GemEmerald }); // 随机粒子类型

            // 绘制花心
            for (int i = 0; i < 20; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(centerRadius);
                Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 150, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            // 绘制花瓣
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi / petalCount * i; // 计算每个花瓣的角度
                for (float t = -1f; t <= 1f; t += 0.1f) // 绘制每个花瓣的轨迹
                {
                    // 使用心形公式调整花瓣形状
                    float radius = petalRadius * (1 - t * t);
                    Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius + new Vector2((float)Math.Cos(angle + MathHelper.PiOver2) * t, (float)Math.Sin(angle + MathHelper.PiOver2) * t) * petalRadius * 0.3f;

                    Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 150, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0.5f;
                }
            }


            // 生成绿色烟雾特效
            int Dusts = 55;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.Green, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 生成 6 个 HellionFlowerJavVine，平均分布
            int vineCount = 6;
            float angleStep = MathHelper.TwoPi / vineCount;
            for (int i = 0; i < vineCount; i++)
            {
                Vector2 velocity = new Vector2(0, 15).RotatedBy(i * angleStep); // 固定初始速度为 X
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<HellionFlowerJavVine>(), (int)(Projectile.damage * 0.2f), Projectile.knockBack, Main.myPlayer);
            }

            // 第一组弹幕：4 个方向，每方向 3 发
            for (int i = 0; i < 4; i++)
            {
                Vector2 baseDirection = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i);
                for (float speedMultiplier = 0.5f; speedMultiplier <= 1.0f; speedMultiplier += 0.25f)
                {                    
                    Vector2 velocity = baseDirection * Projectile.velocity.Length() * speedMultiplier * 0.5f; // 速度
                    int projIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, Main.rand.Next(new int[] { 511, 512, 513 }), (int)(Projectile.damage * 0.25f), 0f, Main.myPlayer);
                    Projectile proj = Main.projectile[projIndex];
                    proj.DamageType = DamageClass.Melee;
                    proj.friendly = true;
                    proj.penetrate = 8;
                    proj.timeLeft = 100;
                    proj.localNPCHitCooldown = 60;
                    proj.usesLocalNPCImmunity = true;
                }
            }


            // 第二组弹幕：正方形分布
            int squareEdgeCount = 5;
            float squareEdgeLength = 50f; // 正方形边长
            for (int i = 0; i < 4; i++) // 四条边
            {
                Vector2 startPoint = Vector2.UnitX * squareEdgeLength / 2f; // 起始点
                startPoint = startPoint.RotatedBy(MathHelper.PiOver2 * i); // 旋转到正确的边
                Vector2 edgeDirection = startPoint.RotatedBy(MathHelper.PiOver4) * 0.4f; // 方向
                for (int j = 0; j < squareEdgeCount; j++) // 每条边 5 个弹幕
                {
                    Vector2 spawnPosition = Projectile.Center + startPoint + edgeDirection * j;
                    Vector2 initialVelocity = edgeDirection * Main.rand.NextFloat(2f, 4f);
                    int projIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, initialVelocity, Main.rand.Next(new int[] { 511, 512, 513 }), (int)(Projectile.damage * 0.25f), 0f, Main.myPlayer);
                    Projectile proj = Main.projectile[projIndex];
                    proj.DamageType = DamageClass.Melee;
                    proj.friendly = true;
                    proj.penetrate = 8;
                    proj.timeLeft = 100;
                    proj.localNPCHitCooldown = 60;
                    proj.usesLocalNPCImmunity = true;
                }
            }

            // 第三组弹幕：随机扩散
            int randomProjectileCount = Main.rand.Next(10, 16); // 随机生成 10~15 个弹幕
            for (int i = 0; i < randomProjectileCount; i++)
            {
                // 随机角度（0 到 2π 弧度）
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);

                // 随机速度（与第一组弹幕速度相似）
                float speed = Projectile.velocity.Length() * Main.rand.NextFloat(0.5f, 1.0f); // 随机速度倍率

                // 计算弹幕速度向量
                Vector2 velocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * speed;

                // 生成弹幕
                int projIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, Main.rand.Next(new int[] { 511, 512, 513 }), (int)(Projectile.damage * 0.25f), 0f, Main.myPlayer);
                Projectile proj = Main.projectile[projIndex];
                proj.DamageType = DamageClass.Melee;
                proj.friendly = true;
                proj.penetrate = 8;
                proj.timeLeft = 100; // 子弹存活时间
                proj.localNPCHitCooldown = 60; // NPC 无敌帧
                proj.usesLocalNPCImmunity = true; // 启用本地无敌帧
            }

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 300); // 原版的酸性毒液效果
        }


       
    }
}
