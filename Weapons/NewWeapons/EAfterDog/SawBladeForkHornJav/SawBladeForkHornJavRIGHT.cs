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
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJavRIGHT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SawBladeForkHornJav/SawBladeForkHornJav";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        // 为了让它能够兼容发射阶段的转向
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 背光效果部分 - 亮白色光晕
            float chargeOffset = 3f; // 控制充能效果扩散的偏移量
            Color chargeColor = Color.White * 0.6f; // 设置为亮白色
            chargeColor.A = 0; // 设置透明度

            // 使用 AI 中的旋转角度
            SpriteEffects direction = SpriteEffects.None;

            // 绘制充能效果 - 圆周上绘制多个充能光效
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, Projectile.rotation, origin, Projectile.scale, direction, 0f);
            }

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 只允许一次伤害
            Projectile.timeLeft = 60000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {

            // 第一阶段：逐帧减速
            if (Projectile.ai[0] == 0)
            {
                // 每帧减速
                Projectile.velocity *= 0.97f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;


                // 当速度小于等于2f时，设置速度为0并进入第二阶段
                if (Projectile.velocity.Length() <= 2f)
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.ai[0] = 1; // 切换到第二阶段

                    // 生成棕褐色的冲击波特效
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Brown, new Vector2(1.5f), Projectile.rotation, 3f, 0.1f, 30);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
            // 第二阶段：锁定目标并发射弹幕
            else if (Projectile.ai[0] == 1)
            {
                // 获取最近的敌人
                NPC target = FindClosestNPC();
                if (target != null)
                {
                    // 计算目标方向
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                    // 实时更新弹幕的 rotation，并加上 MathHelper.PiOver4 使其对齐
                    Projectile.rotation = directionToTarget.ToRotation() + MathHelper.PiOver4;

                    // 发射逻辑
                    if (Projectile.ai[1] % 120 == 0) // 每隔120帧触发一次连续发射
                    {
                        int shotsToFire = Main.rand.Next(3, 5); // 随机发射3到4发弹幕
                        int delayBetweenShots = Main.rand.Next(2, 4); // 每两发之间的间隔为2~3帧

                        // 启动异步任务实现延迟发射
                        for (int i = 0; i < shotsToFire; i++)
                        {
                            Main.QueueMainThreadAction(() =>
                            {
                                // 添加随机偏移和动态伤害浮动
                                float damageMultiplier = Main.rand.NextFloat(0.95f, 1.15f); // 伤害浮动
                                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)); // 偏移角度
                                Vector2 velocity = directionToTarget.RotatedBy(angleOffset) * 20f; // 偏移后的发射方向

                                // 发射弹幕
                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    Projectile.Center,
                                    velocity,
                                    ModContent.ProjectileType<SawBladeForkHornJavRPP>(),
                                    (int)(Projectile.damage * damageMultiplier),
                                    Projectile.knockBack,
                                    Projectile.owner
                                );

                                // 播放音效
                                SoundStyle soundStyle = CommonCalamitySounds.LargeWeaponFireSound with { Volume = 0.25f };
                                SoundEngine.PlaySound(soundStyle, Projectile.Center);
                            });

                            // 等待下次发射
                            Main.QueueMainThreadAction(() =>
                            { Projectile.ai[1] += delayBetweenShots; });
                        }
                    }

                    // 冷却时间
                    Projectile.ai[1]++;
                }
            }












        }

        // 目标锁定逻辑（示例）
        private NPC FindClosestNPC()
        {
            NPC closestNPC = null;
            float closestDistance = 6666f; // 搜索半径

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            return closestNPC;
        }


    }
}