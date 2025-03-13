using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewSpears.EAfterDog.AuricSpear
{
    internal class AuricSpearHoldOut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // **金色包边效果**
            float chargeOffset = 3f + attackTimer * 0.1f; // 包边随着计时器增加
            Color chargeColor = Color.Gold * (0.3f + attackTimer * 0.01f); // 充能颜色强化
            chargeColor.A = 0;

            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            SpriteEffects direction = SpriteEffects.None;

            // **绘制充能效果**
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // **绘制实际长枪**
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }

        public enum BehaviorState
        {
            Aim,
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

          
        }
        private int attackTimer = 0; // 自定义计时器

        public override void AI()
        {
            attackTimer++; // 计时器递增

            if (attackTimer >= 45) // 每 45 帧触发一次攻击
            {
                PerformAttack(); // 触发攻击
                attackTimer = 0; // 重置计时器
            }

            DoBehavior_Aim(); // 维持长枪跟随鼠标
        }
        private void PerformAttack()
        {
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            int projectileCount = Main.rand.Next(6, 9); // 6~8 发
            for (int i = 0; i < projectileCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 360° 随机方向
                float speed = Main.rand.NextFloat(6f, 8f); // 6f ~ 8f 速度
                Vector2 velocity = randomAngle.ToRotationVector2() * speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    headPosition, velocity,
                    ModContent.ProjectileType<AuricSpearCell>(), // 释放光弹
                    Projectile.damage / 2, 0f, Projectile.owner
                );
            }

            // 播放攻击音效
            SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
        }


        private int shootTimer = 0; // 计时器
        private int holdTime = 0; // 握持时间（帧）

        private int postBigShotCooldown = 0; // 大招释放后进入冷却的计时器

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = aimDirection * Projectile.velocity.Length();
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            // **蓄力期间生成粒子**
            if (Main.rand.NextBool(2)) // 控制粒子生成频率
            {
                int[] dustTypes = { DustID.YellowStarDust, DustID.YellowTorch, DustID.YellowStarfish };
                int dustType = dustTypes[Main.rand.Next(dustTypes.Length)];

                Dust dust = Dust.NewDustPerfect(
                    headPosition + Main.rand.NextVector2Circular(8f, 8f), dustType,
                    Vector2.Zero, 100, Color.Yellow, 1.5f
                );

                dust.noGravity = true;
                dust.velocity = (Projectile.Center - dust.position).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);
            }

            if (!Owner.channel)
            {
                Projectile.Kill();
            }
        }






        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
    }
}