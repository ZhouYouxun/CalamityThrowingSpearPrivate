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

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouch10PBuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "ModBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // === 在玩家周围以半径 ~48 的环上分布特效中心 ===
            float offsetAngle = Main.GameUpdateCount * 0.04f + player.whoAmI; // 保证不同玩家不同
            Vector2 ringOffset = offsetAngle.ToRotationVector2() * 48f;
            Vector2 center = player.Center + ringOffset;

            if (Main.GameUpdateCount % 2 == 0) // 控制密度
            {
                int petals = 48;
                float goldenAngle = MathHelper.ToRadians(137.5f);

                for (int i = 0; i < petals; i++)
                {
                    // 玫瑰曲线半径
                    float theta = MathHelper.TwoPi * i / petals;
                    float roseRadius = 14f * (1 + 0.3f * (float)Math.Sin(6 * theta));

                    // 螺旋半径
                    float spiralT = Main.GameUpdateCount * 0.15f;
                    float spiralRadius = 3f + 0.15f * spiralT;

                    // 黄金角偏移喷射方向
                    float angle = i * goldenAngle + Main.GameUpdateCount * 0.08f;
                    Vector2 direction = angle.ToRotationVector2();

                    Vector2 velocity = direction * roseRadius * 0.25f + direction.RotatedBy(MathHelper.PiOver4) * spiralRadius * 0.15f;

                    int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.RedTorch });
                    Color dustColor = Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f));

                    Dust d = Dust.NewDustPerfect(center, dustType, velocity, 100, dustColor, Main.rand.NextFloat(1.2f, 1.8f));
                    d.noGravity = true;
                }
            }

            if (Main.GameUpdateCount % 3 == 0) // 高速火花放射
            {
                int sparks = 8;
                float baseAngle = Main.GameUpdateCount * 0.05f;

                for (int i = 0; i < sparks; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / sparks;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);

                    Particle spark = new SparkParticle(
                        center,
                        velocity,
                        false,
                        Main.rand.Next(30, 45),
                        Main.rand.NextFloat(1.0f, 1.5f),
                        Color.LightYellow * 0.8f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }






    }
}