using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears
{
    public abstract class BaseSpearProjSimple : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner]; // 获取投射物拥有者

        // 定义速度变量，子类可覆盖
        public virtual float InitialSpeed => 3f;
        public virtual float ForwardSpeed => 0.8f;
        public virtual float ReelbackSpeed => 2.4f;

        // 额外属性
        public virtual int LocalNPCHitCooldown => 25; // 默认无敌帧 25，可由子类覆盖
        public virtual int ProjectileSize => 15; // 默认尺寸 15，可由子类覆盖

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = ProjectileSize; // 让子类决定尺寸
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.timeLeft = int.MaxValue; // 让投掷物永远存在
            Projectile.usesLocalNPCImmunity = true; // 始终为 true
            Projectile.localNPCHitCooldown = LocalNPCHitCooldown; // 可由子类覆盖
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 让玩家面向投掷物方向，并设定为当前武器
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation;

            // 让投掷物始终与玩家相对静止
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);

            Vector2 targetDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = targetDirection; // 让投掷物的方向每帧更新

            // **前进和回收逻辑**
            if (Projectile.ai[0] == 0f) // 初始速度设定
            {
                Projectile.ai[0] = InitialSpeed;
                Projectile.netUpdate = true;
            }

            if (player.itemAnimation < player.itemAnimationMax / 3) // **回收阶段**
            {
                Projectile.ai[0] -= ReelbackSpeed;
            }
            else // **前进阶段**
            {
                Projectile.ai[0] += ForwardSpeed;
            }

            // **更新投掷物位置**
            Projectile.position += Projectile.velocity * Projectile.ai[0];

            // **攻击动画结束后，销毁投掷物**
            if (player.itemAnimation <= 1)
            {
                Projectile.Kill();
            }

            // **调整旋转角度**
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.PiOver2;
            }
        }

        // **绘制函数（与原版 PreDraw 逻辑相同）**
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * (texture.Height / 2f);
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f); // 修正偏移
            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, 0, 0);





            return false;
        }




    }
}
