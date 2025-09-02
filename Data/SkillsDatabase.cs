using JetBrains.Annotations;
using Systems.SimpleCore.Storage;

namespace Systems.SimpleSkills.Data
{
    /// <summary>
    ///     Database with all known Skills
    /// </summary>
    public sealed class SkillsDatabase : AddressableDatabase<SkillsDatabase, SkillBase>
    {
        public const string LABEL = "SimpleSkills.Skills";
        [NotNull] protected override string AddressableLabel => LABEL;
    }
}