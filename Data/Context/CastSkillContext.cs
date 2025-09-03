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
        public readonly SkillCasterBase caster;
        
        /// <summary>
        ///     Skill reference
        /// </summary>
        public readonly SkillBase skill;
        
        /// <summary>
        ///     Flags
        /// </summary>
        public readonly SkillCastFlags flags;

        public CastSkillContext(SkillCasterBase caster, SkillBase skill, SkillCastFlags flags)
        {
            this.caster = caster;
            this.skill = skill;
            this.flags = flags;
        }
    }
}