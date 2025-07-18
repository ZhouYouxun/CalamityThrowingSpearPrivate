using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using CalamityMod;
using Terraria.Audio;
using Terraria.Graphics.Renderers;
using CalamityMod.Particles;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;



namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{

    public class ScourgeoftheCosmosJavApple : ModProjectile
    {


        //public override string Texture => "Terraria/Images/Item_4009"; // 使用原版的苹果贴图
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 120;
            Projectile.friendly = false; // 不造成伤害
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // X 秒后自动消失
            Projectile.MaxUpdates = 1;
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.ignoreWater = true;
            //Projectile.aiStyle = ProjAIStyleID.Arrow;
        }

        public override void AI()
        {
            // ✦ 使用 localAI[0] 作为“时间”计数器
            Projectile.localAI[0]++;

            // ✦ 计算一个随时间增长的加速度因子（最多到1.0）
            float factor = Utils.GetLerpValue(0f, 90f, Projectile.localAI[0], clamped: true); // 90帧达到最大

            // ✦ 逐渐增加旋转速度（从0到5度）
            Projectile.rotation += MathHelper.ToRadians(5f * factor);

            // ✦ 逐渐增加下落速度（从0到2.5）
            Projectile.velocity.Y = 2.5f * factor;


            // 添加轻微的左右摆动
            Projectile.velocity.X = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.2f;

            // 增加苹果的光效
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 0.4f);


            {
                // ✦ 每帧生成柔性烟尘
                if (Main.rand.NextBool(2))
                {
                    Dust glow = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.BlueCrystalShard,
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        100,
                        new Color(255, 110, 180, 200),
                        Main.rand.NextFloat(1.2f, 1.7f)
                    );
                    glow.noGravity = true;
                }

                // ✦ 随机轻盈旋转粒子
                if (Main.rand.NextBool(4))
                {
                    Dust swirl = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.PinkTorch,
                        Vector2.Zero,
                        150,
                        new Color(240, 240, 255),
                        Main.rand.NextFloat(0.8f, 1.4f)
                    );
                    swirl.velocity = swirl.position.DirectionTo(Projectile.Center).RotatedBy(MathHelper.PiOver2) * 1.2f;
                    swirl.noGravity = true;
                }

            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {
            Vector2 pos = Projectile.Center;

            // ✦ 播放吸收音效
            SoundEngine.PlaySound(SoundID.Item29, pos);

            // ✦ 核心烟雾向内缩吸
            for (int i = 0; i < 24; i++)
            {
                Vector2 dir = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY);
                Dust core = Dust.NewDustPerfect(
                    pos + dir * 12f,
                    DustID.Shadowflame,
                    -dir * Main.rand.NextFloat(3f, 6f),
                    150,
                    Color.DeepPink,
                    Main.rand.NextFloat(1.8f, 2.6f)
                );
                core.noGravity = true;
            }

            // ✦ 四向爆炸尘：橘红放射（能量火花）
            for (int i = 0; i < 36; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust blast = Dust.NewDustPerfect(
                    pos,
                    DustID.Torch,
                    dir * Main.rand.NextFloat(3.5f, 7f),
                    120,
                    Color.OrangeRed,
                    Main.rand.NextFloat(1.6f, 2.6f)
                );
                blast.noGravity = true;
            }

            // ✦ 辅助爆星尘：蓝+黄点缀
            for (int i = 0; i < 12; i++)
            {
                Vector2 dir = Main.rand.NextVector2Circular(1f, 1f);
                Dust deco = Dust.NewDustPerfect(
                    pos + dir * 20f,
                    DustID.GoldCoin,
                    -dir * 2f,
                    100,
                    Color.Yellow,
                    Main.rand.NextFloat(1f, 1.5f)
                );
                deco.noGravity = true;
            }

            // ✦ 灵魂星光：调用你自定义强化版
            CTSLightingBoltsSystem.Apple_OnKill(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f));

            // ✦ 中心详细爆炸圆环（慢速扩张）
            Particle explosion = new DetailedExplosion(
                pos,
                Vector2.Zero,
                Color.OrangeRed * 0.9f,
                Vector2.One,                      // 圆形
                Main.rand.NextFloat(-5f, 5f),     // 随机旋转
                0f,                               // 初始缩放
                0.21f,                            // 最终缩放（比你原先的 0.28f 慢一点）
                14                                // 持续时间略长
            );
            GeneralParticleHandler.SpawnParticle(explosion);
        }








    }
}
