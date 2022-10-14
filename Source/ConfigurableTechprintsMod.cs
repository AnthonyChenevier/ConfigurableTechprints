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
using ConfigurableTechprints.SettingsPage;
using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;

namespace ConfigurableTechprints
{

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
            get {
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
            CustomTraderSettings,
        }

        //backups of default values for traders and techprints accessible by defname
        //used by settings pages for recognition and resetting of default values.
        private Dictionary<string, TraderData> _traderBackup;
        private Dictionary<string, TechprintData> _techprintBackup;

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
            generalTechprintSettingsPage = new GeneralTechprintSettingsPage();
            customTechprintSettingsPage = new CustomTechprintSettingsPage();
            generalTraderSettingsPage = new GeneralTraderSettingsPage();
            customTraderSettingsPage = new CustomTraderSettingsPage();
            //settings window stuff
            _currentTab = Tab.GeneralTechprintSettings;
            _tabs = new List<TabRecord>();

            //Harmony.DEBUG = true;
            Harmony harmony = new Harmony("makeitso.configurabletechprints");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        public void BackupTraderData()
        {
            _traderBackup = new Dictionary<string, TraderData>();
            foreach (TraderKindDef traderKindDef in DefDatabase<TraderKindDef>.AllDefs)
            {
                StockGenerator_Techprints stockGenerator = traderKindDef.stockGenerators.OfType<StockGenerator_Techprints>().FirstOrDefault();

                TraderData traderData = new TraderData { CountChances = new List<CountChance>(), HasTechprintGeneratorNatively = false };

                if (stockGenerator != null)
                {
                    traderData.CountChances = stockGenerator.GetPrivateCountChances();
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
                TechprintData techprintData = new TechprintData
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

        public override string SettingsCategory()
        {
            return "ConfigurableTechprintsModName".Translate();
        }
        public static void RestartFromChangedModSettings() => Find.WindowStack.Add(new Dialog_MessageBox("ModSettingsChanged".Translate(), "OK".Translate(), () => GenCommandLine.Restart(), "Cancel".Translate()));

        public override void DoSettingsWindowContents(Rect inRect)
        {
            _tabs.Clear();
            _tabs.Add(new TabRecord("Gen. Techprint Settings", () => _currentTab = Tab.GeneralTechprintSettings, () => _currentTab == Tab.GeneralTechprintSettings));
            _tabs.Add(new TabRecord("Custom Techprints", () => _currentTab = Tab.CustomTechprintSettings, () => _currentTab == Tab.CustomTechprintSettings));
            _tabs.Add(new TabRecord("Gen. Trader Settings", () => _currentTab = Tab.GeneralTraderSettings, () => _currentTab == Tab.GeneralTraderSettings));
            _tabs.Add(new TabRecord("Custom Traders", () => _currentTab = Tab.CustomTraderSettings, () => _currentTab == Tab.CustomTraderSettings));

            Listing_Standard main = new Listing_Standard();
            main.Begin(inRect);

            DoSettingsHeader(main, inRect);
            //reserve remaining space for content in main listing
            Rect contentRect = main.GetRect(inRect.height - main.CurHeight);
            contentRect.yMin += TabDrawer.TabHeight; //shift down by tab size

            //start content listing. we reduced it's space at the top
            //to give tabs a position to spawn. We draw the tabs later so their
            //graphics overlay the section box and merge nicely
            Listing_Standard content = new Listing_Standard();
            content.Begin(contentRect);
            Rect contentRectInner = contentRect.ContractedBy(4f);
            Listing_Standard tabSection = content.BeginSection(contentRectInner.height);
            tabSection.Gap();
            switch (_currentTab)
            {
                default:
                case Tab.GeneralTechprintSettings:
                    generalTechprintSettingsPage.DoPage(tabSection, contentRectInner);
                    break;
                case Tab.CustomTechprintSettings:
                    customTechprintSettingsPage.DoPage(tabSection, contentRectInner);
                    break;
                case Tab.GeneralTraderSettings:
                    generalTraderSettingsPage.DoPage(tabSection, contentRectInner);
                    break;
                case Tab.CustomTraderSettings:
                    customTraderSettingsPage.DoPage(tabSection, contentRectInner);
                    break;
            }
            content.EndSection(tabSection);
            content.End();
            //draw tabs now after content to blend
            TabDrawer.DrawTabs(contentRect, _tabs, 400f);
            main.End();
        }

        private void DoSettingsHeader(Listing_Standard list, Rect inRect)
        {
            //Banner image, settings notice and reset button get their own section
            float headerHeight = Text.LineHeight * 2;
            Rect headerContentRect = list.GetRect(headerHeight);
            if (_restartRequired)
            {
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(headerContentRect.LeftPart(0.7f), "<color=red>NOTE: Game restart required for changes to take effect.</color>");
                Text.Anchor = anchor;
            }

            Rect rectR = headerContentRect.RightPart(0.3f).ContractedBy(8f);
            if (Widgets.ButtonText(rectR, "Reset to Default"))
                Settings.SetDefaults();
            TooltipHandler.TipRegion(rectR, "Reset all settings to the mod's default values.");
        }

        //public void ShowQuestGenerationTab(Listing_Standard list, Rect listRect)
        //{
        //    string stsStringBuffer = null;
        //    list.CheckboxLabeled("Generate Techprint Quest", ref Settings.GenerateTechprintSite, "Check to periodically create quests for techprints");
        //    if (Settings.GenerateTechprintSite)
        //    {
        //        list.LabeledSliderWithTextField("TechPrint Outpost Threat Scaling", ref Settings.SiteThreatScale, ref stsStringBuffer, 0f, 1000f, "Sets the threat scale for Techprint Outpost.");
        //        list.Label("TechPrint Outpost Generation Period", -1f, "Decide how many days you want to generate techprint outpost.");
        //        list.IntAdjuster(ref Settings.SiteCycle, 1);
        //        list.Label("TechPrint Outpost Generation Range", -1f, "Will vary the generation period up to this many days");
        //        list.IntAdjuster(ref Settings.SiteCycleRandom, 1);
        //    }
        //}
    }
}
