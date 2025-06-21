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
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterDrumINV : ModProjectile, ILocalizedModType
    {
        public override string Texture => $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string texturePath = $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;

            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
        private int drumForm = 0; // 0-4 对应五种形态

        // 设定模式的方法
        public void SetDrumForm(int form)
        {
            drumForm = form;
            Projectile.netUpdate = true; // 同步数据
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
        }
        public bool exploding
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value == true ? 1f : 0f;
        }
        //1.笞:Flogging-敌怪获得1.02倍的易伤
        //2.杖:Beating-敌怪的接触伤害减少30%
        //3.徒:Imprisoning-敌怪的移动速度减少50%
        //4.流:Banishing-敌怪防御降低40
        //5.死:Executing-2秒的倒计时结束后造成5000点伤害
        public override void AI()
        {
            Projectile.velocity *= 0.988f; // 每帧速度减少
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // 调整旋转
            Lighting.AddLight(Projectile.Center, Color.LightCoral.ToVector3() * 0.7f); // 添加光照

            Projectile.alpha += 5;

            if (Projectile.timeLeft <= 65)
                exploding = true;

            if (exploding)
            {
                Projectile.velocity = Vector2.Zero;

                if (Projectile.timeLeft > 65)
                    Projectile.timeLeft = 65;

                if (Projectile.timeLeft == 65)
                {
                    // 爆炸光环
                    Particle blastRing = new CustomPulse(
                        Projectile.Center,
                        Vector2.Zero,
                        Color.SaddleBrown,
                        "CalamityMod/Particles/HighResHollowCircleHardEdge",
                        Vector2.One,
                        Main.rand.NextFloat(-10, 10),
                        0.12f,
                        0f,
                        25
                    );
                    GeneralParticleHandler.SpawnParticle(blastRing);
                    SoundStyle fire = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeImpact");
                    SoundEngine.PlaySound(fire with { Volume = 1.25f, Pitch = -0.2f, PitchVariance = 0.15f }, Projectile.Center);
                }
            }

            // 释放粒子
            if (Projectile.timeLeft % 2 == 0)
            {
                Vector2 dustVel = new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f);
                int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel, dustType, dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.SaddleBrown, Color.White, Main.rand.NextFloat());
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void OnKill(int timeLeft)
        {
            // 设置伤害倍率（可以调整数值以匹配实际需求）
            float boomDamageMultiplier = 1.0f; // 可以根据需求调整
            int boomDamage = (int)(Projectile.damage * boomDamageMultiplier);

            // 创建爆炸弹幕
            Projectile explosion = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<SurfeiterDrumINVEXP>(),
                boomDamage,
                Projectile.knockBack,
                Projectile.owner
            );

            // 设置爆炸弹幕的额外属性
            float maxRadius = Main.rand.NextFloat(110f, 200f); // 随机最大半径
            explosion.ai[1] = maxRadius;
            explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // 插值步长
            explosion.netUpdate = true;

            // 传递模式
            if (explosion.ModProjectile is SurfeiterDrumINVEXP drumEXP)
            {
                drumEXP.SetDrumForm(drumForm);
            }

            //// 传递模式
            //if (Main.projectile[projID].ModProjectile is SurfeiterDrumINVEXP drumEXP)
            //{
            //    drumEXP.SetDrumForm(drumForm);
            //}
        }

    }
}