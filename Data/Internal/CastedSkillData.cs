using JetBrains.Annotations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;

namespace Systems.SimpleSkills.Data.Internal
{
    public struct CastedSkillData
    {
        /// <summary>
        ///     Skill being casted
        /// </summary>
        [NotNull] public readonly SkillBase skill;

        /// <summary>
        ///     Time spent charging
        /// </summary>
        public float chargingTimer;

        /// <summary>
        ///     Timer spent channeling
        /// </summary>
        public float channelingTimer;

        public CastedSkillData([NotNull] SkillBase contextSkill)
        {
            skill = contextSkill;
            chargingTimer = 0;
            channelingTimer = 0;
        }

        public bool IsChargingComplete => chargingTimer >= skill.ChargingTime;

        /// <summary>
        ///     Checks if cast is complete (channeling was complete or simply skill was casted)
        /// </summary>
        public bool IsCastComplete
        {
            get
            {
                // If charging is not complete, cast is not complete
                if (!IsChargingComplete) return false;

                // If skill is channeling, check if channeling is complete
                if (skill is ChannelingSkillBase channelingSkill)
                    return channelingTimer >= channelingSkill.Duration;

                // If skill is not channeling, it is complete
                return true;
            }
        }
    }
}