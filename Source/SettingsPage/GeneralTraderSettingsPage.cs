// GeneralTraderSettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:36 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:36 PM


using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage
{
    internal class GeneralTraderSettingsPage: ConfigurableTechprintsSettingPage
    {
        public override void DoPage(Listing_Standard list, Rect inRect)
        {
            string tsmStringBuffer = null;
            string tldStringBuffer = null;
            string tleStringBuffer = null;
            list.Label("Stock count multiplier: ");
            list.SliderWithTextField(ref settings.TraderStockCountMultiplier, ref tsmStringBuffer, 1f, 10f,
                                            tooltip:"Traders who natively deal with Techprints will have their possible techprint stock counts multiplied by this value. Default 3.0." +
                                            "\nNOTE: this multiplier is combined with another balancing multiplier based on the number of research projects added by mods. Final stock counts will reflect this");
            list.Gap();
            list.Label("Tech level difference commonality modifier: ");
            list.Label("");
            list.SliderWithTextField(ref settings.TechDifferenceCommonalityMultiplier, ref tldStringBuffer, 0.01f, 100f);
            list.Gap();
            list.Label("Tech equal commonality modifier: ");
            list.Label("");
            list.SliderWithTextField(ref settings.TechEqualCommonalityMultiplier, ref tleStringBuffer, 0.01f, 100f);

        }
    }
}
