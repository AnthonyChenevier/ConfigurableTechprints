// GeneralTraderSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:36 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:36 PM


using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal class GeneralTraderSettingsPage : ConfigurableTechprintsSettingPage
{
    private readonly List<TraderKindDef> _traderKindDefs;

    public GeneralTraderSettingsPage() { _traderKindDefs = DefDatabase<TraderKindDef>.AllDefs.Where(t => t.stockGenerators.Any(sg => sg is StockGenerator_Techprints)).ToList(); }

    protected override void DoPage(Listing_Standard list, Rect inRect)
    {
        string tsmStringBuffer = null;
        string tldStringBuffer = null;
        string tleStringBuffer = null;
        string ncmStringBuffer = null;
        list.Label("NativeCommonalityMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.NativeCommonalityMultiplier, ref ncmStringBuffer, 0.1f, 10f, tooltip: "NativeCommonalityMultiplier_Tooltip".Translate());
        list.Gap();
        list.Label("TechEqualCommonalityMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.TechEqualCommonalityMultiplier, ref tleStringBuffer, 1f, 10f, tooltip: "TechEqualCommonalityMultiplier_Tooltip".Translate());
        list.Gap();
        list.Label("TechDifferenceCommonalityMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.TechDifferenceCommonalityMultiplier,
                                 ref tldStringBuffer,
                                 0.01f,
                                 0.2f,
                                 tooltip: "TechDifferenceCommonalityMultiplier_Tooltip".Translate());

        list.Gap();
        list.Label("TraderStockCountMultiplier_Label".Translate());
        list.SliderWithTextField(ref settings.TraderStockCountMultiplier, ref tsmStringBuffer, 1f, 10f, tooltip: "TraderStockCountMultiplier_Tooltip".Translate());
        list.GapLine();
        list.Label($"<b>{"IgnoreTraders_Heading".Translate()}</b>");


        foreach (TraderKindDef trader in _traderKindDefs)
        {
            string traderDefName = trader.defName;
            bool ignoreThis = settings.IgnoredTraders.Contains(traderDefName);
            list.CheckboxLabeled($"{traderDefName}: '{trader.LabelCap}': ", ref ignoreThis, "IgnoredTrader_Tooltip".Translate());
            if (ignoreThis)
            {
                if (settings.IgnoredTraders.Contains(traderDefName))
                    continue;

                settings.IgnoredTraders.Add(traderDefName);
                //also remove from CustomTraders list if it exists
                if (settings.CustomTraders.ContainsKey(traderDefName))
                    settings.CustomTraders.Remove(traderDefName);
            }
            else
            {
                if (settings.IgnoredTraders.Contains(traderDefName))
                    settings.IgnoredTraders.Remove(traderDefName);
            }
        }
    }
}
