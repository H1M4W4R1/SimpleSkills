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
        public virtual float ChargingTime { get; } = 0f;

        /// <summary>
        ///     Skill cooldown time
        /// </summary>
        public virtual float CooldownTime { get; } = 0f;
        
        /// <summary>
        ///     Checks if skill has cooldown
        /// </summary>
        public bool HasCooldown => CooldownTime > 0;
        
            
        /// <summary>
        ///     Checks if the <paramref name="context"/> skill is available to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill is available to be casted.</returns>
        /// <remarks>
        ///     This method should be used to check general availability of the skill e.g. if skill gem is in inventory,
        ///     but not if skill is on cooldown or caster has enough resources.
        /// </remarks>
        public virtual OperationResult IsSkillAvailable(in CastSkillContext context) =>
            SkillOperations.Permitted();

        
        /// <summary>
        ///     Checks if the <paramref name="context"/> skill has enough resources to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill has enough resources to be casted.</returns>
        public OperationResult HasEnoughResources(in CastSkillContext context) 
            => SkillOperations.Permitted();
        
        /// <summary>
        ///     Checks if the <paramref name="context"/> skill can be casted successfully.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be casted successfully.</returns>
        /// <remarks>
        ///     This method can be used to generate chance-based skills as resources will be consumed before
        ///     casting this check.
        /// </remarks>
        public virtual OperationResult CheckAttemptSuccess(in CastSkillContext context) => 
            SkillOperations.Permitted();

        /// <summary>
        ///     Consumes the resources required to cast the skill.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to consume resources for.</param>
        public virtual void ConsumeResources(in CastSkillContext context)
        {
            
        }
        
        
        /// <summary>
        ///     Event raised when the skill cast has started.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called when the skill cast has started successfully.
        /// </remarks>
        protected internal virtual void OnCastStarted(in CastSkillContext context)
        {
            
        }
        
  
        /// <summary>
        ///     Event raised when the skill cast is ticked while charging.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called every tick while the skill is charging.
        /// </remarks>
        protected internal virtual void OnCastTickWhenCharging(in CastSkillContext context)
        {
            
        }
        
        /// <summary>
        ///     Event raised when the skill cast has ended.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called when the skill cast has finished successfully.
        /// </remarks>
        protected internal virtual void OnCastEnded(in CastSkillContext context)
        {
            
        }

        /// <summary>
        ///     Event raised when the skill cast has failed.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <param name="reason">The reason why the skill cast failed.</param>
        /// <remarks>
        ///     This method is called when the skill cast has failed during pre-start checks.
        /// </remarks>
        protected internal virtual void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }

        /// <summary>
        ///     Checks if the <paramref name="context"/> skill can be interrupted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be interrupted.</returns>
        public virtual OperationResult CanBeInterrupted(in CastSkillContext context) =>
            SkillOperations.Denied();

        
        /// <summary>
        ///     Event raised when the skill cast was interrupted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the skill was interrupted.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling.
        /// </remarks>
        protected internal virtual void OnCastInterrupted(in CastSkillContext context, in OperationResult reason)
        {
            
        }

        /// <summary>
        ///     Event raised when the skill cast was interrupted but the interrupt attempt failed.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the interrupt attempt failed.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling and the interrupt attempt failed.
        /// </remarks>
        protected internal virtual void OnCastInterruptFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }
        
        /// <summary>
        ///     Checks if the <paramref name="context"/> skill can be cancelled.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be cancelled.</returns>
        /// <remarks>
        ///     This method should be used to check if the skill can be cancelled at any point of its lifetime.
        /// </remarks>
        public virtual OperationResult CanBeCancelled(in CastSkillContext context) =>
            SkillOperations.Denied();
        
        /// <summary>
        ///     Event raised when the skill cast was cancelled.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the cancelled skill.</param>
        /// <param name="reason">The reason why the skill was cancelled.</param>
        /// <remarks>
        ///     This method is called when the skill was cancelled while it was charging or channeling.
        /// </remarks>
        protected internal virtual void OnCastCancelled(in CastSkillContext context, in OperationResult reason)
        {
            
        }
        
        /// <summary>
        ///     Event raised when the skill cast was cancelled but the cancellation attempt failed.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the cancelled skill.</param>
        /// <param name="reason">The reason why the cancellation attempt failed.</param>
        /// <remarks>
        ///     This method is called when the skill was cancelled while it was charging or channeling and the cancellation attempt failed.
        /// </remarks>
        protected internal virtual void OnCastCancelFailed(in CastSkillContext context, in OperationResult reason)
        {
            
        }

    
    }
}