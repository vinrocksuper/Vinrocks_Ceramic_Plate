using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VynTekArmory
{
    public class CompShatterOnPierce : ThingComp
    {
        private float minDurabilityThreshold = 0.8f; 
        private float maxDurabilityThreshold = 0.5f; 
        private float maxThresholdDamage = 9f; // Maximum damage threshold when above 80% durability
        private float minThresholdDamage = 3f; // Minimum damage threshold when at or below 50% durability
        private float exponentialFactor = 10f; 

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if (dinfo.Def == DamageDefOf.Bullet || dinfo.Def == DamageDefOf.Stab)
            {
                float remainingDurabilityPercentage = (float)this.parent.HitPoints / (float)this.parent.MaxHitPoints;

                if (remainingDurabilityPercentage <= minDurabilityThreshold)
                {
                    float durabilityBelowThreshold = 1 - remainingDurabilityPercentage / minDurabilityThreshold;
                    float shatterChance = 1 - Mathf.Exp(-exponentialFactor * durabilityBelowThreshold);

                    float damageThreshold = Mathf.Lerp(maxThresholdDamage, minThresholdDamage,
                        (1 - remainingDurabilityPercentage) / (1 - maxDurabilityThreshold));

                    if (dinfo.Amount >= damageThreshold && Rand.Value < shatterChance)
                    {
                        Apparel apparel = this.parent as Apparel;
                        Pawn wearer = apparel.Wearer;
                        shatter();
                        if (apparel != null)
                        {
                            if (wearer != null)
                            {
                                ApplyInjuriesToTorso(wearer);
                            }
                        }
                        
                    }
                }
            }
        }

        private void shatter()
        {
            this.parent.HitPoints = 0;
            this.parent.Destroy(DestroyMode.Vanish);
        }

        private void ApplyInjuriesToTorso(Pawn pawn)
        {
            BodyPartRecord torso = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def == BodyPartDefOf.Torso);

            if (torso != null)
            {
                int numCuts = Rand.RangeInclusive(2, 6);
                for (int i = 0; i < numCuts; i++)
                {
                    float severity = Rand.Range(1f, 4f);

                    Hediff cut = HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, torso);
                    cut.Severity = severity;
                    pawn.health.AddHediff(cut);
                }
            }
        }
    }

}
