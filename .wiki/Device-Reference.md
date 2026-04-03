# Device Reference

Last updated: 2026-04-03

This page collects device-focused information for mod authors and contributors.

## Scope

- Focuses on gameplay devices relevant to framework hooks and feature work.
- Documents only currently verified information.
- Marks open gaps clearly so contributors know what to verify next.

## Why this page exists

Feedback from users/contributors highlighted that device documentation was too light, especially around practical behavior and connection/port expectations. This page is the baseline to fix that.

## Verified device-related targets (current)

Source of truth for verification status: `.wiki/HOOKS.md`.

### SetIP

- `ClickButtonOK`
- `ClickButtonPaste`
- `ClickButtonClear`

Usage:

- Primary integration points for DHCP/IP autofill workflow and safe UI-triggered updates.

### NetworkSwitch

- `GetConnectedDevices`
- `ButtonShowNetworkSwitchConfig`
- `IsAnyCableConnected`
- `DisconnectCables`
- `ReconnectCables`

Usage:

- Topology discovery and switch lifecycle actions.
- Candidate anchors for VLAN and port-management UX.

### NetworkMapDevice

- `getConnections`
- `setConnections`

Usage:

- Read/write network graph edges for topology-aware features.

### Rack

- `IsPositionAvailable`
- `MarkPositionAsUsed`
- `MarkPositionAsUnused`

Usage:

- Rack occupancy consistency and safe insert/remove flow.

### CustomerBase and PlayerData

- `CustomerBase.CheckIfAppRequirementsAreMet`
- `CustomerBase.UpdateMoney`
- `PlayerData.get_reputation` / `set_reputation`
- `PlayerData.get_position` / `set_position`

Usage:

- Customer satisfaction, economy events, and player state interactions.

### Cable types

- `CableLinkTypeOfLink` enum presence verified.
- Useful for cable-type-aware DHCP scope and policy logic (RJ45/SFP/QSFP/Fibre).

## Port order and device behavior status

Current status:

- A complete public port-order map for all devices is **not fully documented yet**.
- Verified hooks provide strong entry points, but per-device port ordering still requires focused validation passes.

Planned documentation strategy:

1. Verify one device class at a time against runtime dump and in-game behavior.
2. Capture practical notes (port order, edge cases, cable-type constraints).
3. Add a small reproducible test recipe for each device section.
4. Keep uncertain items explicitly marked as `UNVERIFIED`.

## Contribution template (device entries)

Use this format when adding or updating device docs:

```markdown
### <Device/Class>

- Game class: `Il2Cpp.<ClassName>`
- Verified methods:
  - `<MethodA>`
  - `<MethodB>`
- Port order: `<Known order or UNVERIFIED>`
- Constraints:
  - `<Cable type / state rules>`
- Test recipe:
  1. `<Step 1>`
  2. `<Step 2>`
- Source evidence:
  - `.wiki/HOOKS.md` entry
  - runtime dump excerpt (`assembly-hooks.txt`)
```

## Device documentation backlog

- Add dedicated sections for `Server`, `NetworkSwitch`, `Patch Panel`, and `CustomerBase` device interactions.
- Add screenshots/diagrams once stable naming and port order are confirmed.
- Link each verified device section to related hook/event IDs.

## Related pages

- [Modding Guide](Modding-Guide)
- [Architecture](Architecture)
- [Setup](Setup)
