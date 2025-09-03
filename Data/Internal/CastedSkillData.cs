using System;
using JetBrains.Annotations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Data.Enums;

namespace Systems.SimpleSkills.Data.Internal
{
    public struct CastedSkillData
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
        ///     State of the skill
        /// </summary>
        public SkillState skillState;

        public CastedSkillData([NotNull] SkillBase contextSkill, SkillCastFlags flags)
        {
            skill = contextSkill;
            chargingTimer = 0;
            channelingTimer = 0;
            cooldownTimer = 0;
            skillState = SkillState.Charging;
            this.flags = flags;
        }

        public bool IsChargingComplete => skillState > SkillState.Charging;

        /// <summary>
        ///     Checks if cast is complete (channeling was complete or simply skill was casted)
        /// </summary>
        public bool IsCastComplete => skillState is SkillState.Complete or SkillState.Interrupted or SkillState.Cancelled;

        /// <summary>
        ///     Checks if skill is on cooldown
        /// </summary>
        public bool IsOnCooldown => skillState == SkillState.Cooldown;
    }
}