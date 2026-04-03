using System;
using System.Runtime.InteropServices;
using System.Text;
using MelonLoader;

namespace DataCenterModLoader;

internal static class EventDispatcher
{
    private static FFIBridge _ffiBridge;
    private static MelonLogger.Instance _logger;

    [StructLayout(LayoutKind.Sequential)]
    private struct ValueChangedEvent
    {
        public float OldValue;
        public float NewValue;
        public float Delta;
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

    public static void Initialize(FFIBridge ffiBridge, MelonLogger.Instance logger)
    {
        _ffiBridge = ffiBridge;
        _logger = logger;
    }

    public static void LogError(string message)
    {
        try
        {
            _logger?.Error(message);
            CrashLog.Log($"EventDispatcher error: {message}");
        }
        catch
        {
        }
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
    public static void FireCustomEmployeeHired(string employeeId) => DispatchString(EventIds.CustomEmployeeHired, employeeId);
    public static void FireCustomEmployeeFired(string employeeId) => DispatchString(EventIds.CustomEmployeeFired, employeeId);
    public static void FireHookBridgeInstalled(int installed, int failed) => Dispatch(EventIds.HookBridgeInstalled, new IntPairEvent { A = installed, B = failed });
    public static void FireHookBridgeTriggered(string methodKey) => DispatchString(EventIds.HookBridgeTriggered, methodKey);

    private static void Dispatch(uint eventId)
    {
        try
        {
            _ffiBridge?.DispatchEvent(eventId, IntPtr.Zero, 0);
        }
        catch (Exception ex)
        {
            LogError($"Dispatch({eventId}) failed: {ex.Message}");
        }
    }

    private static void Dispatch<T>(uint eventId, T payload) where T : struct
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
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
