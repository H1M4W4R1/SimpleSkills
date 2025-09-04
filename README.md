<div align="center">
  <h1>Simple Skills</h1>
</div>

# About

Simple skills is a package of SimpleKit intended for quick and easy implementation of skills -
either charging, channeling or simple ones casted immediately.

*For requirements check .asmdef*

# Creating a skill
To create a skill you simply need to extend proper class such as `SkillBase` or
`ChargingSkillBase`. Afterward, you can implement desired events and validation methods.

```csharp
public sealed class ExampleOneTimeSkill : SkillBase
    {
        public override float CooldownTime => 5f;

        public override float ChargingTime => 1f;

        protected internal override void OnCastTickWhenCharging(in CastSkillContext context)
        {
            base.OnCastTickWhenCharging(in context);
            Debug.Log($"Skill {name} charging for {context.caster.name}");
        }

        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} started for {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            Debug.Log($"Skill {name} ended for {context.caster.name}");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if(OperationResult.AreSimilar(reason, SkillOperations.CooldownNotFinished()))
                Debug.LogError($"Skill {name} failed for {context.caster.name} because cooldown is not finished");
        }
    }
```

# Creating Skill Caster

Skill system uses separate objects for casting skills that are independent of other SimpleKit
systems. They're called SkillCasters and handle all necessary logic.

To create a caster you shall extend `SkillCasterBase` which is MonoBehaviour you should attach
to your caster GameObject.

```csharp
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
            if(OperationResult.IsError(TryInterruptSkill<ExampleChannelingSkill>()))
               Debug.LogError("Failed to interrupt channeling skill");
            else Debug.Log("Channeling skill interrupted");
        }
        
        [ContextMenu("Cast one-time skill")]
        public void CastRegularSkill()
        {
            TryCastSkill<ExampleOneTimeSkill>();
        }
    }
```

You can cast skills using type parameter for specific skill or `TryCastSkill` method with
SkillBase parameter where you can cast using skill instance.

Example:
```csharp
SkillBase mySkill = SkillsDatabase.GetAbstract<MySkillBase>();
Assert.IsNotNull(mySkill, "Skill was not found in database!");
TryCastSkill(mySkill, SkillCastFlags.DoNotConsumeResources, ActionSource.Internal);
```

Flags can be combined using `|` operator and modify casting behaviour for needs of your
specific case. For all available flags check `SkillCastFlags` enum.

# Interrupting and canceling skills

Some skills may support interrupting and canceling. To interrupt skill you can use
`TryInterruptSkill` method with type parameter for specific skill or SkillBase parameter,
same as with `TryCastSkill`.

Example:
```csharp
        public void InterruptChannelingSkill()
        {
            if(OperationResult.IsError(TryInterruptSkill<ExampleChannelingSkill>()))
               Debug.LogError("Failed to interrupt channeling skill");
            else Debug.Log("Channeling skill interrupted");
        }
```

To cancel skill you can use `TryCancelSkill` method with type parameter for specific skill or SkillBase parameter, same as with `TryCastSkill`
or `TryInterruptSkill`.

Example:
```csharp
        public void CancelChannelingSkill()
        {
            if(OperationResult.IsError(TryCancelSkill<ExampleChannelingSkill>()))
               Debug.LogError("Failed to cancel channeling skill");
            else Debug.Log("Channeling skill cancelled");
        }
```

By default, skills won't support interrupting and canceling. To enable it you need to override
`CanBeCancelled` and `CanBeInterrupted` methods in your skill class (or in your caster base).
It is recommended to return `SkillOperations.Permitted()` or `SkillOperations.Denied()` based
on your preferences, however you can also return custom operation values which may be able to provide
reasons for denial.

For operation construction you can review `SkillOperations` static class.
