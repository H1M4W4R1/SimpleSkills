using System;

namespace Systems.SimpleSkills.Data.Enums
{
    [Flags]
    public enum SkillCastFlags
    {
        None = 0,
        
        /// <summary>
        ///     Ignores check if skill is available
        /// </summary>
        IgnoreAvailability = 1 << 0,
        
        /// <summary>
        ///     Ignores check if entity has enough resources
        /// </summary>
        IgnoreCosts = 1 << 1,
        
        /// <summary>
        ///     Ignores check if skill is on cooldown
        /// </summary>
        IgnoreCooldown = 1 << 2,
        
        /// <summary>
        ///     Ignores other skill requirements
        /// </summary>
        IgnoreRequirements = 1 << 3,
        
        /// <summary>
        ///     Disables consumption of resources
        /// </summary>
        DoNotConsumeResources = 1 << 4,
    }
}