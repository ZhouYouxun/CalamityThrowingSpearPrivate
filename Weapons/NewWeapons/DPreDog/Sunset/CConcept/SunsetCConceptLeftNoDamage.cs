using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using Terraria.DataStructures;
using ReLogic.Content;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftNoDamage : ModProjectile
    {
        public int TargetNPCIndex = -1; // 目标敌人索引
        public Color ProjectileColor; // 由 Listener 传递的颜色
        private float RotationAngle; // 旋转角度
        private const float OrbitRadius = 160f; // 旋转半径
        private const float OrbitSpeed = 0.02f; // 旋转速度

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180; // 不会轻易消失
        }
        public int NoDamageIndex; // **这个弹幕的编号**
        private float Time;

        public float HoverOffsetAngle
        {
            get
            {
                int totalCount = 10; // 固定 10 个弹幕
                return MathHelper.TwoPi * NoDamageIndex / totalCount + Time / 30f; // **让弹幕逐渐公转**
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            NoDamageIndex = (int)Projectile.ai[1]; // **获取编号**
        }
    
        public override void AI()
        {
            // **确保 `Magic` 仍然存在**
            if (Projectile.ai[0] < 0 || !Main.projectile[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }

            // **读取颜色**
            Color[] colors = {
                Color.Black, Color.White, Color.Green, new Color(255, 105, 180), // 蓝粉
                Color.Blue, Color.Gold, new Color(50, 0, 50), // 紫黑
                Color.Red, Color.Gray, Color.Silver
            };

            if (Projectile.ai[1] >= 0 && Projectile.ai[1] < colors.Length)
            {
                ProjectileColor = colors[(int)Projectile.ai[1]];
            }

            Projectile parentMagic = Main.projectile[(int)Projectile.ai[0]];

            // **计算围绕 `Magic` 的旋转位置**
            float radius = 160f;
            Vector2 offset = HoverOffsetAngle.ToRotationVector2() * radius;
            Projectile.Center = parentMagic.Center + offset;

            // **更新旋转方向**
            Projectile.rotation = Projectile.AngleFrom(parentMagic.Center) + MathHelper.PiOver4 + MathHelper.Pi;

            // **确保不会消失**
            Projectile.timeLeft = 600;

            Time++; // 删掉这句话就会导致不旋转了
            Projectile.netUpdate = true; // 确保数据同步
        }


        //public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage0";


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture;

            // **根据 NoDamageIndex 选择对应的贴图**
            if (NoDamageIndex % 10 == 0)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage").Value;
            else if (NoDamageIndex % 10 == 1)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage1").Value;
            else if (NoDamageIndex % 10 == 2)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage2").Value;
            else if (NoDamageIndex % 10 == 3)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage3").Value;
            else if (NoDamageIndex % 10 == 4)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage4").Value;
            else if (NoDamageIndex % 10 == 5)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage5").Value;
            else if (NoDamageIndex % 10 == 6)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage6").Value;
            else if (NoDamageIndex % 10 == 7)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage7").Value;
            else if (NoDamageIndex % 10 == 8)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage8").Value;
            else
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftNoDamage9").Value;

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // **充能光晕效果**
            float chargeOffset = 3f;
            Color chargeColor = ProjectileColor * 0.8f; // 使用传递的颜色
            chargeColor.A = 0;

            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;

            // **绘制充能光晕**
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // **绘制投射物本体**
            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }
    }
}
