// TraderProcessor.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/08 9:05 PM
// Last edited by: Anthony Chenevier on 2022/10/08 9:05 PM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.DefProcessors
{
    public static class TraderKindDefProcessor
    {
        //helper methods for accessing private StockGenerator_Techprints.countChance field
        public static void SetPrivateCountChances(this StockGenerator_Techprints stockGenerator, List<CountChance> countChances) => Traverse.Create(stockGenerator).Field("countChances").SetValue(countChances);
        public static List<CountChance> GetPrivateCountChances(this StockGenerator_Techprints stockGenerator) => Traverse.Create(stockGenerator).Field("countChances").GetValue<List<CountChance>>();



        public static void ProcessCustomTrader(TraderKindDef trader, TraderData customData)
        { 
            //Create our new stock generator and set it's count chances from the mod settings
            //using harmony access tools and add it to the trader's def
            string report;
            StockGenerator_Techprints stockGenerator = trader.stockGenerators.OfType<StockGenerator_Techprints>().FirstOrDefault();
            if (stockGenerator == null)
            {
                StockGenerator_Techprints newStockGenerator = new StockGenerator_Techprints();
                newStockGenerator.SetPrivateCountChances(customData.CountChances);
                trader.stockGenerators.Add(newStockGenerator);
                report = $"\n\tAdded new StockGenerator_Techprints with new CountChance values: " +
                         $"\n\tNew values: {string.Join(", ", customData.CountChances.Select(c => $"{c.count}x {c.chance * 100}%"))}";
            }
            else
            {
                string oldChances = string.Join(", ", stockGenerator.GetPrivateCountChances().Select(c => $"{c.count}x {c.chance * 100}%"));

                stockGenerator.SetPrivateCountChances(customData.CountChances);

                string newChances = string.Join(", ", customData.CountChances.Select(c => $"{c.count}x {c.chance * 100}%"));
                report = $"\n\tOverrode StockGenerator_Techprints with custom CountChance values:" +
                         $"\n\tOld values: {oldChances}" +
                         $"\n\tNew values: {newChances}";
            }


            Log.Message($"ConfigurableTechprintsMod :: Processing report for custom trader ({trader.defName}) modifications:{report}");
        }



        public static void ProcessTechprintTrader(TraderKindDef trader, float traderStockCountMultiplier, float extraProjectCountMultiplier)
        {
            StockGenerator_Techprints stockGenerator = trader.stockGenerators.OfType<StockGenerator_Techprints>().First();
            List<CountChance> countChances = stockGenerator.GetPrivateCountChances();

            string oldChances = string.Join(", ", countChances.Select(c => $"{c.count}x {c.chance * 100}%"));
            //apply our multipliers. Won't be allowed to go below native values.
            countChances = countChances.Select(cc => new CountChance { chance = cc.chance, count = Mathf.CeilToInt(cc.count * extraProjectCountMultiplier * traderStockCountMultiplier) }).ToList();

            stockGenerator.SetPrivateCountChances(countChances);

            string newChances = string.Join(", ", countChances.Select(c => $"{c.count}x {c.chance * 100}%"));


            Log.Message($"ConfigurableTechprintsMod :: Processing report for techprint trader ({trader.defName}) modifications:" +
                      $"\n\tEdited trader count chances:" +
                      $"\n\tOld countChances ({oldChances})" +
                      $"\n\tNew countChances ({newChances})");
        }
    }
}
