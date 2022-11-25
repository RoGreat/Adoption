using Adoption.Behaviors;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using TaleWorlds.CampaignSystem;

namespace Adoption.Settings
{
    internal sealed class MCMSettings : AttributeGlobalSettings<MCMSettings>, ISettingsProvider
    {
        public override string Id => SubModule.ModId + "Settings";

        public override string DisplayName => SubModule.ModId + $" {typeof(MCMSettings).Assembly.GetName().Version.ToString(3)}";

        public override string FolderName => SubModule.ModId;

        [SettingPropertyFloatingInteger("{=adoption_chance}Adoption Chance", 0f, 1f, "#0%", RequireRestart = false, Order = 1, HintText = "{=adoption_chance_desc}Chance that a child is up for adoption.")]
        public float AdoptionChance { get; set; } = 0.05f;

        [SettingPropertyButton("{=reset_adoption_attempts}Reset Adoption Attempts", Content = "Press Me", Order = 2, RequireRestart = false, HintText = "{=reset_adoption_attempts_desc}Reset adoption attempts.")]
        public Action ResetAdoptionAttempts { get; set; } = () => Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>().ResetAdoptionAttempts();

        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, Order = 3, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log.")]
        public bool Debug { get; set; } = false;

    }
}
