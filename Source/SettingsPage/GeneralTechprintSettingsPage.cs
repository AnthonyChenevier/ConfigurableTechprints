// GeneralTechprintSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:16 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:16 PM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal class GeneralTechprintSettingsPage : ConfigurableTechprintsSettingPage
{
    private readonly Dictionary<TechLevel, int> _techLevelCounts;
    private readonly List<TechLevel> _techLevels;

    public GeneralTechprintSettingsPage()
    {
        //cache stuff once
        _techLevels = settings.TechLevelsWithTechprints.Keys.ToList();
        List<ResearchProjectDef> projectDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
        _techLevelCounts = new()
        {
            { TechLevel.Neolithic, projectDefs.Count(def => def.techLevel == TechLevel.Neolithic && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
            { TechLevel.Medieval, projectDefs.Count(def => def.techLevel == TechLevel.Medieval && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
            { TechLevel.Industrial, projectDefs.Count(def => def.techLevel == TechLevel.Industrial && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
            { TechLevel.Spacer, projectDefs.Count(def => def.techLevel == TechLevel.Spacer && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
            { TechLevel.Ultra, projectDefs.Count(def => def.techLevel == TechLevel.Ultra && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
            { TechLevel.Archotech, projectDefs.Count(def => def.techLevel == TechLevel.Archotech && !def.HasModExtension<NativeTechprint_DefModExtension>()) },
        };
    }

    protected override void DoPage(Listing_Standard list, Rect inRect)
    {
        string tprStringBuffer = null;
        string rbcStringBuffer = null;
        string mpmStringBuffer = null;
        string bcStringBuffer = null;

        list.Label("TechLevelsWithTechprints_Label".Translate(), tooltip: "TechLevelsWithTechprints_Tooltip".Translate());
        foreach (TechLevel key in _techLevels)
        {
            bool value = settings.TechLevelsWithTechprints[key];
            list.CheckboxLabeled($"{$"TechLevel_{key}".Translate().CapitalizeFirst()} ({"TechLevelProjectCount_Label".Translate(_techLevelCounts[key])}):",
                                 ref value,
                                 "TechLevelsWithTechprints_TechSelectTooltip".Translate($"TechLevel_{key}".Translate().CapitalizeFirst()));

            settings.TechLevelsWithTechprints[key] = value;
        }

        list.GapLine();
        list.Label($"<b>{"TechprintGenerationSetting_Heading".Translate()}</b>");
        list.Label("TechprintPerResearchPoints_Label".Translate());
        list.SliderWithTextField(ref settings.TechprintPerResearchPoints, ref tprStringBuffer, 100, 24000, 50, "TechprintPerResearchPoints_Tooltip".Translate());

        list.Gap();
        list.Label("MarketPriceMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.MarketPriceMultiplier, ref mpmStringBuffer, 0.01f, 10f, tooltip: "MarketPriceMultiplier_Tooltip".Translate());

        list.Gap();
        list.Label("BaseCommonality_Label".Translate());
        list.SliderWithTextField(ref settings.BaseCommonality, ref bcStringBuffer, 0.01f, 100f, tooltip: "BaseCommonality_Tooltip".Translate());

        list.Gap();
        list.Label($"<b>{"ProjectModifications_Heading".Translate()}</b>");
        list.CheckboxLabeled("ModifyBaseCosts_Label".Translate(), ref settings.ModifyBaseCosts, "ModifyBaseCosts_Tooltip".Translate());

        if (!settings.ModifyBaseCosts)
            return;

        list.Gap();
        list.Label("ResearchBaseCostMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.ResearchBaseCostMultiplier, ref rbcStringBuffer, 0.01f, 10f, tooltip: "ResearchBaseCostMultiplier_Tooltip".Translate());
    }
}
