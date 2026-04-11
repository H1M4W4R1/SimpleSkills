using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Example of a skill with multiple charges demonstrating multi-dash behaviour.
    ///     Holds up to 3 charges, each recharging independently over 10 seconds.
    /// </summary>
    public sealed class ExampleDashSkill : SkillBase, ISkillWithCharges
    {
        public int MaxCharges => 3;

        public override float CooldownTime => 10f;

        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} — dash started for {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            int chargesLeft = context.caster.GetAvailableCharges<ExampleDashSkill>();
            Debug.Log($"Skill {name} — dash complete for {context.caster.name}, {chargesLeft} charge(s) remaining");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if (OperationResult.AreSimilar(reason, SkillOperations.NoChargesAvailable()))
                Debug.LogError($"Skill {name} — no charges available for {context.caster.name}");
        }
    }
}
