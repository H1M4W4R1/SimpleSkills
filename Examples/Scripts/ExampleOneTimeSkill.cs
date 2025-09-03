using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    public sealed class ExampleOneTimeSkill : SkillBase
    {
        public override float CooldownTime => 5f;

        public override float ChargingTime => 1f;

        protected internal override void OnCastTickWhenCharging(in CastSkillContext context)
        {
            base.OnCastTickWhenCharging(in context);
            Debug.Log($"Skill {name} charging for {context.caster.name}");
        }

        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} started for {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            Debug.Log($"Skill {name} ended for {context.caster.name}");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if(OperationResult.AreSimilar(reason, SkillOperations.CooldownNotFinished()))
                Debug.LogError($"Skill {name} failed for {context.caster.name} because cooldown is not finished");
        }
    }
}