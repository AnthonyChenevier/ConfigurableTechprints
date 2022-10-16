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
        protected readonly T settings;
        private float _contentHeight;
        private Vector2 _scrollPosition;

        public SettingsPage(T settings)
        {
            this.settings = settings;
            _contentHeight = float.MaxValue;
            _scrollPosition = Vector2.zero;
        }

        public void Draw(Listing_Standard list, Rect inRect)
        {
            Rect viewRect = list.GetRect(inRect.height - list.CurHeight);
            Listing_Standard scrollList = list.BeginScrollView(viewRect, _contentHeight, ref _scrollPosition);

            DoPage(scrollList, viewRect);

            if (_contentHeight != scrollList.CurHeight)
                _contentHeight = scrollList.CurHeight;
            list.EndScrollView(scrollList);
        }

        protected abstract void DoPage(Listing_Standard list, Rect inRect);
    }
}
