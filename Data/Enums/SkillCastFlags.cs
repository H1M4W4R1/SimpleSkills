using System;

namespace Systems.SimpleSkills.Data.Enums
{
    [Flags]
    public enum SkillCastFlags
    {
        None = 0,
        IgnoreCosts = 1 << 0,
        IgnoreCooldown = 1 << 1,
        IgnoreRequirements = 1 << 2,
    }
}