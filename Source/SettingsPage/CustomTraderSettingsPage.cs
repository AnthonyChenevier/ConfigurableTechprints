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

namespace ConfigurableTechprints.SettingsPage
{
    internal class CustomTraderSettingsPage: ConfigurableTechprintsSettingPage
    {
        private float _contentHeight = float.MaxValue;
        private Vector2 _scrollPosition = Vector2.zero;

        public override void DoPage(Listing_Standard list, Rect inRect)
        {
            list.Label("<color=red>NOTE: Customizing a trader bypasses the stock count multiplier for that trader. Default values shown have had the last-loaded multiplier applied</color>");
            list.GapLine();
            Rect viewRect = list.GetRect(inRect.height - list.CurHeight);
            Listing_Standard scrollList = list.BeginScrollView(viewRect, _contentHeight, ref _scrollPosition);
            
            scrollList.Gap();
            float viewHeight = 0f;
            foreach (TraderKindDef traderKindDef in DefDatabase<TraderKindDef>.AllDefs)
            {
                string[] countBuffers = new string[DefDatabase<TraderKindDef>.DefCount];
                string[] chanceBuffers = new string[DefDatabase<TraderKindDef>.DefCount];

                TraderData customTrader;
                bool originalValuesChanged = false;
                if (settings.CustomTraders.ContainsKey(traderKindDef.defName))
                {
                    customTrader = settings.CustomTraders[traderKindDef.defName];
                }
                else
                {
                    customTrader = new TraderData();
                    List<StockGenerator_Techprints> stockGenerators = traderKindDef.stockGenerators.OfType<StockGenerator_Techprints>()
                                                                                   .ToList();
                    if (stockGenerators.Any())
                    {
                        if (stockGenerators.Count > 1)
                            Log.Error($"ConfigurableTechprintsMod :: Found more than one StockGenerator_Techprints on TraderKindDef({traderKindDef.defName}). Only showing the first entry found.");
                        customTrader.CountChances = stockGenerators.First()
                                                                   .GetPrivateCountChances();
                    }
                    else
                    {
                        customTrader.CountChances = new List<CountChance>();
                    }
                }

                float sectionHeight = (12f + 24f + 24f) * customTrader.CountChances.Count + //gap line, count and chance per entry
                                      26f + //heading
                                      30f; //button
                Listing_Standard section = scrollList.BeginSection(sectionHeight);
                section.Label($"<b>Trader Type ({traderKindDef.defName}), '{traderKindDef.label}'</b>");
                if (customTrader.CountChances.Count > 0)
                {
                    float chanceSum = 0f;
                    for (int i = 0; i < customTrader.CountChances.Count; i++)
                    {
                        CountChance currentCountChance = customTrader.CountChances[i];
                        int count = currentCountChance.count;
                        float chance = currentCountChance.chance * 100f;
                        //do UI
                        section.LabeledSliderWithTextField("Count:", ref count, ref countBuffers[i], 1, 99, tooltip:"The number of techprints to generate");
                        section.LabeledSliderWithTextField("Chance of using count (%):", ref chance, ref chanceBuffers[i], 0.1f, 100f,
                                                           tooltip:"The chance this count will be used in generation");
                        section.GapLine();
                        //make sure the chance total isn't over 1
                        chance /= 100f;
                        chanceSum += chance;
                        if (chanceSum > 1f)
                        {
                            float diff = chanceSum - 1f;
                            chance -= diff;
                            chanceSum -= diff;
                        }

                        if (chance == currentCountChance.chance && count == currentCountChance.count)
                            continue;

                        customTrader.CountChances[i] = new CountChance { chance = chance, count = count };
                        originalValuesChanged = true;
                    }
                }

                if (section.ButtonText("Add CountChance"))
                {
                    customTrader.CountChances.Add(new CountChance());
                    originalValuesChanged = true;
                }

                if (originalValuesChanged)
                {
                    settings.CustomTraders[traderKindDef.defName] = customTrader;
                }

                scrollList.EndSection(section);
                scrollList.Gap();

                viewHeight += sectionHeight + 20f; //add gapline and section border height
            }
            
            if (_contentHeight != viewHeight)
                _contentHeight = viewHeight;
            list.EndScrollView(scrollList);

        }
    }
}
