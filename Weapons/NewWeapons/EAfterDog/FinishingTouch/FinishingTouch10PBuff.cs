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
        private List<SparkParticle> ownedSparkParticles = new();
        private List<CritSpark> ownedCritSparks = new();

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FinishingTouch10Player>().finishingTouchOrangeTrailActive = true;

            player.buffTime[buffIndex] = 2;
            player.GetDamage(DamageClass.Melee) *= 1.2f;
            player.statDefense += 50;
            player.endurance += 1.00f;


            {
                Vector2 center = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 velocity = (player.Center - center).SafeNormalize(Vector2.Zero) * 1.2f;

                // === 1️⃣ SparkParticle（核心轨迹橙色粒子） ===
                Particle spark = new SparkParticle(
                    center,
                    velocity,
                    false,
                    60,
                    1.0f,
                    Color.Orange
                );
                GeneralParticleHandler.SpawnParticle(spark);
                ownedSparkParticles.Add((SparkParticle)spark); // 记录引用用于后续修改

                for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
                {
                    SparkParticle p = ownedSparkParticles[i];

                    if (p.Time >= p.Lifetime)
                    {
                        ownedSparkParticles.RemoveAt(i);
                        continue;
                    }

                    Vector2 targetDirection = (player.Center - p.Position).SafeNormalize(Vector2.Zero);
                    float speed = p.Velocity.Length();
                    p.Velocity = Vector2.Lerp(p.Velocity, targetDirection * speed, 0.08f); // 平滑追踪，越小越柔和
                }


                // === 2️⃣ CritSpark（细节闪烁橙光） ===
                int interval = 2; // 每?帧炸一次
                if (Main.GameUpdateCount % interval == 0)
                {
                    int sparkCount = 12; // 每圈生成12个
                    float speed = 3f;
                    for (int k = 0; k < sparkCount; k++)
                    {
                        float angle = MathHelper.TwoPi * k / sparkCount;
                        Vector2 critVel = angle.ToRotationVector2() * speed;

                        CritSpark critSpark = new CritSpark(
                            center,
                            critVel,
                            Color.Orange,
                            Color.Yellow,
                            0.8f,
                            30 // 寿命可适当增加
                        );
                        GeneralParticleHandler.SpawnParticle(critSpark);
                        ownedCritSparks.Add(critSpark); // 若要控制轨迹，记录引用
                    }
                }

                for (int i = ownedCritSparks.Count - 1; i >= 0; i--)
                {
                    CritSpark p = ownedCritSparks[i];

                    if (p.Time >= p.Lifetime)
                    {
                        ownedCritSparks.RemoveAt(i);
                        continue;
                    }

                    // 加速
                    p.Velocity *= 1.05f;

                    // 左拐 2°
                    p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(-2f));
                }


                // === 3️⃣ 轻型烟雾（橙色淡烟） ===
                Particle smoke = new HeavySmokeParticle(
                    center,
                    velocity * 0.3f,
                    new Color(255, 120, 0, 100),
                    30,
                    Main.rand.NextFloat(0.6f, 1.0f),
                    0.3f,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smoke);

                // === 4️⃣ Dust（随机橙色火花） ===
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(
                        center,
                        DustID.Torch,
                        velocity * 0.2f,
                        100,
                        Color.Orange,
                        Main.rand.NextFloat(0.8f, 1.4f)
                    );
                    d.noGravity = true;
                }

                // === 5️⃣ 可选 Bloom 柔和光环（仅适用于需要额外光域感时启用） ===
                if (Main.rand.NextBool(5))
                {
                    Particle bloom = new GenericBloom(
                        center,
                        Vector2.Zero,
                        Color.Orange * 0.5f,
                        1.5f,
                        45
                    );
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
            }



        }






    }
}