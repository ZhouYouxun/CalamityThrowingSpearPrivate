using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class SurfeiterDrumINVEXP : BaseMassiveExplosionProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override int Lifetime => 40;
        public override bool UsesScreenshake => true;
        //public override bool UsesScreenshake => false; // 关闭屏幕震动效果
        public override float GetScreenshakePower(float pulseCompletionRatio) => CalamityUtils.Convert01To010(pulseCompletionRatio) * 0.1f;
        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.Black, Color.Gray, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void PostAI()
        {
            Lighting.AddLight(Projectile.Center, 0, Projectile.Opacity * 0.7f / 255f, Projectile.Opacity);
        }
        private int drumForm = 0;

        public void SetDrumForm(int form)
        {
            drumForm = form;
            Projectile.netUpdate = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //1.笞:Flogging-敌怪获得1.02倍的易伤
            //2.杖:Beating-敌怪的接触伤害减少30%
            //3.徒:Imprisoning-敌怪的移动速度减少50%
            //4.流:Banishing-敌怪防御降低40
            //5.死:Executing-2秒的倒计时结束后造成5000点伤害
            target.AddBuff(ModContent.BuffType<SurfeiterDrumEDebuff>(), 125);

            // 传递模式给 GlobalNPC
            if (target.TryGetGlobalNPC(out SurfeiterDrumGlobalNpc globalNpc))
            {
                globalNpc.drumForm = drumForm;
            }
        }

    }
}
