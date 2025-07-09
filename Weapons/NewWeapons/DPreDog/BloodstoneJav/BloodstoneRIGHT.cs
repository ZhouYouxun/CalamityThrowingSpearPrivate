using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav
{
    public class BloodstoneRIGHT : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/BloodstoneJav/BloodstoneJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 295)
            {
                // 在timeLeft > 295时，不绘制任何内容
                return false;
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Vector2 origin = texture.Size() * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; ++i)
            {
                float afterimageRot = Projectile.oldRot[i];
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Color afterimageColor = Color.MediumVioletRed * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(texture, drawPos, null, afterimageColor, afterimageRot, origin, Projectile.scale * 0.5f, 0, 0f);

                if (i > 0)
                {
                    for (float j = 0.2f; j < 0.8f; j += 0.2f)
                    {
                        drawPos = Vector2.Lerp(Projectile.oldPos[i - 1], Projectile.oldPos[i], j) +
                            Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                        Main.spriteBatch.Draw(texture, drawPos, null, afterimageColor, afterimageRot, origin, Projectile.scale * 0.5f, 0, 0f);
                    }
                }
            }

            Color color = Color.Red * 0.5f;
            color.A = 0;

            Main.spriteBatch.Draw(texture, drawPosition, null, color, Projectile.rotation, origin, Projectile.scale * 0.9f, 0, 0);
            Color bigGleamColor = color;
            Color smallGleamColor = color * 0.5f;
            float opacity = (float)(Utils.GetLerpValue(15f, 30f, Projectile.timeLeft, true) *
                Utils.GetLerpValue(240f, 200f, Projectile.timeLeft, true) *
                (1f + 0.2f * Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * MathHelper.Pi * 6f)) * 0.8f);
            Vector2 bigGleamScale = new Vector2(0.5f, 5f) * opacity;
            Vector2 smallGleamScale = new Vector2(0.5f, 2f) * opacity;
            bigGleamColor *= opacity;
            smallGleamColor *= opacity;

            Main.spriteBatch.Draw(texture, drawPosition, null, bigGleamColor, 1.57079637f, origin, bigGleamScale, 0, 0);
            Main.spriteBatch.Draw(texture, drawPosition, null, bigGleamColor, 0f, origin, smallGleamScale, 0, 0);
            Main.spriteBatch.Draw(texture, drawPosition, null, smallGleamColor, 1.57079637f, origin, bigGleamScale * 0.6f, 0, 0);
            Main.spriteBatch.Draw(texture, drawPosition, null, smallGleamColor, 0f, origin, smallGleamScale * 0.6f, 0, 0);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        private Vector2[] triangleVertices = new Vector2[3]; // 存储三角形顶点的位置
        private float rotationAngle = 0f; // 当前旋转角度

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            if (Projectile.timeLeft <= 295)
            {
                // 初始化等边三角形顶点的相对位置（半径为20像素，可以根据需要调整）
                float triangleRadius = 20f;
                if (triangleVertices[0] == Vector2.Zero)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MathHelper.TwoPi / 3 * i; // 等分的三个顶点
                        triangleVertices[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * triangleRadius;
                    }
                }

                // 更新旋转角度，每帧旋转更大的角度以增加旋转速度
                rotationAngle += MathHelper.ToRadians(2); // 每帧旋转2度

                // 生成直线粒子特效
                if (Main.rand.NextBool(2)) // 每帧有 50% 概率生成
                {
                    Dust trailDust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] { DustID.RedTorch, DustID.Blood, DustID.LifeCrystal, DustID.GemRuby, DustID.Cloud }),
                        Projectile.velocity * Main.rand.NextFloat(0.5f, 1.2f), 0, Color.Red, Main.rand.NextFloat(1.0f, 1.5f));
                    trailDust.noGravity = true;
                }

                // 计算旋转后的顶点位置
                for (int i = 0; i < 3; i++)
                {
                    // 旋转顶点位置
                    Vector2 rotatedPosition = triangleVertices[i].RotatedBy(rotationAngle) + Projectile.Center;

                    // 外围粒子特效
                    for (int j = 0; j < 2; j++) // 每帧每个顶点生成 2 个粒子
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(3f, 3f); // 随机偏移
                        Dust vertexDust = Dust.NewDustPerfect(rotatedPosition + offset, Main.rand.Next(new int[] { DustID.RedTorch, DustID.Blood, DustID.LifeCrystal, DustID.GemRuby, DustID.Cloud }),
                            (rotatedPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 1.5f), 0, Color.Red, Main.rand.NextFloat(1.2f, 1.8f));
                        vertexDust.noGravity = true;
                    }
                }
            }
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 反弹效果
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            {
                // 恢复玩家生命值
                Player player = Main.player[Projectile.owner];
                float healMultiplier = Main.zenithWorld ? 100f : 0.01f; // 根据是否启用 zenithWorld 设置恢复倍率
                int healAmount = (int)(damageDone * healMultiplier);
                player.statLife += healAmount;
                player.HealEffect(healAmount);
            }


            {
                // 生成血雾效果
                int dustCount = 3;
                float radians = MathHelper.Pi / dustCount;
                Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 dustVelocity = smokePoint.RotatedBy(radians * i - MathHelper.ToRadians(15)) * Main.rand.NextFloat(1f, 2.6f);
                    Color smokeColor = Color.Red;
                    Particle bloodFog = new HeavySmokeParticle(Projectile.Center, dustVelocity, smokeColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                    GeneralParticleHandler.SpawnParticle(bloodFog);
                }
            }


            {
                // 生成类似 Visceral 爆炸效果
                int projectileIndex = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<VisceraBoom>(),
                    Projectile.damage,
                    Projectile.knockBack * 4,
                    Projectile.owner
                );
                // 设置属性
                Projectile proj = Main.projectile[projectileIndex];
                proj.DamageType = DamageClass.Ranged;
            }



            {
                // 血液爆炸冲击波
                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(40 * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, 40);
                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
            }


        }

    }
}

