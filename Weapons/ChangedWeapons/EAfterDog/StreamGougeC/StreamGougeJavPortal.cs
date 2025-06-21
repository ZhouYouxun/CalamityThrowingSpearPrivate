//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC
//{
//    public class StreamGougeJavPortal : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
//        public override void SetDefaults()
//        {
//            Projectile.width = 60;
//            Projectile.height = 60;
//            Projectile.friendly = true;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 60; // 存在时间较短
//            Projectile.tileCollide = false;
//            Projectile.light = 0.75f; // 发光
//            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
//            Projectile.localNPCHitCooldown = 15; // 无敌帧冷却时间为10帧
//            Projectile.scale = 1.75f;
//        }

//        public override void AI()
//        {
//            // 旋转效果
//            Projectile.rotation += 0.1f;

//            // 不断改变颜色
//            // 手动实现 PingPong
//            float PingPong(float value, float length)
//            {
//                return length - Math.Abs(value % (length * 2) - length);
//            }

//            // 使用时
//            Color baseColor = Color.Lerp(Color.Cyan, Color.Magenta, PingPong(Main.GameUpdateCount * 0.1f, 1f));
//            Lighting.AddLight(Projectile.Center, baseColor.ToVector3() * 0.75f);

//            // 渐渐消失
//            Projectile.alpha += 4;
//            if (Projectile.alpha >= 255)
//                Projectile.Kill();
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {
//            // 使用传送门的贴图
//            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/StreamGougePortal").Value;
//            Vector2 origin = texture.Size() * 0.5f;
//            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor * ((255 - Projectile.alpha) / 255f), Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
//            return false;
//        }
//    }
//}