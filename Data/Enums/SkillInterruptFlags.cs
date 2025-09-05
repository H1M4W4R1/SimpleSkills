using System;

namespace Systems.SimpleSkills.Data.Enums
{
    [Flags]
    public enum SkillInterruptFlags
    {
        None = 0,
        
        /// <summary>
        ///     Ignores interruption requirements
        /// </summary>
        IgnoreRequirements = 1 << 1

    }
}