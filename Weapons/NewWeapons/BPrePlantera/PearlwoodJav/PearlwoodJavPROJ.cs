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
using CalamityMod.Projectiles.Ranged;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PearlwoodJav/PearlwoodJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private float rotationAngle = 0f; // 自转角度
        private float rotationSpeed = 0.25f; // 自转速度

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        //internal float WidthFunction(float completionRatio) => (1f - completionRatio) * Projectile.scale * 9f;

        //internal Color ColorFunction(float completionRatio)
        //{
        //    float hue = 0.5f + 0.5f * completionRatio * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
        //    Color trailColor = Main.hslToRgb(hue, 1f, 0.8f);
        //    return trailColor * Projectile.Opacity;
        //}

        //public override void PostDraw(Color lightColor)
        //{
        //    // 使用与 MeowCreature 相同的拖尾渲染逻辑
        //    PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f), 30);

        //    // 绘制弹幕本体的发光效果
        //    Texture2D glow = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        //    Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        //}

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            //Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 自转特效：生成7种颜色的粒子围绕自身旋转
            rotationAngle += rotationSpeed;
            if (rotationAngle > MathHelper.TwoPi)
            {
                rotationAngle -= MathHelper.TwoPi;
            }

            // 定义7种颜色的粒子
            int[] torchDusts = new int[]
            {
                DustID.RedTorch, DustID.OrangeTorch, DustID.YellowTorch, DustID.GreenTorch, DustID.ShimmerTorch, DustID.BlueTorch, DustID.PurpleTorch
            };

            // 粒子圆圈半径
            float radius = 4 * 16f; // 1格=16像素

            // 创建7种颜色的粒子特效
            for (int i = 0; i < torchDusts.Length; i++)
            {
                float angle = rotationAngle + MathHelper.TwoPi * i / torchDusts.Length;
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                int dust = Dust.NewDust(position, 0, 0, torchDusts[i], 0f, 0f, 100, default, 1.9f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.Zero;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 清除现有的 RainbowFront 弹幕
            foreach (Projectile p in Main.projectile)
            {
                if ((p.type == ModContent.ProjectileType<PearlwoodJavRainbowFront>() || p.type == ModContent.ProjectileType<PearlwoodJavRainbowTrail>()) && p.owner == Projectile.owner)
                {
                    p.Kill();
                }
            }

            // 释放三个 RainbowFront 弹幕，分别向前方左右偏移8度 彩虹的倍率为0.25倍
            for (int i = -8; i <= 8; i += 8)
            {
                Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.ToRadians(i));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<PearlwoodJavRainbowFront>(), (int)((Projectile.damage) * 0.5), 0f, Projectile.owner);
            }

            SoundEngine.PlaySound(SoundID.Item67, Projectile.Center); // 播放彩虹枪的音效





            // 特效部分---------------------------------
            // 定义7种颜色的火炬粒子
            int[] torchDusts = new int[]
            {
    DustID.RedTorch, DustID.OrangeTorch, DustID.YellowTorch, DustID.GreenTorch, DustID.ShimmerTorch, DustID.BlueTorch, DustID.PurpleTorch
            };

            // 每两个颜色之间的夹角
            float angleIncrement = 360f / torchDusts.Length;

            // 释放彩虹色粒子链
            for (int i = 0; i < torchDusts.Length; i++)
            {
                int dustType = torchDusts[i];
                float baseAngle = MathHelper.ToRadians(i * angleIncrement); // 每种颜色的起始角度

                // 生成粒子链
                for (float speedMultiplier = 2f; speedMultiplier <= 6f; speedMultiplier += 1f)
                {
                    Vector2 velocity = Projectile.velocity.RotatedBy(baseAngle) * speedMultiplier;
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, dustType, velocity.X, velocity.Y, 100, default, 1.8f);
                    Main.dust[dust].noGravity = true;
                }
            }


        }

    }
}
