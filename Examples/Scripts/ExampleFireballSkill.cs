using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Abstract base for a leveled fireball skill.
    ///     Each level variant is a separate ScriptableObject asset in the database.
    ///     The caster resolves the correct level at cast-time via <c>SkillCasterBase.GetSkillLevel</c>.
    /// </summary>
    public abstract class ExampleFireballSkill : SkillWithLevels<ExampleFireballSkill>
    {
        /// <summary>
        ///     Abstract — each level variant must declare its own level.
        /// </summary>
        public abstract override int Level { get; }

        /// <summary>
        ///     Damage output for this level variant.
        /// </summary>
        protected abstract int Damage { get; }

        public override float ChargingTime => 0.5f;

        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} (level {Level}) — fireball launched by {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            Debug.Log($"Skill {name} (level {Level}) — fireball hit for {Damage} damage on {context.caster.name}");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if (OperationResult.AreSimilar(reason, SkillOperations.CooldownNotFinished()))
                Debug.LogError($"Skill {name} — cooldown not finished for {context.caster.name}");
        }
    }

    /// <summary>Level 1 — 10 damage, 3s cooldown.</summary>
    public sealed class ExampleFireballSkillLevel1 : ExampleFireballSkill
    {
        public override int Level => 1;
        protected override int Damage => 10;
        public override float CooldownTime => 3f;
    }

    /// <summary>Level 2 — 25 damage, 2.5s cooldown.</summary>
    public sealed class ExampleFireballSkillLevel2 : ExampleFireballSkill
    {
        public override int Level => 2;
        protected override int Damage => 25;
        public override float CooldownTime => 2.5f;
    }

    /// <summary>Level 3 — 50 damage, 2s cooldown.</summary>
    public sealed class ExampleFireballSkillLevel3 : ExampleFireballSkill
    {
        public override int Level => 3;
        protected override int Damage => 50;
        public override float CooldownTime => 2f;
    }
}
