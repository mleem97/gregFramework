<!-- markdownlint-disable MD060 -->

# Hook Naming Convention (Canonical)

## Format

All hooks must use:

`FFM.[Category].[Entity].[Action]`

Examples:

- `FFM.Economy.Balance.OnChanged`
- `FFM.Objects.Rack.OnPlaced`
- `FFM.Employees.Staff.OnHired`
- `FFM.Network.Cable.OnConnected`
- `FFM.Customer.Contract.OnSigned`

## Rules

- Exactly 4 segments: `FFM`, `Category`, `Entity`, `Action`
- `Action` starts with `On` for events
- PascalCase for all segments
- No abbreviations
- Suppressible hooks must expose `.Suppress`
- Numeric event IDs map to canonical names through `FrikaMF/HookNames.cs`

## Cross-language usage

```csharp
// C#
FFM.Economy.Balance.OnChanged.Subscribe(handler);

// TypeScript
FFM.Economy.Balance.OnChanged.subscribe(handler);

// Python
FFM.Economy.Balance.OnChanged.subscribe(handler)

// Rust
ffm::economy::balance::on_changed::subscribe(handler);
```

## Complete Hook Catalog (categorized and alphabetic)

### `FFM.Customer`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Customer.Contract.OnCancelled` | `{ customerId, reason }` | ✅ | Contract cancelled |
| `FFM.Customer.Contract.OnExpired` | `{ customerId }` | ❌ | Contract expired |
| `FFM.Customer.Contract.OnSigned` | `{ customerId, tier, revenue }` | ✅ | New contract signed |
| `FFM.Customer.Contract.OnUpgraded` | `{ customerId, oldTier, newTier }` | ✅ | Contract upgraded |
| `FFM.Customer.Penalty.OnApplied` | `{ customerId, amount, reason }` | ✅ | SLA penalty applied |
| `FFM.Customer.Reputation.OnChanged` | `{ oldValue, newValue, delta }` | ❌ | Reputation changed |
| `FFM.Customer.SLA.OnBreached` | `{ customerId, metric, threshold }` | ❌ | SLA breach detected |
| `FFM.Customer.SLA.OnRestored` | `{ customerId, metric }` | ❌ | SLA restored |

### `FFM.Economy`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Economy.Balance.OnChanged` | `{ oldBalance, newBalance, delta }` | ❌ | Balance changed |
| `FFM.Economy.Balance.OnNegative` | `{ balance }` | ❌ | Balance below zero |
| `FFM.Economy.Expense.OnCharged` | `{ category, amount, source }` | ✅ | Expense charged |
| `FFM.Economy.Income.OnReceived` | `{ customerId, amount, period }` | ✅ | Income received |
| `FFM.Economy.Period.OnClosed` | `{ period, totalRevenue, totalExpense }` | ❌ | Accounting period closed |
| `FFM.Economy.Salary.OnPaid` | `{ employeeId, amount }` | ✅ | Salary paid |
| `FFM.Economy.Stats.OnUpdated` | `{ statKey, oldValue, newValue }` | ❌ | Stats updated |

### `FFM.Employees`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Employees.Staff.OnAssigned` | `{ employeeId, taskType, targetId }` | ✅ | Staff task assigned |
| `FFM.Employees.Staff.OnHired` | `{ employeeId, role, salary }` | ✅ | Staff hired |
| `FFM.Employees.Staff.OnIdle` | `{ employeeId }` | ❌ | Staff idle |
| `FFM.Employees.Staff.OnTaskCompleted` | `{ employeeId, taskType, targetId }` | ❌ | Task completed |
| `FFM.Employees.Staff.OnTaskFailed` | `{ employeeId, taskType, reason }` | ❌ | Task failed |
| `FFM.Employees.Staff.OnTerminated` | `{ employeeId, reason }` | ✅ | Staff terminated |

### `FFM.Game`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Game.Load.OnCompleted` | `{ saveSlot }` | ❌ | Save loaded |
| `FFM.Game.Save.OnCompleted` | `{ saveSlot }` | ❌ | Save completed |
| `FFM.Game.Save.OnRequested` | `{ saveSlot }` | ✅ | Save requested |
| `FFM.Game.Scene.OnLoaded` | `{ sceneName }` | ❌ | Scene loaded |
| `FFM.Game.Scene.OnUnloaded` | `{ sceneName }` | ❌ | Scene unloaded |
| `FFM.Game.Time.OnDayChanged` | `{ day, month, year }` | ❌ | Day changed |
| `FFM.Game.Time.OnMonthChanged` | `{ month, year }` | ❌ | Month changed |
| `FFM.Game.Time.OnTick` | `{ tick, gameTime }` | ❌ | Time tick |
| `FFM.Game.XP.OnGained` | `{ amount, source, totalXP }` | ✅ | XP gained |
| `FFM.Game.XP.OnLevelUp` | `{ newLevel, unlockedFeature }` | ❌ | Level up |

### `FFM.Network`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Network.Cable.OnConnected` | `{ cableId, portA, portB, cableType }` | ✅ | Cable connected |
| `FFM.Network.Cable.OnDisconnected` | `{ cableId, portA, portB }` | ✅ | Cable disconnected |
| `FFM.Network.Cable.OnLinkDown` | `{ cableId, reason }` | ❌ | Link down |
| `FFM.Network.Cable.OnLinkUp` | `{ cableId }` | ❌ | Link up |
| `FFM.Network.IP.OnAssigned` | `{ deviceId, ip, subnet, gateway }` | ✅ | IP assigned |
| `FFM.Network.IP.OnConflictDetected` | `{ ip, deviceA, deviceB }` | ❌ | IP conflict detected |
| `FFM.Network.IP.OnReleased` | `{ deviceId, ip }` | ✅ | IP released |
| `FFM.Network.LACP.OnGroupCreated` | `{ groupId, memberPorts[] }` | ✅ | LAG created |
| `FFM.Network.LACP.OnGroupDissolved` | `{ groupId }` | ✅ | LAG dissolved |
| `FFM.Network.LACP.OnMemberAdded` | `{ groupId, portId }` | ✅ | Member added |
| `FFM.Network.Port.OnDisabled` | `{ deviceId, portIndex }` | ✅ | Port disabled |
| `FFM.Network.Port.OnEnabled` | `{ deviceId, portIndex }` | ✅ | Port enabled |
| `FFM.Network.Traffic.OnThresholdExceeded` | `{ deviceId, utilizationPct }` | ❌ | Traffic threshold exceeded |

### `FFM.Objects`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Objects.Device.OnDegraded` | `{ deviceId, healthPct }` | ❌ | Device degraded |
| `FFM.Objects.Device.OnEOL` | `{ deviceId, deviceType }` | ❌ | Device reached EOL |
| `FFM.Objects.Device.OnPoweredOff` | `{ deviceId, triggeredBy }` | ✅ | Device powered off |
| `FFM.Objects.Device.OnPoweredOn` | `{ deviceId, triggeredBy }` | ✅ | Device powered on |
| `FFM.Objects.Device.OnRepaired` | `{ deviceId, employeeId }` | ❌ | Device repaired |
| `FFM.Objects.Device.OnRepairRequested` | `{ deviceId }` | ✅ | Repair requested |
| `FFM.Objects.Rack.OnDevicePlaced` | `{ rackId, deviceId, slot }` | ✅ | Device placed in rack |
| `FFM.Objects.Rack.OnDeviceRemoved` | `{ rackId, deviceId }` | ✅ | Device removed from rack |
| `FFM.Objects.Rack.OnOverloaded` | `{ rackId, currentW, maxW }` | ❌ | Rack overloaded |
| `FFM.Objects.Rack.OnPlaced` | `{ rackId, position }` | ✅ | Rack placed |
| `FFM.Objects.Rack.OnRemoved` | `{ rackId }` | ✅ | Rack removed |
| `FFM.Objects.Server.OnClientAssigned` | `{ serverId, customerId }` | ✅ | Client assigned |
| `FFM.Objects.Server.OnClientUnassigned` | `{ serverId, customerId }` | ✅ | Client unassigned |

### `FFM.Store`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.Store.Cart.OnCheckedOut` | `{ items[], totalCost }` | ✅ | Cart checked out |
| `FFM.Store.Cart.OnItemAdded` | `{ itemId, quantity, price }` | ✅ | Item added to cart |
| `FFM.Store.Cart.OnItemRemoved` | `{ itemId }` | ✅ | Item removed from cart |
| `FFM.Store.Item.OnDelivered` | `{ itemId, quantity, destination }` | ❌ | Item delivered |
| `FFM.Store.Item.OnPurchased` | `{ itemId, quantity, cost }` | ✅ | Item purchased |
| `FFM.Store.Item.OnRefunded` | `{ itemId, amount }` | ✅ | Item refunded |
| `FFM.Store.Modded.OnRegistered` | `{ itemId, modSource }` | ❌ | Modded item registered |

### `FFM.UI`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.UI.Menu.OnClosed` | `{ menuId }` | ✅ | Menu closed |
| `FFM.UI.Menu.OnOpened` | `{ menuId }` | ✅ | Menu opened |
| `FFM.UI.Notification.OnDismissed` | `{ notificationId }` | ✅ | Notification dismissed |
| `FFM.UI.Notification.OnShown` | `{ notificationId, type, message }` | ✅ | Notification shown |
| `FFM.UI.Panel.OnClosed` | `{ panelId }` | ✅ | Panel closed |
| `FFM.UI.Panel.OnOpened` | `{ panelId }` | ✅ | Panel opened |
| `FFM.UI.Screen.OnTransitioned` | `{ fromScreen, toScreen }` | ❌ | Screen transitioned |
| `FFM.UI.Tooltip.OnShown` | `{ targetId, content }` | ✅ | Tooltip shown |

### `FFM.World`

| Hook | Payload | Suppressible | Description |
|---|---|---|---|
| `FFM.World.Room.OnExpanded` | `{ newSize, cost }` | ✅ | Room expanded |
| `FFM.World.Room.OnExpansionRequested` | `{ targetSize }` | ✅ | Expansion requested |
| `FFM.World.Temperature.OnCritical` | `{ zone, tempC }` | ❌ | Critical temperature |
| `FFM.World.Temperature.OnNormal` | `{ zone, tempC }` | ❌ | Temperature normalized |
| `FFM.World.Temperature.OnWarning` | `{ zone, tempC, threshold }` | ❌ | Temperature warning |
