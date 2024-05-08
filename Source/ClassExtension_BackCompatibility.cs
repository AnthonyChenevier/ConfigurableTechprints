// ClassExtension_BackCompatibility.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2024/04/17 1:14 PM
// Last edited by: Anthony Chenevier on 2024/04/17 1:14 PM


using System;
using System.Xml;
using HarmonyLib;
using Verse;

namespace ConfigurableTechprints;

[HarmonyPatch(typeof(BackCompatibility))]
public static class ClassExtension_BackCompatibility
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(BackCompatibility.BackCompatibleDefName))]
    private static string GetBackCompatibleDefName_HP(string __result, Type defType, string defName)
    {
        //if (defType == typeof(ResearchProjectDef) && defName == "EnhancedGrowthVatLearningResearch")
        //    return "GrowthVatOverclockingResearch";

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BackCompatibility.WasDefRemoved))]
    private static bool WasDefRemoved_HP(bool __result, string defName, Type type)
    {
        //if (type == typeof(ThingDef) && defName == "Techprint_GrowthVatOverclockingResearch")
        //    return true;

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BackCompatibility.GetBackCompatibleType))]
    private static Type GetBackCompatibleType_HP(Type __result, Type baseType, string providedClassName, XmlNode node)
    {
        //growth tracker
        //if (providedClassName is "EnhancedGrowthVatLearning.GrowthTrackerRepository")
        //    return typeof(GrowthTrackerRepository);
        if (providedClassName is "ConfigurableTechprintSettingsData")
            return typeof(CTModSettings);

        if (providedClassName is "ConfigurableTechprintSettingsScreen")
            return typeof(CTSettingsScreen);

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BackCompatibility.PostExposeData))]
    private static void PostExposeData_HP(object obj)
    {
        //if (Scribe.mode != LoadSaveMode.LoadingVars || obj is not CompOverclockedGrowthVat vatComp)
        //    return;

        //bool? oldEnabled = new();
        //LearningMode? oldMode = new();
        //bool? oldPaused = new();

        //Scribe_Values.Look(ref oldEnabled, "enabled");
        //Scribe_Values.Look(ref oldMode, "mode");
        //Scribe_Values.Look(ref oldPaused, "pausedForLetter");

        //if (oldEnabled.HasValue)
        //    vatComp.overclockingEnabled = oldEnabled.Value;

        //if (oldMode.HasValue)
        //    vatComp.currentMode = oldMode.Value;

        //if (oldPaused.HasValue)
        //    vatComp.vatgrowthPaused = oldPaused.Value;

        //if (oldPaused.HasValue || oldEnabled.HasValue || oldMode.HasValue)
        //    Log.Message($"GrowthVatsOverclockedMod :: converted old save values for {vatComp.ToStringSafe()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BackCompatibility.ResearchManagerPostLoadInit))]
    private static void ResearchManagerPostLoadInit_HP()
    {
        //ResearchManager researchManager = Find.ResearchManager;
        //if (researchManager.GetProgress(GVODefOf.GrowthVatOverclockingResearch) == 2000f)
        //    researchManager.FinishProject(GVODefOf.GrowthVatOverclockingResearch);

        //if (researchManager.GetProgress(GVODefOf.VatLearningLeaderResearch) == 500f)
        //    researchManager.FinishProject(GVODefOf.VatLearningLeaderResearch);

        //if (researchManager.GetProgress(GVODefOf.VatLearningPlayResearch) == 500f)
        //    researchManager.FinishProject(GVODefOf.VatLearningPlayResearch);
    }
}
