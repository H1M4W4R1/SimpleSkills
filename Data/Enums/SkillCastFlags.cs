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

        /// <summary>
        ///     Refunds consumed resources if the cast attempt fails (e.g., chance-based miss).
        ///     Without this flag, resources are consumed before the attempt check and are not refunded on failure.
        /// </summary>
        RefundResourcesOnFailure = 1 << 5,

        /// <summary>
        ///     Allows stacking multiple casts of the same skill simultaneously.
        ///     Without this flag, a skill that is already being cast (charging/channeling) cannot be cast again.
        /// </summary>
        AllowStacking = 1 << 6,

        /// <summary>
        ///     When set, interrupted or cancelled skills skip the cooldown phase entirely.
        /// </summary>
        NoCooldownOnInterrupt = 1 << 7,

        /// <summary>
        ///     When set, casting a skill that is already active will reset its state instead of being blocked.
        ///     Useful for toggle-style skills like shields.
        /// </summary>
        ResetOnRecast = 1 << 8,
    }
}