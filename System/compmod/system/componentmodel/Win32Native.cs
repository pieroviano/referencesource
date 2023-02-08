// Decompiled with JetBrains decompiler
// Type: Microsoft.Win32.Win32Native
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 90EE3CBB-9DBB-439F-B2A3-106CEFDE0581
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.xml

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Microsoft.Win32
{
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    internal static class Win32Native
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);
    }
}
