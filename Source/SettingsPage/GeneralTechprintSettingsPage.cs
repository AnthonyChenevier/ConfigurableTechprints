// GeneralTechprintSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:16 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:16 PM


using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal class GeneralTechprintSettingsPage : ConfigurableTechprintsSettingPage
{
    protected override void DoPage(Listing_Standard list, Rect inRect)
    {
        string tprStringBuffer = null;
        string rbcStringBuffer = null;
        string mpmStringBuffer = null;
        string bcStringBuffer = null;

        list.Label("TechLevelsWithTechprints_Label".Translate(), tooltip: "TechLevelsWithTechprints_Tooltip".Translate());
        bool unlockNeolithic = settings.TechLevelsWithTechprints[TechLevel.Neolithic];
        list.CheckboxLabeled("TechLevel_Neolithic".Translate().CapitalizeFirst() + ":",
                             ref unlockNeolithic,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Neolithic".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Neolithic] = unlockNeolithic;
        bool unlockMedieval = settings.TechLevelsWithTechprints[TechLevel.Medieval];
        list.CheckboxLabeled("TechLevel_Medieval".Translate().CapitalizeFirst() + ":",
                             ref unlockMedieval,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Medieval".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Medieval] = unlockMedieval;
        bool unlockIndustrial = settings.TechLevelsWithTechprints[TechLevel.Industrial];
        list.CheckboxLabeled("TechLevel_Industrial".Translate().CapitalizeFirst() + ":",
                             ref unlockIndustrial,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Industrial".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Industrial] = unlockIndustrial;
        bool unlockSpacer = settings.TechLevelsWithTechprints[TechLevel.Spacer];
        list.CheckboxLabeled("TechLevel_Spacer".Translate().CapitalizeFirst() + ":",
                             ref unlockSpacer,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Spacer".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Spacer] = unlockSpacer;
        bool unlockUltra = settings.TechLevelsWithTechprints[TechLevel.Ultra];
        list.CheckboxLabeled("TechLevel_Ultra".Translate().CapitalizeFirst() + ":",
                             ref unlockUltra,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Ultra".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Ultra] = unlockUltra;
        bool unlockArcho = settings.TechLevelsWithTechprints[TechLevel.Archotech];
        list.CheckboxLabeled("TechLevel_Archotech".Translate().CapitalizeFirst() + ":",
                             ref unlockArcho,
                             "TechLevelsWithTechprints_TechSelectTooltip".Translate("TechLevel_Archotech".Translate().CapitalizeFirst()));

        settings.TechLevelsWithTechprints[TechLevel.Archotech] = unlockArcho;

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
