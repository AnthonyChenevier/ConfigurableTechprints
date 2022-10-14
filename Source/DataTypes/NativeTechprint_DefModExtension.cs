// EmpireOwnedTechprint.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/07 3:18 PM
// Last edited by: Anthony Chenevier on 2022/10/07 3:18 PM


using Verse;

namespace ConfigurableTechprints.DataTypes
{
    //simple mod extension used to tag research projects with Empire-held techprints
    //this tag can be processed by our mod's techprint thingSetMaker and stockGenerator
    //to handle these techprints differently
    public class NativeTechprint_DefModExtension : DefModExtension { }
}
