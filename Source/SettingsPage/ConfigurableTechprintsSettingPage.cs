// ConfigurableTechprintsSettingPage.cs
// 
// Part of ConfigurableTechprints - ConfigurableTechprints
// 
// Created by: Anthony Chenevier on 2022/10/13 8:12 PM
// Last edited by: Anthony Chenevier on 2022/10/13 8:12 PM


namespace ConfigurableTechprints.SettingsPage;

internal abstract class ConfigurableTechprintsSettingPage : SettingsPage<ConfigurableTechprintsSettingsData>
{
    protected ConfigurableTechprintsSettingPage() : base(ConfigurableTechprintsMod.Instance.Settings) { }
}
