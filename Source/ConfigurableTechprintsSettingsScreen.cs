// CustomTechprintSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/17 5:47 PM
// Last edited by: Anthony Chenevier on 2022/10/17 5:47 PM


using System.Collections.Generic;
using ConfigurableTechprints.SettingsPage;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints;

//use this class for stuff that requires defs to be loaded. like a settings page that uses translations
[StaticConstructorOnStartup]
public static class ConfigurableTechprintsSettingsScreen
{
    private readonly static GeneralTechprintSettingsPage generalTechprintSettingsPage;
    private readonly static CustomTechprintSettingsPage customTechprintSettingsPage;
    private readonly static GeneralTraderSettingsPage generalTraderSettingsPage;
    private readonly static CustomTraderSettingsPage customTraderSettingsPage;
    private static List<TabRecord> _tabs;
    private static Tab _currentTab;

    private enum Tab
    {
        GeneralTechprintSettings,
        CustomTechprintSettings,
        GeneralTraderSettings,
        CustomTraderSettings
    }

    //Settings window stuff
    private static bool _restartRequired = true; //try to make this dynamically react to changes from last saved settings, eventually

    static ConfigurableTechprintsSettingsScreen()
    {
        _tabs = new List<TabRecord>
        {
            new("GeneralTechprintSettings_Tab".Translate(), () => _currentTab = Tab.GeneralTechprintSettings, () => _currentTab == Tab.GeneralTechprintSettings),
            new("CustomTechprintsSettings_Tab".Translate(), () => _currentTab = Tab.CustomTechprintSettings, () => _currentTab == Tab.CustomTechprintSettings),
            new("GeneralTraderSettings_Tab".Translate(), () => _currentTab = Tab.GeneralTraderSettings, () => _currentTab == Tab.GeneralTraderSettings),
            new("CustomTradersSettings_Tab".Translate(), () => _currentTab = Tab.CustomTraderSettings, () => _currentTab == Tab.CustomTraderSettings)
        };

        generalTechprintSettingsPage = new GeneralTechprintSettingsPage();
        customTechprintSettingsPage = new CustomTechprintSettingsPage();
        generalTraderSettingsPage = new GeneralTraderSettingsPage();
        customTraderSettingsPage = new CustomTraderSettingsPage();
        _currentTab = Tab.GeneralTechprintSettings;
    }

    public static void Draw(Rect inRect)
    {
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

        DrawRestartButton(inRect);
    }

    private static void DrawRestartButton(Rect inRect)
    {
        Rect buttonRect = new(inRect)
        {
            xMin = inRect.xMax - 250f,
            xMax = inRect.xMax,
            yMin = inRect.yMax + 2f, //go below inrect
            height = 40f
        };

        TooltipHandler.TipRegion(buttonRect, "RestartGame_Tooltip".Translate());
        if (Widgets.ButtonText(buttonRect, "RestartGame_Button".Translate()))
            RestartFromChangedModSettings();
    }

    private static void DoSettingsHeader(Listing_Standard list, Rect inRect)
    {
        //Banner image, settings notice and reset button get their own section
        float headerHeight = Text.LineHeight * 3;
        Rect headerContentRect = list.GetRect(headerHeight);
        if (_restartRequired)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(headerContentRect.LeftPart(0.7f), $"RestartRequired_Note".Translate().Colorize(ColorLibrary.RedReadable));
            Text.Anchor = anchor;
        }

        Rect rectR = headerContentRect.RightPart(0.3f);
        if (Widgets.ButtonText(rectR.TopHalf(), "ResetAll_Button".Translate()))
            ConfigurableTechprintsMod.Instance.Settings.SetDefaults();

        TooltipHandler.TipRegion(rectR.TopHalf(), "ResetAll_Tooltip".Translate());

        if (Widgets.ButtonText(rectR.BottomHalf(), "ViewReport_Button".Translate()))
            FloatMenuUtility.MakeMenu(ConfigurableTechprintsMod.Instance.Reports, p => p.Key, p => () => DisplayReport(p.Key, p.Value));

        TooltipHandler.TipRegion(rectR.BottomHalf(), "ViewReport_Tooltip".Translate());
        list.GapLine();
    }

    // easy restart hook
    public static void RestartFromChangedModSettings()
    {
        Find.WindowStack.Add(new Dialog_MessageBox("ModSettingsChanged_Message".Translate(),
                                                   "OK".Translate(),
                                                   () =>
                                                   {
                                                       ConfigurableTechprintsMod.Instance.WriteSettings();
                                                       GenCommandLine.Restart();
                                                   },
                                                   "Later".Translate()));
    }

    public static void DisplayReport(string key, string report)
    {
        Find.WindowStack.Add(new Dialog_MessageBox($"{"ReportButtonDialog_Message".Translate()}:{report}", title: key));
        Log.Message($"{key}:{report}");
    }
}
