using JetBrains.Annotations;

namespace Systems.SimpleSkills.Data.Abstract
{
    public interface ISkillWithLevels
    {
        /// <summary>
        ///     Skill level
        /// </summary>
        /// <remarks>
        ///     Best way is to implement as abstract property (on abstract class) to ensure levels are properly
        ///     configured (don't trust design team to handle that).
        /// </remarks>
        public int Level { get; }

        /// <summary>
        ///     Acquires skill for level
        /// </summary>
        /// <remarks>
        ///     This has to be implemented on core skill and should not be overriden in derived classes
        ///     to ensure correct behavior in all scenarios.
        /// </remarks>
        [CanBeNull] public SkillBase GetSkillForLevel(int level);
    }
}