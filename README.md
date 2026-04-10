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
- **CastSkillContext**: Read-only context passed to skill callbacks containing caster, skill, flags, and optional target reference
- **InterruptSkillContext**: Read-only context passed to interrupt/cancel callbacks containing caster, source, skill, and flags
- **SkillsDatabase**: Addressable-based database for skill lookup and management

### Skill Interfaces

- **IChannelingSkillBase**: For skills that sustain effects over time (beam attacks, channels). Override `OnCastTickWhenChanneling` and set `Duration`. When `Duration <= 0`, the skill channels infinitely (`IsInfinite` returns `true`)
- **IActivatedSkill**: For persistent-effect skills toggled on/off via `ActivateSkill`/`DeactivateSkill`. Implement `OnActivated()`, `OnDeactivated()`, and `OnTickWhileActive()`. Cannot be combined with `ISkillWithCharges`
- **ISkillWithCharges**: For abilities with multiple uses before each recharges independently (e.g., double-dash). Set `MaxCharges`
- **ISkillWithLevels**: For scalable skills with level-dependent variants. Inherit `SkillWithLevels<TSelf>` which implements `GetSkillForLevel()` automatically; override `Level` on each variant asset

### Flags and Control

- **SkillCastFlags**: Enum for conditional behavior — `IgnoreAvailability`, `IgnoreCosts`, `IgnoreCooldown`, `IgnoreRequirements`, `DoNotConsumeResources`, `RefundResourcesOnFailure`, `AllowStacking`, `NoCooldownOnInterrupt`, `ResetOnRecast`
- **SkillInterruptFlags**: Enum for interrupt behavior — `IgnoreRequirements` bypasses `CanBeInterrupted` check

## Usage Examples

### Basic One-Time Skill

```csharp
public class FireballSkill : SkillBase
{
    public override float ChargingTime => 0.5f;
    public override float CooldownTime => 3f;

    protected internal override OperationResult HasEnoughResources(in CastSkillContext context)
    {
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
        if (context.target is EnemyTarget enemy)
            SpawnFireball(enemy.transform.position);
        else
            SpawnFireball(context.caster.transform.position + context.caster.transform.forward * 2f);
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
        // Allow player cancellation but deny external interruption
        return context.IsCancellation ? SkillOperations.Permitted() : SkillOperations.Denied();
    }

    void IChannelingSkillBase.OnCastTickWhenChanneling(in CastSkillContext context)
    {
        // Heal target each tick
        if (context.target is IHealable healable)
            healable.Heal(10);
    }

    protected internal override void OnCastEnded(in CastSkillContext context)
    {
        // Play completion effect
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
        context.caster.GetComponent<Rigidbody>().velocity =
            context.caster.transform.forward * 20f;
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
        TryCancelSkill<HealingChannelSkill>();
    }

    public void OnActivateAuraPressed() => ActivateSkill<DamageAuraSkill>();
    public void OnDeactivateAuraPressed() => DeactivateSkill<DamageAuraSkill>();
}
```

### Casting with Targets

```csharp
// Target must inherit from ISkillTarget
TryCastSkill<FireballSkill>(target);
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
}

// Cast with stacking enabled
TryCastSkill<MultiStrikeSkill>(SkillCastFlags.AllowStacking);
```

### Group Cooldowns

Assign skills to a cooldown group so casting one skill starts a shared cooldown for all skills in that group. Define the group as a struct implementing `ISkillGroup`, then implement the generic marker interface `IWithSkillGroup<TGroup>` on each skill:

```csharp
public struct PotionGroup : ISkillGroup
{
    public float Cooldown => 1.5f;
}

public class HealthPotionSkill : SkillBase, IWithSkillGroup<PotionGroup> { }
public class ManaPotionSkill : SkillBase, IWithSkillGroup<PotionGroup> { }
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

### Leveled Skills

Create a skill with multiple level variants, each as a separate ScriptableObject asset:

```csharp
public abstract class FireballSkillBase : SkillWithLevels<FireballSkillBase>
{
    // shared overrides (callbacks, cooldown, etc.)
}

// Each asset in the database represents one level
public class FireballSkillLevel1 : FireballSkillBase
{
    public override int Level => 1;
    public override float CooldownTime => 5f;
}

public class FireballSkillLevel2 : FireballSkillBase
{
    public override int Level => 2;
    public override float CooldownTime => 3f;
}
```

Override `GetSkillLevel` on your caster to return the correct level per skill:

```csharp
protected override int GetSkillLevel(ISkillWithLevels skill)
{
    return playerData.GetSkillLevel(skill);
}
```

## Skill Lifecycle

1. **Pre-cast checks**: Availability → Cooldown → Group cooldown → Charges → Resources → Requirements
2. **Charging phase**: Accumulate charge time, call `OnCastTickWhenCharging` each tick
3. **Casting phase**: Call `OnCastStarted`, then enter channeling or completion
4. **Channeling phase** (if `IChannelingSkillBase`): Call `OnCastTickWhenChanneling` each tick until duration expires; skipped if `IsInfinite`
5. **Completion**: Call `OnCastEnded`
6. **Cooldown phase**: Apply cooldown, call `OnCooldownTick` each tick
7. **Interrupt/Cancel**: Call `OnCastInterrupted` on success or `OnCastInterruptFailed` on failure, apply `InterruptedCooldownMultiplier`

## Event Callbacks

All event callbacks on `SkillBase` are `protected internal virtual` and use `in` parameters (pass-by-readonly-reference):

- **OnCastStarted**: Skill charge completed, about to execute
- **OnCastTickWhenCharging**: Called each tick during charge phase
- **OnCastEnded**: Skill cast completed successfully
- **OnCastFailed**: Pre-cast check failed (receives failure reason)
- **OnCastInterrupted**: Skill interrupted or cancelled while charging/channeling
- **OnCastInterruptFailed**: Interrupt attempt rejected (receives rejection reason)
- **OnCooldownTick**: Called each tick while on cooldown
- **OnCastRegistered**: Skill added to the active cast list
- **OnCastRemoved**: Skill removed from the active cast list
- **ConsumeResources**: Called to spend resources before cast attempt
- **RefundResources**: Called to return resources when `RefundResourcesOnFailure` is set and `CheckAttemptSuccess` fails

`OnCastTickWhenChanneling` is declared on `IChannelingSkillBase` (not `SkillBase`) and must be implemented explicitly via the interface.

## Database Integration

All skills are stored in a `SkillsDatabase` indexed by addressable labels. The `[AutoCreate("Skills", SkillsDatabase.LABEL)]` attribute on `SkillBase` auto-registers all derived ScriptableObjects for addressable loading.

## Performance Considerations

- `CastSkillContext` and `InterruptSkillContext` are `readonly ref struct` (stack-only, zero GC). They cannot be captured in lambdas, stored in collections, used with `async/await`, or passed to `System.Action` delegates. Copy required fields to a regular struct if deferred processing is needed
- Skills are ScriptableObjects (no runtime instantiation)
- Supports unlimited concurrent skill instances (charging, channeling, cooldown)
- Reverse-iteration loops for safe addition/removal during callbacks
- Built-in support for custom tick systems: override `Update()` to empty and call `OnTickExecuted(deltaTime)` manually (e.g., for turn-based systems)

## License

See LICENSE.md in this directory.
