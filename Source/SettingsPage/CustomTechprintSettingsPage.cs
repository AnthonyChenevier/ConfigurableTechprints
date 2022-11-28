// CustomTechprintSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:22 PM
// Last edited by: Anthony Chenevier on 2022/10/16 9:30 PM


using System.Collections.Generic;
using System.Linq;
using ConfigurableTechprints.DataTypes;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal class CustomTechprintSettingsPage : ConfigurableTechprintsSettingPage
{
    private readonly List<string> _categoryTags;

    public CustomTechprintSettingsPage()
    {
        List<FactionDef> factionDefs = DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer && !f.hidden && !f.permanentEnemy).ToList();
        factionDefs.SortBy(def => (int)def.techLevel);
        _categoryTags = factionDefs.Select(f => f.categoryTag).Distinct().ToList();
    }

    protected override void DoPage(Listing_Standard list, Rect inRect)
    {
        //cache some values once perframe
        IEnumerable<ResearchProjectDef> uncustomizedProjects = DefDatabase<ResearchProjectDef>.AllDefs.Where(p => !settings.CustomTechprints.ContainsKey(p.defName));

        list.Label($"<color=red>{"CustomTechprintSettings_Note".Translate()}</color>");
        list.GapLine();

        List<string> deleteKeys = new();
        Dictionary<string, TechprintData> newCustomTechprints = new();
        foreach (KeyValuePair<string, TechprintData> pair in settings.CustomTechprints)
            if (!DoCustomTechprintSection(list, out TechprintData? customTechprint, pair.Value, ResearchProjectDef.Named(pair.Key)))
                deleteKeys.Add(pair.Key);
            else if (customTechprint != null)
                newCustomTechprints[pair.Key] = (TechprintData)customTechprint;

        //update the list outside the foreach loop
        settings.CustomTechprints.RemoveAll(t => deleteKeys.Contains(t.Key));
        foreach (KeyValuePair<string, TechprintData> pair in newCustomTechprints)
            if (settings.CustomTechprints.ContainsKey(pair.Key))
                settings.CustomTechprints[pair.Key] = pair.Value;
            else
                settings.CustomTechprints.Add(pair.Key, pair.Value);

        if (!list.ButtonText($"<b>+ {"CustomTechprintAddNew_Button".Translate()} +</b>"))
            return;

        //do project selection menu
        Find.WindowStack.Add(new FloatMenu(uncustomizedProjects.Select(project => ConfigurableTechprintsMod.IsProjectNameMalformed(project.defName)
                                                                                      ? new FloatMenuOption(((string)project.LabelCap).Colorize(ColorLibrary.RedReadable),
                                                                                                            () => { },
                                                                                                            MenuOptionPriority.DisabledOption,
                                                                                                            rect => TooltipHandler.TipRegion(rect,
                                                                                                                "MalformedDefName_Notice".Translate(project.defName)
                                                                                                                    .Colorize(ColorLibrary.RedReadable)))
                                                                                      : new FloatMenuOption(project.LabelCap, () => AddNewCustomEntry(project))).ToList()));
    }

    private void AddNewCustomEntry(ResearchProjectDef projectDef)
    {
        //build defaults from current values
        settings.CustomTechprints[projectDef.defName] = new TechprintData
        {
            techprintCount = projectDef.techprintCount,
            baseCost = projectDef.baseCost,
            techprintCommonality = projectDef.techprintCommonality,
            techprintMarketValue = projectDef.techprintMarketValue,
            heldByFactionCategoryTags = projectDef.heldByFactionCategoryTags ?? new List<string>()
        };
    }

    private bool DoCustomTechprintSection(Listing_Standard list, out TechprintData? techprintDataOut, TechprintData techprintDataIn, ResearchProjectDef projectDef)
    {
        techprintDataOut = null;

        string countBuffer = null;
        string costBuffer = null;
        string priceBuffer = null;
        string commonalityBuffer = null;


        //do UI
        //pre-compute section height: heading, techprint count, base cost, market value,
        //commonality and gap line heights + however many factions we need tickboxes for
        float sectionHeight = 134f + _categoryTags.Count * 24f;
        Listing_Standard section = list.BeginSection(sectionHeight);

        bool removeThis = false;
        section.CheckboxLabeled($"<b>{projectDef.LabelCap}</b>", ref removeThis, "DeleteCustomSettings_Tooltip".Translate(), true);
        if (removeThis)
            return false;

        int oldTechprintCount = techprintDataIn.techprintCount;
        float oldBaseCost = techprintDataIn.baseCost;
        float oldMarketValue = techprintDataIn.techprintMarketValue;
        float oldCommonality = techprintDataIn.techprintCommonality;

        section.LabeledSliderWithTextField("TechprintCount_Label".Translate(), ref techprintDataIn.techprintCount, ref countBuffer, 0, 10);
        section.TextFieldNumericLabeled("ResearchBaseCost_Label".Translate(), ref techprintDataIn.baseCost, ref costBuffer, 1, 10000000);
        section.TextFieldNumericLabeled("TechprintMarketValue_Label".Translate(), ref techprintDataIn.techprintMarketValue, ref priceBuffer, 1, 10000000);
        section.TextFieldNumericLabeled("TechprintCommonality_Label".Translate(), ref techprintDataIn.techprintCommonality, ref commonalityBuffer, 0, 100);
        section.GapLine();

        bool categoriesChanged = false;
        foreach (string tag in _categoryTags)
        {
            bool tagEnabled = techprintDataIn.heldByFactionCategoryTags.Contains(tag);
            section.CheckboxLabeled($"{"EnableCategory_Label".Translate()} ({tag}):", ref tagEnabled);
            if (tagEnabled)
            {
                if (techprintDataIn.heldByFactionCategoryTags.Contains(tag))
                    continue;

                techprintDataIn.heldByFactionCategoryTags.Add(tag);
                categoriesChanged = true;
            }
            else
            {
                if (!techprintDataIn.heldByFactionCategoryTags.Contains(tag))
                    continue;

                techprintDataIn.heldByFactionCategoryTags.Remove(tag);
                categoriesChanged = true;
            }
        }

        list.EndSection(section);
        list.Gap();

        if (categoriesChanged ||
            oldTechprintCount != techprintDataIn.techprintCount ||
            oldBaseCost != techprintDataIn.baseCost ||
            oldMarketValue != techprintDataIn.techprintMarketValue ||
            oldCommonality != techprintDataIn.techprintCommonality)
            techprintDataOut = techprintDataIn; //changed settings, add to custom list

        return true;
    }
}
