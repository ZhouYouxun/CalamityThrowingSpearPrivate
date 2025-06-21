using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsTyphoon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        private Vector2 orbitCenter; // 旋涡的中心点
        private float orbitRadius = 80f; // 旋涡初始半径
        private float orbitSpeed = 0.05f; // 旋转角速度 (小角度旋转更平滑)
        private float zDepth = 0f; // 模拟的Z轴深度 (影响Scale)
        private float zSpeed = 1f; // Z轴运动速度
        private float maxZDepth = 40f; // 最大Z轴深度 (决定Scale范围)

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        //public override void AI()
        //{
        //    if (Projectile.ai[0] == 0)
        //    {
        //        //orbitCenter = Projectile.Center; // 记录初始中心点，移除，因为它会让每个弹幕都一样
        //        Projectile.ai[0] = 1;
        //    }

        //    // **增加 Z 轴的模拟效果** (让其在 Z 轴上来回运动，形成视觉上的前后远近感)
        //    zDepth += zSpeed;
        //    if (zDepth >= maxZDepth || zDepth <= -maxZDepth)
        //    {
        //        zSpeed *= -1; // 让Z轴运动来回摆动，模拟3D透视感
        //    }

        //    // **计算绕中心的旋转 (XY 平面上的运动)**
        //    float angle = Projectile.ai[1] * orbitSpeed; // 计算当前旋转角度
        //    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
        //    Projectile.Center = orbitCenter + offset; // 更新弹幕位置

        //    // **投影变换: 根据Z轴深度调整 Scale 和 Rotation**
        //    float scaleFactor = 1f + (zDepth / maxZDepth) * 0.5f; // 根据Z轴深度调整大小
        //    Projectile.scale = MathHelper.Clamp(scaleFactor, 0.5f, 1.5f); // 限制Scale范围，防止过大或过小

        //    // **调整旋转角度**
        //    Projectile.rotation = angle + (zDepth / maxZDepth) * 0.5f; // 让旋涡在旋转过程中产生倾斜感

        //    // **动态调整旋转半径 (可选，让旋涡轨迹更自然)**
        //    if (Projectile.ai[1] % 2 == 0)
        //    {
        //        orbitRadius += 1f;
        //    }

        //    CreateWaterDust(); // 生成水雾特效
        //    Projectile.ai[1]++;
        //}


        public override void AI()
        {
            // **1. 确保 orbitCenter 只在生成时决定，不再强制设定**
            if (Projectile.ai[0] == 0)
            {
                orbitCenter = Projectile.Center; // 生成时的位置作为旋转中心
                Projectile.ai[0] = 1;

                // 🚀 **随机初始化初始角度，让弹幕分布在圆上的随机点**
                Projectile.ai[2] = Main.rand.NextFloat(0, MathHelper.TwoPi); // 随机初始角度
            }

            // **2. 控制旋转半径的缓慢增长**
            if (Projectile.ai[1] % 2 == 0) // 每 2 帧增加 1
            {
                orbitRadius += 1f;
            }

            // **3. 控制旋转角速度，使其一开始缓慢，随后加速**
            //float currentOrbitSpeed = orbitSpeed + Projectile.ai[1] * 0.0005f; // 加速，加个屁
            float currentOrbitSpeed = orbitSpeed; // 角速度逐渐加速
            float angle = Projectile.ai[2] + (Projectile.ai[1] * currentOrbitSpeed); // 从随机点开始

            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
            Projectile.Center = orbitCenter + offset;

            // **4. 保持 3D 旋转：Z 轴前后摆动**
            zDepth += zSpeed;
            if (zDepth >= maxZDepth || zDepth <= -maxZDepth)
            {
                zSpeed *= -1; // 让Z轴运动来回摆动
            }

            // **5. 投影变换：根据 Z 轴深度调整 Scale 和 Rotation**
            float scaleFactor = 1f + (zDepth / maxZDepth) * 0.5f;
            Projectile.scale = MathHelper.Clamp(scaleFactor, 0.75f, 1.45f);
            Projectile.rotation = (orbitCenter - Projectile.Center).ToRotation() + (zDepth / maxZDepth) * 0.2f; // 保持旋涡视觉动态

            // **6. 生成水雾粒子特效**
            CreateWaterDust();

            // **7. 更新旋转角度**
            Projectile.ai[1]++;
        }

        private void CreateWaterDust()
        {
            Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 3 * Main.rand.Next(3)) * 10f;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.CadetBlue, 1.5f);
            dust.noGravity = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
