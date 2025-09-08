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
using UnityEngine.Assertions;

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
        }

        /// <summary>
        ///     Method that is responsible for handling skill charging state (if any exists)
        /// </summary>
        protected void HandleCharging(float deltaTime)
        {
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
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
                    castedSkillReference.skillState = castedSkillReference.skill is ChannelingSkillBase
                        ? SkillState.Channeling
                        : SkillState.Complete;

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
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];
                if (castedSkillReference.skill is not ChannelingSkillBase channelingSkill) continue;

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
                    castedSkillReference.skillState = SkillState.Complete;

                currentlyCastedSkills[i] = castedSkillReference;
            }
        }

        /// <summary>
        ///     Method that is responsible for handling skill completion state (if skill channeling was completed,
        ///     skill casting was finished or skill was cancelled / interrupted).
        /// </summary>
        protected void HandleSkillsCompleted(float deltaTime)
        {
            for (int i = 0; i < currentlyCastedSkills.Count; i++)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[i];

                // Skill has to be casted
                if (!castedSkillReference.IsCastComplete) continue;

                // Skill has not yet started cooldown
                if (castedSkillReference.IsOnCooldown) continue;

                // Handle cast end event if wasn't cancelled / interrupted
                if (castedSkillReference.skillState == SkillState.Complete)
                    OnSkillCastEnd(GetCastedSkillContextFor(i));

                // Update data
                castedSkillReference.skillState = SkillState.Cooldown;
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

                // Clear casted skill context if cooldown is finished
                if (castedSkillReference.cooldownTimer >= castedSkillReference.skill.CooldownTime)
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
            Assert.IsNotNull(skill, "Skill was not found in database");
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

            // Consume skill resources if flag is not set
            if((context.flags & SkillCastFlags.DoNotConsumeResources) == 0)
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
            RegisterCastedDataFor(context);
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
            Assert.IsNotNull(skill, "Skill was not found in database");
            return TryCancelSkill(skill, flags, actionSource);
        }

        /// <summary>
        ///     Tries to cancel casted skill
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
            Assert.IsNotNull(skill, "Skill was not found in database");
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
        ///     Tries to interrupt casted skill
        /// </summary>
        /// <param name="context">Context of skill cast</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of operation</returns>
        internal OperationResult TryInterruptSkill(
            in InterruptSkillContext context,
            ActionSource actionSource = ActionSource.External)
        {
            // Ensure skill is casted
            CastedSkillReference? skillData = GetCastedSkillDataFor(context.skill);
            if (skillData is null)
            {
                OperationResult opResult = SkillOperations.SkillNotCasted();
                if (actionSource == ActionSource.Internal) return opResult;
                OnSkillCastInterruptFailed(context, opResult);
                return opResult;
            }

            // Check if skill is on cooldown
            if (skillData.Value.IsOnCooldown)
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
            CastedSkillReference skillReferenceValue = skillData.Value;
            skillReferenceValue.skillState = SkillState.Interrupted;
            UpdateCastedSkillDataFor(context.skill, skillReferenceValue);

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
        ///     Register casted skill in list
        /// </summary>
        private void RegisterCastedDataFor(in CastSkillContext context)
        {
            // Convert context to casted skill data
            CastedSkillReference castedSkillReference = new(context.skill, context.flags);
            currentlyCastedSkills.Add(castedSkillReference);
        }

        /// <summary>
        ///     Clear casted skill from list
        /// </summary>
        private void ClearCastedSkillDataAt(int index)
        {
            currentlyCastedSkills.RemoveAt(index);
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
            Assert.IsNotNull(skill, "Skill was not found in database");
            return TryGetCastedSkillDataFor(skill, out castedSkillReference);
        }

        /// <summary>
        ///     Tries to get casted skill data for skill
        /// </summary>
        public bool TryGetCastedSkillDataFor(
            [NotNull] SkillBase skill,
            out CastedSkillReference castedSkillReference)
        {
            castedSkillReference = default;

            CastedSkillReference? internalResult = GetCastedSkillDataFor(skill);
            if (internalResult is null) return false;
            castedSkillReference = internalResult.Value;
            return true;
        }

        /// <summary>
        ///     Creates a new <see cref="CastSkillContext"/> instance from currently casted skill data at given index.
        /// </summary>
        /// <param name="index">Index of the currently casted skill in <see cref="CurrentlyCastedSkills"/>.</param>
        /// <returns>A new instance of <see cref="CastSkillContext"/>.</returns>
        private CastSkillContext GetCastedSkillContextFor(int index)
        {
            return new CastSkillContext(this, currentlyCastedSkills[index].skill,
                currentlyCastedSkills[index].flags);
        }

        
        /// <summary>
        ///     Tries to get the <see cref="CastedSkillReference"/> for the given <paramref name="skill"/>.
        /// </summary>
        /// <param name="skill">The <see cref="SkillBase"/> to find the casted data for.</param>
        /// <returns>The <see cref="CastedSkillReference"/> if found, otherwise <see langword="null"/>.</returns>
        /// <remarks>
        ///     This method uses reference equality to find the casted skill data.
        /// </remarks>
        private CastedSkillReference? GetCastedSkillDataFor([NotNull] SkillBase skill)
        {
            for (int index = 0; index < currentlyCastedSkills.Count; index++)
            {
                CastedSkillReference castedSkillReference = currentlyCastedSkills[index];
                if (ReferenceEquals(castedSkillReference.skill, skill)) return castedSkillReference;
            }

            return null;
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
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill is on cooldown.</returns>
        /// <remarks>
        ///     If the skill has no cooldown, it is never on cooldown.
        ///     If the skill is not casted, it is not on cooldown.
        ///     If the skill is casted, it is on cooldown if its cooldown timer has not finished yet.
        /// </remarks>
        protected virtual OperationResult IsSkillOnCooldown(in CastSkillContext context)
        {
            // If skill has no cooldown, it is not on cooldown
            if (!context.skill.HasCooldown) return SkillOperations.Permitted();

            // If skill is not casted, it is not on cooldown
            CastedSkillReference? data = GetCastedSkillDataFor(context.skill);
            if (data is null) return SkillOperations.Permitted();

            // If skill is casted, check if it is on cooldown
            return data.Value.IsOnCooldown ? SkillOperations.CooldownNotFinished() : SkillOperations.Permitted();
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
        ///     Checks if the <paramref name="context"/> skill can be interrupted.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to check.</param>
        /// <returns>An <see cref="OperationResult"/> indicating whether the skill can be interrupted.</returns>
        protected virtual OperationResult CanSkillBeInterrupted(in InterruptSkillContext context) =>
            context.skill.CanBeInterrupted(context);
      
#endregion

#region Events

        
        /// <summary>
        ///     Consumes the resources required to cast the skill.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> to consume resources for.</param>
        protected virtual void ConsumeSkillResources(in CastSkillContext context) =>
            context.skill.ConsumeResources(context);

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
        protected virtual void OnSkillTickWhenChanneling(in CastSkillContext context)
        {
            if (context.skill is ChannelingSkillBase channelingSkillBase)
                channelingSkillBase.OnCastTickWhenChanneling(context);
            else
                Debug.LogError($"Skill {context.skill.name} is not a channeling skill");
        }

        /// <summary>
        ///     Event raised when the skill cast has ended.
        /// </summary>
        /// <param name="context">The <see cref="CastSkillContext"/> of the casted skill.</param>
        /// <remarks>
        ///     This method is called when the skill cast has finished successfully.
        /// </remarks>
        protected virtual void OnSkillCastEnd(in CastSkillContext context) =>
            context.skill.OnCastEnded(context);

        
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

#endregion
    }
}