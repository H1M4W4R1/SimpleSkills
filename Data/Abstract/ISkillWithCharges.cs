namespace Systems.SimpleSkills.Data.Abstract
{
    /// <summary>
    ///     Interface for skills that support multiple charges before entering cooldown.
    ///     Each charge recharges independently via its own cooldown entry in the casted skills list.
    /// </summary>
    /// <remarks>
    ///     When a skill with charges is cast, each use consumes one charge.
    ///     Available charges = <see cref="MaxCharges"/> - (number of cooldown entries for this skill).
    ///     When all charges are consumed, the skill cannot be cast until at least one charge recharges.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     public class DashSkill : SkillBase, ISkillWithCharges
    ///     {
    ///         public int MaxCharges => 2;
    ///         public override float CooldownTime => 5f;
    ///     }
    ///     </code>
    /// </example>
    public interface ISkillWithCharges
    {
        /// <summary>
        ///     Maximum number of charges this skill can hold.
        ///     Default is 1 (single charge, behaves like a regular skill).
        /// </summary>
        int MaxCharges => 1;
    }
}
