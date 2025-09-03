namespace Systems.SimpleSkills.Data.Internal
{
    /// <summary>
    ///     States of skill, order matters!
    /// </summary>
    public enum SkillState
    {
        Unknown = 0,
        Charging = 1,
        Channeling = 2,
        Complete = 3,
        Interrupted = 4,
        Cancelled = 5,
        Cooldown = 6
    }
}