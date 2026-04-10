using System.Runtime.InteropServices;

namespace SteamUtility.Core.Interop;

public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper
{
    protected IntPtr InstanceAddress { get; private set; }

    protected TNativeFunctions NativeFunctions { get; private set; } = default!;

    public void Initialize(IntPtr instanceAddress)
    {
        if (instanceAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException("Cannot initialize wrapper with a null native instance.");
        }

        InstanceAddress = instanceAddress;
        var nativeInstance = Marshal.PtrToStructure<NativeClass>(instanceAddress);
        NativeFunctions = Marshal.PtrToStructure<TNativeFunctions>(nativeInstance.VTablePointer)!;
    }

    protected TReturn Call<TReturn, TDelegate>(IntPtr functionPointer, params object[] arguments)
        where TDelegate : Delegate
    {
        var nativeDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPointer);
        return (TReturn)nativeDelegate.DynamicInvoke(arguments)!;
    }

    protected void Call<TDelegate>(IntPtr functionPointer, params object[] arguments)
        where TDelegate : Delegate
    {
        var nativeDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPointer);
        nativeDelegate.DynamicInvoke(arguments);
    }
}
