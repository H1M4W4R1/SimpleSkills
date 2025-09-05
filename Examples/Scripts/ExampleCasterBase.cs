using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Components;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    public sealed class ExampleCasterBase : SkillCasterBase
    {
        [ContextMenu("Cast channeling skill")]
        public void CastChannelingSkill()
        {
            TryCastSkill<ExampleChannelingSkill>();
        }
        
        [ContextMenu("Cancel channeling skill")]
        public void CancelChannelingSkill()
        {
            if(OperationResult.IsError(TryCancelSkill<ExampleChannelingSkill>()))
               Debug.LogError("Failed to cancel channeling skill");
            else Debug.Log("Channeling skill cancelled");
        }
        
        [ContextMenu("Interrupt channeling skill")]
        public void InterruptChannelingSkill()
        {
            if(OperationResult.IsError(TryInterruptSkill<ExampleChannelingSkill>(null)))
               Debug.LogError("Failed to interrupt channeling skill");
            else Debug.Log("Channeling skill interrupted");
        }
        
        [ContextMenu("Cast one-time skill")]
        public void CastRegularSkill()
        {
            TryCastSkill<ExampleOneTimeSkill>();
        }
    }
}