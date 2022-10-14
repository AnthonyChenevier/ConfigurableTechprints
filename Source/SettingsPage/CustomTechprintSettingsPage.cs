using ConfigurableTechprints.DataTypes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage
{
    internal class CustomTechprintSettingsPage: ConfigurableTechprintsSettingPage
    {
        private float _contentHeight = float.MaxValue;
        private Vector2 _scrollPosition = Vector2.zero;

        public override void DoPage(Listing_Standard list, Rect inRect)
        {
            list.Label("<color=red>NOTE: Customizing a research project's techprint settings overrides all multipliers defined above.</color>");
            list.GapLine();

            Rect viewRect = list.GetRect(inRect.height - list.CurHeight);
            Listing_Standard scrollList = list.BeginScrollView(viewRect, _contentHeight, ref _scrollPosition);

            scrollList.Gap();
            IEnumerable<ResearchProjectDef> projectDefs = DefDatabase<ResearchProjectDef>.AllDefs;
            float viewHeight = 0f;
            foreach (ResearchProjectDef projectDef in projectDefs)
            {
                string projectDefName = projectDef.defName;
                TechprintData customTechprint;

                string countBuffer = null;
                string costBuffer = null;
                string priceBuffer = null;
                string commonalityBuffer = null;

                bool originalValuesChanged = false;
                if (settings.CustomTechprints.ContainsKey(projectDefName))
                {
                    customTechprint = settings.CustomTechprints[projectDefName];
                }
                else
                {
                    //build defaults from current values
                    customTechprint = new TechprintData
                    {
                        techprintCount = projectDef.techprintCount,
                        baseCost = projectDef.baseCost,
                        techprintCommonality = projectDef.techprintCommonality,
                        techprintMarketValue = projectDef.techprintMarketValue,
                        heldByFactionCategoryTags = projectDef.heldByFactionCategoryTags ?? new List<string>()
                    };
                }

                List<FactionDef> factionDefs = DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer && !f.hidden && !f.permanentEnemy)
                                                                      .ToList();
                factionDefs.SortBy(def => (int)def.techLevel);
                List<string> categoryTags = factionDefs.Select(f => f.categoryTag)
                                                       .Distinct()
                                                       .ToList();

                float sectionHeight = (12f + 24f + 24f + 24f + 24f) + //gap line, techprint count, base cost, market value and commonality
                                               (categoryTags.Count * 24f) + //however many factions we need tickboxes for
                                               26f; //heading
                Listing_Standard techprintSection = scrollList.BeginSection(sectionHeight);
                techprintSection.Label($"<b>Research Project ({projectDefName}), '{projectDef.label}'</b>");
                //do UI
                int oldTechprintCount = customTechprint.techprintCount;
                float oldBaseCost = customTechprint.baseCost;
                float oldMarketValue = customTechprint.techprintMarketValue;
                float oldCommonality = customTechprint.techprintCommonality;
                techprintSection.LabeledSliderWithTextField("Techprint Count:", ref customTechprint.techprintCount, ref countBuffer, 0, 10);
                techprintSection.TextFieldNumericLabeled("Research Base Cost:", ref customTechprint.baseCost, ref costBuffer, 1, 10000000);
                techprintSection.TextFieldNumericLabeled("Techprint Market Value:", ref customTechprint.techprintMarketValue, ref priceBuffer, 1, 10000000);
                techprintSection.TextFieldNumericLabeled("Techprint Commonality:", ref customTechprint.techprintCommonality, ref commonalityBuffer, 0, 100);
                originalValuesChanged = oldTechprintCount != customTechprint.techprintCount ||
                                        oldBaseCost != customTechprint.baseCost ||
                                        oldMarketValue != customTechprint.techprintMarketValue ||
                                        oldCommonality != customTechprint.techprintCommonality;
                techprintSection.GapLine();

                foreach (string categoryTag in categoryTags)
                {
                    bool val = customTechprint.heldByFactionCategoryTags.Contains(categoryTag);
                    techprintSection.CheckboxLabeled($"Enable faction tag ({categoryTag}):", ref val);
                    if (val)
                    {
                        if (customTechprint.heldByFactionCategoryTags.Contains(categoryTag))
                            continue;

                        customTechprint.heldByFactionCategoryTags.Add(categoryTag);
                        originalValuesChanged = true;
                    }
                    else
                    {
                        if (!customTechprint.heldByFactionCategoryTags.Contains(categoryTag))
                            continue;

                        customTechprint.heldByFactionCategoryTags.Remove(categoryTag);
                        originalValuesChanged = true;
                    }
                }

                if (originalValuesChanged)
                {
                    settings.CustomTechprints[projectDefName] = customTechprint;
                }

                scrollList.EndSection(techprintSection);
                scrollList.Gap();

                viewHeight += sectionHeight + 20f;
            }

            if (_contentHeight != viewHeight)
                _contentHeight = viewHeight;
            list.EndScrollView(scrollList);
        }
    }
}
