using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace Adoption.Settings
{
    internal sealed class MCMSettings : AttributeGlobalSettings<MCMSettings>, ISettingsProvider
    {
        public override string Id => SubModule.ModId + "Settings";

        public override string DisplayName => SubModule.ModId + $" {typeof(MCMSettings).Assembly.GetName().Version.ToString(3)}";

        public override string FolderName => SubModule.ModId;

        [SettingPropertyFloatingInteger("{=adoption_chance}Adoption Chance", 0f, 1f, "#0%", RequireRestart = false, Order = 1, HintText = "{=adoption_chance_desc}Chance that a child is up for adoption.")]
        public float AdoptionChance { get; set; } = 0.05f;

        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, Order = 3, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log.")]
        public bool Debug { get; set; } = false;
    }
}
