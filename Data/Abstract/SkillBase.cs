using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Data.Abstract
{
    [AutoCreate("Skills", SkillsDatabase.LABEL)]
    public abstract class SkillBase : ScriptableObject
    {
        /// <summary>
        ///     Skill charging time
        /// </summary>
        public float ChargingTime { get; set; }
        
        /// <summary>
        ///     Skill cooldown time
        /// </summary>
        public float CooldownTime { get; protected set; }
        
        /// <summary>
        ///     Checks if skill has cooldown
        /// </summary>
        public bool HasCooldown => CooldownTime > 0;
        
        public virtual OperationResult IsSkillAvailable(in CastSkillContext context) =>
            SkillOperations.Permitted();

        public OperationResult HasEnoughResources(in CastSkillContext context) 
            => SkillOperations.Permitted();
        
        public virtual OperationResult CheckAttemptSuccess(in CastSkillContext context) => 
            SkillOperations.Permitted();

        public virtual void ConsumeResources(in CastSkillContext context)
        {
            
        }

        protected internal virtual void OnCastStarted(in CastSkillContext context)
        {
            
        }
  
        protected internal virtual void OnCastTickWhenCharging(in CastSkillContext context)
        {
            
        }
        
        protected internal virtual void OnCastEnded(in CastSkillContext context)
        {
            
        }

        protected internal virtual void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }

        public virtual OperationResult CanBeInterrupted(in CastSkillContext context) =>
            SkillOperations.Denied();

        protected internal virtual void OnCastInterrupted(in CastSkillContext context, in OperationResult reason)
        {
            
        }

        protected internal virtual void OnCastInterruptFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }
        
        public virtual OperationResult CanBeCancelled(in CastSkillContext context) =>
            SkillOperations.Denied();
        
        protected internal virtual void OnCastCancelled(in CastSkillContext context, in OperationResult reason)
        {
            
        }
        
        protected internal virtual void OnCastCancelFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }

    
    }
}