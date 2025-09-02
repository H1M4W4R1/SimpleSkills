using Systems.SimpleCore.Automation.Attributes;
using UnityEngine;

namespace Systems.SimpleSkills.Data
{
    [AutoCreate("Skills", SkillsDatabase.LABEL)]
    public abstract class SkillBase : ScriptableObject
    {
        // TODO: Properties: Cooldown, Duration (ChannelingSkillBase), IsChanneling
        
        // TODO: IsOnCooldown
        
        // TODO: HasEnoughResources
        
        // TODO: CanBeCast
        
        // TODO: OnCastFailed
        
        // TODO: ConsumeResources
        
        // TODO: OnCasted
        
        // TODO: CanBeInterrupted
        
        // TODO: OnCastInterrupted
        
        // TODO: CanBeCancelled
        
        // TODO: OnCastCancelled
        
        // TODO: OnCastEnded
    }
}