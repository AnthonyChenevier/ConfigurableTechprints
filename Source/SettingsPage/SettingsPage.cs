using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage
{
    internal abstract class SettingsPage<T> where T:ModSettings
    {
        protected T settings;

        public SettingsPage(T settings)
        {
            this.settings = settings;
        }


        public abstract void DoPage(Listing_Standard list, Rect inRect);
    }
}
