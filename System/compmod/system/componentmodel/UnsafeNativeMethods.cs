// Decompiled with JetBrains decompiler
// Type: Microsoft.Win32.UnsafeNativeMethods
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 90EE3CBB-9DBB-439F-B2A3-106CEFDE0581
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.xml

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32
{
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

    }
}
