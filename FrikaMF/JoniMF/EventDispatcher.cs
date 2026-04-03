using System;
using System.Runtime.InteropServices;
using System.Text;
using MelonLoader;

namespace FrikaMF;

internal static class EventDispatcher
{
    private static FFIBridge _ffiBridge;
    private static MelonLogger.Instance _logger;
    private static uint _lastEventId;
    private static long _lastEventTick;
    private static double _lastEventPayloadHash;

    [StructLayout(LayoutKind.Sequential)]
    private struct ValueChangedEvent
    {
        public double OldValue;
        public double NewValue;
        public double Delta;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UIntEvent
    {
        public uint Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IntEvent
    {
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByteEvent
    {
        public byte Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ShopItemAddedEvent
    {
        public int ItemId;
        public int Price;
        public int Quantity;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IntPairEvent
    {
        public int A;
        public int B;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NetWatchDispatchedEvent
    {
        public int DeviceType;
        public int Reason;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CustomEmployeeEventData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] EmployeeId;

        public static CustomEmployeeEventData Create(string employeeId)
        {
            var data = new CustomEmployeeEventData { EmployeeId = new byte[64] };
            if (string.IsNullOrWhiteSpace(employeeId))
                return data;

            byte[] bytes = Encoding.ASCII.GetBytes(employeeId);
            Array.Copy(bytes, data.EmployeeId, Math.Min(bytes.Length, 63));
            return data;
        }
    }

    public static void Initialize(FFIBridge ffiBridge, MelonLogger.Instance logger)
    {
        _ffiBridge = ffiBridge;
        _logger = logger;
    }

    public static void LogError(string message)
    {
        try
        {
            string formatted = $"[events] {message}";
            _logger?.Error(formatted);
            CrashLog.Log(formatted);
        }
        catch
        {
        }
    }

    private static bool IsDuplicate(uint eventId, double payloadHash = 0.0)
    {
        long now = System.Diagnostics.Stopwatch.GetTimestamp();
        long elapsed = now - _lastEventTick;
        long threshold = System.Diagnostics.Stopwatch.Frequency / 20;

        bool duplicate = eventId == _lastEventId
                         && elapsed < threshold
                         && Math.Abs(payloadHash - _lastEventPayloadHash) < 0.0001;

        _lastEventId = eventId;
        _lastEventTick = now;
        _lastEventPayloadHash = payloadHash;
        return duplicate;
    }

    public static void FireSimple(uint eventId) => Dispatch(eventId);

    public static void FireValueChanged(uint eventId, float oldValue, float newValue, float delta)
    {
        Dispatch(eventId, new ValueChangedEvent
        {
            OldValue = oldValue,
            NewValue = newValue,
            Delta = delta,
        });
    }

    public static void FireServerPowered(bool isOn) => Dispatch(EventIds.ServerPowered, new ByteEvent { Value = isOn ? (byte)1 : (byte)0 });
    public static void FireDayEnded(uint day) => Dispatch(EventIds.DayEnded, new UIntEvent { Value = day });
    public static void FireCustomerAccepted(int customerId) => Dispatch(EventIds.CustomerAccepted, new IntEvent { Value = customerId });
    public static void FireCustomerSatisfied(int customerId) => Dispatch(EventIds.CustomerSatisfied, new IntEvent { Value = customerId });
    public static void FireCustomerUnsatisfied(int customerId) => Dispatch(EventIds.CustomerUnsatisfied, new IntEvent { Value = customerId });
    public static void FireCableConnected() => Dispatch(EventIds.CableConnected);
    public static void FireCableDisconnected() => Dispatch(EventIds.CableDisconnected);
    public static void FireServerCustomerChanged(int customerId) => Dispatch(EventIds.ServerCustomerChanged, new IntEvent { Value = customerId });
    public static void FireServerAppChanged(int appId) => Dispatch(EventIds.ServerAppChanged, new IntEvent { Value = appId });
    public static void FireRackUnmounted() => Dispatch(EventIds.RackUnmounted);
    public static void FireSwitchBroken() => Dispatch(EventIds.SwitchBroken);
    public static void FireSwitchRepaired() => Dispatch(EventIds.SwitchRepaired);
    public static void FireMonthEnded(int month) => Dispatch(EventIds.MonthEnded, new IntEvent { Value = month });

    public static void FireShopItemAdded(int itemId, int price, int quantity)
    {
        Dispatch(EventIds.ShopItemAdded, new ShopItemAddedEvent
        {
            ItemId = itemId,
            Price = price,
            Quantity = quantity,
        });
    }

    public static void FireShopCartCleared() => Dispatch(EventIds.ShopCartCleared);
    public static void FireWallPurchased() => Dispatch(EventIds.WallPurchased);
    public static void FireGameAutoSaved() => Dispatch(EventIds.GameAutoSaved);
    public static void FireShopItemRemoved(int itemId) => Dispatch(EventIds.ShopItemRemoved, new IntEvent { Value = itemId });
    public static void FireNetWatchDispatched(int deviceType, int reason) => Dispatch(EventIds.NetWatchDispatched, new NetWatchDispatchedEvent { DeviceType = deviceType, Reason = reason }, deviceType * 10.0 + reason);
    public static void FireCustomEmployeeHired(string employeeId) => Dispatch(EventIds.CustomEmployeeHired, CustomEmployeeEventData.Create(employeeId), employeeId?.GetHashCode() ?? 0.0);
    public static void FireCustomEmployeeFired(string employeeId) => Dispatch(EventIds.CustomEmployeeFired, CustomEmployeeEventData.Create(employeeId), (employeeId?.GetHashCode() ?? 0.0) + 0.5);
    public static void FireHookBridgeInstalled(int installed, int failed) => Dispatch(EventIds.HookBridgeInstalled, new IntPairEvent { A = installed, B = failed });
    public static void FireHookBridgeTriggered(string methodKey) => DispatchString(EventIds.HookBridgeTriggered, methodKey);

    private static void Dispatch(uint eventId)
    {
        try
        {
            if (IsDuplicate(eventId))
                return;

            _ffiBridge?.DispatchEvent(eventId, IntPtr.Zero, 0);
        }
        catch (Exception ex)
        {
            LogError($"Dispatch({eventId}) failed: {ex.Message}");
        }
    }

    private static void Dispatch<T>(uint eventId, T payload) where T : struct
        => Dispatch(eventId, payload, 0.0);

    private static void Dispatch<T>(uint eventId, T payload, double payloadHash) where T : struct
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            if (IsDuplicate(eventId, payloadHash))
                return;

            int size = Marshal.SizeOf<T>();
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(payload, ptr, false);
            _ffiBridge?.DispatchEvent(eventId, ptr, (uint)size);
        }
        catch (Exception ex)
        {
            LogError($"Dispatch({eventId}) with payload failed: {ex.Message}");
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }
    }

    private static void DispatchString(uint eventId, string value)
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes((value ?? string.Empty) + "\0");
            ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            _ffiBridge?.DispatchEvent(eventId, ptr, (uint)bytes.Length);
        }
        catch (Exception ex)
        {
            LogError($"Dispatch({eventId}) with string payload failed: {ex.Message}");
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }
    }
}
