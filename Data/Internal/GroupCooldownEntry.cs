using System;

namespace Systems.SimpleSkills.Data.Internal
{
    /// <summary>
    ///     Tracks the cooldown state of a skill group.
    /// </summary>
    public struct GroupCooldownEntry
    {
        /// <summary>
        ///     Type of the skill group (used for identity comparison)
        /// </summary>
        public readonly Type groupType;

        /// <summary>
        ///     Total cooldown duration for this group
        /// </summary>
        public readonly float cooldownDuration;

        /// <summary>
        ///     Time spent cooling down
        /// </summary>
        public float cooldownTimer;

        public GroupCooldownEntry(Type groupType, float cooldownDuration)
        {
            this.groupType = groupType;
            this.cooldownDuration = cooldownDuration;
            cooldownTimer = 0;
        }

        /// <summary>
        ///     Whether this group cooldown has finished
        /// </summary>
        public bool IsComplete => cooldownTimer >= cooldownDuration;

        /// <summary>
        ///     Normalized progress (0 to 1)
        /// </summary>
        public float Progress => cooldownDuration > 0
            ? UnityEngine.Mathf.Clamp01(cooldownTimer / cooldownDuration)
            : 1f;
    }
}
