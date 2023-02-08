// Decompiled with JetBrains decompiler
// Type: Microsoft.Win32.SafeLibraryHandle
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 90EE3CBB-9DBB-439F-B2A3-106CEFDE0581
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.xml

using Microsoft.Win32.SafeHandles;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Win32
{
    [SecurityCritical]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLibraryHandle()
            : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle() => UnsafeNativeMethods.FreeLibrary(this.handle);
    }
}