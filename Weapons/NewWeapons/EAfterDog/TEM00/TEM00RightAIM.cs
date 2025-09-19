using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;
using Terraria.Utilities;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00RightAIM : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Texture/KsTexture/magic_03";
       
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // ========== 可调参数 ==========
            int totalLifetime = 15;        // 总耗时
            float startScale = 0.6f;       // 出生时的缩放
            float endScale = 0.0f;         // 消失时的缩放
            Color baseColor = Color.Cyan;  // 初始颜色
            Color edgeColor = Color.White; // 结束渐变颜色
            float rotationSpeed = 0.25f;   // 每帧旋转速度
                                           // =============================

            // 生命周期进度（0 = 出生, 1 = 死亡）
            float progress = 1f - (Projectile.timeLeft / (float)totalLifetime);
            float fade = 1f - progress;

            // 使用线性插值计算缩放
            float scale = MathHelper.Lerp(startScale, endScale, progress);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 加载纹理
            Texture2D texMagic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;
            Texture2D texMagic2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value;
            Texture2D texMagic3 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_03").Value;
            Texture2D texMagic4 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_04").Value;

            Texture2D texTwirl1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
            Texture2D texTwirl2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_02").Value;
            Texture2D texTwirl3 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_03").Value;

            Texture2D texHalo = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Halo2").Value;
            Texture2D texFlare = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Flare8").Value;

            // 颜色渐变
            Color col = Color.Lerp(baseColor, edgeColor, progress) * fade;

            Vector2 origin;

            // ====== 魔法系列绘制 ======
            Texture2D[] magics = { texMagic1, texMagic2, texMagic3, texMagic4 };
            for (int i = 0; i < magics.Length; i++)
            {
                Texture2D tex = magics[i];
                origin = tex.Size() * 0.5f;
                float rot = (Projectile.whoAmI % 2 == 0 ? 1f : -1f) * rotationSpeed * Main.GlobalTimeWrappedHourly * (i + 1);
                spriteBatch.Draw(tex, drawPos, null, col, rot, origin, scale * (1.0f - i * 0.1f), SpriteEffects.None, 0f);
            }

            // ====== Twirl 系列绘制（稍大） ======
            Texture2D[] twirls = { texTwirl1, texTwirl2, texTwirl3 };
            for (int i = 0; i < twirls.Length; i++)
            {
                Texture2D tex = twirls[i];
                origin = tex.Size() * 0.5f;
                float rot = -rotationSpeed * Main.GlobalTimeWrappedHourly * (i + 1);
                spriteBatch.Draw(tex, drawPos, null, col * 0.8f, rot, origin, scale * 1.15f, SpriteEffects.None, 0f);
            }

            // ====== Halo 绘制 ======
            origin = texHalo.Size() * 0.5f;
            spriteBatch.Draw(texHalo, drawPos, null, col * 0.7f, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // ====== Flare8 绘制（两次，互相垂直） ======
            origin = texFlare.Size() * 0.5f;
            spriteBatch.Draw(texFlare, drawPos, null, col * 0.9f, 0f, origin, scale * 1.1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texFlare, drawPos, null, col * 0.9f, MathHelper.PiOver2, origin, scale * 1.1f, SpriteEffects.None, 0f);

            return false;
        }





        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80; // 范围型爆炸判定大小
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 35; // 存活时间短
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 600; // 防止重复命中
            //Projectile.alpha = 255; // 完全透明
        }

        public override void AI()
        {
            // 此处通常留空，仅用于保证存活期间的判定
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中 NPC 时触发，可添加爆炸特效、附加 Buff 等
        }

        public override void OnKill(int timeLeft)
        {
            // 仅由拥有者实例化，避免多人下重复生成
            if (Main.myPlayer != Projectile.owner)
                return;

            Player owner = Main.player[Projectile.owner];
            Vector2 center = Projectile.Center;

            // ===== 可调基础参数（保持矩阵秩序）=====
            int beamCount = 8;                            // 激光条数（固定 8 条）
            float baseSpawnHeight = 600f;                 // 基准生成高度（在 center 上方）
            float baseSpreadWidth = 700f;                 // 基准横向总宽度
            float baseAngleSpreadDeg = 30f;               // 基准扇形角度总范围（±15°）

            // ===== 受控随机：每次死亡都稍有不同 =====
            // 用一个“轻量随机”做整体抖动（不需要 localAI / 字段）
            UnifiedRandom rng = new UnifiedRandom(
                (int)(Projectile.identity * 73856093 ^ (int)Main.GameUpdateCount)
            );

            // 整体参数抖动（±）
            float spawnHeight = baseSpawnHeight + rng.NextFloat(-90f, 90f);                // 生成高度  ±90
            float spreadWidth = baseSpreadWidth + rng.NextFloat(-140f, 140f);              // 横向宽度  ±140
            float baseAngleDeg = rng.NextFloat(-12f, 12f);                                  // 整体基准倾角 ±12°
            float angleSpreadDeg = baseAngleSpreadDeg + rng.NextFloat(-6f, 6f);             // 扇形范围  ±6°

            // 在几种“有秩序”的构图模式里随机取 1 种
            // 0: 平行扇形（基础款），1: 剪切斜列，2: 微波纹倾角，3: 轻度会聚/发散
            int motif = rng.Next(0, 4);

            // 简易 Halton 序列函数：形成“蓝噪声感”的位置抖动（比完全随机更均匀）
            float Halton(int index, int b)
            {
                float f = 1f, r = 0f;
                while (index > 0)
                {
                    f /= b;
                    r += f * (index % b);
                    index /= b;
                }
                return r;
            }

            for (int i = 0; i < beamCount; i++)
            {
                // 归一化位置 u ∈ [0,1]
                float u = i / (float)(beamCount - 1);

                // 横向位置：线性排布 + 低幅“蓝噪声”抖动
                float xBase = MathHelper.Lerp(-spreadWidth / 2f, spreadWidth / 2f, u);
                float xJitter = (Halton(i + 17, 2) - 0.5f) * 28f + rng.NextFloat(-6f, 6f); // 蓝噪声 + 少量白噪
                float yJitter = (Halton(i + 29, 3) - 0.5f) * 22f;                          // 纵向微抖，避免死板

                Vector2 spawnPos = center + new Vector2(xBase + xJitter, -(spawnHeight + yJitter));

                // 基础向下方向
                float angleDeg = MathHelper.Lerp(-angleSpreadDeg / 2f, angleSpreadDeg / 2f, u);
                angleDeg += baseAngleDeg;

                // 根据“构图模式”做有秩序的变化
                switch (motif)
                {
                    case 1:
                        // 剪切：两端角度偏移更大，中间更小（线性 -> 更利落）
                        angleDeg += (u - 0.5f) * rng.NextFloat(6f, 12f);
                        break;
                    case 2:
                        // 微波纹：在平行的基础上叠一丝正弦起伏
                        angleDeg += (float)Math.Sin(u * MathHelper.TwoPi) * rng.NextFloat(3f, 7f);
                        break;
                    case 3:
                        // 轻度会聚/发散：整体向某个倾向稍微靠拢或展开
                        angleDeg += MathHelper.Lerp(-6f, 6f, (float)rng.NextDouble());
                        break;
                    default:
                        // 0: 基础平行扇形（不额外处理）
                        break;
                }

                // 再加一点点每条束自身的小抖动（±1.5°），保留“活性”但不破坏矩阵秩序
                angleDeg += rng.NextFloat(-1.5f, 1.5f);

                // 方向向量
                Vector2 dir = Vector2.UnitY.RotatedBy(MathHelper.ToRadians(angleDeg));

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dir,
                    ModContent.ProjectileType<TEM00LeftLazer>(),
                    Projectile.damage,
                    0f,
                    owner.whoAmI
                );
            }

            // === 绚烂特效：保留但也稍许随机 ===
            int dustCount = 32 + rng.Next(12); // 32~44
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.Electric, dustVel);
                d.noGravity = true;
                d.scale = 1.2f + Main.rand.NextFloat(0.5f);
                d.color = Color.Lerp(Color.White, Color.Cyan, 0.6f);
            }

            SoundEngine.PlaySound(SoundID.Item122, center);
        }




    }
}
