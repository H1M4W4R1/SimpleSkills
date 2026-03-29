using JetBrains.Annotations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Enums;

namespace Systems.SimpleSkills.Data.Context
{
    /// <remarks>
    ///     This is a <c>ref struct</c> for performance (stack-only, no GC allocation).
    ///     As a consequence it cannot be captured in lambdas, stored in collections,
    ///     used with async/await, or passed to C# events/delegates (e.g. <c>System.Action&lt;InterruptSkillContext&gt;</c>).
    ///     If deferred processing is needed, copy the relevant fields into a regular struct first.
    /// </remarks>
    public readonly ref struct InterruptSkillContext
    {
        /// <summary>
        ///     Object that casts the skill
        /// </summary>
        [NotNull] public readonly SkillCasterBase caster;

        /// <summary>
        ///     Source of the skill interruption
        /// </summary>
        /// <remarks>
        ///     When same as <see cref="caster"/> then it is considered as skill cast cancellation.
        ///     When null then it's considered world interruption.
        /// </remarks>
        [CanBeNull] public readonly object source;
        
        /// <summary>
        ///     Skill reference
        /// </summary>
        [NotNull] public readonly SkillBase skill;
        
        /// <summary>
        ///     Flags
        /// </summary>
        public readonly SkillInterruptFlags flags;

        /// <summary>
        ///     Checks if the skill cast was cancelled
        /// </summary>
        public bool IsCancellation => ReferenceEquals(caster, source);
        
        public InterruptSkillContext([NotNull] SkillCasterBase caster, 
            [CanBeNull] object source,
            [NotNull] SkillBase skill, SkillInterruptFlags flags)
        {
            this.caster = caster;
            this.source = source;
            this.skill = skill;
            this.flags = flags;
        }
    }
}