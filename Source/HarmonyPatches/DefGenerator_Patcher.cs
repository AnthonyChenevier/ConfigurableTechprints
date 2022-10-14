using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using ConfigurableTechprints.DefProcessors;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ConfigurableTechprints.HarmonyPatches
{
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
            ConfigurableTechprintsSettings modSettings = mod.Settings;

            //backing up original data so we can use it for resetting
            mod.BackupTraderData();
            mod.BackupTechprintData();
            
            //trader modifications
            Log.Message("ConfigurableTechprintsMod :: Processing traders...");
            List<string> ignoredTraders = modSettings.IgnoredTraders;
            Dictionary<string, TraderData> customTraders = modSettings.CustomTraders;
            //look for ignored traders and report
            if (ignoredTraders.Count > 0)
            {
                string ignoredTradersString = string.Join("\n\t- ", ignoredTraders);
                Log.Message($"ConfigurableTechprintsMod :: Found {ignoredTraders.Count} 'ignore trader' entries. The following traders will not be processed:{ignoredTradersString}");
            }

            //process custom trader entries
            if (customTraders.Count > 0)
            {
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

                    
                    TraderKindDefProcessor.ProcessCustomTrader(traderKindDef, pair.Value);
                }
            }
            Log.Message($"ConfigurableTechprintsMod :: Processed {customTraders.Count} TraderKindDefs with custom settings.");


            //filter for traders to those who aren't ignored or custom and have a techprint stock generator
            List<TraderKindDef> techprintTraders =
                DefDatabase<TraderKindDef>.AllDefs.Where(t => t.stockGenerators.Any(m => m is StockGenerator_Techprints) && 
                                                              !(ignoredTraders.Contains(t.defName) || customTraders.ContainsKey(t.defName))).ToList();
            //process all other applicable techprint traders with default settings
            foreach (TraderKindDef trader in techprintTraders)
            {

                TraderKindDefProcessor.ProcessTechprintTrader(trader, modSettings.TraderStockCountMultiplier, modSettings.ExtraProjectCountMultiplier);
            }

            Log.Message($"ConfigurableTechprintsMod :: Processed {techprintTraders.Count} TraderKindDefs with default settings.");


            //techprint modifications
            Log.Message("ConfigurableTechprintsMod :: Processing research projects...");

            //find all category tags for trading factions and sort them into a dictionary
            //by tech level. if the faction has the highest tech level available it is also
            //added to the levels above itself
            List<FactionDef> tradingFactions = DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer && !f.hidden && !f.permanentEnemy).ToList();
            TechLevel highestFactionTechLevel = tradingFactions.Max(f => f.techLevel);
            Dictionary<TechLevel, List<string>> factionTagsByTechLevel = new Dictionary<TechLevel, List<string>>
            {
                { TechLevel.Neolithic, new List<string>() },
                { TechLevel.Medieval, new List<string>() },
                { TechLevel.Industrial, new List<string>() },
                { TechLevel.Spacer, new List<string>() },
                { TechLevel.Ultra, new List<string>() },
                { TechLevel.Archotech, new List<string>() }
            };
            
            foreach (FactionDef factionDef in tradingFactions)
            {
                for (TechLevel iTechLevel = TechLevel.Archotech; iTechLevel >= TechLevel.Neolithic; iTechLevel--)
                {
                    if ((iTechLevel <= factionDef.techLevel || factionDef.techLevel == highestFactionTechLevel) &&
                        !factionTagsByTechLevel[iTechLevel].Contains(factionDef.categoryTag))
                    {
                        factionTagsByTechLevel[iTechLevel].Add(factionDef.categoryTag);
                    }
                }
            }
            
            //process custom techprints
            Dictionary<string, TechprintData> customTechprints = modSettings.CustomTechprints;
            if (customTechprints.Count > 0)
            {
                foreach (KeyValuePair<string, TechprintData> pair in customTechprints)
                {
                    ResearchProjectDef researchProjectDef = DefDatabase<ResearchProjectDef>.GetNamed(pair.Key);
                    if (researchProjectDef == null)
                    {
                        Log.Message($"ConfigurableTechprintsMod :: Found custom settings for ResearchProjectDef ({pair.Key}), but couldn't find the matching ResearchProjectDef. Skipping.");
                        continue;
                    }

                    ResearchProjectDefProcessor.ProcessCustomTechprint(researchProjectDef, pair.Value);
                }
            }
            Log.Message($"ConfigurableTechprintsMod :: Processed {customTechprints.Count} ResearchProjectDefs with custom settings.");


            //filter all non-custom projects.
            List<ResearchProjectDef> projectDefs = DefDatabase<ResearchProjectDef>.AllDefs.Where(p=> !customTechprints.ContainsKey(p.defName)).ToList();

            //filter and process native techprints - ones added by Royalty or other mods via XML defs.
            //Empire implant techs, as well as jumppacks and Empire cataphract armour are the only
            //research projects that require techprints natively and come with defined values. We have
            //to modify their commonality with a multiplier (customizable in settings) to balance them
            //with the number of new techprint definitions
            List<ResearchProjectDef> projectDefsWithNativeTechprints = projectDefs.Where(p => p.HasModExtension<NativeTechprint_DefModExtension>()).ToList();
            foreach (ResearchProjectDef projectDef in projectDefsWithNativeTechprints)
            {
                ResearchProjectDefProcessor.ProcessNativeTechprint(projectDef, modSettings.NativeCommonalityMultiplier);
            }
            Log.Message($"ConfigurableTechprintsMod :: Processed {projectDefsWithNativeTechprints.Count} ResearchProjectDefs with native techprints.");


            //generate techprints from settings
            List<ResearchProjectDef> projectsRequiringTechprints = projectDefs.Where(p => !p.HasModExtension<NativeTechprint_DefModExtension>() &&
                                                                                          modSettings.TechLevelsWithTechprints[p.techLevel]).ToList();
            foreach (ResearchProjectDef projectDef in projectsRequiringTechprints)
            {
                ResearchProjectDefProcessor.ProcessGeneratedTechprint(projectDef, factionTagsByTechLevel[projectDef.techLevel], modSettings);
            }
            Log.Message($"ConfigurableTechprintsMod :: Processed {projectsRequiringTechprints.Count} ResearchProjectDefs with default settings.");
        }
    }
}