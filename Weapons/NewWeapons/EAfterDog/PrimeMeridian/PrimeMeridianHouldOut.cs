using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    internal class PrimeMeridianHouldOut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/PrimeMeridian/PrimeMeridian";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 155;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public Player Owner => Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {


        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 不断的重置剩余时间
            Projectile.timeLeft = 1;

            // 设置穿透次数为 -1
            Projectile.penetrate = -1;

            // 不断的让它可以穿透方块
            Projectile.tileCollide = false;

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 将投射物位置与玩家中心对齐，模拟持握效果
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.05f);
            Owner.heldProj = Projectile.whoAmI;








            // 让玩家的双手一直朝向投射物方向，模拟握持长枪
            ManipulatePlayerArmPositions();

            // 检查玩家是否松开鼠标
            if (!Owner.channel)
            {
                Projectile.timeLeft = 1;
                Projectile.netUpdate = true;
            }
        }

        public void ManipulatePlayerArmPositions()
        {
            // 让玩家的手臂方向始终朝向长枪的方向
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            // 计算双臂应当指向的角度，使其平行向前
            float armRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 设置玩家前臂（主手）和后臂（副手）的角度，使其平行前伸
            // 第1个参数设置为正确，意味着它将会使用自定义手臂，设置为错误，则不进行更改
            // 第2个参数决定了伸手臂的长度：【也就是伸出了多少，并不是指的角度】
            // Full（完全伸展，适用于拿长枪、拉弓等）
            // None（不伸展，手臂保持贴近身体）
            // Quarter（25 % 伸展，适用于轻微举起手臂）
            // ThreeQuarters（75 % 伸展，适用于半握持状态）
            // 第3个参数armRotation决定了手臂的弯曲角度，你要想让他平行向前？高举45度？还是往下放？

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
            //Owner.fullRotation = armRotation;
            //Owner.headRotation = armRotation; // 让玩家的头部旋转
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          
        }

        public override void OnKill(int timeLeft)
        {


        }
    }
}
