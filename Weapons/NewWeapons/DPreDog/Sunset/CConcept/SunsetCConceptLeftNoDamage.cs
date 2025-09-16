using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.Audio;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftNoDamage : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180; // 不会轻易消失
            Projectile.alpha = 255; // 🚩 初始完全透明
        }


        public int NoDamageIndex;   // 这个刀片的编号
        private float Time;

        private bool catchingMagic = false;   // 是否正在捕捉魔法阵
        private float catchProgress = 0f;     // 捕捉进度

        // ======= 可调参数（直接改数值即可） =======
        public const float AngleStepDegrees = 51f; // 每个刀片之间的夹角（改成 5f/60f 都行）
        private static readonly float AngleStepRadians = MathHelper.ToRadians(AngleStepDegrees);


        private const float OrbitRadius = 290f;    // 魔法阵公转半径

        public Color ProjectileColor; // 颜色由 ai[1] 决定

        public float HoverOffsetAngle
        {
            get
            {
                // 用固定角度间隔来计算，不再写死等分
                return NoDamageIndex * AngleStepRadians + Time / 30f;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            NoDamageIndex = (int)Projectile.ai[1]; // 初始化编号
            SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.2f, Pitch = -0.0f }, Projectile.Center);

            // ========== 参数 ==========
            bool isBig = Projectile.scale > 1.5f; // 🚩 判断是否特大号（可换成 ai[2] 等显式标志）
            int dustCount = isBig ? 48 : 24;
            float dustScale = isBig ? 1.5f : 1f;
            float baseRadius = isBig ? 36f : 24f;

            // ========== 1) 往后喷射（尾焰） ==========
            for (int i = 0; i < (isBig ? 12 : 6); i++)
            {
                Vector2 backDir = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.25f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Electric,
                    backDir * Main.rand.NextFloat(2f, 5f),
                    150,
                    Color.Cyan,
                    1.2f * dustScale
                );
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // ========== 2) 黄金角向外扩散（有序印刻） ==========
            float golden = MathHelper.ToRadians(137.5f);
            for (int n = 0; n < dustCount; n++)
            {
                float angle = n * golden;
                float rad = baseRadius * MathF.Sqrt((n + 1f) / dustCount);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * rad;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    (n % 2 == 0) ? DustID.BlueTorch : DustID.GemDiamond,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f),
                    160,
                    Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.5f),
                    0.9f * dustScale
                );
                d.noGravity = true;
            }

            // ========== 3) 粒子点缀（科技感） ==========
            for (int i = 0; i < (isBig ? 4 : 2); i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    12,
                    0.8f * dustScale,
                    Color.LightCyan,
                    true, false, true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }

        public override void AI()
        {
            // 颜色表（10 种颜色）
            Color[] colors = {
                Color.Black, Color.White, Color.Green, new Color(255, 105, 180),
                Color.Blue, Color.Gold, new Color(50, 0, 50),
                Color.Red, Color.Gray, Color.Silver
            };



            // 🚩 每帧降低 alpha，直到 0 为止
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 8; // 8 × 30 ≈ 240 -> 大约30帧完全淡入
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }




            if (Projectile.ai[1] >= 0 && Projectile.ai[1] < colors.Length)
                ProjectileColor = colors[(int)Projectile.ai[1]];

            // === 模式 A：玩家环绕 ===
            if (Projectile.ai[0] == -1)
            {
                Player owner = Main.player[Projectile.owner];

                // 查找魔法阵
                int magicIndex = -1;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active &&
                        Main.projectile[i].type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() &&
                        Main.projectile[i].owner == Projectile.owner)
                    {
                        magicIndex = i;
                        break;
                    }
                }

                // 进入捕捉状态
                if (magicIndex != -1 && !catchingMagic)
                {
                    catchingMagic = true;
                    Projectile.ai[0] = magicIndex;
                    catchProgress = 0f;
                }

                if (!catchingMagic)
                {
                    float angle = HoverOffsetAngle;
                    Vector2 offset = angle.ToRotationVector2() * 150f; // 这里的数字代表围绕玩家的时候半径
                    Projectile.Center = owner.Center + offset;
                    Projectile.rotation = Projectile.AngleFrom(owner.Center) + MathHelper.PiOver4;
                    Projectile.timeLeft = 60;
                    Time++;
                    return;
                }
            }

            // === 模式 B：魔法阵捕捉/公转 ===
            if (Projectile.ai[0] < 0 || !Main.projectile[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile parentMagic = Main.projectile[(int)Projectile.ai[0]];

            float targetAngle = NoDamageIndex * AngleStepRadians + Time / 30f;
            Vector2 targetPos = parentMagic.Center + targetAngle.ToRotationVector2() * OrbitRadius;

            if (catchingMagic && catchProgress < 1f)
            {
                catchProgress += 0.02f;

                Vector2 currentDir = Projectile.Center - parentMagic.Center;
                Vector2 targetDir = targetPos - parentMagic.Center;

                float currentAngle = currentDir.ToRotation();
                float desiredAngle = targetDir.ToRotation();
                float angleDiff = MathHelper.WrapAngle(desiredAngle - currentAngle);

                float maxStep = MathHelper.ToRadians(2f + 10f * catchProgress);
                float newAngle = currentAngle + MathHelper.Clamp(angleDiff, -maxStep, maxStep);

                float newRadius = MathHelper.Lerp(currentDir.Length(), OrbitRadius, catchProgress * 0.1f);
                Projectile.Center = parentMagic.Center + newAngle.ToRotationVector2() * newRadius;
            }
            else
            {
                Projectile.Center = targetPos;
            }

            Projectile.rotation = Projectile.AngleFrom(parentMagic.Center) + MathHelper.PiOver4 + MathHelper.Pi;
            Projectile.timeLeft = 600;
            Time++;
            Projectile.netUpdate = true;




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
