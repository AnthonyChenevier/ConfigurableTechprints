// CountChanceExposable.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/12 5:25 PM
// Last edited by: Anthony Chenevier on 2022/10/12 5:25 PM


using RimWorld;
using Verse;

namespace ConfigurableTechprints.DataTypes
{
    internal class CountChanceExposable :IExposable
    {
        public int count;
        public float chance;

        public void ExposeData()
        {
            Scribe_Values.Look(ref count, nameof(count));
            Scribe_Values.Look(ref chance, nameof(chance));
        }

        //type conversion
        public static explicit operator CountChanceExposable(CountChance c) => new CountChanceExposable { count = c.count, chance = c.chance };
        public static implicit operator CountChance(CountChanceExposable ce) => new CountChance { count = ce.count, chance = ce.chance };
    }
}
