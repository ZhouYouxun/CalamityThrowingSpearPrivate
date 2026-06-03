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
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.DeveloperWeapons.ShadowJav
{
    public class ShadowJavREPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/DeveloperWeapons/ShadowJav/ShadowJav";
        public int Time = 0;

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private bool hasSplit = false; // �Ƿ��ѷ���
        private static readonly string[] SplitProjectiles = new string[]
        {
            "AmidiasTridentJavPROJ", "GoldplumeJavPROJ", "SausageMakerJavPROJ", "YateveoBloomJavPROJ",
            "BrimlanceJavPROJ", "EarthenJavPROJ", "StarnightLanceJavPROJ",
            "AstralPikeJavPROJ", "BotanicPiercerJavPROJ", "DiseasedJavPROJ", "GalvanizingGlaiveJavPROJ", "HellionFlowerJavPROJ",
            "TenebreusTidesJavPROJ", "TyphonsGreedJavPROJ", "VulcaniteLanceJavPROJ",
            "BansheeHookJavPROJ", "GildedProboscisJavPROJ",
            "ElementalLanceJavPROJNebula", "ElementalLanceJavPROJSolar", "ElementalLanceJavPROJStardust", "ElementalLanceJavPROJVortex", "ElementalLanceJavPROJEntropy",
            "DragonRageJavPROJ", "NadirJavPROJ", "ScourgeoftheCosmosJavPROJ", "StreamGougeJavPROJ", "ViolenceJavPROJ",

            "GraniteJavPROJ", "WulfrimJavPROJ", "RedtideJavPROJ", "BraisedPorkJavPROJ", "ElectrocoagulationTenmonJavPROJ",
            "ElectrocutionHalberdPROJ", "HeartSwordPROJ", "PearlwoodJavPROJ",
            "ChaosEssenceJavPROJ", "SunEssenceJavPROJ", "PolarEssenceJavPROJ",
            "SHPCKPROJ", "SHPCKFast", "FestiveHalberdPROJ",
            "TerraLancePROJ", "BloodstoneJavPROJ",
            "EndlessDevourJavPROJ", "ChaosWindJavPROJ", "InfiniteDarknessJavPROJ", "SoulHunterJavPROJ",
            "AuricJavPROJ", "MiracleMatterJavPROJ", "TheOtherMiracleMatterJavPROJ",
            "SoulSeekerJavPROJ"
        };


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // ÿ��һ֡��������Ҳ���Ըĳ� Main.GameUpdateCount % 2 == 0 ÿ��֡��
            if (Main.rand.NextBool(1))
            {
                // ���п�ѡ��������Ч
                ParticleOrchestraType[] choices = new[]
                {
            ParticleOrchestraType.Keybrand,
            ParticleOrchestraType.NightsEdge,
            ParticleOrchestraType.TrueNightsEdge,
            ParticleOrchestraType.Excalibur,
            ParticleOrchestraType.TrueExcalibur,
            ParticleOrchestraType.TerraBlade,
            ParticleOrchestraType.RainbowRodHit,
            ParticleOrchestraType.SilverBulletSparkle,
            ParticleOrchestraType.ShimmerArrow
        };

                // ���ѡһ��
                ParticleOrchestraType chosen = Main.rand.NextFromList(choices);

                // �ڵ�ǰλ��������Ч
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    chosen,
                    new ParticleOrchestraSettings
                    {
                        PositionInWorld = Projectile.Center
                    },
                    Projectile.owner
                );
            }

            // ���ճ����Ŀ���ϣ��򱣳�Ͷ�������Ŀ��
            if (Projectile.ai[0] == 1f)
            {
                int targetIndex = (int)Projectile.ai[1];
                if (targetIndex >= 0 && targetIndex < 200)
                {
                    NPC target = Main.npc[targetIndex];
                    if (target.active)
                    {
                        // ����Ŀ���ƶ�
                        Projectile.Center = target.Center;
                        // ����ԭ�е���ת�Ƕ�
                        Projectile.rotation += 0.33f; // ճ�ڵ�������ʱ�Ὺʼ���ϵ���ת
                        Projectile.alpha = 255; // �Ժ��������ɼ�
                        if (Main.zenithWorld)
                        {
                            Projectile.timeLeft = 600; // һ��ҧס�Ͳ����ɿڣ������ֶ�ȡ��
                        }

                        //// ����Ч��������ͷ�
                        //if (Time % 3 == 0)
                        //{
                        //    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                        //    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // �������ƫ��
                        //    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                        //    Particle Smear = new CircularSmearVFX(particlePosition, Color.Black * Main.rand.NextFloat(0.78f, 0.85f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                        //    GeneralParticleHandler.SpawnParticle(Smear);
                        //}
                        //Time++;

                        // ����Ļ𤸽�ڵ�������ʱ�����ɶ��ⵯĻ
                        if (Projectile.timeLeft % 5 == 0) // ÿ��5֡����һ�����ⵯĻ
                        {
                            string selectedProjectile = SplitProjectiles[Main.rand.Next(SplitProjectiles.Length)];

                            // ����Ӹ�Զ�������������ɵ�Ļ�����������·�Χ���нϴ��ƫ��
                            float offsetX = Main.rand.NextBool() ? -900f : 900f; // �����Ҳ�ƫ�ƣ����ֶ�������ֵ��������ƫ�Ƶľ��룩
                            float offsetY = Main.rand.NextFloat(-400f, 400f); // ����ƫ�Ʒ�Χ�����ֶ�������ֵ��������ƫ�Ƶķ�Χ��

                            // ��������λ��
                            Vector2 spawnPosition = new Vector2(Projectile.Center.X + offsetX, Projectile.Center.Y + offsetY);

                            // �����ٶ�������ʹ��Ļ������λ�ó��ŵ�Ļ�������
                            Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitX) * 15f;

                            // ���ɶ���ĵ�Ļ
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, Mod.Find<ModProjectile>(selectedProjectile).Type, (int)(Projectile.damage * 35f), 0f, Projectile.owner);
                        }

                        //// ����ɫ�����Ч��
                        //if (Projectile.timeLeft % 20 == 0)
                        //{
                        //    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Black, new Vector2(1.5f), Projectile.rotation, 1f, 0.1f, 30);
                        //    GeneralParticleHandler.SpawnParticle(pulse);
                        //}
                    }
                    else
                    {
                        // ���Ŀ�겻�ٻ�Ծ��������Ͷ����
                        Projectile.Kill();
                    }
                }
            }
            else
            {
                // ǰ30֡��׷�٣�֮��ʼ׷�ٵ���
                if (Projectile.ai[1] > 30)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(88888); // ���ҷ�Χ������ĵ���
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 50f, 0.08f); // ׷���ٶ�Ϊxf
                    }
                }
                else
                {
                    Projectile.ai[1]++;
                }

                // ���ֵ�Ļ��ԭʼ��ת�Ƕȣ�ֱ����һ�λ���Ŀ��
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                //Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.ai[0];

                // �޸�ճ��ʱ��ת����������
                if (Projectile.ai[0] == 0f)
                {
                    //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    Projectile.ai[0] = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // ��¼��ʼ��ת�Ƕ�
                }

                // ��Ӻ�ɫ��Դ
                Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

                // ��Ļ�𽥼���
                Projectile.velocity *= 1.005f;

                // ��Ӻ�ɫ������Ч
                LineParticle energy = new LineParticle(Projectile.Center + Projectile.velocity * 4, Projectile.velocity * 4.95f, false, 9, 2.4f, Color.Black);
                GeneralParticleHandler.SpawnParticle(energy);
            }
        }


        // ���е��˺�𤸽�߼�
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ModifyHitNPCSticky(30); // ��Ļ����Ŀ���ճ����������6֡���޵�״̬
            hasSplit = true; // ��ʾ��Ļ�Ѿ������˵��ˣ����븽��״̬
            Projectile.tileCollide = false; // ��ײʱ������ʧ
            Projectile.velocity = Vector2.Zero; // ʹ��Ļ�����ƶ�
            Projectile.ai[1] = target.whoAmI; // ��¼Ŀ��ID��ȷ������Ŀ���ƶ�
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasSplit)
            {
                NPC.HitModifiers modifiers = default; // ����һ���ɸ�ֵ�ı���
                ModifyHitNPC(target, ref modifiers);  // �����������
            }
        }



    }
}