// TechprintData.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/12 5:25 PM
// Last edited by: Anthony Chenevier on 2022/10/12 5:25 PM


using System.Collections.Generic;
using Verse;

namespace ConfigurableTechprints.DataTypes;

public struct TechprintData : IExposable
{
    public int techprintCount;
    public float baseCost;
    public float techprintMarketValue;
    public float techprintCommonality;
    public List<string> heldByFactionCategoryTags;

    public void ExposeData()
    {
        Scribe_Values.Look(ref techprintCount, nameof(techprintCount));
        Scribe_Values.Look(ref baseCost, nameof(baseCost));
        Scribe_Values.Look(ref techprintMarketValue, nameof(techprintMarketValue));
        Scribe_Values.Look(ref techprintCommonality, nameof(techprintCommonality));
        Scribe_Collections.Look(ref heldByFactionCategoryTags, nameof(heldByFactionCategoryTags), LookMode.Value);
    }
}
