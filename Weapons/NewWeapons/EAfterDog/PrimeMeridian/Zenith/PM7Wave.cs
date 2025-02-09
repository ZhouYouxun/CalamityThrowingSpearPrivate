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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM7Wave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_2880"; // 使用原版的贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("Terraria/Images/Item_2880").Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色渐变，使全息光晕呈现蓝色变化
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 30f + Main.GlobalTimeWrappedHourly / 15f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color holoBlue = Color.Lerp(Color.Cyan, Color.BlueViolet, colorInterpolation) * 0.5f;
                holoBlue.A = 0;

                // 计算绘制位置，并引入随机偏移，使全息效果具有跳动感
                Vector2 randomOffset = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f));
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + randomOffset + new Vector2(0f, Projectile.gfxOffY);

                // 计算拖尾强度，使其随着时间衰减
                float intensity = 0.8f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.2f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 逐渐消失
                }

                // 计算外部和内部的缩放比例
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(1.5f) * intensity;
                holoBlue *= intensity;

                // 绘制拖尾
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, holoBlue, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, holoBlue * 0.6f, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }
            return false;
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


            if (Main.rand.NextBool(2)) // 持续释放
            {
                int dustType = Main.rand.NextBool() ? 229 : 226; // 混合两种粒子
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.4f);
                dust.velocity *= 0f; // 让粒子静止形成直线
                dust.noGravity = true;
            }

            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 110)
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
            SoundEngine.PlaySound(SoundID.Item90, Projectile.Center); // 播放音效

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 forwardOffset = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (5 * 16);
            Vector2 leftOffset = forwardOffset + new Vector2(-1 * 16, 0);
            Vector2 rightOffset = forwardOffset + new Vector2(1 * 16, 0);

            // 生成两个平行的 Influx Waver 弹幕，向自己方向飞行
            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnPos = i == 0 ? leftOffset : rightOffset;
                Vector2 shootVelocity = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    shootVelocity,
                    ProjectileID.InfluxWaver,
                    (int)(Projectile.damage * 1.5f), // 伤害倍率 1.5
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }
}
