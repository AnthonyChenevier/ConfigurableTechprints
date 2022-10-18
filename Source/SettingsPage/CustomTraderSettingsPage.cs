// CustomTraderSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:36 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:36 PM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using ConfigurableTechprints.DefProcessors;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal class CustomTraderSettingsPage : ConfigurableTechprintsSettingPage
{
    protected override void DoPage(Listing_Standard list, Rect inRect)
    {
        //cache values at beginning of each frame as they may change from input
        List<TraderKindDef> traderKindDefs = DefDatabase<TraderKindDef>.AllDefs.Where(t => !settings.IgnoredTraders.Contains(t.defName)).ToList();
        IEnumerable<TraderKindDef> uncustomizedTraders = traderKindDefs.Where(p => !settings.CustomTraders.ContainsKey(p.defName));

        list.Label($"<color=red>{"CustomTraders_Note".Translate()}</color>");
        list.GapLine();
        List<string> deleteKeys = new();
        Dictionary<string, TraderData> newCustomTraders = new();
        foreach ((string traderDefName, TraderData traderData) in settings.CustomTraders)
            if (!DoCustomTraderSection(list, out TraderData? newTraderData, traderData, traderKindDefs.First(t => t.defName == traderDefName)))
                deleteKeys.Add(traderDefName);
            else if (newTraderData != null)
                newCustomTraders[traderDefName] = (TraderData)newTraderData;

        //save changes
        settings.CustomTraders.RemoveAll(t => deleteKeys.Contains(t.Key));
        foreach ((string traderDefName, TraderData traderData) in newCustomTraders)
            if (settings.CustomTraders.ContainsKey(traderDefName))
                settings.CustomTraders[traderDefName] = traderData;
            else
                settings.CustomTraders.Add(traderDefName, traderData);

        if (list.ButtonText($"<b>+ {"CustomTradersAddNew_Button".Translate()} +</b>"))
            FloatMenuUtility.MakeMenu(uncustomizedTraders, p => $"{p.defName} - '{p.LabelCap}'", p => () => AddNewCustomEntry(p));
    }

    private void AddNewCustomEntry(TraderKindDef traderDef)
    {
        TraderData customTrader = new();
        List<StockGenerator_Techprints> stockGenerators = traderDef.stockGenerators.OfType<StockGenerator_Techprints>().ToList();
        if (stockGenerators.Any())
        {
            if (stockGenerators.Count > 1)
                Log.Error($"ConfigurableTechprintsMod :: Found more than one StockGenerator_Techprints on TraderKindDef({traderDef.defName}). Only showing the first entry found.");

            customTrader.CountChances = stockGenerators.First().GetCountChances();
        }
        else
        {
            customTrader.CountChances = new List<CountChance>();
        }

        settings.CustomTraders[traderDef.defName] = customTrader;
    }

    private bool DoCustomTraderSection(Listing_Standard list, out TraderData? traderDataOut, TraderData traderDataIn, TraderKindDef trader)
    {
        traderDataOut = null;
        string[] countBuffers = new string[DefDatabase<TraderKindDef>.DefCount];
        string[] chanceBuffers = new string[DefDatabase<TraderKindDef>.DefCount];


        //do UI
        //heading and button + gap line, count and chance per entry
        float sectionHeight = 66f + 60f * traderDataIn.CountChances.Count;
        Listing_Standard section = list.BeginSection(sectionHeight);

        bool originalValuesChanged = false;
        string traderDefName = trader.defName;

        bool removeThis = false;
        section.CheckboxLabeled($"<b>{traderDefName}: '{trader.LabelCap}'</b>", ref removeThis, "DeleteCustomSettings_Tooltip".Translate(), true);
        if (removeThis)
            return false;

        section.GapLine();
        if (traderDataIn.CountChances.Count > 0)
        {
            float chanceSum = 0f;
            float maxSingleChance = 100f - (traderDataIn.CountChances.Count - 1);
            for (int i = 0; i < traderDataIn.CountChances.Count; i++)
            {
                CountChance currentCountChance = traderDataIn.CountChances[i];
                int count = currentCountChance.count;
                float chance = currentCountChance.chance * 100f;
                section.LabeledSliderWithTextField("CustomTraderCount_Label".Translate(), ref count, ref countBuffers[i], 1, 99);
                section.LabeledSliderWithTextField("CustomTraderChance_Label".Translate(),
                                                   ref chance,
                                                   ref chanceBuffers[i],
                                                   0f,
                                                   100f,
                                                   tooltip: "CustomTraderChance_Tooltip".Translate());

                section.GapLine();
                //make sure the chance is within acceptable range
                chance = Mathf.Clamp(Mathf.Ceil(chance), 0, maxSingleChance); //clamp between 1 and max single chance
                float maxForThis = Mathf.Max(0f, 100f - chanceSum); //the new maximum for this entry is whatever is left in the chance sum
                if (chance > maxForThis)
                    chance = maxForThis;

                chanceSum += chance;
                chance /= 100f; //normalize to 0-1
                if (chance == currentCountChance.chance && count == currentCountChance.count)
                    continue;

                traderDataIn.CountChances[i] = new CountChance
                {
                    chance = chance,
                    count = count
                };

                originalValuesChanged = true;
            }
        }

        Rect buttonRect = section.GetRect(30f);
        if (Widgets.ButtonText(buttonRect.LeftHalf().RightHalf(), "CustomTradersAddCountChance_Button".Translate()))
        {
            traderDataIn.CountChances.Add(new CountChance());
            originalValuesChanged = true;
        }

        if (Widgets.ButtonText(buttonRect.RightHalf().LeftHalf(), "CustomTradersRemoveLastCountChance_Button".Translate()))
        {
            traderDataIn.CountChances.RemoveAt(traderDataIn.CountChances.Count - 1);
            originalValuesChanged = true;
        }

        list.EndSection(section);
        list.Gap();

        if (originalValuesChanged)
            traderDataOut = traderDataIn;

        return true;
    }
}
