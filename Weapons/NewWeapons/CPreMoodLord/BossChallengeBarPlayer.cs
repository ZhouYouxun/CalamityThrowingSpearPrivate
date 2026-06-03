using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord
{
    public class BossChallengeBarPlayer : ModPlayer
    {
        private const int TidalTimeLimitFrames = 60 * 120;
        private const int SagittariusAllowedHits = 4;
        private const int SagittariusFailureHits = SagittariusAllowedHits + 1;
        private const int EncounterResolutionGraceFrames = 60 * 2;
        private const float FadeInStep = 1f / 12f;
        private const float FadeOutStep = 1f / 20f;

        private bool tidalEncounterActive;
        private bool tidalChallengeFailed;
        private int tidalTimer;
        private int tidalResolutionGraceTimer;
        private float tidalBarOpacity;

        private bool sagittariusEncounterActive;
        private bool sagittariusChallengeFailed;
        private int sagittariusHitCount;
        private int sagittariusResolutionGraceTimer;
        private float sagittariusBarOpacity;

        public bool TidalChallengeSucceeded => tidalEncounterActive && !tidalChallengeFailed && tidalTimer <= TidalTimeLimitFrames;
        public bool SagittariusChallengeSucceeded => sagittariusEncounterActive && !sagittariusChallengeFailed;

        public float CurrentBarOpacity => tidalBarOpacity > 0f ? tidalBarOpacity : sagittariusBarOpacity;

        public float CurrentBarProgress
        {
            get
            {
                if (tidalBarOpacity > 0f)
                {
                    return MathHelper.Clamp(tidalTimer / (float)TidalTimeLimitFrames, 0f, 1f);
                }

                return MathHelper.Clamp(sagittariusHitCount / (float)SagittariusFailureHits, 0f, 1f);
            }
        }

        public Color CurrentBarColor
        {
            get
            {
                if (tidalBarOpacity > 0f)
                {
                    float progress = CurrentBarProgress;
                    return Color.Lerp(new Color(70, 150, 255), new Color(110, 220, 255), progress);
                }

                float progressSagittarius = CurrentBarProgress;
                return Color.Lerp(new Color(55, 135, 245), new Color(145, 225, 255), progressSagittarius);
            }
        }

        public override void UpdateDead()
        {
            ResetTidalEncounter();
            ResetSagittariusEncounter();
        }

        public override void PostUpdate()
        {
            UpdateTidalEncounterState();
            UpdateSagittariusEncounterState();
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            RegisterSagittariusHit();
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            RegisterSagittariusHit();
        }

        public void CompleteTidalEncounter()
        {
            ResetTidalEncounter();
        }

        public void CompleteSagittariusEncounter()
        {
            ResetSagittariusEncounter();
        }

        private void UpdateTidalEncounterState()
        {
            bool challengeAvailable = !NPC.downedAncientCultist && !CalamityThrowingSpearSystem.HasGivenTidalMechanicsWeapon;
            bool dukeAlive = IsBossAlive(NPCID.DukeFishron);

            if (challengeAvailable && dukeAlive)
            {
                tidalResolutionGraceTimer = 0;

                if (!tidalEncounterActive)
                {
                    tidalEncounterActive = true;
                    tidalChallengeFailed = false;
                    tidalTimer = 0;
                }

                if (!tidalChallengeFailed)
                {
                    tidalTimer++;
                    if (tidalTimer > TidalTimeLimitFrames)
                    {
                        tidalChallengeFailed = true;
                    }
                }
            }
            else if (tidalEncounterActive)
            {
                tidalResolutionGraceTimer++;
                if (!challengeAvailable || tidalResolutionGraceTimer > EncounterResolutionGraceFrames)
                {
                    ResetTidalEncounter();
                }
            }

            bool showBar = tidalEncounterActive && !tidalChallengeFailed && dukeAlive;
            UpdateBarOpacity(ref tidalBarOpacity, showBar);
        }

        private void UpdateSagittariusEncounterState()
        {
            bool challengeAvailable = !NPC.downedAncientCultist && !CalamityThrowingSpearSystem.HasGivenSagittariusWeapon;
            bool empressAlive = IsBossAlive(NPCID.HallowBoss);

            if (challengeAvailable && empressAlive)
            {
                sagittariusResolutionGraceTimer = 0;

                if (!sagittariusEncounterActive)
                {
                    sagittariusEncounterActive = true;
                    sagittariusChallengeFailed = false;
                    sagittariusHitCount = 0;
                }
            }
            else if (sagittariusEncounterActive)
            {
                sagittariusResolutionGraceTimer++;
                if (!challengeAvailable || sagittariusResolutionGraceTimer > EncounterResolutionGraceFrames)
                {
                    ResetSagittariusEncounter();
                }
            }

            bool showBar = sagittariusEncounterActive && !sagittariusChallengeFailed && empressAlive;
            UpdateBarOpacity(ref sagittariusBarOpacity, showBar);
        }

        private void RegisterSagittariusHit()
        {
            if (!sagittariusEncounterActive || sagittariusChallengeFailed)
            {
                return;
            }

            if (!IsBossAlive(NPCID.HallowBoss) || NPC.downedAncientCultist || CalamityThrowingSpearSystem.HasGivenSagittariusWeapon)
            {
                return;
            }

            sagittariusHitCount = global::System.Math.Min(sagittariusHitCount + 1, SagittariusFailureHits);
            if (sagittariusHitCount >= SagittariusFailureHits)
            {
                sagittariusChallengeFailed = true;
            }
        }

        private static bool IsBossAlive(int npcType)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == npcType)
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateBarOpacity(ref float opacity, bool showBar)
        {
            if (showBar)
            {
                opacity = global::System.Math.Min(1f, opacity + FadeInStep);
            }
            else if (opacity > 0f)
            {
                opacity = global::System.Math.Max(0f, opacity - FadeOutStep);
            }
        }

        private void ResetTidalEncounter()
        {
            tidalEncounterActive = false;
            tidalChallengeFailed = false;
            tidalTimer = 0;
            tidalResolutionGraceTimer = 0;
            tidalBarOpacity = 0f;
        }

        private void ResetSagittariusEncounter()
        {
            sagittariusEncounterActive = false;
            sagittariusChallengeFailed = false;
            sagittariusHitCount = 0;
            sagittariusResolutionGraceTimer = 0;
            sagittariusBarOpacity = 0f;
        }
    }
}
