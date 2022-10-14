// ConfigurableTechprintsSettings.cs
// 
// Part of lostTechnologyCore - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on // 
// Last edited by: Anthony Chenevier on 2022/10/01 12:09 AM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints {
    public class ConfigurableTechprintsSettings : ModSettings
    {
        //NOTE: Just add a new const here and to the final sum when a dlc comes
        //out to account for new techs. Unless I can find a way to do it dynamically

        //counted on 2022/10/01 using notepad++ -> find all for '<ResearchProjectDef' in 'Rimworld/Data/Core/Defs/ResearchProjectDefs'. 76 results, 1 was abstract = 75 projects
        private const int VanillaTechCount = 75;
        //counted on 2022/10/01 using notepad++ -> find all for '<ResearchProjectDef' in 'Rimworld/Data/Royalty/Defs/ResearchProjectDefs'. 21 results, 2 were abstract = 19 projects (11 with techprints)
        private const int RoyaltyTechCount = 19;
        //counted on 2022/10/01 using notepad++ -> find all for '<ResearchProjectDef' in 'Rimworld/Data/Ideology/Defs/ResearchProjectDefs'. 3 results, 0 were abstract = 3 projects
        private const int IdeologyTechCount = 3;

        // total number of vanilla and dlc techs
        private const int VanillaAndDlcTechCount = VanillaTechCount + RoyaltyTechCount + IdeologyTechCount; // = 97
        public readonly float ExtraProjectCountMultiplier = Mathf.Clamp(DefDatabase<ResearchProjectDef>.DefCount / (float)VanillaAndDlcTechCount, 1, 3);

        //general techprint settings
        public int TechprintPerResearchPoints;
        public float MarketPriceMultiplier;
        
        public bool ModifyBaseCosts;
        public float ResearchBaseCostMultiplier;
        public float BaseCommonality;
        public float NativeCommonalityMultiplier;

        public float TraderStockCountMultiplier;
        public float TechDifferenceCommonalityMultiplier;
        public float TechEqualCommonalityMultiplier;

        public Dictionary<string, TechprintData> CustomTechprints;
        public List<string> IgnoredTraders;
        public Dictionary<string, TraderData> CustomTraders;


        public Dictionary<TechLevel, bool> TechLevelsWithTechprints;

        public ConfigurableTechprintsSettings()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            TechprintPerResearchPoints = 4000;
            MarketPriceMultiplier = 0.5f;
            ModifyBaseCosts = false;
            ResearchBaseCostMultiplier = 1f;
            BaseCommonality = 3f;
            
            NativeCommonalityMultiplier = 1.5f;

            TraderStockCountMultiplier = 4f;
            TechDifferenceCommonalityMultiplier = 0.2f;
            TechEqualCommonalityMultiplier = 2f;
            
            CustomTechprints = new Dictionary<string, TechprintData>();
            CustomTraders = new Dictionary<string, TraderData>();
            IgnoredTraders = new List<string>();

            TechLevelsWithTechprints = new Dictionary<TechLevel, bool>
            {
                [TechLevel.Neolithic] = true,
                [TechLevel.Medieval] = true,
                [TechLevel.Industrial] = true,
                [TechLevel.Spacer] = true,
                [TechLevel.Ultra] = true,
                [TechLevel.Archotech] = true
            };
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TechprintPerResearchPoints, nameof(TechprintPerResearchPoints));
            Scribe_Values.Look(ref MarketPriceMultiplier, nameof(MarketPriceMultiplier));
            Scribe_Values.Look(ref ModifyBaseCosts, nameof(ModifyBaseCosts));
            Scribe_Values.Look(ref ResearchBaseCostMultiplier, nameof(ResearchBaseCostMultiplier));
            Scribe_Values.Look(ref BaseCommonality, nameof(BaseCommonality));
            
            Scribe_Values.Look(ref NativeCommonalityMultiplier, nameof(NativeCommonalityMultiplier));

            Scribe_Values.Look(ref TraderStockCountMultiplier, nameof(TraderStockCountMultiplier));
            Scribe_Values.Look(ref TechDifferenceCommonalityMultiplier, nameof(TechDifferenceCommonalityMultiplier));
            Scribe_Values.Look(ref TechEqualCommonalityMultiplier, nameof(TechEqualCommonalityMultiplier));

            Scribe_Collections.Look(ref CustomTechprints, nameof(CustomTechprints), LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref IgnoredTraders, nameof(IgnoredTraders), LookMode.Value);
            Scribe_Collections.Look(ref CustomTraders, nameof(CustomTraders), LookMode.Value, LookMode.Deep);

            Scribe_Collections.Look(ref TechLevelsWithTechprints, nameof(TechLevelsWithTechprints), LookMode.Value, LookMode.Value);
        }
    }
}
