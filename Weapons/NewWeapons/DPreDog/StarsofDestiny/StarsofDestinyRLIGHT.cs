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
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyRLIGHT : ModProjectile
    {
        //public override string Texture => "Terraria/Images/Item_4923"; // 星光
        public override string Texture => "Terraria/Images/Extra_89";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 计算脉动透明度 (呼吸效果)
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI) * 0.15f + 0.85f;
            Color pulseColor = Color.White * pulse;
            pulseColor.A = 0;

            // 绘制拖尾
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], pulseColor, 1);

            // 绘制本体
            Main.EntitySpriteDraw(texture, drawPos, frame, pulseColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 200;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.scale = 1.5f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.6f);
            Projectile.velocity *= 1.01f;

            // 生成白色科技流动粒子
            if (Main.rand.NextBool(2))
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 0.6f);

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + spawnOffset,
                    DustID.WhiteTorch,
                    velocity,
                    150,
                    Color.White * 0.8f,
                    Main.rand.NextFloat(1.0f, 1.4f)
                );
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // 可选：加入细线流动感 SparkParticle
            if (Main.rand.NextBool(10))
            {
                SparkParticle spark = new SparkParticle(
                    Projectile.Center,
                    Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 0.6f),
                    false,
                    10,
                    1.2f,
                    Color.White
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            Time++;
        }




        public ref float Time => ref Projectile.ai[1];

        public override bool? CanDamage() => Time >= 5f; // 初始的时候不会造成伤害，直到x为止



    }
}