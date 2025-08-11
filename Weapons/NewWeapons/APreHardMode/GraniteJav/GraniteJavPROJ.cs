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
        private int deathDropDelayTimer = 0; // 死亡下落延迟计时器
        private bool hasTriggeredDeathDrop = false;

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
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 允许3次伤害
            Projectile.timeLeft = 480;
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

            bool isAltMode = Projectile.localAI[0] == 0f; // 来自右键的标记

            if (isAltMode)
            {
                RunChaosFlight(); // 👈 原来的扰动+追踪逻辑
            }
            /*else
            {
                Projectile.aiStyle = ProjAIStyleID.Arrow; // 标准箭矢逻辑
                RunDeathDropCheck();                      // 👈 加入死亡下落机制
            }*/

            DoSharedDustEffects(); // 两种模式共用的视觉效果


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


        private void RunChaosFlight()
        {
            if (!returning)
            {
                if (Projectile.localAI[1] == 0f)
                {
                    Projectile.localAI[1] = 1f;
                    Projectile.localAI[2] = Main.rand.NextFloat(-0.05f, 0.05f);
                }

                float spiralStrength = Projectile.localAI[2];
                Projectile.velocity = Projectile.velocity.RotatedBy(spiralStrength);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f;

                NPC target = FindClosestTarget(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.02f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
        }

        /*private void RunDeathDropCheck()
        {
            // 在正下方范围寻找敌人
            Rectangle checkBox = new Rectangle(
                (int)(Projectile.Center.X - 25),
                (int)(Projectile.Center.Y),
                50,
                50 * 16 // 50格下方
            );

            bool enemyFound = false;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && npc.Hitbox.Intersects(checkBox))
                {
                    enemyFound = true;
                    break;
                }
            }

            if (enemyFound && !hasTriggeredDeathDrop)
            {
                deathDropDelayTimer++;

                if (deathDropDelayTimer >= 3) // 🕒 延迟 3 帧才触发
                {
                    float speed = Projectile.velocity.Length() * 1.5f;
                    Projectile.velocity = Vector2.UnitY * speed;

                    afterimageCooldown = 10; // ✅ 只执行一次的拖影关闭
                    hasTriggeredDeathDrop = true; // ✅ 标记已触发
                }
            }
            else if (!enemyFound)
            {
                deathDropDelayTimer = 0;
            }

        }*/



        private void DoSharedDustEffects()
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
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.0f, Pitch = 0.0f }, Projectile.position);

            {
                // 获取命中时的真实角度
                float baseAngle = Projectile.velocity.ToRotation();

                int petals = 30;
                for (int i = 0; i < petals; i++)
                {
                    float theta = MathHelper.TwoPi * i / petals;
                    float r = 3f * (1 + 0.4f * (float)Math.Sin(5 * theta));

                    float modifier = (float)Math.Tan(theta * 1.5f) * 0.1f;
                    modifier = MathHelper.Clamp(modifier, -0.6f, 0.6f);

                    Vector2 local = theta.ToRotationVector2() * r * (1 + modifier);

                    // 🌟 整体绕命中角度旋转
                    Vector2 velocity = local.RotatedBy(baseAngle);

                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.Electric,
                        velocity,
                        100,
                        Color.RoyalBlue,
                        1.3f
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