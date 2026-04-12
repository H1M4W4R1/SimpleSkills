using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;
// ReSharper disable Unity.NoNullPropagation

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Example of a passive aura skill using <see cref="IActivatedSkill"/>.
    ///     Casting the skill activates the aura; casting it again deactivates it (toggle).
    ///     While active, <see cref="IActivatedSkill.OnTickWhileActive"/> is called every frame.
    /// </summary>
    /// <remarks>
    ///     The caster is received as <see cref="ISkillTarget"/> and cast to <see cref="MonoBehaviour"/>
    ///     to resolve its name. In real implementations, cast to your game-specific caster type
    ///     (e.g. a character class) to access stats, components, or other caster state.
    /// </remarks>
    public sealed class ExampleRegenerationAuraSkill : SkillBase, IActivatedSkill
    {
        private const float HEAL_PER_SECOND = 5f;

        void IActivatedSkill.OnActivated(ISkillTarget caster)
        {
            string casterName = (caster as MonoBehaviour)?.name ?? "unknown";
            Debug.Log($"Skill {name} — regeneration aura activated on {casterName}");
        }

        void IActivatedSkill.OnDeactivated(ISkillTarget caster)
        {
            string casterName = (caster as MonoBehaviour)?.name ?? "unknown";
            Debug.Log($"Skill {name} — regeneration aura deactivated on {casterName}");
        }

        void IActivatedSkill.OnTickWhileActive(ISkillTarget caster, float deltaTime)
        {
            string casterName = (caster as MonoBehaviour)?.name ?? "unknown";
            float healThisTick = HEAL_PER_SECOND * deltaTime;
            Debug.Log($"Skill {name} — healing {casterName} for {healThisTick:F3} HP this tick");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if (OperationResult.AreSimilar(reason, SkillOperations.CooldownNotFinished()))
                Debug.LogError($"Skill {name} — cooldown not finished for {context.caster.name}");
        }
    }
}
