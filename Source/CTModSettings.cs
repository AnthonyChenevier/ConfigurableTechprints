// ConfigurableTechprintsSettings.cs
// 
// Part of ConfigurableTechprints
// 
// Created by: Anthony Chenevier on // 
// Last edited by: Anthony Chenevier on 2022/10/01 12:09 AM


using System.Collections.Generic;
using ConfigurableTechprints.DataTypes;
using RimWorld;
using Verse;

namespace ConfigurableTechprints;

public class CTModSettings : ModSettings
{
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

    public CTModSettings() { SetDefaults(); }

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
            /*[TechLevel.Undefined] = false,*/
            [TechLevel.Animal] = false,
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
