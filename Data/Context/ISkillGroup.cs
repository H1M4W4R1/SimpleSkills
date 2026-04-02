namespace Systems.SimpleSkills.Data.Context
{
    /// <summary>
    ///     Marker interface for skill group types.
    ///     Implement as a struct for zero-allocation group identification.
    /// </summary>
    /// <example>
    ///     <code>
    ///     public struct PotionGroup : ISkillGroup
    ///     {
    ///         public float Cooldown => 1.5f;
    ///     }
    ///     </code>
    /// </example>
    public interface ISkillGroup
    {
        /// <summary>
        ///     Shared cooldown duration for all skills in this group
        /// </summary>
        float Cooldown { get; }
    }
}
