using CalamityMod;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.HeartSwordSpear
{
    internal class HeartSwordSpearHoldOut : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }


        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;


            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);

                // 选择极快或瞬间转向
                Projectile.velocity = aimDirection * Projectile.velocity.Length(); // 立即转向
                //Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.9f); // 较慢但平滑
            }


            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 0.11f);
            Owner.heldProj = Projectile.whoAmI;

            // 枪头的位置
            Vector2 HeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);


            CheckDamageAlongTrail();

            {
                // 让手臂在 `Full` 和 `ThreeQuarters` 之间随机切换
                Player.CompositeArmStretchAmount armStretch = Main.rand.NextBool()
                    ? Player.CompositeArmStretchAmount.Full
                    : Player.CompositeArmStretchAmount.ThreeQuarters;

                // 让手臂对准鼠标方向，并增加 ±15° 抖动
                Vector2 directionToMouse = Owner.SafeDirectionTo(Main.MouseWorld);
                float armRotation = directionToMouse.ToRotation();
                float randomArmShake = Main.rand.NextFloat(-MathHelper.ToRadians(15f), MathHelper.ToRadians(15f));
                armRotation += randomArmShake;
                armRotation -= MathHelper.PiOver2; // 额外旋转 90° 补偿

                // 设置手臂指向，并使其不断变化
                Owner.SetCompositeArmFront(true, armStretch, armRotation);
            }



            // 检测松手
            if (!Owner.channel)
            {
                Projectile.Kill();
            }

        }
        private int hitCooldownTimer = 0; // 记录上次伤害的时间戳

        private void CheckDamageAlongTrail()
        {
            if (hitCooldownTimer > 0)
            {
                hitCooldownTimer--; // 递减计时器
                return; // 在冷却时间内，不进行伤害检测
            }

            float step = 0.05f; // 每 5% 进行一次检测（可以调整更高/更低）

            for (float i = 0f; i <= 1f; i += step)
            {
                // 计算当前检测点
                float distanceFactor = Utils.Remap(i, 0f, 1f, 1f, 5f);
                Vector2 checkPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * distanceFactor * Projectile.scale;

                // 生成碰撞矩形
                Rectangle hitbox = new Rectangle((int)checkPosition.X - 10, (int)checkPosition.Y - 10, 20, 20);

                // 遍历所有敌人
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && hitbox.Intersects(npc.Hitbox))
                    {
                        // 造成伤害
                        npc.StrikeNPC(new NPC.HitInfo
                        {
                            Damage = Projectile.damage / 2, // 伤害值（这里设定为原伤害的一半）
                            Knockback = 0f, // 设定击退值（这里设为 0，避免影响敌人）
                            HitDirection = (Projectile.velocity.X > 0) ? 1 : -1, // 确定攻击方向
                            Crit = false // 设定是否暴击
                        });

                        // 触发伤害后，进入冷却
                        hitCooldownTimer = 5; // 5 帧内不能再次造成伤害
                    }
                }
            }
        }


        public override bool PreDraw(ref Color lightColor)
        {
            // 返回 false，阻止原本的绘制逻辑
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // **实现随机抖动**
            //float randomRotationOffset = Main.rand.NextFloatDirection() * MathHelper.PiOver4 * 0.1f; // 让它在 ±0.1 * π/4 之间微抖动
            //Vector2 randomOffset = Main.rand.NextVector2Circular(2f, 2f); // 让位置也随机偏移一点点

            //Vector2 drawPosition = Projectile.Center - Main.screenPosition + randomOffset;
            //Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            //SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // 绘制随机抖动的武器
            //Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation + randomRotationOffset, origin, Projectile.scale, effects, 0);

            // **调用星光特效绘制**
            DrawStarlightEffects(Main.spriteBatch);
        }

        // 绘制围绕武器的星光特效
        private void DrawStarlightEffects(SpriteBatch spriteBatch)
        {
            float scaleFactor = Projectile.scale;
            int numStars = (int)Math.Ceiling(3f * scaleFactor);
            Vector2 basePosition = Projectile.Center - Projectile.rotation.ToRotationVector2() * 2f;

            // **使用我们自己的贴图**
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            for (int i = 0; i < 1; i++)
            {
                float randFactor = Main.rand.NextFloat();
                float alphaFactor = Utils.GetLerpValue(0f, 0.3f, randFactor, clamped: true) * Utils.GetLerpValue(1f, 0.5f, randFactor, clamped: true);
                Color weaponColor = Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) * alphaFactor;

                float randomAngle = Main.rand.NextFloatDirection();
                float weaponDistance = 8f + MathHelper.Lerp(0f, 20f, randFactor) + Main.rand.NextFloat() * 6f;
                float weaponRotation = Projectile.rotation + randomAngle * ((float)Math.PI * 2f) * 0.04f;
                float adjustedRotation = weaponRotation + (float)Math.PI / 4f;

                Vector2 weaponPosition = basePosition + weaponRotation.ToRotationVector2() * weaponDistance + Main.rand.NextVector2Circular(8f, 8f) - Main.screenPosition;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (Projectile.rotation < -(float)Math.PI / 2f || Projectile.rotation > (float)Math.PI / 2f)
                {
                    adjustedRotation += (float)Math.PI / 2f;
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                }

                // **绘制我们的武器贴图**
                spriteBatch.Draw(texture, weaponPosition, null, weaponColor, adjustedRotation, origin, 1f, spriteEffects, 0f);
            }

            // **绘制星光**
            // 遍历生成 numStars 颗星光，每颗星光都会随机出现，并带有颜色渐变和随机偏移
            for (int j = 0; j < numStars; j++)
            {
                // 生成一个随机数 (0.0 ~ 1.0)，用于控制星光的透明度、大小等参数
                float randFactor = Main.rand.NextFloat();

                // 计算星光的透明度因子（让星光在一定范围内有渐变效果）
                float alphaFactor = Utils.GetLerpValue(0f, 0.3f, randFactor, clamped: true)
                                  * Utils.GetLerpValue(1f, 0.5f, randFactor, clamped: true);

                // 计算星光的缩放比例，使其大小在 0.6 到 1.0 之间变化
                float sizeFactor = MathHelper.Lerp(0.6f, 1f, alphaFactor);

                // 获取星光的颜色，颜色会随着时间变化
                // 计算一个 0~1 之间的平滑渐变因子，使星光在不同红色间过渡
                float colorLerpFactor = (Main.rand.NextFloat() * 0.33f + Main.GlobalTimeWrappedHourly) % 1f;

                // 让颜色在 **浅红色（粉色） 和 深红色（暗红色）** 之间渐变
                Color glowColor = Color.Lerp(new Color(255, 80, 80), new Color(150, 10, 10), colorLerpFactor);

                // 让颜色有更细微的变化，添加轻微的橙色或粉色偏移
                glowColor = Color.Lerp(glowColor, new Color(255, 50, 100), 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi));

                // 保留原版的 alphaChannelMultiplier（影响透明度）
                glowColor *= 0.25f;


                // 选择用于绘制星光的贴图（使用投射物的贴图）
                Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewSpears/BPrePlantera/HeartSwordSpear/Starlight_(projectile)").Value;

                // 计算最终的星光颜色，乘以 alphaFactor 让颜色有渐变效果
                Color finalGlowColor = glowColor * alphaFactor * 0.5f;

                // 获取贴图的中心点，确保星光围绕这个点进行绘制
                Vector2 glowOrigin = glowTexture.Size() / 2f;

                // 计算星光的随机缩放大小，使其在一定范围内变化
                float randomFactor = Main.rand.NextFloat() * 2f * scaleFactor;

                // 计算星光的旋转偏移方向，使其产生不同角度的变化
                float randomDirection = Main.rand.NextFloatDirection();

                // 计算星光的最终缩放大小（横向拉伸，使其类似光束）
                Vector2 starScale = new Vector2(2.8f + randomFactor * (1f + scaleFactor), 1f) * sizeFactor;

                // 计算星光可以达到的最大距离
                float maxDistance = 50f * scaleFactor;

                // 让某些星光偏移，使其有前后景层次感（类似深度效果）
                Vector2 baseOffset = Projectile.rotation.ToRotationVector2() * ((j >= 1) ? 56 : 0);

                // 计算星光的角度偏移，使星光围绕武器呈现动态旋转效果
                float angleOffset = 0.03f - (float)j * 0.012f;
                angleOffset /= scaleFactor;

                // 计算星光的最终距离，使其在 30 ~ 50f 范围内变化
                float starDistance = 30f + MathHelper.Lerp(0f, maxDistance, randFactor) + randomFactor * 16f;

                // 计算星光的最终旋转角度，使其沿不同方向旋转
                float finalRotation = Projectile.rotation + randomDirection * ((float)Math.PI * 2f) * angleOffset;

                // 计算星光的最终绘制位置：
                // 1. 以 basePosition（弹幕的中心）为基准
                // 2. 施加旋转 finalRotation，使星光呈环绕状
                // 3. 施加 starDistance 让星光远离武器一定距离
                // 4. 加入随机偏移，使其分布更自然
                Vector2 finalPosition = basePosition
                    + finalRotation.ToRotationVector2() * starDistance
                    + Main.rand.NextVector2Circular(20f, 20f)
                    + baseOffset
                    - Main.screenPosition;

                // **绘制星光**
                spriteBatch.Draw(glowTexture, finalPosition, null, finalGlowColor, finalRotation, glowOrigin, starScale, SpriteEffects.None, 0f);
            }
        }








    }
}
