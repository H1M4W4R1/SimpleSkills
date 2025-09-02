using Systems.SimpleSkills.Components;
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
        
        /// <summary>
        ///     Result of the cast
        /// </summary>
        public readonly SkillCastResult result;

        public CastSkillContext WithResult(SkillCastResult newResult)
        {
            return new CastSkillContext(caster, skill, flags, result);
        }
        
        public CastSkillContext(SkillCasterBase caster, SkillBase skill, SkillCastFlags flags, SkillCastResult result)
        {
            this.caster = caster;
            this.skill = skill;
            this.flags = flags;
            this.result = result;
        }
    }
}