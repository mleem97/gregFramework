namespace FrikaMF;

internal static class EventIds
{
    public const uint MoneyChanged = 100;
    public const uint XPChanged = 101;
    public const uint ReputationChanged = 102;

    public const uint ServerPowered = 200;
    public const uint ServerBroken = 201;
    public const uint ServerRepaired = 202;
    public const uint ServerInstalled = 203;
    public const uint CableConnected = 204;
    public const uint CableDisconnected = 205;
    public const uint ServerCustomerChanged = 206;
    public const uint ServerAppChanged = 207;
    public const uint RackUnmounted = 208;
    public const uint SwitchBroken = 209;
    public const uint SwitchRepaired = 210;

    public const uint DayEnded = 300;
    public const uint MonthEnded = 301;

    public const uint CustomerAccepted = 400;
    public const uint CustomerSatisfied = 401;
    public const uint CustomerUnsatisfied = 402;

    public const uint ShopCheckout = 500;
    public const uint ShopItemAdded = 501;
    public const uint ShopCartCleared = 502;
    public const uint ShopItemRemoved = 503;

    public const uint EmployeeHired = 600;
    public const uint EmployeeFired = 601;

    public const uint GameSaved = 700;
    public const uint GameLoaded = 701;
    public const uint GameAutoSaved = 702;

    public const uint WallPurchased = 800;
    public const uint NetWatchDispatched = 900;

    public const uint CustomEmployeeHired = 1000;
    public const uint CustomEmployeeFired = 1001;

    public const uint HookBridgeInstalled = 1100;
    public const uint HookBridgeTriggered = 1101;
}
