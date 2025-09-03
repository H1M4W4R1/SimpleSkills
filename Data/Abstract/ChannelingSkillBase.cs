using Systems.SimpleSkills.Data.Context;

namespace Systems.SimpleSkills.Data.Abstract
{
    /// <summary>
    ///     Simple channeling skill
    /// </summary>
    public abstract class ChannelingSkillBase : SkillBase
    {
        /// <summary>
        ///     Total channel duration
        /// </summary>
        public float Duration { get; set; }

        protected internal virtual void OnCastTickWhenChanneling(in CastSkillContext context)
        {
            
        }
    }
}