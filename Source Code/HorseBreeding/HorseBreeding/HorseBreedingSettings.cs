using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace HorseBreeding
{
    public class HorseBreedingSettings : AttributeGlobalSettings<HorseBreedingSettings>
    {
        public override string Id => nameof(HorseBreedingSettings);

        public override string DisplayName => "Horse Breeding Settings";

        public override string FolderName => nameof(HorseBreedingSettings);

        public override string Format => "json";

        [SettingPropertyFloatingInteger("Base Breeding Time", 1, 1000, "0.00", HintText = "Time in hours to breed horses with riding skill 0", Order = 0, RequireRestart = false)]
        public float BreedingTime { get; set; } = 24;

        [SettingPropertyFloatingInteger("Riding Exp Per Horse Tier", 1, 1000, "0.00", HintText = "Riding XP gain equals this number times the tier of the horse breed", Order = 0, RequireRestart = false)]
        public float RidingXPPerHorseTier { get; set; } = 200;
    }
}