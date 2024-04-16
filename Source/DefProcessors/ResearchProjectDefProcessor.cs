// TechprintProcessor.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/08 9:02 PM
// Last edited by: Anthony Chenevier on 2022/10/08 9:02 PM


using System.Collections.Generic;
using ConfigurableTechprints.DataTypes;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.DefProcessors;

public static class ResearchProjectDefProcessor
{
    public static string ProcessCustomTechprint(ResearchProjectDef projectDef, TechprintData customData)
    {
        if (ConfigurableTechprintsMod.IsProjectNameMalformed(projectDef.defName))
            return ErrorReport(projectDef.defName);

        int newTechprintCount = customData.techprintCount;
        float newProjectBaseCost = customData.baseCost;
        float newTechprintMarketValue = customData.techprintMarketValue;
        float newTechprintCommonality = customData.techprintCommonality;
        List<string> newHeldByFactionCategoryTags = customData.heldByFactionCategoryTags;

        string report = ApplyTechprintSettingsAndReport(projectDef,
                                                        newProjectBaseCost,
                                                        newTechprintCount,
                                                        newTechprintMarketValue,
                                                        newTechprintCommonality,
                                                        newHeldByFactionCategoryTags);


        return $"\n\t- ResearchProjectDef ({projectDef.defName}) custom data processing report:{report}";
    }

    public static string ProcessGeneratedTechprint(ResearchProjectDef projectDef, List<string> factionCategoryTagsForTechLevel, ConfigurableTechprintsSettingsData modSettings)
    {
        if (ConfigurableTechprintsMod.IsProjectNameMalformed(projectDef.defName))
            return ErrorReport(projectDef.defName);

        int newTechprintCount = CalcTechprintCount(projectDef.baseCost, modSettings.TechprintPerResearchPoints);
        float newProjectBaseCost = CalcBaseCost(projectDef.baseCost, newTechprintCount, modSettings.ResearchBaseCostMultiplier);
        float newTechprintMarketValue = CalcTechprintMarketValue(projectDef.baseCost, newTechprintCount, modSettings.MarketPriceMultiplier);
        float newTechprintCommonality = CalcTechprintCommonality(modSettings.BaseCommonality);
        List<string> newHeldByFactionCategoryTags = factionCategoryTagsForTechLevel;

        string report = ApplyTechprintSettingsAndReport(projectDef,
                                                        newProjectBaseCost,
                                                        newTechprintCount,
                                                        newTechprintMarketValue,
                                                        newTechprintCommonality,
                                                        newHeldByFactionCategoryTags);


        return $"\n\t- ResearchProjectDef ({projectDef.defName}) generated techprint processing report:{report}";
    }

    private static string ErrorReport(string defName)
    {
        string errMsg =
            $"\n\t- ResearchProjectDef ({defName}) is malformed (Rimworld's techprint generator cannot create a techprint from a def with a defName that ends in a digit). " +
            $"This would generate an error if the techprint was added by XML also, and is a naming convention issue that should be fixed by the developer of the mod project def.";

        return errMsg.Colorize(ColorLibrary.RedReadable);
    }

    /// <summary>
    /// Higher values here = this techprint is more likely to generate in a techprint-enabled trader's inventory or as a quest
    /// reward or gift. This likelihood is also modified by factors such as the techprint price, the prerequisites of the research project
    /// the techprint unlocks, and which faction is attempting to offer the techprint.
    /// </summary>
    /// <param name="baseCommonality"></param>
    /// <returns></returns>
    private static float CalcTechprintCommonality(float baseCommonality)
    {
        float techprintCommonality = baseCommonality; //vanilla/royalty values are 3 for Cataphract Armour, 2 for JumpPacks and 1 for all implants (rare)
        return techprintCommonality;
    }

    /// <summary>
    /// the market value of each techprint is simply the default research cost divided by the
    /// number of techprints. High-tech projects have their own price multiplier and finally a
    /// multiplier from our mod settings is applied
    /// </summary>
    /// <param name="defaultBaseCost"></param>
    /// <param name="techprintCount"></param>
    /// <param name="marketPriceMultiplier"></param>
    /// <returns></returns>
    private static float CalcTechprintMarketValue(float defaultBaseCost, int techprintCount, float marketPriceMultiplier)
    {
        return defaultBaseCost / 2f / techprintCount * marketPriceMultiplier;
    }

    /// <summary>
    /// techprint count starts at 1 and is increased at a rate of 1 for every X research
    /// points the project costs (set in mod settings, default 4000) 
    /// </summary>
    /// <param name="defaultBaseCost"></param>
    /// <param name="techprintPerResearchPoints"></param>
    /// <returns></returns>
    private static int CalcTechprintCount(float defaultBaseCost, int techprintPerResearchPoints) { return 1 + (int)defaultBaseCost / techprintPerResearchPoints; }

    /// <summary>
    /// the research's base cost is reduced proportionally to the number of techprints
    /// it now requires, with an optional multiplier from the mod settings as well (default 1.0f).
    /// apply the discount to half of the base cost and round the result to the nearest 50.
    /// this ensures expensive projects with many techprints still maintain a large proportion
    /// of their research cost even after discounts.
    /// </summary>
    /// <param name="defaultBaseCost"></param>
    /// <param name="techprintCount"></param>
    /// <param name="researchBaseCostMultiplier"></param>
    /// <param name="roundTo"></param>
    /// <returns></returns>
    private static float CalcBaseCost(float defaultBaseCost, int techprintCount, float researchBaseCostMultiplier, int roundTo = 50)
    {
        float halfBaseCost = defaultBaseCost / 2;
        return halfBaseCost + Mathf.RoundToInt(halfBaseCost * (1f / techprintCount) / roundTo) * roundTo * researchBaseCostMultiplier;
    }

    /// <summary>
    /// Apply our settings to the project def and return a report of the differences
    /// </summary>
    /// <param name="projectDef"></param>
    /// <param name="newBaseCost"></param>
    /// <param name="newTechprintCount"></param>
    /// <param name="newTechprintMarketValue"></param>
    /// <param name="newTechprintCommonality"></param>
    /// <param name="newHeldByFactionCategoryTags"></param>
    private static string ApplyTechprintSettingsAndReport(ResearchProjectDef projectDef,
                                                          float newBaseCost,
                                                          int newTechprintCount,
                                                          float newTechprintMarketValue,
                                                          float newTechprintCommonality,
                                                          List<string> newHeldByFactionCategoryTags)
    {
        //we build and return a report to be shown later
        string[] oldValuesString =
        {
            $"{projectDef.baseCost}",
            $"{projectDef.techprintCount}",
            $"{projectDef.techprintMarketValue}",
            $"{projectDef.techprintCommonality}",
            $"{(projectDef.heldByFactionCategoryTags != null ? string.Join(", ", projectDef.heldByFactionCategoryTags) : "NONE")}"
        };

        projectDef.baseCost = newBaseCost;
        projectDef.techprintCount = newTechprintCount;

        projectDef.techprintMarketValue = newTechprintMarketValue;
        projectDef.techprintCommonality = newTechprintCommonality;

        projectDef.heldByFactionCategoryTags = newHeldByFactionCategoryTags;

        string[] newValuesString =
        {
            $"{projectDef.baseCost}",
            $"{projectDef.techprintCount}",
            $"{projectDef.techprintMarketValue}",
            $"{projectDef.techprintCommonality}",
            $"{(projectDef.heldByFactionCategoryTags != null ? string.Join(", ", projectDef.heldByFactionCategoryTags) : "NONE")}"
        };

        string report = $"\n{" ",30}{"old values",20}{"new values",20}" +
                        $"\n{"baseCost",30}{oldValuesString[0],20}{newValuesString[0],20}" +
                        $"\n{"techprintCount",30}{oldValuesString[1],20}{newValuesString[1],20}" +
                        $"\n{"techPrintMarketValue",30}{oldValuesString[2],20}{newValuesString[2],20}" +
                        $"\n{"techprintCommonality",30}{oldValuesString[3],20}{newValuesString[3],20}" +
                        $"\n{"heldByFactionCategoryTags",30}{oldValuesString[4],20}{newValuesString[4],20}";


        if (newTechprintCount > 0 && projectDef.requiredAnalyzed != null && projectDef.requiredAnalyzed.Any())
        {
            projectDef.requiredAnalyzed.Clear();
            report += "\n- Cleared project.requiredStudied.";
        }

        return report;
    }
}
