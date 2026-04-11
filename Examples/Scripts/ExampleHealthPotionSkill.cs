using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Example healing consumable belonging to <see cref="HealingPotionSkillGroup"/>.
    ///     Casting this potion triggers the shared 30-second group cooldown on all group members.
    /// </summary>
    public sealed class ExampleHealthPotionSkill : SkillBase, IWithSkillGroup<HealingPotionSkillGroup>
    {
        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} — health potion consumed by {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            Debug.Log($"Skill {name} — healing applied to {context.caster.name}");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if (OperationResult.AreSimilar(reason, SkillOperations.GroupCooldownNotFinished()))
                Debug.LogError($"Skill {name} — healing items are on group cooldown for {context.caster.name}");
        }
    }
}
