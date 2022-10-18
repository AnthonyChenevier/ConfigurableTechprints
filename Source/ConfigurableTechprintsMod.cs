// ConfigurableTechprintsMod.cs
// 
// Part of ConfigurableTechprints
// The point of this mod is to add a highly-customizable way to add techprints to any research, using
// the vanilla techprint mechanics added in the Royalty DLC as much as possible to reduce overhead.
// The heart of the mod is the settings page, allowing users to:
// - customize all research project's techprint requirements and base cost.
// - customize each traders's techprint stock counts, and specific techprint prices and commonalities
// 
// Created by: Anthony Chenevier on // 
// Last edited by: Anthony Chenevier on 2022/09/30 10:05 PM


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurableTechprints.DataTypes;
using ConfigurableTechprints.DefProcessors;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints;

//This class holds settings for the mod and is constructed early, before defs are loaded.
//Therefore it can't do anything that requires defs to be loaded already. This is where Harmony
//should be initialized if modifications to def generation are required so that changes to the
//code are actually applied.
public class ConfigurableTechprintsMod : Mod
{
    private static ConfigurableTechprintsMod _instance;

    public static ConfigurableTechprintsMod Instance
    {
        get
        {
            if (_instance == null)
                throw new Exception("ConfigurableTechprintsMod :: ERROR Static class instance not found");

            return _instance;
        }
        private set
        {
            if (_instance != null)
                throw new Exception("ConfigurableTechprintsMod :: ERROR Static class instance already exists on ctor");

            _instance = value;
        }
    }


    public ConfigurableTechprintsSettingsData Settings { get; }

    //TODO:backups of default values for traders and techprints accessible by defname
    //used by settings pages for recognition and resetting of default values.
    private Dictionary<string, TraderData> _traderBackup;
    private Dictionary<string, TechprintData> _techprintBackup;
    public Dictionary<string, string> Reports { get; }

    public ConfigurableTechprintsMod(ModContentPack content) : base(content)
    {
        //requires royalty
        if (!ModLister.RoyaltyInstalled)
            throw new Exception("ConfigurableTechprintsMod: ERROR Royalty DLC required and not found");

        Instance = this;

        Settings = GetSettings<ConfigurableTechprintsSettingsData>();
        Reports = new Dictionary<string, string>();

        //Harmony.DEBUG = true;
        Harmony harmony = new("makeitso.configurabletechprints");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }


    public void BackupTraderData()
    {
        _traderBackup = new Dictionary<string, TraderData>();
        foreach (TraderKindDef traderKindDef in DefDatabase<TraderKindDef>.AllDefs)
        {
            StockGenerator_Techprints stockGenerator = traderKindDef.stockGenerators.OfType<StockGenerator_Techprints>().FirstOrDefault();

            TraderData traderData = new()
            {
                CountChances = new List<CountChance>(), HasTechprintGeneratorNatively = false
            };

            if (stockGenerator != null)
            {
                traderData.CountChances = stockGenerator.GetCountChances();
                traderData.HasTechprintGeneratorNatively = true;
            }

            _traderBackup[traderKindDef.defName] = traderData;
        }
    }

    public void BackupTechprintData()
    {
        _techprintBackup = new Dictionary<string, TechprintData>();
        foreach (ResearchProjectDef projectDef in DefDatabase<ResearchProjectDef>.AllDefs)
        {
            TechprintData techprintData = new()
            {
                techprintCount = projectDef.techprintCount,
                baseCost = projectDef.baseCost,
                techprintMarketValue = projectDef.techprintMarketValue,
                techprintCommonality = projectDef.techprintCommonality,
                heldByFactionCategoryTags = projectDef.heldByFactionCategoryTags
            };

            _techprintBackup[projectDef.defName] = techprintData;
        }
    }

    public override string SettingsCategory() { return "ConfigurableTechprintsModName".Translate(); }

    //pass through to static class 
    public override void DoSettingsWindowContents(Rect inRect) { ConfigurableTechprintsSettingsScreen.Draw(inRect); }
}
