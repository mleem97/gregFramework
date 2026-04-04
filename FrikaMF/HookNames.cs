using System.Collections.Generic;

namespace DataCenterModLoader;

internal static class HookNames
{
    public const string EconomyBalanceOnChanged = "FFM.Economy.Balance.OnChanged";
    public const string GameXpOnGained = "FFM.Game.XP.OnGained";
    public const string CustomerReputationOnChanged = "FFM.Customer.Reputation.OnChanged";

    public const string ObjectsDeviceOnPoweredOn = "FFM.Objects.Device.OnPoweredOn";
    public const string ObjectsDeviceOnPoweredOff = "FFM.Objects.Device.OnPoweredOff";
    public const string ObjectsDeviceOnDegraded = "FFM.Objects.Device.OnDegraded";
    public const string ObjectsDeviceOnEOL = "FFM.Objects.Device.OnEOL";
    public const string ObjectsDeviceOnRepaired = "FFM.Objects.Device.OnRepaired";
    public const string ObjectsRackOnDevicePlaced = "FFM.Objects.Rack.OnDevicePlaced";
    public const string NetworkCableOnConnected = "FFM.Network.Cable.OnConnected";
    public const string NetworkCableOnDisconnected = "FFM.Network.Cable.OnDisconnected";
    public const string ObjectsServerOnClientAssigned = "FFM.Objects.Server.OnClientAssigned";
    public const string ObjectsServerOnClientUnassigned = "FFM.Objects.Server.OnClientUnassigned";
    public const string ObjectsRackOnRemoved = "FFM.Objects.Rack.OnRemoved";
    public const string NetworkCableOnLinkUp = "FFM.Network.Cable.OnLinkUp";
    public const string NetworkCableOnLinkDown = "FFM.Network.Cable.OnLinkDown";
    public const string NetworkCableOnConnectedSuppress = "FFM.Network.Cable.OnConnected.Suppress";
    public const string NetworkCableOnDisconnectedSuppress = "FFM.Network.Cable.OnDisconnected.Suppress";

    public const string GameTimeOnDayChanged = "FFM.Game.Time.OnDayChanged";
    public const string GameTimeOnMonthChanged = "FFM.Game.Time.OnMonthChanged";

    public const string CustomerContractOnSigned = "FFM.Customer.Contract.OnSigned";
    public const string CustomerSlaOnRestored = "FFM.Customer.SLA.OnRestored";
    public const string CustomerSlaOnBreached = "FFM.Customer.SLA.OnBreached";

    public const string StoreCartOnCheckedOut = "FFM.Store.Cart.OnCheckedOut";
    public const string StoreCartOnItemAdded = "FFM.Store.Cart.OnItemAdded";
    public const string StoreCartOnCheckedOutCleared = "FFM.Store.Cart.OnCheckedOut";
    public const string StoreCartOnItemRemoved = "FFM.Store.Cart.OnItemRemoved";

    public const string EmployeesStaffOnHired = "FFM.Employees.Staff.OnHired";
    public const string EmployeesStaffOnTerminated = "FFM.Employees.Staff.OnTerminated";

    public const string GameSaveOnCompleted = "FFM.Game.Save.OnCompleted";
    public const string GameLoadOnCompleted = "FFM.Game.Load.OnCompleted";
    public const string GameSaveOnRequested = "FFM.Game.Save.OnRequested";

    public const string WorldRoomOnExpanded = "FFM.World.Room.OnExpanded";
    public const string NetworkTrafficOnThresholdExceeded = "FFM.Network.Traffic.OnThresholdExceeded";

    public const string EmployeesStaffOnHiredCustom = "FFM.Employees.Staff.OnHired";
    public const string EmployeesStaffOnTerminatedCustom = "FFM.Employees.Staff.OnTerminated";

    public const string FrameworkHooksOnBridgeInstalled = "FFM.Framework.Hooks.OnBridgeInstalled";
    public const string FrameworkHooksOnBridgeTriggered = "FFM.Framework.Hooks.OnBridgeTriggered";

    private static readonly IReadOnlyDictionary<uint, string> EventIdToHookName =
        new Dictionary<uint, string>
        {
            [EventIds.MoneyChanged] = EconomyBalanceOnChanged,
            [EventIds.XPChanged] = GameXpOnGained,
            [EventIds.ReputationChanged] = CustomerReputationOnChanged,

            [EventIds.ServerPowered] = ObjectsDeviceOnPoweredOn,
            [EventIds.ServerBroken] = ObjectsDeviceOnDegraded,
            [EventIds.ServerRepaired] = ObjectsDeviceOnRepaired,
            [EventIds.ServerInstalled] = ObjectsRackOnDevicePlaced,
            [EventIds.CableConnected] = NetworkCableOnConnected,
            [EventIds.CableDisconnected] = NetworkCableOnDisconnected,
            [EventIds.ServerCustomerChanged] = ObjectsServerOnClientAssigned,
            [EventIds.ServerAppChanged] = ObjectsServerOnClientUnassigned,
            [EventIds.RackUnmounted] = ObjectsRackOnRemoved,
            [EventIds.SwitchBroken] = NetworkCableOnLinkDown,
            [EventIds.SwitchRepaired] = NetworkCableOnLinkUp,
            [EventIds.CableCreated] = NetworkCableOnConnected,
            [EventIds.CableRemoved] = NetworkCableOnDisconnected,
            [EventIds.CableCleared] = StoreCartOnCheckedOutCleared,
            [EventIds.CableSpeedChanged] = NetworkTrafficOnThresholdExceeded,
            [EventIds.CableSfpInserted] = NetworkCableOnConnected,
            [EventIds.CableSfpRemoved] = NetworkCableOnDisconnected,

            [EventIds.DayEnded] = GameTimeOnDayChanged,
            [EventIds.MonthEnded] = GameTimeOnMonthChanged,

            [EventIds.CustomerAccepted] = CustomerContractOnSigned,
            [EventIds.CustomerSatisfied] = CustomerSlaOnRestored,
            [EventIds.CustomerUnsatisfied] = CustomerSlaOnBreached,

            [EventIds.ShopCheckout] = StoreCartOnCheckedOut,
            [EventIds.ShopItemAdded] = StoreCartOnItemAdded,
            [EventIds.ShopCartCleared] = StoreCartOnCheckedOutCleared,
            [EventIds.ShopItemRemoved] = StoreCartOnItemRemoved,

            [EventIds.EmployeeHired] = EmployeesStaffOnHired,
            [EventIds.EmployeeFired] = EmployeesStaffOnTerminated,

            [EventIds.GameSaved] = GameSaveOnCompleted,
            [EventIds.GameLoaded] = GameLoadOnCompleted,
            [EventIds.GameAutoSaved] = GameSaveOnRequested,

            [EventIds.WallPurchased] = WorldRoomOnExpanded,
            [EventIds.NetWatchDispatched] = NetworkTrafficOnThresholdExceeded,

            [EventIds.CustomEmployeeHired] = EmployeesStaffOnHiredCustom,
            [EventIds.CustomEmployeeFired] = EmployeesStaffOnTerminatedCustom,

            [FrikaMF.EventIds.HookBridgeInstalled] = FrameworkHooksOnBridgeInstalled,
            [FrikaMF.EventIds.HookBridgeTriggered] = FrameworkHooksOnBridgeTriggered,
        };

    public static string Resolve(uint eventId)
    {
        if (EventIdToHookName.TryGetValue(eventId, out string name))
            return name;

        return "FFM.Framework.Unknown.OnEvent";
    }
}