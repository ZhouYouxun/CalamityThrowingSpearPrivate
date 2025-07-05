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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav
{
    public class GraniteJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/GraniteJav/GraniteJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 允许3次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色更改为偏黑的深蓝色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.1f, 0.5f) * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            {
                // 🪨 GraniteJav 飞行特效：李萨如曲线动态 Dust + 双螺旋环绕
                float t = Main.GameUpdateCount * 0.15f;
                float lissX = 8f * (float)Math.Sin(3 * t);
                float lissY = 8f * (float)Math.Sin(2 * t + MathHelper.PiOver4);
                Vector2 lissajousOffset = new Vector2(lissX, lissY);

                // 每帧微抖动尾迹
                Dust d1 = Dust.NewDustPerfect(
                    Projectile.Center + lissajousOffset,
                    DustID.GemSapphire,
                    -Projectile.velocity * 0.1f,
                    100,
                    Color.CornflowerBlue,
                    1.0f
                );
                d1.noGravity = true;

                // 每 5 帧生成螺旋环绕 Dust
                if (Main.GameUpdateCount % 5 == 0)
                {
                    float spiralRadius = 10f;
                    float spiralAngle = Main.GameUpdateCount * 0.2f;
                    for (int s = 0; s < 2; s++)
                    {
                        float offsetAngle = spiralAngle + s * MathHelper.Pi;
                        Vector2 offset = offsetAngle.ToRotationVector2() * spiralRadius;

                        Dust d2 = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            DustID.BlueTorch,
                            offset.RotatedBy(MathHelper.PiOver2) * 0.2f,
                            120,
                            Color.LightBlue,
                            1.2f
                        );
                        d2.noGravity = true;
                    }
                }

            }


            // 每帧增加 ai[0] 计数
            Projectile.ai[0]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[0] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 当命中敌人时，弹幕向上飞行，且带有一定的左右偏移
            float angle = MathHelper.ToRadians(Main.rand.Next(-10, 10)); // 在正上方左右10度范围内取随机角度
            Vector2 upwardVelocity = new Vector2(0, -8f).RotatedBy(angle); // 初始向上飞行的速度
            Projectile.velocity = upwardVelocity;

            // 继续受到重力影响
            Projectile.velocity.Y += 0.2f; // 模拟逐渐受重力下坠

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);


            {
                // 🪨 GraniteJav 命中特效：花岗岩爆裂散射 Dust（玫瑰曲线 + 双曲线）
                int petals = 60;
                for (int i = 0; i < petals; i++)
                {
                    float theta = MathHelper.TwoPi * i / petals;
                    float r = 6f * (1 + 0.5f * (float)Math.Sin(5 * theta)); // 五瓣玫瑰

                    // 使用双曲线变形增强离散感
                    float modifier = (float)Math.Tan(theta * 1.5f) * 0.1f;
                    modifier = MathHelper.Clamp(modifier, -1f, 1f); // 防止爆炸性发散

                    Vector2 velocity = theta.ToRotationVector2() * r * (1 + modifier);

                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.Electric,
                        velocity,
                        100,
                        Color.RoyalBlue,
                        1.4f
                    );
                    d.noGravity = true;
                }

            }


        }

        public override void OnKill(int timeLeft)
        {


            {
                // 🪨 GraniteJav 死亡特效：螺旋爆散 Dust（阿基米德螺旋）
                int spiralDusts = 80;
                for (int i = 0; i < spiralDusts; i++)
                {
                    float t = i / (float)spiralDusts * 6f * MathHelper.Pi;
                    float r = 2f + 0.5f * t; // 螺旋半径递增
                    Vector2 velocity = t.ToRotationVector2() * r;

                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.BlueTorch,
                        velocity,
                        100,
                        Color.MediumBlue,
                        1.3f
                    );
                    d.noGravity = false;
                }

            }


        }



    }
}