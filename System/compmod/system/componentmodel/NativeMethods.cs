using System.Security;

namespace System.Runtime.InteropServices
{
    internal static class NativeMethods
    {
        public const int E_ABORT = -2147467260;

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("oleaut32.dll", PreserveSig = false)]
        internal static extern void VariantClear(IntPtr variant);

        [SuppressUnmanagedCodeSecurity]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("00020400-0000-0000-C000-000000000046")]
        [ComImport]
        internal interface IDispatch
        {
            [SecurityCritical]
            void GetTypeInfoCount(out uint pctinfo);

            [SecurityCritical]
            void GetTypeInfo(uint iTInfo, int lcid, out IntPtr info);

            [SecurityCritical]
            void GetIDsOfNames(ref Guid iid, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.LPWStr)] string[] names, uint cNames, int lcid, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.I4), Out] int[] rgDispId);

            [SecurityCritical]
            void Invoke(
                int dispIdMember,
                ref Guid riid,
                int lcid,
                System.Runtime.InteropServices.ComTypes.INVOKEKIND wFlags,
                ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pDispParams,
                IntPtr pvarResult,
                IntPtr pExcepInfo,
                IntPtr puArgErr);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal unsafe byte* pSecurityDescriptor;
            internal int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PDH_RAW_COUNTER
        {
            public int CStatus;
            public long TimeStamp;
            public long FirstValue;
            public long SecondValue;
            public int MultiCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PDH_FMT_COUNTERVALUE
        {
            public int CStatus;
            public double data;
        }
    }
}