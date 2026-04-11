using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Context;

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
        ///     Called when the skill is activated.
        /// </summary>
        /// <param name="target">
        ///     The caster that activated the skill, passed as <see cref="ISkillTarget"/>.
        ///     Cast to <see cref="SkillCasterBase"/> or a game-specific type to access caster components.
        /// </param>
        void OnActivated(ISkillTarget target)
        {
        }

        /// <summary>
        ///     Called when the skill is deactivated.
        /// </summary>
        /// <param name="target">
        ///     The caster that deactivated the skill, passed as <see cref="ISkillTarget"/>.
        ///     Cast to <see cref="SkillCasterBase"/> or a game-specific type to access caster components.
        /// </param>
        void OnDeactivated(ISkillTarget target)
        {
        }

        /// <summary>
        ///     Called each tick while the skill is active.
        /// </summary>
        /// <param name="target">
        ///     The caster owning this active skill, passed as <see cref="ISkillTarget"/>.
        ///     Cast to <see cref="SkillCasterBase"/> or a game-specific type to access caster components.
        /// </param>
        /// <param name="deltaTime">Time elapsed since the last tick in seconds.</param>
        void OnTickWhileActive(ISkillTarget target, float deltaTime)
        {
        }
    }
}
