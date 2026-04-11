using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    public sealed class ExampleCasterBase : SkillCasterBase
    {
        [SerializeField] private int fireballLevel = 1;

        /// <summary>
        ///     Override to drive fireball level from the serialized field.
        ///     Any other leveled skill falls back to the default (skill's own Level property).
        /// </summary>
        protected override int GetSkillLevel(ISkillWithLevels skill)
        {
            if (skill is ExampleFireballSkill)
                return fireballLevel;

            return base.GetSkillLevel(skill);
        }

        [ContextMenu("Cast channeling skill")]
        public void CastChannelingSkill()
        {
            TryCastSkill<ExampleIChannelingSkill>();
        }
        
        [ContextMenu("Cancel channeling skill")]
        public void CancelChannelingSkill()
        {
            if(OperationResult.IsError(TryCancelSkill<ExampleIChannelingSkill>()))
               Debug.LogError("Failed to cancel channeling skill");
            else Debug.Log("Channeling skill cancelled");
        }
        
        [ContextMenu("Interrupt channeling skill")]
        public void InterruptChannelingSkill()
        {
            if(OperationResult.IsError(TryInterruptSkill<ExampleIChannelingSkill>(null)))
               Debug.LogError("Failed to interrupt channeling skill");
            else Debug.Log("Channeling skill interrupted");
        }
        
        [ContextMenu("Cast one-time skill")]
        public void CastRegularSkill()
        {
            TryCastSkill<ExampleOneTimeSkill>();
        }

        [ContextMenu("Cast dash skill (charges)")]
        public void CastDashSkill()
        {
            TryCastSkill<ExampleDashSkill>();
        }

        [ContextMenu("Cast health potion (skill group)")]
        public void CastHealthPotion()
        {
            TryCastSkill<ExampleHealthPotionSkill>();
        }

        [ContextMenu("Cast mushroom (skill group)")]
        public void CastMushroom()
        {
            TryCastSkill<ExampleMushroomSkill>();
        }

        /// <summary>
        ///     Casts the fireball at the level configured in <see cref="fireballLevel"/>.
        ///     Any level-1 variant is used as the entry point — the system resolves the correct asset.
        /// </summary>
        [ContextMenu("Cast fireball (leveled skill)")]
        public void CastFireball()
        {
            TryCastSkill<ExampleFireballSkillLevel1>();
        }

        /// <summary>
        ///     Toggles the regeneration aura on/off.
        ///     First call activates it; casting again while active deactivates it.
        /// </summary>
        [ContextMenu("Toggle regeneration aura (activated skill)")]
        public void ToggleRegenerationAura()
        {
            TryCastSkill<ExampleRegenerationAuraSkill>();
        }
    }
}