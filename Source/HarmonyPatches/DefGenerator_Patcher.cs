// DefGenerator_Patcher.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/12 5:41 PM
// Last edited by: Anthony Chenevier on 2022/10/16 9:24 PM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using ConfigurableTechprints.DefProcessors;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ConfigurableTechprints.HarmonyPatches;

[HarmonyPatch(typeof(DefGenerator))]
public static class DefGenerator_Patcher
{
    //Hitching a ride on this method to resolve issues after defs are loaded but before implied defs are generated.
    //We can use this to resolve any issues with traders or research options caused by other mods before techprintsDefs are generated
    [HarmonyPrefix]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public static void GenerateImpliedDefs_PreResolve_Prefix()
    {
        ConfigurableTechprintsMod mod = ConfigurableTechprintsMod.Instance;
        CTModSettings modSettings = mod.Settings;

        //backing up original data so we can use it for recognizing default values and resetting changes to settings
        Log.Message("ConfigurableTechprintsMod :: Backing up pre-modification values...");
        mod.BackupTraderData();
        mod.BackupTechprintData();

        //trader modifications
        Log.Message("ConfigurableTechprintsMod :: Processing traders...");
        List<string> ignoredTraders = modSettings.IgnoredTraders;
        Dictionary<string, TraderData> customTraders = modSettings.CustomTraders;
        if (ignoredTraders.Count > 0)
            Log.Message($"ConfigurableTechprintsMod :: Found {ignoredTraders.Count} 'ignore trader' entries. The following traders will not be processed by this mod:{string.Join("\n\t- ", ignoredTraders)}");

        if (customTraders.Count > 0)
        {
            string customTradersReport = "";
            foreach (KeyValuePair<string, TraderData> pair in customTraders)
            {
                if (ignoredTraders.Contains(pair.Key))
                {
                    Log.Error($"ConfigurableTechprintsMod :: Found custom settings for TraderKindDef ({pair.Key}), but is also set to be ignored. Was this intended? Skipping.");
                    continue;
                }

                TraderKindDef traderKindDef = DefDatabase<TraderKindDef>.GetNamed(pair.Key);
                if (traderKindDef == null)
                {
                    Log.Error($"ConfigurableTechprintsMod :: Found custom settings for TraderKindDef ({pair.Key}), but couldn't find the matching TraderKindDef. Skipping.");
                    continue;
                }

                string report = TraderKindDefProcessor.ProcessCustomTrader(traderKindDef, pair.Value);
                customTradersReport += $"\n{report}";
            }

            Log.Message($"ConfigurableTechprintsMod :: Processed {customTraders.Count} TraderKindDefs with custom settings. Reports can be found in mod settings.");
            mod.Reports["CustomTraders"] = customTradersReport;
        }
        else
        {
            Log.Message("ConfigurableTechprintsMod :: No TraderKindDefs with custom settings found.");
            mod.Reports["CustomTraders"] = "None processed";
        }


        //filter for traders to those who aren't ignored or custom and have a techprint stock generator
        List<TraderKindDef> techprintTraders = DefDatabase<TraderKindDef>.AllDefs
                                                                         .Where(t => t.stockGenerators.Any(m => m is StockGenerator_Techprints) &&
                                                                                     !(ignoredTraders.Contains(t.defName) || customTraders.ContainsKey(t.defName))).ToList();

        string processedTradersReport = "";
        //process all other applicable techprint traders with default settings
        foreach (TraderKindDef trader in techprintTraders)
        {
            string report = TraderKindDefProcessor.ProcessTechprintTrader(trader, modSettings.TraderStockCountMultiplier);
            processedTradersReport += $"\n{report}";
        }

        Log.Message($"ConfigurableTechprintsMod :: Processed {techprintTraders.Count} TraderKindDefs. Reports can be found in mod settings. ");
        mod.Reports["ProcessedTraders"] = processedTradersReport;


        //techprint modifications
        Log.Message("ConfigurableTechprintsMod :: Processing research projects...");

        //find all category tags for trading factions and sort them into a dictionary
        //by tech level. if the faction has the highest tech level available it is also
        //added to the levels above itself
        List<FactionDef> tradingFactions = DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer && !f.hidden && !f.permanentEnemy).ToList();
        TechLevel highestFactionTechLevel = tradingFactions.Max(f => f.techLevel);
        Dictionary<TechLevel, List<string>> factionTagsByTechLevel = new()
        {
            { TechLevel.Animal, new List<string>() },
            { TechLevel.Neolithic, new List<string>() },
            { TechLevel.Medieval, new List<string>() },
            { TechLevel.Industrial, new List<string>() },
            { TechLevel.Spacer, new List<string>() },
            { TechLevel.Ultra, new List<string>() },
            { TechLevel.Archotech, new List<string>() }
        };

        foreach (FactionDef factionDef in tradingFactions)
            for (TechLevel iTechLevel = TechLevel.Archotech; iTechLevel >= TechLevel.Neolithic; iTechLevel--)
                if ((iTechLevel <= factionDef.techLevel || factionDef.techLevel == highestFactionTechLevel) && !factionTagsByTechLevel[iTechLevel].Contains(factionDef.categoryTag))
                    factionTagsByTechLevel[iTechLevel].Add(factionDef.categoryTag);

        //process customized techprints
        Dictionary<string, TechprintData> customTechprints = modSettings.CustomTechprints;
        if (customTechprints.Count > 0)
        {
            string customTechprintsReport = "";
            foreach (KeyValuePair<string, TechprintData> pair in customTechprints)
            {
                ResearchProjectDef researchProjectDef = DefDatabase<ResearchProjectDef>.GetNamed(pair.Key);
                if (researchProjectDef == null)
                {
                    Log.Message($"ConfigurableTechprintsMod :: Found custom settings for ResearchProjectDef ({pair.Key}), but couldn't find the matching ResearchProjectDef. Skipping.");
                    continue;
                }

                string report = ResearchProjectDefProcessor.ProcessCustomTechprint(researchProjectDef, pair.Value);
                customTechprintsReport += $"\n{report}";
            }

            Log.Message($"ConfigurableTechprintsMod :: Processed {customTechprints.Count} ResearchProjectDefs with custom settings. Reports can be found in mod settings. ");
            mod.Reports["CustomProjects"] = customTechprintsReport;
        }
        else
        {
            Log.Message("ConfigurableTechprintsMod :: No ResearchProjectDefs with custom settings found.");
            mod.Reports["CustomProjects"] = "None processed";
        }

        //filter all non-custom, non-native, non-studiableThing-having
        List<ResearchProjectDef> projectsRequiringTechprints = DefDatabase<ResearchProjectDef>.AllDefs.Where(p => p.techLevel != TechLevel.Undefined &&
                                                                                                                  ConfigurableTechprintsMod.CanHaveAutoTechprints(p) &&
                                                                                                                  !customTechprints.ContainsKey(p.defName) &&
                                                                                                                  modSettings.TechLevelsWithTechprints[p.techLevel]).ToList();

        string generatedTechprintsReport = "";
        //generate techprints from settings
        foreach (ResearchProjectDef projectDef in projectsRequiringTechprints)
        {
            string report = ResearchProjectDefProcessor.ProcessGeneratedTechprint(projectDef, factionTagsByTechLevel[projectDef.techLevel], modSettings);
            generatedTechprintsReport += report;
        }

        Log.Message($"ConfigurableTechprintsMod :: Processed {projectsRequiringTechprints.Count} ResearchProjectDefs with default settings. Reports can be found in mod settings. ");
        mod.Reports["ProcessedProjects"] = generatedTechprintsReport;
    }
}
