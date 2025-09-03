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
        public virtual float Duration { get; } = 0f;

        /// <summary>
        ///     Channeling is infinite
        /// </summary>
        public bool IsInfinite => Duration <= 0;

        
        /// <summary>
        ///     Event raised when the skill cast is ticked while channeling.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        protected internal virtual void OnCastTickWhenChanneling(in CastSkillContext context)
        {
            
        }
    }
}