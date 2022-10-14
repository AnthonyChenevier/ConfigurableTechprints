// TraderData.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/12 5:25 PM
// Last edited by: Anthony Chenevier on 2022/10/12 5:25 PM


using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ConfigurableTechprints.DataTypes
{
    public struct TraderData : IExposable
    {

        public List<CountChance> CountChances;
        public bool HasTechprintGeneratorNatively;

        public void ExposeData()
        {
            Scribe_Values.Look(ref HasTechprintGeneratorNatively, nameof(HasTechprintGeneratorNatively));
            List<CountChanceExposable> countChancesE = CountChances.Select(cc => (CountChanceExposable)cc).ToList();
            Scribe_Collections.Look(ref countChancesE, nameof(CountChances), LookMode.Deep);
            CountChances = countChancesE.Select(cce => (CountChance)cce).ToList();
        }

    }
}
