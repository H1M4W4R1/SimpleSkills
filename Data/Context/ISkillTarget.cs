namespace Systems.SimpleSkills.Data.Context
{
    /// <summary>
    ///     Marker interface for skill targets.
    ///     Any object can implement this (enemies, allies, world positions, etc.).
    /// </summary>
    /// <example>
    ///     <code>
    ///     public class Enemy : MonoBehaviour, ISkillTarget { }
    ///     public struct WorldPosition : ISkillTarget { public Vector3 Position; }
    ///     </code>
    /// </example>
    public interface ISkillTarget
    {
    }
}
