using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Data.Enums;
using Systems.SimpleSkills.Data.Internal;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Components
{
    /// <summary>
    ///     Represents a caster of a skill - either entity or even world
    /// </summary>
    public abstract class SkillCasterBase : MonoBehaviour
    {
        // TODO: Cooldowns
        // TODO: Channeling
        // TODO: Resources
        // TODO: Handle Ticks
        public OperationResult TryCastSkill(
            [NotNull] SkillBase skill,
            SkillCastFlags flags = SkillCastFlags.None,
            ActionSource actionSource = ActionSource.External)
        {
            CastSkillContext context = new(this, skill, flags);
            return TryCastSkill(context, actionSource);
        }

        public OperationResult TryCastSkill(
            in CastSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            // Check if skill is available for caster
            OperationResult isSkillAvailableCheck = IsSkillAvailable(context);
            if (!isSkillAvailableCheck && (context.flags & SkillCastFlags.IgnoreAvailability) == 0)
            {
                if (actionSource == ActionSource.Internal) return isSkillAvailableCheck;
                OnSkillCastFailed(context, isSkillAvailableCheck);
                return isSkillAvailableCheck;
            }

            // Check if skill is on cooldown for this caster
            OperationResult isSkillOnCooldownCheck = IsSkillOnCooldown(context);
            if (!isSkillOnCooldownCheck && (context.flags & SkillCastFlags.IgnoreCooldown) == 0)
            {
                if (actionSource == ActionSource.Internal) return isSkillOnCooldownCheck;
                OnSkillCastFailed(context, isSkillOnCooldownCheck);
                return isSkillOnCooldownCheck;
            }

            // Check if caster has enough resources
            OperationResult hasEnoughSkillResourcesCheck = HasEnoughSkillResources(context);
            if (!hasEnoughSkillResourcesCheck && (context.flags & SkillCastFlags.IgnoreCosts) == 0)
            {
                if (actionSource == ActionSource.Internal) return hasEnoughSkillResourcesCheck;
                OnSkillCastFailed(context, hasEnoughSkillResourcesCheck);
                return hasEnoughSkillResourcesCheck;
            }

            // Consume skill resources
            ConsumeSkillResources(context);

            // Check if cast can be performed
            OperationResult canSkillBeCastedCheck = CheckCastAttemptSuccess(context);
            if (!canSkillBeCastedCheck && (context.flags & SkillCastFlags.IgnoreRequirements) == 0)
            {
                if (actionSource == ActionSource.Internal) return canSkillBeCastedCheck;
                OnSkillCastFailed(context, canSkillBeCastedCheck);
                return canSkillBeCastedCheck;
            }

            // Execute events
            RegisterCastedSkill(context);
            OnSkillCastStart(context);
            return SkillOperations.Casted();
        }

        public OperationResult TryCancelSkill(
            in CastSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            OperationResult canSkillBeCancelledCheck = CanSkillBeCancelled(context);
            if (!canSkillBeCancelledCheck && (context.flags & SkillCastFlags.IgnoreRequirements) == 0)
            {
                if (actionSource == ActionSource.Internal) return canSkillBeCancelledCheck;
                OnSkillCastFailed(context, canSkillBeCancelledCheck);
                return canSkillBeCancelledCheck;
            }
            
            // Execute events
            ClearCastedSkill(context);
            OnSkillCastCancelled(context, canSkillBeCancelledCheck);
            return canSkillBeCancelledCheck;
        }

        public OperationResult TryInterruptSkill(
            in CastSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            OperationResult canSkillBeInterruptedCheck = CanSkillBeInterrupted(context);
            if (!canSkillBeInterruptedCheck && (context.flags & SkillCastFlags.IgnoreRequirements) == 0)
            {
                if (actionSource == ActionSource.Internal) return canSkillBeInterruptedCheck;
                OnSkillCastInterruptFailed(context, canSkillBeInterruptedCheck);
                return canSkillBeInterruptedCheck;
            }

            // Execute events
            ClearCastedSkill(context);
            OnSkillCastInterrupted(context, canSkillBeInterruptedCheck);
            return canSkillBeInterruptedCheck;
        }

#region Skill List management

        /// <summary>
        ///     List of all currently casted skills
        /// </summary>
        protected readonly List<CastedSkillData> currentlyCastedSkills = new();

        /// <summary>
        ///     Access to currently casted skills
        /// </summary>
        public IReadOnlyList<CastedSkillData> CurrentlyCastedSkills => currentlyCastedSkills;

        /// <summary>
        ///     Register casted skill in list
        /// </summary>
        private void RegisterCastedSkill(in CastSkillContext context)
        {
            // Convert context to casted skill data
            CastedSkillData castedSkillData = new(context.skill);
            currentlyCastedSkills.Add(castedSkillData);
        }

        /// <summary>
        ///     Clear casted skill from list
        /// </summary>
        private void ClearCastedSkill(in CastSkillContext context)
        {
            SkillBase skill = context.skill;
            currentlyCastedSkills.RemoveAll(info => ReferenceEquals(skill, info.skill));
        }

#endregion

#region Checks

        public virtual OperationResult IsSkillAvailable(in CastSkillContext context) =>
            context.skill.IsSkillAvailable(context);

        public virtual OperationResult IsSkillOnCooldown(in CastSkillContext context) =>
            SkillOperations.Permitted(); // TODO: Implement cooldowns

        public virtual OperationResult HasEnoughSkillResources(in CastSkillContext context) =>
            context.skill.HasEnoughResources(context);

        public virtual OperationResult CheckCastAttemptSuccess(in CastSkillContext context) =>
            context.skill.CheckAttemptSuccess(context);

        public virtual OperationResult CanSkillBeInterrupted(in CastSkillContext context) =>
            context.skill.CanBeInterrupted(context);

        public virtual OperationResult CanSkillBeCancelled(in CastSkillContext context) =>
            context.skill.CanBeCancelled(context);

#endregion

#region Events

        public virtual void ConsumeSkillResources(in CastSkillContext context) =>
            context.skill.ConsumeResources(context);

        protected virtual void OnSkillCastStart(in CastSkillContext context) =>
            context.skill.OnCastStarted(context);

        protected virtual void OnSkillCastFailed(in CastSkillContext context, in OperationResult reason) =>
            context.skill.OnCastFailed(context, reason);

        protected virtual void OnSkillTickWhenCharging(in CastSkillContext context) =>
            context.skill.OnCastTickWhenCharging(context);

        protected virtual void OnSkillTickWhenChanneling(in CastSkillContext context)
        {
            if (context.skill is ChannelingSkillBase channelingSkillBase)
                channelingSkillBase.OnCastTickWhenChanneling(context);
            else
                Debug.LogError($"Skill {context.skill.name} is not a channeling skill");
        }

        protected virtual void OnSkillCastEnd(in CastSkillContext context) =>
            context.skill.OnCastEnded(context);

        protected virtual void OnSkillCastInterrupted(in CastSkillContext context, in OperationResult reason) =>
            context.skill.OnCastInterrupted(context, reason);

        protected virtual void OnSkillCastInterruptFailed(in CastSkillContext context, in OperationResult reason)
            =>
                context.skill.OnCastInterruptFailed(context, reason);

        protected virtual void OnSkillCastCancelled(in CastSkillContext context, in OperationResult reason) =>
            context.skill.OnCastCancelled(context, reason);

        protected virtual void OnSkillCastCancelFailed(in CastSkillContext context, in OperationResult reason) =>
            context.skill.OnCastCancelFailed(context, reason);

#endregion
    }
}