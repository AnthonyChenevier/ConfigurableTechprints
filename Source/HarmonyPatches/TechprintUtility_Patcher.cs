﻿// TechprintUtility_Patcher.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/09 1:25 AM
// Last edited by: Anthony Chenevier on 2022/10/09 1:25 AM


using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.HarmonyPatches
{
    [HarmonyPatch(typeof(TechprintUtility))]
    public static class TechprintUtility_Patcher
    {
        //just override this to make it take faction into account during selection
        [HarmonyPostfix]
        [HarmonyPatch("TryGetTechprintDefToGenerate")]
        public static bool TryGetTechprintDefToGenerate_Postfix(bool __state, Faction faction, out ThingDef result, List<ThingDef> alreadyGeneratedTechprints, float maxMarketValue)
        {
            Log.Message("ConfigurableTechprintsMod :: Using overridden TryGetTechprintDefToGenerate");
            if (!TechprintUtility.GetResearchProjectsNeedingTechprintsNow(faction, alreadyGeneratedTechprints, maxMarketValue).TryRandomElementByWeight(def => GetFactionSpecificSelectionWeight(def, faction), out ResearchProjectDef projectDef))
            {
                result = null;
                return false;
            }
            result = projectDef.Techprint;
            return true;
        }

        //Changed to take selecting faction tech level into account. If the research project
        //is above or below the faction's tech level then make this techprint less likely to
        //spawn. If the research project is native then make it more common as well (configurable)
        public static float GetFactionSpecificSelectionWeight(ResearchProjectDef project, Faction faction = null)
        {
            //no faction means this is probably a ThingSetMaker asking. just in case more work needs to be done here.
            //just use vanilla behaviour for now
            float baseCommonality = project.techprintCommonality * (project.PrerequisitesCompleted ? 1f : 0.02f);
            if (faction == null)
                return baseCommonality;

            int techDifference = Math.Abs((int)project.techLevel - (int)faction.def.techLevel);
            //give projects at our tech level a set commonality (configurable),
            //otherwise make projects less common per level of difference (configurable)
            float commonalityMultiplier = techDifference == 0
                                              ? ConfigurableTechprintsMod.Instance.Settings.TechEqualCommonalityMultiplier
                                              : 1f - techDifference * ConfigurableTechprintsMod.Instance.Settings.TechDifferenceCommonalityMultiplier;

            return baseCommonality * commonalityMultiplier;
        }
    }
}
