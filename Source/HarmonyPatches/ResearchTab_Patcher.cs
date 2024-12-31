// TechprintUtility_Patcher.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/09 1:25 AM
// Last edited by: Anthony Chenevier on 2022/10/09 1:25 AM


using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.HarmonyPatches;

[HarmonyPatch(typeof(MainTabWindow_Research))]
public static class ResearchTab_Patcher
{
    private static bool isFactionListHidden = true;

    [HarmonyPrefix]
    [HarmonyPatch("DrawTechprintInfo")]
    public static bool DrawTechprintInfo_Prefix(ResearchProjectDef ___selectedProject, ref Rect rect, ref float y)
    {
        if (___selectedProject?.heldByFactionCategoryTags == null || ___selectedProject.heldByFactionCategoryTags.Count == 0)
            return false;

        Rect btnRect = new Rect(rect).TopPart(0.05f);
        y += btnRect.height;
        rect.y += btnRect.height;

        if (Widgets.ButtonText(btnRect, "Toggle held factions"))
            isFactionListHidden = !isFactionListHidden;

        return !isFactionListHidden;
        //TBD: limit number of factions shown, then create a scrollview
    }
}
