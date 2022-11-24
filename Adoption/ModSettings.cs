using Adoption.Settings;

namespace Adoption
{
    internal sealed class ModSettings
    {
        private readonly ISettingsProvider _provider;

        public ModSettings()
        {
            if (MCMSettings.Instance is not null)
            {
                _provider = MCMSettings.Instance;
                return;
            }
            else if (ModSettingsConfig.Instance is null)
            {
                new ModSettingsConfig();
            }
            _provider = ModSettingsConfig.Instance!;
            ModConfig.Initialize();
        }

        public float AdoptionChance
        {
            get => _provider.AdoptionChance;
            set => _provider.AdoptionChance = value;
        }

        public bool Debug
        {
            get => _provider.Debug;
            set => _provider.Debug = value;
        }
    }
}