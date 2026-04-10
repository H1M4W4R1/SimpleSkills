using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleSkills.Data;
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
#region Ticks

        /// <summary>
        ///     Standard way to perform tick updates. Override to empty if using custom tick system
        ///     e.g. turn-based system and call <see cref="OnTickExecuted"/> method manually.
        /// </summary>
        protected virtual void Update()
        {
            OnTickExecuted(Time.deltaTime);
        }

        /// <summary>
        ///     Method used to perform all time-based updates
        /// </summary>
        protected virtual void OnTickExecuted(float deltaTime)
        {
            HandleCharging(deltaTime);
            HandleChanneling(deltaTime);
            HandleSkillsCompleted(deltaTime);
            HandleCooldowns(deltaTime);
            HandleGroupCooldowns(deltaTime);
            HandleActivatedSkillTicks(deltaTime);
        }

        /// <summary>
        ///     Method that is responsible for handling skill charging state (if any exists)
        /// </summary>
        protected void HandleCharging(float deltaTime)
        {
            // Iterate in reverse for safety — new skills added by event handlers start iterating next cycle
            for (int i = currentlyCastedSkills.Count - 1; i >= 0; i--)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];

                // Skill has not yet finished charging
                if (castedSkillReference.IsChargingComplete) continue;

                // Update timer and progress events
                castedSkillReference.chargingTimer += deltaTime;

                CastSkillContext skillCastContext = GetCastedSkillContextFor(i);

                OnSkillTickWhenCharging(skillCastContext);
                if (castedSkillReference.chargingTimer >= castedSkillReference.skill.ChargingTime)
                {
                    SkillState nextState = castedSkillReference.skill is IChannelingSkillBase
                        ? SkillState.Channeling
                        : SkillState.Complete;

                    castedSkillReference.stateMachine.TryTransitionTo(nextState);

                    // We start casting the skill
                    OnSkillCastStart(skillCastContext);
                }

                currentlyCastedSkills[i] = castedSkillReference;
            }
        }

        /// <summary>
        ///     Method that is responsible for handling skill channeling state (if any exists aka. if skill
        ///     can be channeled)
        /// </summary>
        protected void HandleChanneling(float deltaTime)
        {
            // Iterate in reverse for safety — new skills added by event handlers start iterating next cycle
            for (int i = currentlyCastedSkills.Count - 1; i >= 0; i--)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];
                if (castedSkillReference.skill is not IChannelingSkillBase channelingSkill) continue;

                // Skill has to be charged
                if (!castedSkillReference.IsChargingComplete) continue;

                // And not yet completed
                if (castedSkillReference.IsCastComplete) continue;

                // And not yet on cooldown
                if (castedSkillReference.IsOnCooldown) continue;

                // Update timer and progress events
                castedSkillReference.channelingTimer += deltaTime;

                OnSkillTickWhenChanneling(GetCastedSkillContextFor(i));

                if (castedSkillReference.channelingTimer >= channelingSkill.Duration &&
                    !channelingSkill.IsInfinite)
                    castedSkillReference.stateMachine.TryTransitionTo(SkillState.Complete);

                currentlyCastedSkills[i] = castedSkillReference;
            }
        }

        /// <summary>
        ///     Method that is responsible for handling skill completion state (if skill channeling was completed,
        ///     skill casting was finished or skill was cancelled / interrupted).
        /// </summary>
        protected void HandleSkillsCompleted(float deltaTime)
        {
            for (int i = currentlyCastedSkills.Count - 1; i >= 0; i--)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];

                // Skill has to be casted
                if (!castedSkillReference.IsCastComplete) continue;

                // Skill has not yet started cooldown
                if (castedSkillReference.IsOnCooldown) continue;

                // Handle cast end event if wasn't cancelled / interrupted
                if (castedSkillReference.skillState == SkillState.Complete)
                    OnSkillCastEnd(GetCastedSkillContextFor(i));

                // Skip cooldown for interrupted/cancelled skills when flag or multiplier says so
                bool wasInterrupted = castedSkillReference.skillState is SkillState.Interrupted or SkillState.Cancelled;
                if (wasInterrupted)
                {
                    bool noCooldownFlag = (castedSkillReference.flags & SkillCastFlags.NoCooldownOnInterrupt) != 0;
                    bool zeroCooldownMultiplier = castedSkillReference.skill.InterruptedCooldownMultiplier <= 0f;

                    if (noCooldownFlag || zeroCooldownMultiplier)
                    {
                        ClearCastedSkillDataAt(i);
                        continue;
                    }
                }

                // Update data
                castedSkillReference.wasInterrupted = wasInterrupted;
                castedSkillReference.stateMachine.TryTransitionTo(SkillState.Cooldown);
                currentlyCastedSkills[i] = castedSkillReference;
            }
        }

        /// <summary>
        ///     Method that is responsible for handling skill cooldown state and removing casted skill data
        ///     when cooldown is finished.
        /// </summary>
        protected void HandleCooldowns(float deltaTime)
        {
            for (int i = currentlyCastedSkills.Count - 1; i >= 0; i--)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];
                if (!castedSkillReference.IsOnCooldown) continue;

                castedSkillReference.cooldownTimer += deltaTime;
                currentlyCastedSkills[i] = castedSkillReference;

                OnSkillCooldownTick(GetCastedSkillContextFor(i));

                // Apply interrupted cooldown multiplier if applicable
                float effectiveCooldown = castedSkillReference.skill.CooldownTime;
                if (castedSkillReference.wasInterrupted)
                    effectiveCooldown *= castedSkillReference.skill.InterruptedCooldownMultiplier;

                // Clear casted skill context if cooldown is finished
                if (castedSkillReference.cooldownTimer >= effectiveCooldown)
                    ClearCastedSkillDataAt(i);
            }
        }

#endregion

#region Casting, Interrupting, Cancelling

        /// <summary>
        ///     Tries to cast skill
        /// </summary>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TSkill">Type of skill to cast</typeparam>
        /// <returns>Result of operation</returns>
        public OperationResult TryCastSkill<TSkill>(
            SkillCastFlags flags = SkillCastFlags.None,
            ActionSource actionSource = ActionSource.External)
            where TSkill : SkillBase, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return SkillOperations.SkillNotFound();
            return TryCastSkill(skill, flags, actionSource);
        }

        /// <summary>
        ///     Tries to cast skill
        /// </summary>
        /// <param name="skill">Skill to cast</param>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        public OperationResult TryCastSkill(
            [NotNull] SkillBase skill,
            SkillCastFlags flags = SkillCastFlags.None,
            ActionSource actionSource = ActionSource.External)
        {
            CastSkillContext context = new(this, skill, flags);
            return TryCastSkill(context, actionSource);
        }


        /// <summary>
        ///     Tries to cast skill
        /// </summary>
        /// <param name="context">Context of casted skill</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        public OperationResult TryCastSkill(
            in CastSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            // Resolve skill level if applicable
            SkillBase resolvedSkill = context.skill;
            if (resolvedSkill is ISkillWithLevels leveledSkill)
            {
                int level = GetSkillLevel(leveledSkill);
                resolvedSkill = leveledSkill.GetSkillForLevel(level);
            }
            
            // Ensure if skill was resolved correctly, level may not exist if somebody made
            // a mistake during configuration
            if (ReferenceEquals(resolvedSkill, null))
                return SkillOperations.SkillNotFound();

            // Deactivate passive if this passive is active
            if (IsSkillActivated(resolvedSkill))
            {
                DeactivateSkill(resolvedSkill);
                return SkillOperations.SkillDeactivated();
            }

            // Create resolved context (may differ from input if level was resolved)
            CastSkillContext resolvedContext = ReferenceEquals(resolvedSkill, context.skill)
                ? context
                : new CastSkillContext(context.caster, resolvedSkill, context.flags, context.target);

            // Check if skill requires a target
            if (resolvedContext.skill.RequiresTarget && resolvedContext.target == null)
            {
                OperationResult noTargetResult = SkillOperations.NoTargetSelected();
                if (actionSource == ActionSource.Internal) return noTargetResult;
                OnSkillCastFailed(resolvedContext, noTargetResult);
                return noTargetResult;
            }

            // Check if skill is available for caster
            OperationResult isSkillAvailableCheck = IsSkillAvailable(resolvedContext);
            if (!isSkillAvailableCheck && (resolvedContext.flags & SkillCastFlags.IgnoreAvailability) == 0)
            {
                if (actionSource == ActionSource.Internal) return isSkillAvailableCheck;
                OnSkillCastFailed(resolvedContext, isSkillAvailableCheck);
                return isSkillAvailableCheck;
            }

            // Check if skill is on cooldown for this caster
            OperationResult isSkillOnCooldownCheck = IsSkillOnCooldown(resolvedContext);
            if (!isSkillOnCooldownCheck && (resolvedContext.flags & SkillCastFlags.IgnoreCooldown) == 0)
            {
                if (actionSource == ActionSource.Internal) return isSkillOnCooldownCheck;
                OnSkillCastFailed(resolvedContext, isSkillOnCooldownCheck);
                return isSkillOnCooldownCheck;
            }

            // Check if skill group is on cooldown
            OperationResult groupCooldownCheck = IsSkillGroupOnCooldown(resolvedContext);
            if (!groupCooldownCheck && (resolvedContext.flags & SkillCastFlags.IgnoreCooldown) == 0)
            {
                if (actionSource == ActionSource.Internal) return groupCooldownCheck;
                OnSkillCastFailed(resolvedContext, groupCooldownCheck);
                return groupCooldownCheck;
            }

            // Check if skill has available charges
            if (resolvedContext.skill is ISkillWithCharges {MaxCharges: > 1} chargeSkill)
            {
                int availableCharges = GetAvailableCharges(resolvedContext.skill, chargeSkill.MaxCharges);
                if (availableCharges <= 0 && (resolvedContext.flags & SkillCastFlags.IgnoreCooldown) == 0)
                {
                    OperationResult noChargesResult = SkillOperations.NoChargesAvailable();
                    if (actionSource == ActionSource.Internal) return noChargesResult;
                    OnSkillCastFailed(resolvedContext, noChargesResult);
                    return noChargesResult;
                }
            }
            else
            {
                // Standard active check for non-charge skills
                OperationResult isSkillAlreadyActiveCheck = IsSkillAlreadyCast(resolvedContext);
                if (!isSkillAlreadyActiveCheck)
                {
                    // If ResetOnRecast is set, reset the existing skill state instead of blocking
                    if ((resolvedContext.flags & SkillCastFlags.ResetOnRecast) != 0)
                    {
                        ResetCastSkill(resolvedContext.skill);
                    }
                    else
                    {
                        if (actionSource == ActionSource.Internal) return isSkillAlreadyActiveCheck;
                        OnSkillCastFailed(resolvedContext, isSkillAlreadyActiveCheck);
                        return isSkillAlreadyActiveCheck;
                    }
                }
            }

            // Check if caster has enough resources
            OperationResult hasEnoughSkillResourcesCheck = HasEnoughSkillResources(resolvedContext);
            if (!hasEnoughSkillResourcesCheck && (resolvedContext.flags & SkillCastFlags.IgnoreCosts) == 0)
            {
                if (actionSource == ActionSource.Internal) return hasEnoughSkillResourcesCheck;
                OnSkillCastFailed(resolvedContext, hasEnoughSkillResourcesCheck);
                return hasEnoughSkillResourcesCheck;
            }

            // Consume skill resources if flag is not set
            bool resourcesConsumed = false;
            if((resolvedContext.flags & SkillCastFlags.DoNotConsumeResources) == 0)
            {
                ConsumeSkillResources(resolvedContext);
                resourcesConsumed = true;
            }

            // Check if cast can be performed
            OperationResult canSkillBeCastedCheck = CheckCastAttemptSuccess(resolvedContext);
            if (!canSkillBeCastedCheck && (resolvedContext.flags & SkillCastFlags.IgnoreRequirements) == 0)
            {
                // Refund resources if flag is set and resources were consumed
                if (resourcesConsumed && (resolvedContext.flags & SkillCastFlags.RefundResourcesOnFailure) != 0)
                    RefundSkillResources(resolvedContext);

                if (actionSource == ActionSource.Internal) return canSkillBeCastedCheck;
                OnSkillCastFailed(resolvedContext, canSkillBeCastedCheck);
                return canSkillBeCastedCheck;
            }

            // Clear other levels if activated skill
            if (resolvedContext.skill is ISkillWithLevels withLevels and IActivatedSkill)
            {
                int index = 0;
                SkillBase skill = withLevels.GetSkillForLevel(index);
                SkillBase previousSkill = null;

                // We check at least 3 indices, because design loves to sabotage projects.
                while (!ReferenceEquals(skill, null) || index < 3)
                {
                    // Same result as last iteration means no more distinct levels exist (fallback kicked in)
                    if (ReferenceEquals(skill, previousSkill)) break;

                    if (!ReferenceEquals(skill, null) && IsSkillActivated(skill))
                        DeactivateSkill(skill);

                    previousSkill = skill;
                    index++;
                    skill = withLevels.GetSkillForLevel(index);
                }
            }
            
            // Execute events
            RegisterCastedDataFor(resolvedContext);

            // Set group cooldown after successful cast
            SetGroupCooldownForSkill(resolvedContext.skill);

            return SkillOperations.Casted();
        }

        /// <summary>
        ///     Tries to cancel casted skill
        /// </summary>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TSkill">Type of skill to cancel</typeparam>
        /// <returns>Result of operation</returns>
        public OperationResult TryCancelSkill<TSkill>(
            SkillInterruptFlags flags = SkillInterruptFlags.None,
            ActionSource actionSource = ActionSource.External)
            where TSkill : SkillBase, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return SkillOperations.SkillNotFound();
            return TryCancelSkill(skill, flags, actionSource);
        }

        /// <summary>
        ///     Tries to cancel cast skill
        /// </summary>
        /// <param name="skill">Skill to cancel</param>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        public OperationResult TryCancelSkill(
            [NotNull] SkillBase skill,
            SkillInterruptFlags flags = SkillInterruptFlags.None,
            ActionSource actionSource = ActionSource.External)
        {
            InterruptSkillContext context = new(this, this, skill, flags);
            return TryInterruptSkill(context, actionSource);
        }

        /// <summary>
        ///     Tries to interrupt skill
        /// </summary>
        /// <param name="source">Source of interruption</param>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TSkill">Type of skill to interrupt</typeparam>
        /// <returns>Result of operation</returns>
        public OperationResult TryInterruptSkill<TSkill>(
            [CanBeNull] object source,
            SkillInterruptFlags flags = SkillInterruptFlags.None,
            ActionSource actionSource = ActionSource.External)
            where TSkill : SkillBase, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return SkillOperations.SkillNotFound();
            return TryInterruptSkill(skill, source, flags, actionSource);
        }

        /// <summary>
        ///     Tries to interrupt skill
        /// </summary>
        /// <param name="skill">Skill to interrupt</param>
        /// <param name="source">Source of interruption</param>
        /// <param name="flags">Flags that describe how skill should be casted</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        public OperationResult TryInterruptSkill(
            [NotNull] SkillBase skill,
            [CanBeNull] object source,
            SkillInterruptFlags flags = SkillInterruptFlags.None,
            ActionSource actionSource = ActionSource.External)
        {
            InterruptSkillContext context = new(this, source, skill, flags);
            return TryInterruptSkill(context, actionSource);
        }

        /// <summary>
        ///     Tries to interrupt cast skill
        /// </summary>
        /// <param name="context">Context of skill cast</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        internal OperationResult TryInterruptSkill(
            in InterruptSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            // Ensure skill is casted
            if (!TryGetCastedSkillDataFor(context.skill, out CastedSkillReference skillData))
            {
                OperationResult opResult = SkillOperations.SkillNotCasted();
                if (actionSource == ActionSource.Internal) return opResult;
                OnSkillCastInterruptFailed(context, opResult);
                return opResult;
            }

            // Check if skill is on cooldown
            if (skillData.IsOnCooldown)
            {
                OperationResult opResult = SkillOperations.CooldownNotFinished();
                if (actionSource == ActionSource.Internal) return opResult;
                OnSkillCastInterruptFailed(context, opResult);
                return opResult;
            }

            OperationResult canSkillBeInterruptedCheck = CanSkillBeInterrupted(context);
            if (!canSkillBeInterruptedCheck && (context.flags & SkillInterruptFlags.IgnoreRequirements) == 0)
            {
                if (actionSource == ActionSource.Internal) return canSkillBeInterruptedCheck;
                OnSkillCastInterruptFailed(context, canSkillBeInterruptedCheck);
                return canSkillBeInterruptedCheck;
            }

            // Update casted skill data
            SkillState targetState = context.IsCancellation ? SkillState.Cancelled : SkillState.Interrupted;
            skillData.stateMachine.TryTransitionTo(targetState);
            UpdateCastedSkillDataFor(context.skill, skillData);

            // Execute events
            if (actionSource == ActionSource.Internal) return canSkillBeInterruptedCheck;
            OnSkillCastInterrupted(context, canSkillBeInterruptedCheck);
            return canSkillBeInterruptedCheck;
        }

#endregion

#region Skill List management

        /// <summary>
        ///     List of all currently casted skills
        /// </summary>
        protected readonly List<CastedSkillReference> currentlyCastedSkills = new();

        /// <summary>
        ///     Access to currently casted skills
        /// </summary>
        public IReadOnlyList<CastedSkillReference> CurrentlyCastedSkills => currentlyCastedSkills;

        /// <summary>
        ///     Register casted skill in list. For instant-cast skills (ChargingTime &lt;= 0),
        ///     skips the Charging state and fires OnSkillCastStart immediately.
        /// </summary>
        private void RegisterCastedDataFor(in CastSkillContext context)
        {
            // Convert context to casted skill data
            CastedSkillReference castedSkillReference = new(context.skill, context.flags, context.target);

            // Skip charging for instant-cast skills
            if (context.skill.ChargingTime <= 0)
            {
                SkillState nextState = context.skill is IChannelingSkillBase
                    ? SkillState.Channeling
                    : SkillState.Complete;
                castedSkillReference.stateMachine.TryTransitionTo(nextState);

                currentlyCastedSkills.Add(castedSkillReference);
                OnSkillCastRegistered(context);
                OnSkillCastStart(context);
            }
            else
            {
                currentlyCastedSkills.Add(castedSkillReference);
                OnSkillCastRegistered(context);
            }
        }

        /// <summary>
        ///     Clear casted skill from list
        /// </summary>
        private void ClearCastedSkillDataAt(int index)
        {
            CastSkillContext context = GetCastedSkillContextFor(index);
            currentlyCastedSkills.RemoveAt(index);
            OnSkillCastRemoved(context);
        }

        private void UpdateCastedSkillDataFor([NotNull] SkillBase skill, CastedSkillReference updatedReference)
        {
            for (int index = 0; index < currentlyCastedSkills.Count; index++)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[index];
                if (!ReferenceEquals(castedSkillReference.skill, skill)) continue;
                currentlyCastedSkills[index] = updatedReference;
                break;
            }
        }

        /// <summary>
        ///     Tries to get casted skill data for skill
        /// </summary>
        public bool TryGetCastedSkillDataFor<TSkill>(out CastedSkillReference castedSkillReference)
            where TSkill : SkillBase, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (!ReferenceEquals(skill, null)) return TryGetCastedSkillDataFor(skill, out castedSkillReference);

            castedSkillReference = default;
            return false;
        }

        /// <summary>
        ///     Tries to get casted skill data for skill
        /// </summary>
        public bool TryGetCastedSkillDataFor(
            [NotNull] SkillBase skill,
            out CastedSkillReference castedSkillReference)
        {
            for (int index = 0; index < currentlyCastedSkills.Count; index++)
            {
                castedSkillReference = currentlyCastedSkills[index];
                if (ReferenceEquals(castedSkillReference.skill, skill)) return true;
            }

            castedSkillReference = default;
            return false;
        }

        /// <summary>
        ///     Creates a new <see cref="CastSkillContext"/> instance from currently casted skill data at given index.
        /// </summary>
        /// <param name="index">Index of the currently casted skill in <see cref="CurrentlyCastedSkills"/>.</param>
        /// <returns>A new instance of <see cref="CastSkillContext"/>.</returns>
        private CastSkillContext GetCastedSkillContextFor(int index)
        {
            CastedSkillReference entry = currentlyCastedSkills[index];
            return new CastSkillContext(this, entry.skill, entry.flags, entry.target);
        }


#endregion

#region Checks

        /// <summary>
        ///     Checks if the <paramref name="context"/> skill is available to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill is available to be casted.</returns>
        protected virtual OperationResult IsSkillAvailable(in CastSkillContext context) =>
            context.skill.IsSkillAvailable(context);

        /// <summary>
        ///     Checks if the <paramref name="context"/> skill is currently on cooldown.
        ///     For skills with charges, checks if all charges are on cooldown.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill is on cooldown.</returns>
        protected virtual OperationResult IsSkillOnCooldown(in CastSkillContext context)
        {
            // If skill has no cooldown, it is not on cooldown
            if (!context.skill.HasCooldown) return SkillOperations.Permitted();

            // For charge skills, cooldown check is handled separately via charge count
            if (context.skill is ISkillWithCharges chargeSkill && chargeSkill.MaxCharges > 1)
                return SkillOperations.Permitted();

            // If skill is not casted, it is not on cooldown
            if (!TryGetCastedSkillDataFor(context.skill, out CastedSkillReference data))
                return SkillOperations.Permitted();

            // If skill is casted, check if it is on cooldown
            return data.IsOnCooldown ? SkillOperations.CooldownNotFinished() : SkillOperations.Permitted();
        }


        /// <summary>
        ///     Checks if the <paramref name="context"/> skill has enough resources to be casted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill has enough resources to be casted.</returns>
        protected virtual OperationResult HasEnoughSkillResources(in CastSkillContext context) =>
            context.skill.HasEnoughResources(context);


        /// <summary>
        ///     Checks if the <paramref name="context"/> skill can be casted successfully.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be casted successfully.</returns>
        /// <remarks>
        ///     This method can be used to generate chance-based skills as resources will be consumed before
        ///     casting this check.
        /// </remarks>
        protected virtual OperationResult CheckCastAttemptSuccess(in CastSkillContext context) =>
            context.skill.CheckAttemptSuccess(context);

        /// <summary>
        ///     Checks if the <paramref name="context"/> skill is already actively being cast
        ///     (charging, channeling, or complete but not yet on cooldown).
        ///     Respects <see cref="SkillCastFlags.AllowStacking"/> and <see cref="SkillBase.MaxStacks"/>.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be cast.</returns>
        protected virtual OperationResult IsSkillAlreadyCast(in CastSkillContext context)
        {
            int activeCount = GetActiveStackCount(context.skill);
            if (activeCount == 0) return SkillOperations.Permitted();

            // Allow stacking if flag is set and under max stacks
            if ((context.flags & SkillCastFlags.AllowStacking) != 0)
            {
                return activeCount < context.skill.MaxStacks
                    ? SkillOperations.Permitted()
                    : SkillOperations.SkillMaxStacks();
            }

            return SkillOperations.SkillAlreadyBeingCast();
        }

        /// <summary>
        ///     Returns the number of active (non-cooldown, non-removed) casts for the given skill.
        /// </summary>
        protected int GetActiveStackCount([NotNull] SkillBase skill)
        {
            int count = 0;
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                if (entry.skillState is SkillState.Charging or SkillState.Channeling or SkillState.Complete)
                    count++;
            }
            return count;
        }

        /// <summary>
        ///     Resets an active skill's state by interrupting it. Used when <see cref="SkillCastFlags.ResetOnRecast"/> is set.
        /// </summary>
        private void ResetCastSkill([NotNull] SkillBase skill)
        {
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                if (entry.skillState is SkillState.Charging or SkillState.Channeling or SkillState.Complete)
                {
                    entry.stateMachine.TryTransitionTo(SkillState.Cancelled);
                    currentlyCastedSkills[i] = entry;
                    return;
                }
            }
        }

        /// <summary>
        ///     Checks if the <paramref name="context"/> skill can be interrupted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be interrupted.</returns>
        protected virtual OperationResult CanSkillBeInterrupted(in InterruptSkillContext context) =>
            context.skill.CanBeInterrupted(context);

#endregion

#region Skill Leveling

        /// <summary>
        ///     Returns the current level for a leveled skill on this caster.
        ///     Override to implement per-caster skill progression.
        /// </summary>
        /// <param name="skill">The leveled skill to query</param>
        /// <returns>The skill level (1-based). Default returns 1.</returns>
        protected virtual int GetSkillLevel([NotNull] ISkillWithLevels skill) => skill.Level;

#endregion

#region Cooldown Groups

        /// <summary>
        ///     Active group cooldown timers
        /// </summary>
        protected readonly List<GroupCooldownEntry> groupCooldowns = new();

        /// <summary>
        ///     Read-only access to group cooldowns
        /// </summary>
        public IReadOnlyList<GroupCooldownEntry> GroupCooldowns => groupCooldowns;

        /// <summary>
        ///     Checks if any of the skill's groups (if any) are on cooldown.
        /// </summary>
        protected virtual OperationResult IsSkillGroupOnCooldown(in CastSkillContext context)
        {
            GetSkillGroupTypes(context.skill, sharedGroupTypeBuffer);
            if (sharedGroupTypeBuffer.Count == 0) return SkillOperations.Permitted();

            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                Type groupType = sharedGroupTypeBuffer[g];
                for (int i = 0; i < groupCooldowns.Count; i++)
                {
                    if (groupCooldowns[i].groupType == groupType && !groupCooldowns[i].IsComplete)
                        return SkillOperations.GroupCooldownNotFinished();
                }
            }

            return SkillOperations.Permitted();
        }

        /// <summary>
        ///     Checks if a specific group type is on cooldown.
        /// </summary>
        public bool IsGroupOnCooldown<TGroup>() where TGroup : struct, ISkillGroup
        {
            Type groupType = typeof(TGroup);
            for (int i = 0; i < groupCooldowns.Count; i++)
            {
                if (groupCooldowns[i].groupType == groupType && !groupCooldowns[i].IsComplete)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Sets the group cooldown(s) after a successful cast.
        ///     A skill may belong to multiple groups.
        /// </summary>
        private void SetGroupCooldownForSkill([NotNull] SkillBase skill)
        {
            GetSkillGroupTypes(skill, sharedGroupTypeBuffer);

            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                Type groupType = sharedGroupTypeBuffer[g];
                float cooldown = GetSkillGroupCooldown(groupType);
                if (cooldown <= 0) continue;

                // Replace existing entry or add new one
                bool found = false;
                for (int i = 0; i < groupCooldowns.Count; i++)
                {
                    if (groupCooldowns[i].groupType != groupType) continue;
                    groupCooldowns[i] = new GroupCooldownEntry(groupType, cooldown);
                    found = true;
                    break;
                }

                if (!found)
                    groupCooldowns.Add(new GroupCooldownEntry(groupType, cooldown));
            }
        }

        /// <summary>
        ///     Handles group cooldown timer updates.
        /// </summary>
        protected void HandleGroupCooldowns(float deltaTime)
        {
            for (int i = groupCooldowns.Count - 1; i >= 0; i--)
            {
                GroupCooldownEntry entry = groupCooldowns[i];
                entry.cooldownTimer += deltaTime;
                groupCooldowns[i] = entry;

                if (entry.IsComplete)
                    groupCooldowns.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Reusable buffer for group type lookups to avoid allocation per call.
        /// </summary>
        private readonly List<Type> sharedGroupTypeBuffer = new();

        /// <summary>
        ///     Extracts all group types from a skill implementing IWithSkillGroup.
        ///     Results are written to the provided buffer (cleared first).
        /// </summary>
        private static void GetSkillGroupTypes([NotNull] SkillBase skill, List<Type> buffer)
        {
            buffer.Clear();
            Type skillType = skill.GetType();
            foreach (Type interfaceType in skillType.GetInterfaces())
            {
                if (!interfaceType.IsGenericType) continue;
                if (interfaceType.GetGenericTypeDefinition() != typeof(IWithSkillGroup<>)) continue;
                buffer.Add(interfaceType.GetGenericArguments()[0]);
            }
        }

        /// <summary>
        ///     Gets the cooldown duration for a group type.
        /// </summary>
        private static float GetSkillGroupCooldown([NotNull] Type groupType)
        {
            ISkillGroup group = (ISkillGroup) Activator.CreateInstance(groupType);
            return group.Cooldown;
        }

#endregion

#region Skill Charges

        /// <summary>
        ///     Returns the number of available charges for a skill.
        /// </summary>
        /// <param name="skill">The skill to check</param>
        /// <param name="maxCharges">Maximum charges for this skill</param>
        /// <returns>Number of charges available to use</returns>
        protected int GetAvailableCharges([NotNull] SkillBase skill, int maxCharges)
        {
            int coolingCharges = 0;
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                coolingCharges++;
            }
            return maxCharges - coolingCharges;
        }

        /// <summary>
        ///     Returns the number of available charges for a skill type.
        /// </summary>
        public int GetAvailableCharges<TSkill>() where TSkill : SkillBase, ISkillWithCharges, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return 0;
            return GetAvailableCharges(skill, skill.MaxCharges);
        }

        /// <summary>
        ///     Returns the recharge progress (0 to 1) of the oldest cooling charge for a skill type.
        ///     Returns 1 if no charges are currently recharging.
        /// </summary>
        public float GetOldestChargeRechargeProgress<TSkill>() where TSkill : SkillBase, ISkillWithCharges, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return 1f;

            float oldestProgress = 0f;
            bool found = false;
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                if (!entry.IsOnCooldown) continue;

                found = true;
                float progress = entry.CooldownProgress;
                if (progress > oldestProgress)
                    oldestProgress = progress;
            }
            return found ? oldestProgress : 1f;
        }

#endregion

#region Activated Skills

        /// <summary>
        ///     Set of currently active skills
        /// </summary>
        private readonly List<SkillBase> activeSkills = new();

        /// <summary>
        ///     Activates a skill. Calls <see cref="IActivatedSkill.OnActivated"/>.
        /// </summary>
        /// <returns>Result of the operation</returns>
        protected OperationResult ActivateSkill<TSkill>()
            where TSkill : SkillBase, IActivatedSkill, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return SkillOperations.SkillNotFound();
            return ActivateSkill(skill);
        }

        /// <summary>
        ///     Activates a skill. Calls <see cref="IActivatedSkill.OnActivated"/>.
        /// </summary>
        /// <returns>Result of the operation</returns>
        protected OperationResult ActivateSkill([NotNull] SkillBase skill)
        {
            if (skill is not IActivatedSkill activatedSkill)
            {
                Debug.LogError($"Skill {skill.name} does not implement necessary interface: {nameof(IActivatedSkill)}");
                return SkillOperations.Forbidden();
            }
            
            if (activatedSkill is ISkillWithCharges)
            {
                Debug.LogError($"Activated skill {skill.name} cannot have charges");
                return SkillOperations.Forbidden();
            }

            if (activeSkills.Contains(skill))
                return SkillOperations.SkillAlreadyBeingCast();

            activeSkills.Add(skill);
            activatedSkill.OnActivated();
            return SkillOperations.Permitted();
        }

        /// <summary>
        ///     Deactivates a skill. Calls <see cref="IActivatedSkill.OnDeactivated"/>.
        /// </summary>
        /// <returns>Result of the operation</returns>
        protected OperationResult DeactivateSkill<TSkill>()
            where TSkill : SkillBase, IActivatedSkill, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            if (ReferenceEquals(skill, null)) return SkillOperations.SkillNotFound();
            return DeactivateSkill(skill);
        }

        /// <summary>
        ///     Deactivates a skill. Calls <see cref="IActivatedSkill.OnDeactivated"/>.
        /// </summary>
        /// <returns>Result of the operation</returns>
        protected OperationResult DeactivateSkill([NotNull] SkillBase skill)
        {
            Debug.Assert(skill is IActivatedSkill, $"Skill {skill.name} does not implement IPassiveSkill");

            if (!activeSkills.Remove(skill))
                return SkillOperations.PassiveNotActive();

            ((IActivatedSkill) skill).OnDeactivated();
            return SkillOperations.Permitted();
        }

        /// <summary>
        ///     Checks if a skill is currently active.
        /// </summary>
        public bool IsSkillActivated<TSkill>() where TSkill : SkillBase, IActivatedSkill, new()
        {
            TSkill skill = SkillsDatabase.GetExact<TSkill>();
            return !ReferenceEquals(skill, null) && activeSkills.Contains(skill);
        }

        /// <summary>
        ///     Checks if a skill is currently active.
        /// </summary>
        protected bool IsSkillActivated([NotNull] SkillBase skill) => activeSkills.Contains(skill);

        /// <summary>
        ///     Handles skill tick updates.
        /// </summary>
        protected void HandleActivatedSkillTicks(float deltaTime)
        {
            for (int i = activeSkills.Count - 1; i >= 0; i--)
            {
                ((IActivatedSkill) activeSkills[i]).OnTickWhileActive(deltaTime);
            }
        }

#endregion

#region Utility

        /// <summary>
        ///     Returns the effective cooldown duration for a skill, considering both the skill's own
        ///     cooldown and any group cooldowns it belongs to. Returns the maximum of all applicable cooldowns.
        ///     This is the value that should be displayed on UI cooldown indicators.
        /// </summary>
        /// <param name="skill">The skill to query</param>
        /// <returns>The effective cooldown duration in seconds, or 0 if the skill has no cooldown</returns>
        public float GetSkillEffectiveCooldown([NotNull] SkillBase skill)
        {
            float effectiveCooldown = skill.CooldownTime;

            GetSkillGroupTypes(skill, sharedGroupTypeBuffer);
            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                float groupCooldown = GetSkillGroupCooldown(sharedGroupTypeBuffer[g]);
                if (groupCooldown > effectiveCooldown)
                    effectiveCooldown = groupCooldown;
            }

            return effectiveCooldown;
        }

        /// <summary>
        ///     Returns the effective cooldown progress (0 to 1) for a skill, considering both
        ///     individual skill cooldown and group cooldowns. Returns the least progressed (most time remaining)
        ///     cooldown as that is what blocks the skill from being cast.
        ///     This is the value that should be used for UI cooldown fill indicators.
        /// </summary>
        /// <param name="skill">The skill to query</param>
        /// <returns>
        ///     Normalized progress from 0 (just started) to 1 (complete/ready).
        ///     Returns 1 if the skill is not on any cooldown.
        /// </returns>
        public float GetSkillEffectiveCooldownPercentage([NotNull] SkillBase skill)
        {
            float lowestProgress = 1f;

            // Check individual skill cooldown
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                if (!entry.IsOnCooldown) continue;

                float progress = entry.CooldownProgress;
                if (progress < lowestProgress)
                    lowestProgress = progress;
            }

            // Check group cooldowns
            GetSkillGroupTypes(skill, sharedGroupTypeBuffer);
            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                Type groupType = sharedGroupTypeBuffer[g];
                for (int i = 0; i < groupCooldowns.Count; i++)
                {
                    if (groupCooldowns[i].groupType != groupType) continue;
                    if (groupCooldowns[i].IsComplete) continue;

                    float progress = groupCooldowns[i].Progress;
                    if (progress < lowestProgress)
                        lowestProgress = progress;
                }
            }

            return lowestProgress;
        }

        /// <summary>
        ///     Returns the remaining cooldown time for a skill, considering both individual
        ///     skill cooldown and group cooldowns. Returns the longest remaining time.
        /// </summary>
        /// <param name="skill">The skill to query</param>
        /// <returns>Remaining cooldown time in seconds, or 0 if not on cooldown</returns>
        public float GetSkillCooldownTimeLeft([NotNull] SkillBase skill)
        {
            float longestTimeLeft = 0f;

            // Check individual skill cooldown
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (!ReferenceEquals(entry.skill, skill)) continue;
                if (!entry.IsOnCooldown) continue;

                float timeLeft = entry.CooldownTimeLeft;
                if (timeLeft > longestTimeLeft)
                    longestTimeLeft = timeLeft;
            }

            // Check group cooldowns
            GetSkillGroupTypes(skill, sharedGroupTypeBuffer);
            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                Type groupType = sharedGroupTypeBuffer[g];
                for (int i = 0; i < groupCooldowns.Count; i++)
                {
                    if (groupCooldowns[i].groupType != groupType) continue;
                    if (groupCooldowns[i].IsComplete) continue;

                    float timeLeft = groupCooldowns[i].cooldownDuration - groupCooldowns[i].cooldownTimer;
                    if (timeLeft > longestTimeLeft)
                        longestTimeLeft = timeLeft;
                }
            }

            return longestTimeLeft;
        }

        /// <summary>
        ///     Checks whether a skill is currently blocked by any cooldown (individual or group).
        /// </summary>
        /// <param name="skill">The skill to query</param>
        /// <returns>True if the skill is blocked by any cooldown</returns>
        public bool IsSkillOnAnyCooldown([NotNull] SkillBase skill)
        {
            // Check individual skill cooldown
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference entry = currentlyCastedSkills[i];
                if (ReferenceEquals(entry.skill, skill) && entry.IsOnCooldown)
                    return true;
            }

            // Check group cooldowns
            GetSkillGroupTypes(skill, sharedGroupTypeBuffer);
            for (int g = 0; g < sharedGroupTypeBuffer.Count; g++)
            {
                Type groupType = sharedGroupTypeBuffer[g];
                for (int i = 0; i < groupCooldowns.Count; i++)
                {
                    if (groupCooldowns[i].groupType == groupType && !groupCooldowns[i].IsComplete)
                        return true;
                }
            }

            return false;
        }

#endregion

#region Events


        /// <summary>
        ///     Consumes the resources required to cast the skill.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to consume resources for.</param>
        protected virtual void ConsumeSkillResources(in CastSkillContext context) =>
            context.skill.ConsumeResources(context);

        /// <summary>
        ///     Refunds the resources that were consumed for the skill cast.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to refund resources for.</param>
        protected virtual void RefundSkillResources(in CastSkillContext context) =>
            context.skill.RefundResources(context);

        /// <summary>
        ///     Event raised when the skill cast has started.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        protected virtual void OnSkillCastStart(in CastSkillContext context) =>
            context.skill.OnCastStarted(context);

        /// <summary>
        ///     Event raised when the skill cast has failed.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <param name="reason">The reason why the skill failed.</param>
        protected virtual void OnSkillCastFailed(in CastSkillContext context, in OperationResult reason) =>
            context.skill.OnCastFailed(context, reason);

        /// <summary>
        ///     Event raised when the skill cast is charging.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called every tick while the skill is charging.
        /// </remarks>
        protected virtual void OnSkillTickWhenCharging(in CastSkillContext context) =>
            context.skill.OnCastTickWhenCharging(context);

        /// <summary>
        ///     Event raised when the skill cast is channeling.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called every tick while the skill is channeling.
        /// </remarks>
        protected virtual void OnSkillTickWhenChanneling(in CastSkillContext context) =>
            ((IChannelingSkillBase) context.skill).OnCastTickWhenChanneling(context);

        /// <summary>
        ///     Event raised when the skill cast has ended.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called when the skill cast has finished successfully.
        /// </remarks>
        protected virtual void OnSkillCastEnd(in CastSkillContext context)
        {
            context.skill.OnCastEnded(context);
            
            if (context.skill is IActivatedSkill passiveSkill)
                ActivateSkill((SkillBase) passiveSkill); // Guaranteed to be true
        }


        /// <summary>
        ///     Event raised when the skill cast was interrupted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the skill was interrupted.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling.
        /// </remarks>
        protected virtual void OnSkillCastInterrupted(in InterruptSkillContext context, in OperationResult reason) =>
            context.skill.OnCastInterrupted(context, reason);


        /// <summary>
        ///     Event raised when the skill cast was interrupted but the interrupt attempt failed.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the interrupted skill.</param>
        /// <param name="reason">The reason why the interrupt attempt failed.</param>
        /// <remarks>
        ///     This method is called when the skill was interrupted while it was charging or channeling and the interrupt attempt failed.
        /// </remarks>
        protected virtual void OnSkillCastInterruptFailed(in InterruptSkillContext context, in OperationResult reason)
            => context.skill.OnCastInterruptFailed(context, reason);

        /// <summary>
        ///     Event raised each tick while a skill is on cooldown.
        ///     Override to implement UI cooldown indicators or other per-tick cooldown logic.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the skill on cooldown.</param>
        protected virtual void OnSkillCooldownTick(in CastSkillContext context) =>
            context.skill.OnCooldownTick(context);

        /// <summary>
        ///     Event raised when a skill cast is registered (added to the active cast list).
        ///     Override to react to new casts for AI, UI, or logging.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the registered skill.</param>
        protected virtual void OnSkillCastRegistered(in CastSkillContext context) =>
            context.skill.OnCastRegistered(context);

        /// <summary>
        ///     Event raised when a skill cast is removed from the active cast list
        ///     (cooldown finished or cleared without cooldown).
        ///     Override to react to cast removal for AI, UI, or logging.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the removed skill.</param>
        protected virtual void OnSkillCastRemoved(in CastSkillContext context) =>
            context.skill.OnCastRemoved(context);

#endregion
    }
}
