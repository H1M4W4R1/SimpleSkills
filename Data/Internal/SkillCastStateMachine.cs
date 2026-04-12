using System.Runtime.CompilerServices;

namespace Systems.SimpleSkills.Data.Internal
{
    /// <summary>
    ///     Lightweight state machine that validates and manages skill cast state transitions.
    ///     Designed as a struct for zero-allocation usage within <see cref="CastedSkillReference"/>.
    /// </summary>
    public struct SkillCastStateMachine
    {
        private SkillState state;

        /// <summary>
        ///     Current state of the skill cast
        /// </summary>
        public SkillState CurrentState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => state;
        }

        public SkillCastStateMachine(SkillState initialState)
        {
            state = initialState;
        }

        /// <summary>
        ///     Attempts to transition to the specified state. Returns true if the transition is valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryTransitionTo(SkillState next)
        {
            if (!IsValidTransition(state, next)) return false;
            state = next;
            return true;
        }

        /// <summary>
        ///     Forces a transition to the specified state without validation.
        ///     Use only when the transition is guaranteed to be valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForceTransitionTo(SkillState next)
        {
            state = next;
        }

        /// <summary>
        ///     Checks whether a transition from one state to another is valid.
        /// </summary>
        /// <remarks>
        ///     Valid transitions:
        ///     <list type="bullet">
        ///         <item>Charging → Channeling (skill is IChannelingSkillBase)</item>
        ///         <item>Charging → Complete (skill is not IChannelingSkillBase)</item>
        ///         <item>Channeling → Complete</item>
        ///         <item>Complete → Cooldown</item>
        ///         <item>Interrupted → Cooldown</item>
        ///         <item>Cancelled → Cooldown</item>
        ///         <item>Charging → Interrupted</item>
        ///         <item>Channeling → Interrupted</item>
        ///         <item>Charging → Cancelled</item>
        ///         <item>Channeling → Cancelled</item>
        ///     </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTransition(SkillState from, SkillState to)
        {
            switch (from)
            {
                case SkillState.Charging:
                    return to is SkillState.Channeling or SkillState.Complete
                        or SkillState.Interrupted or SkillState.Cancelled;

                case SkillState.Channeling:
                    return to is SkillState.Complete
                        or SkillState.Interrupted or SkillState.Cancelled;

                case SkillState.Complete:
                    return to is SkillState.Cooldown;

                case SkillState.Interrupted:
                    return to is SkillState.Cooldown;

                case SkillState.Cancelled:
                    return to is SkillState.Cooldown;

                default:
                    return false;
            }
        }
    }
}
