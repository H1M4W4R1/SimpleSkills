# SimpleSkills

A flexible, event-driven skill and ability system for Unity. SimpleSkills provides a complete framework for managing skill casting, cooldowns, charging, channeling, and lifecycle events with minimal overhead and full inspector integration.

## Overview

SimpleSkills is a modular ability system designed for games that need instant-cast abilities, charged skills, and continuous channeled effects. It handles the full skill lifecycle including availability checks, resource consumption, charging, casting, channeling, cooldowns, and interrupts. Skills are data-driven (ScriptableObjects) and loosely coupled from game logic through a context-based API and extensible event callbacks.

## Requirements

- **Unity**: 2022.3 or later
- **Dependencies**:
  - SimpleCore (included in parent kit)
  - Unity.Addressables
  - Unity.Mathematics
  - Unity.ResourceManager

See `SimpleSkills.asmdef` for assembly definition details.

## Key Components

### Core Classes

- **SkillBase**: Abstract base class for all skills. Override to define availability checks, resource consumption, and lifecycle callbacks
- **SkillCasterBase**: MonoBehaviour that manages skill casting, cooldowns, charging, and channeling. Inherit and override to create unit/player controllers
- **CastSkillContext**: Read-only context struct passed to skill callbacks containing caster, skill, flags, and optional target reference
- **SkillsDatabase**: Addressable-based database for skill lookup and management

### Skill Interfaces

- **IChannelingSkillBase**: For skills that sustain effects over time (beam attacks, channels). Override `OnCastTickWhenChanneling` and set `Duration`
- **IActivatedSkill**: For passive/aura skills toggled on/off without cooldown. Implement `OnActivated()`, `OnDeactivated()`, and `OnTickWhileActive()`
- **ISkillWithCharges**: For abilities with multiple uses before entering shared cooldown (e.g., double-dash). Set `MaxCharges`
- **ISkillWithLevels**: For scalable skills with level-dependent versions. Implement `Level` property and `GetSkillForLevel()`

### Flags and Control

- **SkillCastFlags**: Enum for conditional behavior (ignore cooldown, refund on failure, allow stacking, etc.)
- **InterruptSkillContext**: Context for interrupt/cancellation operations with flags to distinguish intent

## Usage Examples

### Basic One-Time Skill

```csharp
public class FireballSkill : SkillBase
{
    public override float ChargingTime => 0.5f;
    public override float CooldownTime => 3f;

    protected internal override OperationResult HasEnoughResources(in CastSkillContext context)
    {
        // Check if caster has enough mana
        return context.caster.GetComponent<ManaPool>().mana >= 50
            ? SkillOperations.Permitted()
            : SkillOperations.Denied();
    }

    protected internal override void ConsumeResources(in CastSkillContext context)
    {
        context.caster.GetComponent<ManaPool>().ConsumeMana(50);
    }

    protected internal override void OnCastEnded(in CastSkillContext context)
    {
        // Spawn projectile, play effects, deal damage, etc.
        SpawnFireball(context.target?.Position ?? context.caster.transform.position);
    }
}
```

### Channeled Skill

```csharp
public class HealingChannelSkill : SkillBase, IChannelingSkillBase
{
    public float Duration => 3f; // Channel for 3 seconds
    public override float CooldownTime => 5f;

    protected internal override OperationResult CanBeInterrupted(in InterruptSkillContext context)
    {
        // Allow player to cancel but not AI interrupt
        return context.IsCancellation ? SkillOperations.Permitted() : SkillOperations.Denied();
    }

    void IChannelingSkillBase.OnCastTickWhenChanneling(in CastSkillContext context)
    {
        // Heal target each tick
        if (context.target is Health health)
            health.Heal(10);
    }

    protected internal override void OnCastEnded(in CastSkillContext context)
    {
        // Play completion effect
        PlayEffectAt(context.target?.Position ?? Vector3.zero);
    }
}
```

### Charged Skill (Multiple Uses)

```csharp
public class DashSkill : SkillBase, ISkillWithCharges
{
    public int MaxCharges => 2;
    public override float CooldownTime => 4f;

    protected internal override void OnCastEnded(in CastSkillContext context)
    {
        var direction = (context.target?.Position ?? context.caster.transform.forward) - context.caster.transform.position;
        context.caster.GetComponent<Rigidbody>().velocity = direction.normalized * 20f;
    }
}
```

### Passive/Aura Skill

```csharp
public class DamageAuraSkill : SkillBase, IActivatedSkill
{
    private float tickTimer = 0f;

    void IActivatedSkill.OnActivated()
    {
        tickTimer = 0f;
    }

    void IActivatedSkill.OnTickWhileActive(float deltaTime)
    {
        tickTimer += deltaTime;
        if (tickTimer >= 0.5f)
        {
            // Deal AOE damage every 0.5 seconds
            DealAoeDamage(10f);
            tickTimer = 0f;
        }
    }

    void IActivatedSkill.OnDeactivated()
    {
        // Cleanup effects
    }
}
```

### Caster Controller

```csharp
public class PlayerSkillCaster : SkillCasterBase
{
    public void OnFireballPressed() => TryCastSkill<FireballSkill>();
    public void OnHealPressed() => TryCastSkill<HealingChannelSkill>();
    
    public void OnCancelPressed()
    {
        // Cancel any channeled healing skill
        foreach (var type in new[] { typeof(HealingChannelSkill) })
            TryCancelSkill(type);
    }

    public void OnActivateAuraPressed() => ActivateSkill<DamageAuraSkill>();
    public void OnDeactivateAuraPressed() => DeactivateSkill<DamageAuraSkill>();
}
```

### Casting with Targets

```csharp
public void CastOnTarget(SkillBase skill, Transform target)
{
    var context = new CastSkillContext(
        caster: this,
        skill: skill,
        flags: SkillCastFlags.None,
        target: target.GetComponent<ISkillTarget>()
    );
    
    TryCastSkillWithContext(context);
}
```

### Skill Availability Checks

```csharp
public class ConditionalSkill : SkillBase
{
    protected internal override OperationResult IsSkillAvailable(in CastSkillContext context)
    {
        // Only castable in combat
        return context.caster.GetComponent<CombatState>().IsInCombat
            ? SkillOperations.Permitted()
            : SkillOperations.Denied();
    }

    protected internal override OperationResult CheckAttemptSuccess(in CastSkillContext context)
    {
        // Chance-based skill (80% hit)
        return Random.value <= 0.8f
            ? SkillOperations.Permitted()
            : SkillOperations.Denied();
    }
}
```

## Advanced Features

### Skill Stacking

Allow multiple concurrent casts of the same skill:

```csharp
public class MultiStrikeSkill : SkillBase
{
    public override int MaxStacks => 3;

    // Cast the skill up to 3 times simultaneously
}
```

### Group Cooldowns

Assign skills to cooldown groups (via `IWithSkillGroup`) so casting one skill cools down others:

```csharp
public interface IWithSkillGroup
{
    public ISkillGroup SkillGroup { get; }
}
```

### Interrupted Cooldown Multiplier

Reduce cooldown when a skill is interrupted:

```csharp
public class InterruptibleSkill : SkillBase
{
    public override float InterruptedCooldownMultiplier => 0.5f; // 50% cooldown on interrupt
}
```

### Resource Refunding

Refund resources if a chance-based skill fails:

```csharp
TryCastSkill<ChanceSkill>(SkillCastFlags.RefundResourcesOnFailure);
```

## Skill Lifecycle

1. **Pre-cast checks**: Availability → Resources → Requirements (target, etc.)
2. **Charging phase**: Accumulate charge time, call `OnCastTickWhenCharging`
3. **Casting phase**: Call `OnCastStarted`, then enter channeling or completion
4. **Channeling phase** (if `IChannelingSkillBase`): Call `OnCastTickWhenChanneling` until duration expires
5. **Completion**: Call `OnCastEnded`
6. **Cooldown phase**: Apply cooldown, call `OnCooldownTick` each tick
7. **Interrupt/Cancel**: Call `OnCastInterrupted`, apply interrupted cooldown multiplier

## Inheritance Pattern

When creating custom skills, follow this hierarchy:

```
SkillBase (core)
├── OneTimeSkill (instant cast, no channeling)
├── ChargedSkill (has charging phase)
├── ChanneledSkill (inherits + implements IChannelingSkillBase)
├── PassiveSkill (inherits + implements IActivatedSkill)
└── LeveledSkill (inherits SkillWithLevels)
```

## Event Callbacks

All event callbacks are `protected internal virtual` and use `ref` parameters:

- **OnCastStarted**: Skill charge/cast completed, about to execute
- **OnCastTickWhenCharging**: Called each tick during charge phase
- **OnCastTickWhenChanneling**: Called each tick during channel phase (implement via interface)
- **OnCastEnded**: Skill cast completed successfully
- **OnCastFailed**: Pre-cast check failed
- **OnCastInterrupted**: Skill interrupted while charging/channeling
- **OnCooldownTick**: Called each tick while on cooldown
- **OnCastRegistered**: Skill added to active cast list
- **OnCastRemoved**: Skill removed from active cast list

## Database Integration

All skills are stored in a SkillsDatabase indexed by addressable labels. Use the `[AutoCreate("Skills", SkillsDatabase.LABEL)]` attribute on skill ScriptableObjects to auto-register them for addressable loading.

## Performance Considerations

- `CastSkillContext` is a ref struct (stack-allocated, zero GC)
- Skills are ScriptableObjects (no runtime instantiation)
- Supports unlimited concurrent skill instances (charged, channeled, cooldown)
- Reverse-iteration loops for safe addition/removal during callbacks
- Built-in support for custom tick systems (override `Update()` or call `OnTickExecuted()` manually)

## License

See LICENSE.md in this directory.
