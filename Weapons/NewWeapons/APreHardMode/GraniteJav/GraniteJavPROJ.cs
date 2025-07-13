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
        private int afterimageCooldown = 0; // 拖影暂停计时器
        private bool returning = false; // 是否进入回程状态
        private int stopTime = 10;      // 停顿时间
        private Vector2 originalVelocity; // 记录初始速度

        public override bool PreDraw(ref Color lightColor)
        {
            if (afterimageCooldown <= 0)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
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
            //Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色更改为偏黑的深蓝色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.1f, 0.5f) * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            // === 📐 古典数学式“模糊追踪轨迹” ===

            if (!returning)
            {
                // 每颗弹幕有一个属于自己的“随机扰动参数”
                if (Projectile.localAI[0] == 0f)
                {
                    Projectile.localAI[0] = Main.rand.NextFloat(-0.05f, 0.05f); // 旋偏角因子
                    Projectile.localAI[1] = Main.rand.NextFloat(0.9f, 1.1f);    // 飞行速度因子
                }

                float spiralStrength = Projectile.localAI[0];
                float speedMultiplier = Projectile.localAI[1];

                Projectile.velocity = Projectile.velocity.RotatedBy(spiralStrength);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f * speedMultiplier;

                // 模糊地吸向最近敌人（但精度极差）
                NPC target = FindClosestTarget(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    float curveIn = 0.02f; // 极弱角度偏修正
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), curveIn);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }





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

            if (afterimageCooldown > 0)
                afterimageCooldown--;


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
        private NPC FindClosestTarget(float range)
        {
            NPC closest = null;
            float minDist = range;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/花岗岩矛音效") with { Volume = 1.0f, Pitch = 0.0f }, Projectile.Center);
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

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.penetrate > 1)
            {
                Projectile.penetrate--;

                // 检测当前是撞哪种类型的面
                bool hitHorizontal = Math.Abs(oldVelocity.Y) > Math.Abs(oldVelocity.X);

                // 计算一个偏移角度（受控的偏转，模拟“理性跳弹”）
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));

                // 获取碰撞法线
                Vector2 reflectNormal = hitHorizontal ? Vector2.UnitY : Vector2.UnitX;
                Vector2 inDirection = oldVelocity.SafeNormalize(Vector2.Zero);
                Vector2 reflectDir = Vector2.Reflect(inDirection, reflectNormal).RotatedBy(angleOffset);

                // 应用速度（保持能量感）
                Projectile.velocity = reflectDir * oldVelocity.Length() * 0.85f;

                // 🧱 反弹修正：把弹幕往新方向稍微推出一点点，避免卡墙体
                Projectile.position += reflectDir * 4f;

                // 拖影关闭 + 撞击音效
                afterimageCooldown = 10;
                SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);

                // 临时关闭 tileCollide，避免下一帧误判
                Projectile.tileCollide = false;

                return false;
            }

            return true;
        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/花岗岩矛音效") with { Volume = 1.0f, Pitch = 0.0f }, Projectile.Center);

            // 收敛版阿基米德螺旋 Dust 爆散
            int spiralDusts = 30; // 原 80 ➜ 30
            for (int i = 0; i < spiralDusts; i++)
            {
                float t = i / (float)spiralDusts * 4f * MathHelper.Pi; // 螺旋次数少
                float r = 1.5f + 0.2f * t; // 半径压缩

                Vector2 velocity = t.ToRotationVector2() * r;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.BlueTorch,
                    velocity,
                    80,
                    Color.LightSteelBlue,
                    1.1f
                );
                d.noGravity = true;
            }
        }


    }
}