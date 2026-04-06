---
id: fmf-hook-naming
title: FMF hook and event naming
slug: /reference/fmf-hook-naming
description: Canonical naming for hooks, events, and cross-language documentation stubs.
---

# FMF hook and event naming

## Target format

All **new** public hook and event identifiers should follow:

```text
FMF.<Domain>.<EventOrHook>
```

- **`FMF`** — Fixed prefix (Frika Mod Framework).
- **`<Domain>`** — Uppercase domain from the [approved domain list](#approved-domain-segments). Describes *where* the signal belongs in the game (player, rack, server, economy, …).
- **`<EventOrHook>`** — `PascalCase` segment(s), usually `OnSomething` for events or a verb phrase for commands.

Examples (illustrative): `FMF.RACK.CableSpinnerColorResolved`, `FMF.PLAYER.InputPoll`, `FMF.NETWORK.Cable.OnConnected`.

## Approved domain segments

Domains are **closed by default**. Add a new domain only via changelog + maintainer review.

| Domain | Scope |
|--------|--------|
| `GAMEPLAY` | Rules, scoring, simulation beats not covered elsewhere |
| `PLAYER` | Local player input, camera, UI focus |
| `EMPLOYEE` | NPC staff, hiring, schedules |
| `CUSTOMER` | Contracts, SLA, satisfaction |
| `SERVER` | In-game server racks, VMs, power |
| `RACK` | Physical rack placement, mounting |
| `NETWORK` | Cables, switches, traffic |
| `STORE` | Shop cart, checkout |
| `WORLD` | Rooms, expansion, walls |
| `UI` | HUD overlays, menus (when not `PLAYER`) |
| `SAVE` | Save/load lifecycle |
| `FRAMEWORK` | Loader, bridge, diagnostics |

## Legacy: `FFM.*` strings in code

The runtime currently maps numeric event IDs to **`FFM.*`** string constants in [`FrikaMF/HookNames.cs`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF/HookNames.cs) (e.g. `FFM.Economy.Balance.OnChanged`). That is **legacy naming** (four segments after `FFM`).

**Policy**

- New documentation and greenfield APIs should use **`FMF.<Domain>.*`** as above.
- When touching `HookNames.cs`, prefer aligning new entries to **`FMF.*`**; otherwise keep **`FFM.*`** until a planned major version bump documents a rename map.
- The [generated hook catalog](./fmf-hooks-catalog.md) lists **what the code emits today** (including `FFM.*`).

## Cross-language stubs (documentation)

For each canonical hook, the wiki may add **non-normative** snippets:

| Language | Convention |
|----------|------------|
| C# | `FMF.Domain.OnSomething.Subscribe(...)` or string literal |
| Lua | `FMF.Domain.OnSomething:subscribe(...)` |
| Rust | `fmf::domain::on_something::subscribe(...)` |
| Python | `fmf.domain.on_something.subscribe(...)` |
| TypeScript | `FMF.Domain.OnSomething.subscribe(...)` |

Bindings are **not** auto-generated for all languages; stubs are for contributor clarity.

## Related

- [FMF hooks catalog](./fmf-hooks-catalog.md) (generated)
- [Legacy wiki: HOOK-NAMING-CONVENTION](../wiki-import/HOOK-NAMING-CONVENTION.md) (extended examples)
