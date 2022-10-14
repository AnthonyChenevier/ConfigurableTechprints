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

namespace ConfigurableTechprints.DefProcessors
{
    public static class ResearchProjectDefProcessor
    {
        public static void ProcessCustomTechprint(ResearchProjectDef projectDef, TechprintData customData)
        {
            int newTechprintCount = customData.techprintCount;
            float newProjectBaseCost = customData.baseCost;
            float newTechprintMarketValue = customData.techprintMarketValue;
            float newTechprintCommonality = customData.techprintCommonality;
            List<string> newHeldByFactionCategoryTags = customData.heldByFactionCategoryTags;

            string report = ApplyTechprintSettingsToProjectDef(projectDef, newProjectBaseCost, newTechprintCount, newTechprintMarketValue, newTechprintCommonality, newHeldByFactionCategoryTags);


            Log.Message($"ConfigurableTechprintsMod :: ResearchProjectDef ({projectDef.defName}) custom data processing report:{report}");
        }

        public static void ProcessNativeTechprint(ResearchProjectDef projectDef, float nativeCommonalityMultiplier)
        {
            Log.Message($"ConfigurableTechprintsMod :: ResearchProjectDef ({projectDef.defName}) with native techprint report:" +
                      $"\n\tModified commonality values:" +
                      $"\n\tOld value: {projectDef.techprintCommonality}" +
                      $"\n\tNew value: {projectDef.techprintCommonality * nativeCommonalityMultiplier}");
            //We don't want to hide native techprints from traders with the giant amount of techprints
            //added by this mod, so apply a multiplier to their commonality (configurable, natively 2x)
            projectDef.techprintCommonality *= nativeCommonalityMultiplier;
        }

        public static void ProcessGeneratedTechprint(ResearchProjectDef projectDef, List<string> factionCategoryTagsForTechLevel, ConfigurableTechprintsSettings modSettings)
        {
            int newTechprintCount = CalcTechprintCount(projectDef.baseCost, modSettings.TechprintPerResearchPoints);
            float newProjectBaseCost = CalcBaseCost(projectDef.baseCost, newTechprintCount, modSettings.ResearchBaseCostMultiplier);
            float newTechprintMarketValue = CalcTechprintMarketValue(projectDef.baseCost, newTechprintCount, modSettings.MarketPriceMultiplier);
            float newTechprintCommonality = CalcTechprintCommonality(modSettings.BaseCommonality);
            List<string> newHeldByFactionCategoryTags = factionCategoryTagsForTechLevel;

            string report = ApplyTechprintSettingsToProjectDef(projectDef, newProjectBaseCost, newTechprintCount, newTechprintMarketValue, newTechprintCommonality, newHeldByFactionCategoryTags);
            

            Log.Message($"ConfigurableTechprintsMod :: ResearchProjectDef ({projectDef.defName}) generated techprint processing report:{report}");
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
        private static int CalcTechprintCount(float defaultBaseCost, int techprintPerResearchPoints)
        {
            return 1 + (int)(defaultBaseCost) / techprintPerResearchPoints;
        }

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
            return halfBaseCost + Mathf.RoundToInt(halfBaseCost * (1f / techprintCount)/ roundTo) * roundTo * researchBaseCostMultiplier;
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
        private static string ApplyTechprintSettingsToProjectDef(ResearchProjectDef projectDef, float newBaseCost, int newTechprintCount, float newTechprintMarketValue,
                                                                 float newTechprintCommonality, List<string> newHeldByFactionCategoryTags)
        {
            //we build and return a report to be shown later
            string oldValuesString = $"baseCost = {projectDef.baseCost}; " +
                                     $"techprintCount = {projectDef.techprintCount}; " +
                                     $"techPrintMarketValue = {projectDef.techprintMarketValue}; " +
                                     $"techprintCommonality = {projectDef.techprintCommonality}, " +
                                     $"heldByFactionCategoryTags = {(projectDef.heldByFactionCategoryTags != null ? string.Join(", ", projectDef.heldByFactionCategoryTags) : "NONE")};";

            projectDef.baseCost = newBaseCost;
            projectDef.techprintCount = newTechprintCount;

            projectDef.techprintMarketValue = newTechprintMarketValue;
            projectDef.techprintCommonality = newTechprintCommonality;

            projectDef.heldByFactionCategoryTags = newHeldByFactionCategoryTags;

            string newValuesString = $"baseCost = {projectDef.baseCost}; " +
                                     $"techprintCount = {projectDef.techprintCount}; " +
                                     $"techPrintMarketValue = {projectDef.techprintMarketValue}; " +
                                     $"techprintCommonality = {projectDef.techprintCommonality}, " +
                                     $"heldByFactionCategoryTags = {(projectDef.heldByFactionCategoryTags != null ? string.Join(", ", projectDef.heldByFactionCategoryTags) : "NONE")};";

            return $"\n\tApplied techprint settings:" +
                   $"\n\tOld values: {oldValuesString}" +
                   $"\n\tNew values: {newValuesString}";
        }
    }
}
