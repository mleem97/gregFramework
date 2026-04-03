namespace DataCenterModLoader;

internal static class EventIds
{
    public const uint MoneyChanged = 1;
    public const uint XPChanged = 2;
    public const uint ReputationChanged = 3;

    public const uint ServerPowered = 10;
    public const uint ServerBroken = 11;
    public const uint ServerRepaired = 12;
    public const uint ServerInstalled = 13;
    public const uint ServerCustomerChanged = 14;
    public const uint ServerAppChanged = 15;

    public const uint DayEnded = 20;
    public const uint MonthEnded = 21;

    public const uint CustomerAccepted = 30;
    public const uint CustomerSatisfied = 31;
    public const uint CustomerUnsatisfied = 32;

    public const uint ShopCheckout = 40;
    public const uint ShopItemAdded = 41;
    public const uint ShopCartCleared = 42;
    public const uint ShopItemRemoved = 43;

    public const uint EmployeeHired = 50;
    public const uint EmployeeFired = 51;
    public const uint CustomEmployeeHired = 52;
    public const uint CustomEmployeeFired = 53;

    public const uint CableConnected = 60;
    public const uint CableDisconnected = 61;
    public const uint RackUnmounted = 62;
    public const uint SwitchBroken = 63;
    public const uint SwitchRepaired = 64;
    public const uint WallPurchased = 65;

    public const uint GameSaved = 70;
    public const uint GameLoaded = 71;
    public const uint GameAutoSaved = 72;

    public const uint HookBridgeInstalled = 90;
    public const uint HookBridgeTriggered = 91;
}
