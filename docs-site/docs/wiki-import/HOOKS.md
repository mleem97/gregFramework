# HOOKS.md — Verified Hook Targets

Canonical naming rules and the complete hook catalog are defined in:

- `.wiki/HOOK-NAMING-CONVENTION.md`

Game Version: Data Center v1.0.38  
Engine: Unity 6000.3.12f1  
Source: Runtime method dump (`assembly-hooks.txt`, Assembly-CSharp, April 2026)

| Class (`Il2Cpp.*`) | Method | Status | Notes |
| --- | --- | --- | --- |
| `SetIP` | `ClickButtonOK` | ✅ VERIFIED | UI confirm button; use for DHCP auto-fill workflow |
| `SetIP` | `ClickButtonPaste` | ✅ VERIFIED | Programmatic IP paste hook point |
| `SetIP` | `ClickButtonClear` | ✅ VERIFIED | UI clear/reset hook point |
| `NetworkSwitch` | `GetConnectedDevices` | ✅ VERIFIED | DHCP scope discovery from live switch topology |
| `NetworkSwitch` | `ButtonShowNetworkSwitchConfig` | ✅ VERIFIED | Hook point for IPAM/switch UI integration |
| `NetworkSwitch` | `IsAnyCableConnected` | ✅ VERIFIED | Guard check before flow-pause or cable operations |
| `NetworkSwitch` | `DisconnectCables` | ✅ VERIFIED | VLAN/port profile transitions |
| `NetworkSwitch` | `ReconnectCables` | ✅ VERIFIED | VLAN/port profile transitions |
| `NetworkSwitchConfigurationc` | `(class present in dump)` | ✅ VERIFIED | Existing in-game switch config logic; extend instead of replacing |
| `NetworkMapDevice` | `getConnections` | ✅ VERIFIED | Read network topology edges |
| `NetworkMapDevice` | `setConnections` | ✅ VERIFIED | Write/update topology connections |
| `CustomerBase` | `CheckIfAppRequirementsAreMet` | ✅ VERIFIED | Coroutine-style satisfaction gate (`d35` suffix variant may appear) |
| `CustomerBase` | `UpdateMoney` | ✅ VERIFIED | Revenue-related coroutine (`d36` suffix variant may appear) |
| `PlayerData` | `get_reputation` / `set_reputation` | ✅ VERIFIED | Direct reputation access confirmed |
| `PlayerData` | `get_position` / `set_position` | ✅ VERIFIED | Direct player position access confirmed |
| `Rack` | `IsPositionAvailable` | ✅ VERIFIED | Mandatory pre-insert rack slot validation |
| `Rack` | `MarkPositionAsUsed` | ✅ VERIFIED | Keep rack occupancy state consistent |
| `Rack` | `MarkPositionAsUnused` | ✅ VERIFIED | Restore rack occupancy state on remove |
| `CableLinkTypeOfLink` | `(enum present)` | ✅ VERIFIED | Cable-type aware DHCP scopes (RJ45/SFP/QSFP/Fibre) |

## Auto Export Policy

- Runtime method dump is automatically exported by framework startup to:
  - `Data Center/Mods/FrikaMF/Diagnostics/assembly-hooks.txt`
- File is overwritten on each startup to keep it up-to-date with current runtime state.
- Export format is one line per method:
  - `runtimetrigger asm[Assembly-CSharp] type[Il2Cpp.ClassName] method[MethodName]`
