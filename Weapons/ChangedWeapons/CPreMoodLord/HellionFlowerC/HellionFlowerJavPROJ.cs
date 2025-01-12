using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC
{
    public class HellionFlowerJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private bool isStuck = false; // 标记弹幕是否已经粘附
        private int stuckTime = 0; // 计时器
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/HellionFlowerC/HellionFlowerJav").Value;

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 使用红绿渐变
                Color color = Color.Lerp(Color.OrangeRed, Color.LightGreen, colorInterpolation) * 0.4f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.5f;

                // 计算强度，使拖尾逐渐变弱
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 如果弹幕即将消失，则拖尾也逐渐消失
                }

                // 计算外部和内部的缩放比例，使拖尾具有渐变效果
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // 绘制默认的弹幕，并应用旋转
            Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            if (isStuck)
            {
                // 当弹幕已经粘附时，开始计时
                stuckTime++;

                // 经过60帧时，在随机方向释放HellionFlowerJavVine弹幕
                if (stuckTime >= 60)
                {
                    // ReleaseVineProjectile();
                    stuckTime = 0; // 重置计时器，方便循环释放
                }

                // 保持粘附在敌人身上
                Projectile.velocity = Vector2.Zero; // 停止运动
            }
            else
            {
                // 弹幕直线运动
                //Projectile.velocity *= 1.01f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 300); // 原版的酸性毒液效果
        }


        public override void OnKill(int timeLeft)
        {
            // 生成绿色烟雾特效，类似CinderArrowProj的效果
            int Dusts = 55;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.Green, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            int numberOfProjectiles = Main.rand.Next(5, 8); // 随机选择 5 到 7 个弹幕

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                // 随机选择一个角度 (0 到 360 度) 并将其转换为弧度
                float rotation = MathHelper.ToRadians(Main.rand.Next(360));

                // 设置随机速度 (例如 4f 到 8f 的范围)
                float speed = Main.rand.NextFloat(4f, 8f);

                // 计算弹幕的速度向量
                Vector2 velocity = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)) * speed;

                // 随机选择一个弹幕编号 (511, 512, 513)
                int projectileType = Main.rand.Next(new int[] { 511, 512, 513 });

                // 生成弹幕
                int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, projectileType, (int)(Projectile.damage * 0.25f), 0f, Main.myPlayer);

                // 设置弹幕的属性
                Projectile proj = Main.projectile[projectileIndex];
                proj.DamageType = DamageClass.Melee;
                proj.friendly = true; // 确保弹幕是友好的
                proj.penetrate = 8; // 设置穿透次数
                proj.localNPCHitCooldown = 60; // 设置本地 NPC 无敌帧的冷却时间
                proj.usesLocalNPCImmunity = true; // 使用本地 NPC 无敌帧
            }

            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<HellionFlowerJavVine>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);

            // 发射一条藤蔓，直接沿当前方向发射
            ReleaseVineProjectile();

        }

        // 释放HellionFlowerJavVine弹幕
        private void ReleaseVineProjectile()
        {
            // 固定初始速度为 12f 的方向
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero) * 18f; // 确保方向并设为 18f 的固定速度
            int damage = (int)(Projectile.damage * 0.5f); // 伤害倍率为 0.5 倍

            // 发射具有固定速度的藤蔓弹幕
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction, ModContent.ProjectileType<HellionFlowerJavVine>(), damage, Projectile.knockBack, Projectile.owner);
        }



    }
}
