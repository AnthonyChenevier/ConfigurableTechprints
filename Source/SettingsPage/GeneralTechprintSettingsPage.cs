// GeneralTechprintSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:16 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:16 PM


using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage
{
    internal class GeneralTechprintSettingsPage : ConfigurableTechprintsSettingPage
    {
        private float _contentHeight = float.MaxValue;
        private Vector2 _scrollPosition = Vector2.zero;

        public override void DoPage(Listing_Standard list, Rect inRect)
        {
            string tprStringBuffer = null;
            string rbcStringBuffer = null;
            string mpmStringBuffer = null;
            string bcStringBuffer = null;
            string ncmStringBuffer = null;
            Rect viewRect = list.GetRect(inRect.height - list.CurHeight);
            Listing_Standard scrollList = list.BeginScrollView(viewRect, _contentHeight, ref _scrollPosition);

            scrollList.Label("<b>Automatically generate techprints for all research projects at these tech levels: </b>");
            scrollList.Label("Research projects at selected tech levels will have techprints automatically generated for them, using the settings below.");
            bool unlockNeolithic = settings.TechLevelsWithTechprints[TechLevel.Neolithic];
            scrollList.CheckboxLabeled("Neolithic:", ref unlockNeolithic);
            settings.TechLevelsWithTechprints[TechLevel.Neolithic] = unlockNeolithic;
            bool unlockMedieval = settings.TechLevelsWithTechprints[TechLevel.Medieval];
            scrollList.CheckboxLabeled("Medieval:", ref unlockMedieval);
            settings.TechLevelsWithTechprints[TechLevel.Medieval] = unlockMedieval;
            bool unlockIndustrial = settings.TechLevelsWithTechprints[TechLevel.Industrial];
            scrollList.CheckboxLabeled("Industrial:", ref unlockIndustrial);
            settings.TechLevelsWithTechprints[TechLevel.Industrial] = unlockIndustrial;
            bool unlockSpacer = settings.TechLevelsWithTechprints[TechLevel.Spacer];
            scrollList.CheckboxLabeled("Spacer:", ref unlockSpacer);
            settings.TechLevelsWithTechprints[TechLevel.Spacer] = unlockSpacer;
            bool unlockUltra = settings.TechLevelsWithTechprints[TechLevel.Ultra];
            scrollList.CheckboxLabeled("Ultratech:", ref unlockUltra);
            settings.TechLevelsWithTechprints[TechLevel.Ultra] = unlockUltra;
            bool unlockArcho = settings.TechLevelsWithTechprints[TechLevel.Archotech];
            scrollList.CheckboxLabeled("Archotech:", ref unlockArcho);
            settings.TechLevelsWithTechprints[TechLevel.Archotech] = unlockArcho;
            scrollList.GapLine();

            scrollList.Label("<b>Automatic techprint generation settings</b>");
            scrollList.Gap();
            scrollList.Label("Generate a techprint for every X research points:");
            scrollList.SliderWithTextField(ref settings.TechprintPerResearchPoints, ref tprStringBuffer, 100, 24000, 50,
                                           tooltip: "Use this setting to tweak the number of techprints each research will require. " +
                                                    "The <b>number of techprints</b> generated for a research project is completely determined by this value. " +
                                                    "For every multiple (or part thereof) of this value that the unmodified research costs, one extra techprint is generated and required. " +
                                                    "\n\n<i>Example: At the default value of 4000, if a project's base cost is between 0-3999 it will require 1 techprint; between 4000-7999 it will require 2; and so on</i>");
            scrollList.Gap();
            scrollList.Label("Market value final modifer:");
            scrollList.SliderWithTextField(ref settings.MarketPriceMultiplier, ref mpmStringBuffer, 0.01f, 10f,
                                           tooltip: "This modifier is used to tweak the <b>base market value</b> of generated techprints. " +
                                                    "The market value of a techprint is calculated for each project as (half base research cost / techprint count * this modifier)." +
                                                    "\n\n<i>Example: At the default value of 0.5, a techprint for a project with a research cost of 1000 and 1 techprint will cost (500 / 1) * 0.5 = 250 silver, " +
                                                    "one that cost 4000 with 2 techprints, each would cost (2000 / 2) * 0.5 = 500 silver</i>");
            scrollList.Gap();
            scrollList.Label("The base research cost can optionally be reduced by an amount proportional to the number of techprints it requires. Check this to calculate and use reduced research costs. ");
            scrollList.CheckboxLabeled("Reduce research cost by number of techprints required:", ref settings.ModifyBaseCosts);
            //bug:
            //self note: when this checkbox changes from true->false, content is removed, when false->true, content is added.
            //adding content causes the layout system to shit itself.
            //Why? The space assigned for content at the beginning of the frame is no longer enough for what we want to show,
            //and when we reach the end of the available space some elements are not rendered, changing the overall
            //height of the content, so when we use this value to resize our view for next frame, it still doesn't have enough
            //space to display everything.
            if (settings.ModifyBaseCosts)
            {
                scrollList.Gap();
                scrollList.Label("Base research cost final modifier: ");
                scrollList.SliderWithTextField(ref settings.ResearchBaseCostMultiplier, ref rbcStringBuffer, 0.01f, 10f,
                                               tooltip: "This optional modifier tweaks the final calculated base cost for all techprinted research projects; Higher values here means higher overall research costs, lower values will lower research costs. " +
                                                        "The base research cost (the cost before other gameplay modifiers like player tech level) is reduced by an amount proportional to its original cost and the number of techprints now required. " +
                                                        "The base cost is calculated as (half base cost + (half base cost * (1 / techprint count), rounded to nearest 50) * final modifier)" +
                                                        "\n\n<i>Example: with a default modifier of 1, for a project costing 4000 points with 2 techprints it's final research base cost " +
                                                        "is 2000 + (2000 * (1 / 2)) * 1 = 3000 points, with a modifier of 0.5 the same project would cost 1500 points, with a modifier of 2 it would cost 6000 points</i>");

            } 
            scrollList.Gap();
            scrollList.Label("Techprint commonality: ");
            scrollList.SliderWithTextField(ref settings.BaseCommonality, ref bcStringBuffer, 0.01f, 100f, 
                                           tooltip: "A techprint's commonality relates to how often it will show up in techprint trader's inventories (only traders who stock techprints by default, or " +
                                                    "have them added in the customs settings) or as quest rewards and gifts. Vanilla values are 3 for Cataphract Armour, 2 for JumpPacks and 1 for all implants " +
                                                    "(rarest) before modifiers are applied.");

            scrollList.GapLine();
            scrollList.Label("Native techprint commonality modifier: ");
            scrollList.SliderWithTextField(ref settings.NativeCommonalityMultiplier, ref ncmStringBuffer, 0.1f, 10f,
                                           tooltip: "Techprints that are natively defined in ResearchProjectDef XMLs, either from Royalty/other DLC or added by mods are handled separately to other research projects." +
                                                    "All of the below modifiers and calculations are skipped, and this modifier is applied to the commonality of all of these techprints. It attempts to balance the " +
                                                    "commonality of native techprints with the amount added by this mod. " +
                                                    "\n\n<i>Example: With the default value of 1.5, jump pack techprint commonality goes up to 3, which at the default base commonality of 3 for generated techprints, makes them equally likely to generate.</i>");
            scrollList.Gap();

            list.EndScrollView(scrollList);

            if (_contentHeight != scrollList.CurHeight)
            {
                Log.Message($"TEST HEIGHT CHANGE {_contentHeight}, {scrollList.CurHeight}, {viewRect.height}");
                _contentHeight = scrollList.CurHeight; //use this to overcome the bug
                //_viewHeight = scrollList.CurHeight;
            }
        }
    }
}
