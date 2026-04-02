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
        ///     Maximum number of concurrent stacks allowed when <see cref="SkillCastFlags.AllowStacking"/> is set.
        ///     Default is 1 (no stacking). Override to allow multiple concurrent casts.
        /// </summary>
        public virtual int MaxStacks => 1;

        /// <summary>
        ///     Cooldown duration multiplier applied when the skill is interrupted or cancelled.
        ///     Override to reduce or eliminate cooldown on interrupt (e.g., return 0 for no cooldown).
        ///     Default is 1 (full cooldown).
        /// </summary>
        public virtual float InterruptedCooldownMultiplier => 1f;

        /// <summary>
        ///     Whether this skill requires a target to be cast.
        ///     When true, <see cref="CastSkillContext.target"/> must be non-null.
        /// </summary>
        public virtual bool RequiresTarget => false;


        /// <summary>
        ///     Checks if the <paramref name="context"/> skill is available to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill is available to be casted.</returns>
        /// <remarks>
        ///     This method should be used to check general availability of the skill e.g. if skill gem is in inventory,
        ///     but not if skill is on cooldown or caster has enough resources.
        /// </remarks>
        protected internal virtual OperationResult IsSkillAvailable(in CastSkillContext context) =>
            SkillOperations.Permitted();

        
        /// <summary>
        ///     Checks if the <paramref name="context"/> skill has enough resources to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill has enough resources to be casted.</returns>
        protected internal virtual OperationResult HasEnoughResources(in CastSkillContext context)
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
        protected internal virtual OperationResult CheckAttemptSuccess(in CastSkillContext context) => 
            SkillOperations.Permitted();

        /// <summary>
        ///     Consumes the resources required to cast the skill.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to consume resources for.</param>
        protected internal virtual void ConsumeResources(in CastSkillContext context)
        {

        }

        /// <summary>
        ///     Refunds the resources that were consumed for the skill cast.
        ///     Called when a cast attempt fails and <see cref="SkillCastFlags.RefundResourcesOnFailure"/> is set.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to refund resources for.</param>
        protected internal virtual void RefundResources(in CastSkillContext context)
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
        /// <param name="context">The <see cref="InterruptSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be interrupted.</returns>
        protected internal virtual OperationResult CanBeInterrupted(in InterruptSkillContext context) =>
            SkillOperations.Denied();

        
        /// <summary>
        ///     Event raised when the skill cast was interrupted.
        /// </summary>
        /// <param name="context">The <see cref="InterruptSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the skill was interrupted.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling.
        /// </remarks>
        protected internal virtual void OnCastInterrupted(in InterruptSkillContext context, in OperationResult reason)
        {
            
        }

        /// <summary>
        ///     Event raised when the skill cast was interrupted but the interrupt attempt failed.
        /// </summary>
        /// <param name="context">The <see cref="InterruptSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the interrupt attempt failed.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling and the interrupt attempt failed.
        /// </remarks>
        protected internal virtual void OnCastInterruptFailed(in InterruptSkillContext context, in OperationResult reason)
        {

        }

        /// <summary>
        ///     Event raised each tick while the skill is on cooldown.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the skill on cooldown.</param>
        protected internal virtual void OnCooldownTick(in CastSkillContext context)
        {
        }

        /// <summary>
        ///     Event raised when the skill cast is registered (added to the active cast list).
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the registered skill.</param>
        protected internal virtual void OnCastRegistered(in CastSkillContext context)
        {
        }

        /// <summary>
        ///     Event raised when the skill cast is removed from the active cast list.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the removed skill.</param>
        protected internal virtual void OnCastRemoved(in CastSkillContext context)
        {
        }
    }
}