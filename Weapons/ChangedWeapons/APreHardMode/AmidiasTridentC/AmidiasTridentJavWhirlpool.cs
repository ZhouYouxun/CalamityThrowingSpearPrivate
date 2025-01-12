using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC
{
    public class AmidiasTridentJavWhirlpool : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 58;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 100;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 270;

        }

        public override void AI()
        {
            // 如果是第一次执行，记录生成时的初始位置和角度，确保每个弹幕有自己的初始角度
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;  // 标记为已经初始化
                Projectile.ai[0] = Projectile.Center.X;  // 保存初始X坐标
                Projectile.ai[1] = Projectile.Center.Y;  // 保存初始Y坐标

                // 为每个弹幕分配一个独立的角度偏移
                Projectile.ai[2] = Main.rand.NextFloat(0f, MathHelper.TwoPi);  // 随机生成初始角度偏移
            }

            // 获取生成点作为旋转的圆心
            Vector2 initialCenter = new Vector2(Projectile.ai[0], Projectile.ai[1]);

            // 旋转上升轨迹，围绕生成点螺旋，包含初始角度偏移
            if (Projectile.timeLeft > 200)
            {
                Projectile.localAI[1] += 1f / 60f;  // 增加时间因子
                float radius = 100f;  // 定义旋转的半径

                // 每帧递增角度，计算新的旋转位置，包含初始角度偏移
                float angle = Projectile.localAI[1] * MathHelper.TwoPi + Projectile.ai[2];  // 加入初始角度偏移
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                // 更新弹幕位置，使其沿着圆心螺旋上升
                Projectile.Center = initialCenter + offset;

                // 让弹幕逐渐上升
                Projectile.velocity = new Vector2(0, -1);  // 只上升，不水平移动
            }
            else
            {
                Projectile.velocity *= 1.05f;

                // 开始追踪最近的敌人，螺旋上升后才执行
                NPC target = null;
                float maxDistance = 800f;  // 半径50个方块（800像素）

                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy() && !npc.friendly)
                    {
                        float distanceToNPC = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distanceToNPC < maxDistance)
                        {
                            maxDistance = distanceToNPC;
                            target = npc;
                        }
                    }
                }

                if (target != null)
                {
                    // 在追踪敌人时生成海蓝色小三角形特效
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 3 * i) * 10f;
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.CadetBlue, 1.5f);
                        dust.noGravity = true;
                    }

                    // 追踪敌人
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = direction * 5f;  // 追踪速度
                }
                else
                {
                    // 如果没有找到任何敌人，销毁弹幕
                    Projectile.Kill();
                }


            }

            // 释放海蓝色粒子特效
            if (Main.rand.NextBool(5))
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f)];
                dust.noGravity = true;
                dust.scale = 1.2f;
            }
        }


        // 阻止前30帧内对敌人造成伤害
        //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        //{
        //    if (Projectile.timeLeft > 90)  // 前30帧内不造成伤害
        //    {
        //        modifiers.SetMaxDamage(0);  // 设置伤害为0
        //    }
        //}



        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(30, 255, 253);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int k = 0; k < 20; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Water, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 0, new Color(0, 142, 255), 1f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 300); // 激流
        }
    }
}
