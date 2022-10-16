// SettingsPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:08 PM
// Last edited by: Anthony Chenevier on 2022/10/16 9:35 PM


using UnityEngine;
using Verse;

namespace ConfigurableTechprints.SettingsPage;

internal abstract class SettingsPage<T> where T : ModSettings
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
