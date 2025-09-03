using Systems.SimpleCore.Operations;

namespace Systems.SimpleSkills.Operations
{
    public static class SkillOperations
    {
        public const ushort SYSTEM_SKILL = 0x0008;

        public const ushort ERROR_SKILL_ON_COOLDOWN = 0x0001;
        public const ushort ERROR_NOT_ENOUGH_RESOURCES = 0x0002;

        public const ushort SUCCESS_CAST_STARTED = 0x0001;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_SKILL, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_SKILL, OperationResult.ERROR_DENIED);

        public static OperationResult Casted() => OperationResult.Success(SYSTEM_SKILL, SUCCESS_CAST_STARTED);
    }
}