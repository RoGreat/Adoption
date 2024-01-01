using Adoption.Behaviors;

using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Adoption
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "AdoptionSettings_v2";
        public override string FolderName => "Adoption";
        public override string FormatType => "json2";
        public override string DisplayName => new TextObject("{=UQS8Ot22}Adoption {VERSION}", new()
        {
            { "VERSION", typeof(Settings).Assembly.GetName().Version?.ToString(3) ?? "ERROR" }
        }).ToString();

        [SettingPropertyFloatingInteger("{=TRLkkn8V}Adoption Chance", 0f, 1f, "#0%", RequireRestart = false, Order = 1, HintText = "{=zqWrjVgu}Chance that a child is up for adoption.")]
        public float AdoptionChance { get; set; } = 1f;

        [SettingPropertyButton("{=zSklyZ76}Reset Adoption Attempts", Content = "{=Z3WQjWfW}Reset", Order = 2, RequireRestart = false)]
        public Action ResetAdoptionAttempts { get; set; } = () => Campaign.Current.GetCampaignBehavior<AdoptionCampaignBehavior>().ResetAdoptionAttempts();

    }
}