using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class SurfeiterDrumINVBack : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override string Texture => "Terraria/Images/Extra_89"; // 使用原版光点

        private int drumForm = 0;
        private int trackTimer = 0;
        private bool sticking = false;

        public void SetDrumForm(int form)
        {
            drumForm = form;
            Projectile.netUpdate = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity *= 1.01f;

            if (trackTimer < 15)
            {
                Projectile.rotation += MathHelper.ToRadians(1f); // 每帧右拐 1 度
                trackTimer++;
            }
            else if (!sticking)
            {

                Player player = Main.player[Projectile.owner];
                Projectile targetDrum = null;
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<SurfeiterDrum>())
                    {
                        targetDrum = proj;
                        break;
                    }
                }

                if (targetDrum != null)
                {
                    Projectile.velocity *= 1.01f;


                    Vector2 direction = targetDrum.Center - Projectile.Center;

                    // 先计算当前目标速度长度（允许增长）
                    float desiredSpeed = Projectile.velocity.Length() + 1.05f; // 每帧线性增长（可调整增长速度）

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction.SafeNormalize(Vector2.UnitY) * desiredSpeed, 0.08f);



                    if (Projectile.Hitbox.Intersects(targetDrum.Hitbox))
                    {
                        sticking = true;
                        Projectile.timeLeft = 10;
                        Projectile.velocity *= 0f;

                        // 释放粒子
                        for (int i = 0; i < 20; i++)
                        {
                            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Main.rand.NextVector2Circular(3f, 3f));
                            d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                            d.color = GetColorByForm();
                        }
                    }
                }
            }

            if (sticking)
            {
                //Projectile.velocity *= 0.97f;
            }

            // 持续释放重型烟雾和线性粒子
            if (Main.rand.NextBool(3))
            {
                Particle smokeH = new HeavySmokeParticle(
                    Projectile.Center,
                    new Vector2(0, -1f) * Main.rand.NextFloat(1f, 3f),
                    GetColorByForm(),
                    40,
                    Projectile.scale * Main.rand.NextFloat(0.8f, 1.3f),
                    0.8f,
                    Main.rand.NextFloat(-0.02f, 0.02f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smokeH);
            }

            // 飞行期间额外释放灵魂仪式 Dust 粒子特效
            if (Main.rand.NextBool(2)) // 每 2 帧平均释放一次
            {
                int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f); // 略偏离中心，制造漂移感
                Vector2 dustPos = Projectile.Center + offset;
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));

                Dust d = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0, GetColorByForm(), Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Vector2 origin = texture.Size() * 0.5f;

            // === Extra_89 多层脉动圆环绘制 ===
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[89].Value;
            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 0.6f; // 旋转
                Color ringColor = new Color(180, 50, 50, 0) * 1.6f; // 深红色带透明
                Vector2 offset = Vector2.Zero;

                float scale = (0.25f + 0.05f * i) * pulse * 3f; // 大小翻三倍

                Main.EntitySpriteDraw(
                    ringTex,
                    drawPosition + offset,
                    null,
                    ringColor,
                    angle,
                    ringTex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // === Shader 拖尾绘制 ===
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["ModNamespace:TrailBlazingFlameEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                .UseColor(new Color(200, 50, 50)) // 仪式深红主色
                .UseSecondaryColor(new Color(80, 10, 10)) // 暗红副色
                .Apply();

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(TrailWidth, (completionRatio, vertexPos) => TrailColor(completionRatio, vertexPos), (completionRatio, vertexPos) => Projectile.Size * 0.5f, shader: GameShaders.Misc["ModNamespace:TrailBlazingFlameEffect"]),
                10
            );

            Main.spriteBatch.ExitShaderRegion();

        
            return false;
        }



        public override void OnKill(int timeLeft)
        {
            Color color = GetColorByForm();

            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Main.rand.NextVector2Circular(4f, 4f));
                d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                d.color = color;
            }

            for (int i = 0; i < 8; i++)
            {
                Particle p = new SparkParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    false,
                    40,
                    1.2f,
                    color
                );
                GeneralParticleHandler.SpawnParticle(p);
            }

            Particle ring = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                color,
                "CalamityMod/Particles/HighResHollowCircleHardEdge",
                Vector2.One,
                Main.rand.NextFloat(-5f, 5f),
                0.05f,
                0.2f,
                20
            );
            GeneralParticleHandler.SpawnParticle(ring);
        }

        public Color GetColorByForm()
        {
            return drumForm switch
            {
                0 => new Color(180, 40, 40),   // 笞：深血红
                1 => new Color(200, 100, 20),  // 杖：赭石橙
                2 => new Color(80, 130, 180),  // 徒：阴蓝
                3 => new Color(140, 100, 180), // 流：紫灰
                4 => new Color(20, 20, 20),    // 死：深黑灰
				_ => Color.Gray,
			};
        }

        public Color TrailColor(float completionRatio, Vector2 vertexPos)
        {
            float opacity = Utils.GetLerpValue(1f, 0.5f, completionRatio, true) * Projectile.Opacity;
            return GetColorByForm() * opacity;
        }

        public float TrailWidth(float completionRatio, Vector2 vertexPos)
        {
            return MathHelper.SmoothStep(12f, 25f, Utils.GetLerpValue(0f, 1f, completionRatio, true));
        }
    }
}
