namespace Adoption.Settings
{
    internal interface ISettingsProvider
    {
        float AdoptionChance { get; set; }

        bool Debug { get; set; }
    }
}