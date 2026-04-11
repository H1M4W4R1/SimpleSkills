using Systems.SimpleSkills.Data.Context;

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Shared cooldown group for healing consumables.
    ///     Casting any skill in this group places all group members on a 30-second cooldown.
    /// </summary>
    public struct HealingPotionSkillGroup : ISkillGroup
    {
        public float Cooldown => 30f;
    }
}
