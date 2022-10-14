using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ConfigurableTechprints.SettingsPage {
    internal static class ListingHelper
    {
        public static Listing_Standard BeginScrollView(this Listing_Standard list, Rect viewRect, float contentHeight, ref Vector2 scrollPosition)
        {
            //set up scrolling environment
            Rect innerContentRect = new Rect(0f, 0f, viewRect.width - 16f, contentHeight);
            Widgets.BeginScrollView(viewRect, ref scrollPosition, innerContentRect);

            viewRect.width -= 20f;//reduce inner width for scrollbar + buffer
            Vector2 position = scrollPosition;
            Listing_Standard scrollList = new Listing_Standard(viewRect, () => position);
            scrollList.maxOneColumn = true; //restricted to prevent the listing 
            scrollList.Begin(innerContentRect);

            return scrollList;
        }
        public static void EndScrollView(this Listing_Standard list, Listing_Standard scrollList)
        {
            scrollList.End();
            Widgets.EndScrollView();
        }





        internal static Rect SliderWithTextField(this Listing_Standard l, 
                                                 ref float val,
                                                 ref string buffer,
                                                 float min = 0f,
                                                 float max = float.MaxValue, 
                                                 float roundTo = -1, 
                                                 string tooltip = null)
        {
            Rect contentRect = l.GetRect(22f);
            if (!l.BoundingRectCached.HasValue || contentRect.Overlaps(l.BoundingRectCached.Value))
            {
                //do slider
                float num = Widgets.HorizontalSlider(contentRect.LeftPart(0.75f).Rounded(), val, min, max, true, roundTo: roundTo);
                if (num != val)
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val = num;

                //do text entry
                Widgets.TextFieldNumeric(contentRect.RightPart(0.24f).Rounded(), ref val, ref buffer, min, max);

                //do tooltip
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(contentRect))
                        Widgets.DrawHighlight(contentRect);
                    TooltipHandler.TipRegion(contentRect, (TipSignal)tooltip);
                }

                l.Gap(l.verticalSpacing);
            }

            return contentRect;
        }

        internal static Rect SliderWithTextField(this Listing_Standard l,
                                                 ref int val, 
                                                 ref string buffer, 
                                                 int min = 0, 
                                                 int max = int.MaxValue,
                                                 int roundTo = -1,
                                                 string tooltip = null)
        {
            float valFloat = val;
            Rect r = l.SliderWithTextField(ref valFloat, ref buffer, min, max, roundTo > 1 ? roundTo : 1, tooltip);
            val = (int)valFloat;
            return r;
        }

        internal static Rect LabeledSliderWithTextField(this Listing_Standard l,
                                                        string label,
                                                        ref float val,
                                                        ref string buffer,
                                                        float min = 0f,
                                                        float max = float.MaxValue,
                                                        float roundTo = -1, 
                                                        string tooltip = null)
        {
            float height = Text.CalcHeight(label, l.ColumnWidth / 2f);
            Rect contentRect = l.GetRect(height);
            if (!l.BoundingRectCached.HasValue || contentRect.Overlaps(l.BoundingRectCached.Value))
            {
                Rect leftRect = contentRect.LeftHalf().Rounded();
                Rect midRect = contentRect.RightHalf().LeftPart(0.74f).Rounded();
                Rect rightRect = contentRect.RightHalf().RightPart(0.24f).Rounded();

                //do label
                TextAnchor textAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(leftRect, label);
                Text.Anchor = textAnchor;

                //do slider
                float num = Widgets.HorizontalSlider(midRect, val, min, max, true, roundTo: roundTo);
                if (num != val)
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val = num;

                //do text entry
                Widgets.TextFieldNumeric(rightRect, ref val, ref buffer, min, max);

                //do tooltip
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(contentRect))
                        Widgets.DrawHighlight(contentRect);
                    TooltipHandler.TipRegion(contentRect, (TipSignal)tooltip);
                }

                l.Gap(l.verticalSpacing);
            }

            return contentRect;
        }

        internal static Rect LabeledSliderWithTextField(this Listing_Standard l,
                                                        string label,
                                                        ref int val,
                                                        ref string buffer,
                                                        int min = 0,
                                                        int max = int.MaxValue,
                                                        int roundTo = -1,
                                                        string tooltip = null)
        {
            float valFloat = val;
            Rect r = l.LabeledSliderWithTextField(label, ref valFloat, ref buffer, min, max, roundTo > 1 ? roundTo : 1, tooltip);
            val = (int)valFloat;
            return r;
        }
    }
}