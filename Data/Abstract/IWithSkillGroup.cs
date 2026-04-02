using Systems.SimpleSkills.Data.Context;

namespace Systems.SimpleSkills.Data.Abstract
{
    /// <summary>
    ///     Interface that skills implement to declare membership in a cooldown group.
    ///     When a skill with this interface finishes casting, the group cooldown is triggered
    ///     for all skills sharing the same group type.
    /// </summary>
    /// <typeparam name="TSkillGroup">
    ///     The group type. Must be a struct implementing <see cref="ISkillGroup"/> for zero-allocation.
    /// </typeparam>
    /// <example>
    ///     <code>
    ///     public class HealthPotionSkill : SkillBase, IWithSkillGroup&lt;PotionGroup&gt; { }
    ///     public class ManaPotionSkill : SkillBase, IWithSkillGroup&lt;PotionGroup&gt; { }
    ///     </code>
    /// </example>
    public interface IWithSkillGroup<TSkillGroup> where TSkillGroup : struct, ISkillGroup
    {
    }
}
