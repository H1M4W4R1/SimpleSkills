using Systems.SimpleSkills.Components;

namespace Systems.SimpleSkills.Data.Abstract
{
    /// <summary>
    ///     Interface for skills that provide persistent effects.
    ///     Those skills are toggled on/off via <see cref="SkillCasterBase.ActivateSkill{TPassive}"/>
    ///     and <see cref="SkillCasterBase.DeactivateSkill{TPassive}"/> rather than cast with cooldowns.
    /// </summary>
    public interface IActivatedSkill
    {
        /// <summary>
        ///     Called when the skill is activated
        /// </summary>
        void OnActivated()
        {
        }

        /// <summary>
        ///     Called when the skill is deactivated
        /// </summary>
        void OnDeactivated()
        {
        }

        /// <summary>
        ///     Called each tick while the skill is active
        /// </summary>
        void OnTickWhileActive(float deltaTime)
        {
        }
    }
}
