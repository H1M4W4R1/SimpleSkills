using Systems.SimpleCore.Operations;

namespace Systems.SimpleSkills.Operations
{
    public static class SkillOperations
    {
        public const ushort SYSTEM_SKILL = 0x0008;

        public const ushort SUCCESS_CAST_STARTED = 0x0001;
        public const ushort ERROR_COOLDOWN_NOT_FINISHED = 0x0002;
        public const ushort ERROR_SKILL_NOT_CASTED = 0x0003;
        public const ushort ERROR_SKILL_ALREADY_ACTIVE = 0x0004;
        public const ushort ERROR_SKILL_MAX_STACKS = 0x0005;
        public const ushort ERROR_SKILL_NOT_FOUND = 0x0006;
        public const ushort ERROR_NO_TARGET_SELECTED = 0x0007;
        public const ushort ERROR_NO_CHARGES_AVAILABLE = 0x0008;
        public const ushort ERROR_GROUP_COOLDOWN_NOT_FINISHED = 0x0009;
        public const ushort ERROR_PASSIVE_ALREADY_ACTIVE = 0x000A;
        public const ushort ERROR_PASSIVE_NOT_ACTIVE = 0x000B;
        public const ushort SUCCESS_SKILL_DEACTIVATED = 0x000C;
        public const ushort ERROR_FORBIDDEN = 0x000D;
        

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_SKILL, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_SKILL, OperationResult.ERROR_DENIED);

        public static OperationResult SkillDeactivated()
            => OperationResult.Success(SYSTEM_SKILL, SUCCESS_SKILL_DEACTIVATED);
        
        public static OperationResult CooldownNotFinished() => OperationResult.Error(SYSTEM_SKILL, ERROR_COOLDOWN_NOT_FINISHED);
        public static OperationResult SkillNotCasted() => OperationResult.Error(SYSTEM_SKILL, ERROR_SKILL_NOT_CASTED);
        public static OperationResult SkillAlreadyBeingCast() => OperationResult.Error(SYSTEM_SKILL, ERROR_SKILL_ALREADY_ACTIVE);
        public static OperationResult SkillMaxStacks() => OperationResult.Error(SYSTEM_SKILL, ERROR_SKILL_MAX_STACKS);
        public static OperationResult SkillNotFound() => OperationResult.Error(SYSTEM_SKILL, ERROR_SKILL_NOT_FOUND);
        public static OperationResult NoTargetSelected() => OperationResult.Error(SYSTEM_SKILL, ERROR_NO_TARGET_SELECTED);
        public static OperationResult NoChargesAvailable() => OperationResult.Error(SYSTEM_SKILL, ERROR_NO_CHARGES_AVAILABLE);
        public static OperationResult GroupCooldownNotFinished() => OperationResult.Error(SYSTEM_SKILL, ERROR_GROUP_COOLDOWN_NOT_FINISHED);
        public static OperationResult PassiveNotActive() => OperationResult.Error(SYSTEM_SKILL, ERROR_PASSIVE_NOT_ACTIVE);

        
        public static OperationResult Casted() => OperationResult.Success(SYSTEM_SKILL, SUCCESS_CAST_STARTED);

        public static OperationResult Forbidden() => OperationResult.Error(SYSTEM_SKILL, ERROR_FORBIDDEN);
    }
}
