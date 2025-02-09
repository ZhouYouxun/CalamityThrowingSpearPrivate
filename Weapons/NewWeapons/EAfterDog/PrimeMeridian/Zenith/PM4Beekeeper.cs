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
using CalamityMod.NPCs.PlaguebringerGoliath;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM4Beekeeper : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1123"; // 使用原版的贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 计算交替颜色（黄色 ↔ 黑色）
            Color[] beeColors = { Color.Yellow, Color.Black };
            int colorIndex = (int)(Main.GlobalTimeWrappedHourly * 6f) % 2;
            Color outlineColor = beeColors[colorIndex] * 0.6f;
            outlineColor.A = 0; // 透明度

            // 充能描边
            float chargeOffset = 3f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, outlineColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

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


            if (Main.rand.NextBool(3)) // 随机释放
            {
                int dustType = Main.rand.NextBool() ? 152 : 153; // 152 和 153 混用
                Dust honeyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
                honeyDust.velocity *= 0.2f; // 初速度非常慢
                honeyDust.rotation = Main.rand.NextFloat(MathHelper.TwoPi) * 0.1f; // 旋转也很慢
                honeyDust.noGravity = true;
            }

            // 前X帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 80)
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


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type == NPCID.QueenBee || target.type == ModContent.NPCType<PlaguebringerGoliath>())
            {
                modifiers.FinalDamage *= 50f; // 对特定敌人造成 50 倍伤害
            }
        }

        public override void OnKill(int timeLeft)
        {

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); // 播放蜂鸣音效

            int beeAmount = Main.rand.Next(5, 8); // 随机 5~7 个
            for (int i = 0; i < beeAmount; i++)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10 * 16, 10 * 16); // 10×16 范围内随机点
                Vector2 shootVelocity = (spawnPos - Projectile.Center).SafeNormalize(Vector2.Zero) * (Projectile.velocity.Length() * 1.0f);

                int beeType = Main.rand.Next(new int[] { ProjectileID.Bee, ProjectileID.GiantBee, ProjectileID.Wasp });

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    shootVelocity,
                    beeType,
                    (int)(Projectile.damage * 1.0f), // 伤害倍率 1.0
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

        }
    }
}
