using JetBrains.Annotations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Enums;

namespace Systems.SimpleSkills.Data.Context
{
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

        public CastSkillContext([NotNull] SkillCasterBase caster, [NotNull] SkillBase skill, SkillCastFlags flags)
        {
            this.caster = caster;
            this.skill = skill;
            this.flags = flags;
        }
    }
}