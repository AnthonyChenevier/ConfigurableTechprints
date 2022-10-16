﻿// ConfigurableTechprintsMod.cs
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
using ConfigurableTechprints.SettingsPage;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints;

//use this class for stuff that requires defs to be loaded.
//[StaticConstructorOnStartup]
//public static class AfterLoadUtility
//{
//    static AfterLoadUtility()
//    {
//        Log.Message("ConfigurableTechprintsMod :: Static Constructor Called...");
//    }
//}

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

    //Settings window stuff
    private bool _restartRequired = true; //try to make this dynamically react to changes from last saved settings, eventually
    private List<TabRecord> _tabs;
    private Tab _currentTab;

    private enum Tab
    {
        GeneralTechprintSettings,
        CustomTechprintSettings,
        GeneralTraderSettings,
        CustomTraderSettings
    }

    //backups of default values for traders and techprints accessible by defname
    //used by settings pages for recognition and resetting of default values.
    private Dictionary<string, TraderData> _traderBackup;
    private Dictionary<string, TechprintData> _techprintBackup;
    public Dictionary<string, string> Reports { get; }

    private readonly GeneralTechprintSettingsPage generalTechprintSettingsPage;
    private readonly CustomTechprintSettingsPage customTechprintSettingsPage;
    private readonly GeneralTraderSettingsPage generalTraderSettingsPage;
    private readonly CustomTraderSettingsPage customTraderSettingsPage;

    public ConfigurableTechprintsSettings Settings { get; }

    public ConfigurableTechprintsMod(ModContentPack content) : base(content)
    {
        //requires royalty
        if (!ModLister.RoyaltyInstalled)
            throw new Exception("ConfigurableTechprintsMod: ERROR Royalty DLC required and not found");

        Instance = this;

        Settings = GetSettings<ConfigurableTechprintsSettings>();
        Reports = new Dictionary<string, string>();
        generalTechprintSettingsPage = new GeneralTechprintSettingsPage();
        customTechprintSettingsPage = new CustomTechprintSettingsPage();
        generalTraderSettingsPage = new GeneralTraderSettingsPage();
        customTraderSettingsPage = new CustomTraderSettingsPage();

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

    // not sure if this method is necessary, but nice to have in the toolbox if I need to implement it
    public static void RestartFromChangedModSettings()
    {
        Find.WindowStack.Add(new Dialog_MessageBox("ModSettingsChanged_Message".Translate(), "OK".Translate(), GenCommandLine.Restart, "Later".Translate()));
    }

    public static void DisplayReport(string key, string report)
    {
        Find.WindowStack.Add(new Dialog_MessageBox($"{"ReportButtonDialog_Message".Translate()}:{report}", title: $"{key}"));
        Log.Message($"{key}:{report}");
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        _currentTab = Tab.GeneralTechprintSettings;
        _tabs = new List<TabRecord>
        {
            new("GeneralTechprintSettings_Tab".Translate(), () => _currentTab = Tab.GeneralTechprintSettings, () => _currentTab == Tab.GeneralTechprintSettings),
            new("CustomTechprintsSettings_Tab".Translate(), () => _currentTab = Tab.CustomTechprintSettings, () => _currentTab == Tab.CustomTechprintSettings),
            new("GeneralTraderSettings_Tab".Translate(), () => _currentTab = Tab.GeneralTraderSettings, () => _currentTab == Tab.GeneralTraderSettings),
            new("CustomTradersSettings_Tab".Translate(), () => _currentTab = Tab.CustomTraderSettings, () => _currentTab == Tab.CustomTraderSettings)
        };

        Listing_Standard main = new();
        main.Begin(inRect);

        DoSettingsHeader(main, inRect);
        //reserve remaining space for content in main listing
        Rect contentRect = main.GetRect(inRect.height - main.CurHeight);
        contentRect.yMin += TabDrawer.TabHeight; //shift down by tab size

        //start content listing. we reduced it's space at the top
        //to give tabs a position to spawn. We draw the tabs later so their
        //graphics overlay the section box and merge nicely
        Listing_Standard content = new();
        content.Begin(contentRect);
        Rect contentRectInner = contentRect.ContractedBy(4f);
        Listing_Standard tabSection = content.BeginSection(contentRectInner.height);
        tabSection.Gap();
        switch (_currentTab)
        {
            default:
            case Tab.GeneralTechprintSettings:
                generalTechprintSettingsPage.Draw(tabSection, contentRectInner);
                break;
            case Tab.CustomTechprintSettings:
                customTechprintSettingsPage.Draw(tabSection, contentRectInner);
                break;
            case Tab.GeneralTraderSettings:
                generalTraderSettingsPage.Draw(tabSection, contentRectInner);
                break;
            case Tab.CustomTraderSettings:
                customTraderSettingsPage.Draw(tabSection, contentRectInner);
                break;
        }

        content.EndSection(tabSection);
        content.End();
        //draw tabs now after content to blend the top
        TabDrawer.DrawTabs(contentRect, _tabs, 400f);
        main.End();
    }

    private void DoSettingsHeader(Listing_Standard list, Rect inRect)
    {
        //Banner image, settings notice and reset button get their own section
        float headerHeight = Text.LineHeight * 3;
        Rect headerContentRect = list.GetRect(headerHeight);
        if (_restartRequired)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(headerContentRect.LeftPart(0.7f), $"<color=red>{"RestartRequired_Note".Translate()}</color>");
            Text.Anchor = anchor;
        }

        Rect rectR = headerContentRect.RightPart(0.3f);
        if (Widgets.ButtonText(rectR.TopHalf(), "ResetAll_Button".Translate()))
            Settings.SetDefaults();

        TooltipHandler.TipRegion(rectR.TopHalf(), "ResetAll_Tooltip".Translate());

        if (Widgets.ButtonText(rectR.BottomHalf(), "ViewReport_Button".Translate()))
            FloatMenuUtility.MakeMenu(Reports, p => p.Key, p => () => DisplayReport(p.Key, p.Value));

        TooltipHandler.TipRegion(rectR.BottomHalf(), "ViewReport_Tooltip".Translate());
        list.GapLine();
    }
}
