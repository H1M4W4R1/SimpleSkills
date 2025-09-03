using System;

namespace Systems.SimpleSkills.Data.Enums
{
    [Flags]
    public enum SkillCastFlags
    {
        None = 0,
        IgnoreAvailability = 1 << 0,
        IgnoreCosts = 1 << 1,
        IgnoreCooldown = 1 << 2,
        IgnoreRequirements = 1 << 3,
    }
}