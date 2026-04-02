using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Data.Enums;
using Unity.Mathematics;

namespace Systems.SimpleSkills.Data.Internal
{
    public struct CastedSkillReference
    {
        /// <summary>
        ///     Skill being casted
        /// </summary>
        [NotNull] public readonly SkillBase skill;

        /// <summary>
        ///     Flags for the cast
        /// </summary>
        public readonly SkillCastFlags flags;

        /// <summary>
        ///     Time spent charging
        /// </summary>
        public float chargingTimer;

        /// <summary>
        ///     Timer spent channeling
        /// </summary>
        public float channelingTimer;

        /// <summary>
        ///     Timer spent cooling down
        /// </summary>
        public float cooldownTimer;

        /// <summary>
        ///     State machine managing skill cast transitions
        /// </summary>
        public SkillCastStateMachine stateMachine;

        /// <summary>
        ///     Whether this skill was interrupted or cancelled before entering cooldown.
        ///     Used to apply <see cref="SkillBase.InterruptedCooldownMultiplier"/>.
        /// </summary>
        public bool wasInterrupted;

        /// <summary>
        ///     Optional target for this skill cast. Stored so it's available during tick callbacks.
        /// </summary>
        [CanBeNull] public readonly ISkillTarget target;

        /// <summary>
        ///     State of the skill. Delegates to the state machine.
        /// </summary>
        public SkillState skillState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => stateMachine.CurrentState;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => stateMachine.ForceTransitionTo(value);
        }

        public CastedSkillReference([NotNull] SkillBase contextSkill, SkillCastFlags flags,
            [CanBeNull] ISkillTarget target = null)
        {
            skill = contextSkill;
            chargingTimer = 0;
            channelingTimer = 0;
            cooldownTimer = 0;
            stateMachine = new SkillCastStateMachine(SkillState.Charging);
            wasInterrupted = false;
            this.flags = flags;
            this.target = target;
        }

        /// <summary>
        ///     Checks if charging is complete
        /// </summary>
        public bool IsChargingComplete => skillState > SkillState.Charging;

        /// <summary>
        ///     Checks if cast is complete (channeling was complete or simply skill was casted)
        /// </summary>
        public bool IsCastComplete
            => skillState is SkillState.Complete or SkillState.Interrupted or SkillState.Cancelled;

        /// <summary>
        ///     Checks if skill is on cooldown
        /// </summary>
        public bool IsOnCooldown => skillState == SkillState.Cooldown;

        /// <summary>
        ///     Normalized charging progress (0 to 1). Returns 1 if skill doesn't have charging time
        /// </summary>
        public float ChargingProgress
        {
            get
            {
                if (skill.ChargingTime <= 0) return 1f;
                return math.clamp(chargingTimer / skill.ChargingTime, 0, 1);
            }
        }

        /// <summary>
        ///     [Usually] Normalized channeling progress (0 to 1). Returns 1 if skill is not a channeling skill.
        ///     Returns -1 if skill has infinite channeling time.
        /// </summary>
        public float ChannelingProgress
        {
            get
            {
                if (skill is not IChannelingSkillBase channelingSkill) return 1;
                if (channelingSkill.Duration <= 0) return -1;
                return math.clamp(channelingTimer / channelingSkill.Duration, 0, 1);
            }
        }

        /// <summary>
        ///     Normalized cooldown progress (0 to 1). Returns 1 if skill doesn't have cooldown time
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (skill.CooldownTime <= 0) return 1f;
                return math.clamp(cooldownTimer / skill.CooldownTime, 0, 1);
            }
        }

        /// <summary>
        ///     Skill charging time, returns -1 if skill doesn't have charging time
        /// </summary>
        public float ChargingTime => skill.ChargingTime > 0 ? skill.ChargingTime : -1;

        /// <summary>
        ///     Skill charging time left, returns -1 if skill doesn't have charging time
        /// </summary>
        public float ChargingTimeLeft =>
            skill.ChargingTime > 0 ? skill.ChargingTime - chargingTimer : -1;

        /// <summary>
        ///     Skill channeling total duration, returns -1 if skill is not a channeling skill or is infinite
        /// </summary>
        public float ChannelingTime => skill is IChannelingSkillBase channelingSkill
            ? channelingSkill.Duration > 0 ? channelingSkill.Duration : -1
            : -1;

        /// <summary>
        ///     Skill channeling time left, returns -1 if skill is not a channeling skill or is infinite
        /// </summary>
        public float ChannelingTimeLeft => skill is IChannelingSkillBase channelingSkill
            ? channelingSkill.IsInfinite ? -1 : channelingSkill.Duration - channelingTimer
            : -1;

        /// <summary>
        ///     Skill cooldown time, returns -1 if skill doesn't have cooldown time
        /// </summary>
        public float CooldownTime => skill.CooldownTime > 0 ? skill.CooldownTime : -1;

        /// <summary>
        ///     Skill cooldown time left, returns -1 if skill doesn't have cooldown time
        /// </summary>
        public float CooldownTimeLeft => skill.CooldownTime > 0 ? skill.CooldownTime - cooldownTimer : -1;
    }
}
