using JetBrains.Annotations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Enums;

namespace Systems.SimpleSkills.Data.Context
{
    /// <remarks>
    ///     This is a <c>ref struct</c> for performance (stack-only, no GC allocation).
    ///     As a consequence it cannot be captured in lambdas, stored in collections,
    ///     used with async/await, or passed to C# events/delegates (e.g. <c>System.Action&lt;CastSkillContext&gt;</c>).
    ///     If deferred processing is needed, copy the relevant fields into a regular struct first.
    /// </remarks>
    public readonly ref struct CastSkillContext
    {
        /// <summary>
        ///     Object that casts the skill
        /// </summary>
        [NotNull] public readonly SkillCasterBase caster;

        /// <summary>
        ///     Skill reference
        /// </summary>
        [NotNull] public readonly SkillBase skill;

        /// <summary>
        ///     Flags
        /// </summary>
        public readonly SkillCastFlags flags;

        /// <summary>
        ///     Optional target for targeted skills. Set before calling TryCastSkill.
        ///     Null for non-targeted or self-cast skills.
        /// </summary>
        [CanBeNull] public readonly ISkillTarget target;

        public CastSkillContext([NotNull] SkillCasterBase caster, [NotNull] SkillBase skill, SkillCastFlags flags)
        {
            this.caster = caster;
            this.skill = skill;
            this.flags = flags;
            target = null;
        }

        public CastSkillContext(
            [NotNull] SkillCasterBase caster,
            [NotNull] SkillBase skill,
            SkillCastFlags flags,
            [CanBeNull] ISkillTarget target)
        {
            this.caster = caster;
            this.skill = skill;
            this.flags = flags;
            this.target = target;
        }
    }
}
