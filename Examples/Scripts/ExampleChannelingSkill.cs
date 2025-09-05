using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    public sealed class ExampleChannelingSkill : ChannelingSkillBase
    {
        public override float Duration => 10f;

        public override float CooldownTime => 1f;

        public override OperationResult CanBeInterrupted(in InterruptSkillContext context)
        {
            // Permit cancellation, but deny (base behavior) interruption
            if (context.IsCancellation) return SkillOperations.Permitted();
            
            return base.CanBeInterrupted(in context);
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
                Debug.Log($"Skill {name} failed for {context.caster.name} because cooldown is not finished");
        }

        protected internal override void OnCastTickWhenChanneling(in CastSkillContext context)
        {
            base.OnCastTickWhenChanneling(in context);
            Debug.Log($"Skill {name} is channeling for {context.caster.name}");
        }
    }
}