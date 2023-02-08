namespace System
{
    internal static class AppContextDefaultValues
    {
        private static void ParseTargetFrameworkName(out string identifier, out string profile, out int version)
        {
            if (!AppContextDefaultValues.TryParseFrameworkName(".NETCore", out identifier, out version, out profile))
            {
                identifier = ".NETFramework";
                version = 40000;
                profile = string.Empty;
            }
        }

        public static void PopulateDefaultValues()
        {
            string str;
            string str1;
            int num;
            AppContextDefaultValues.ParseTargetFrameworkName(out str, out str1, out num);
            AppContextDefaultValues.PopulateDefaultValuesPartial(str, str1, num);
        }

        private static void PopulateDefaultValuesPartial(string platformIdentifier, string profile, int version)
        {
            if (platformIdentifier == ".NETCore" || platformIdentifier == ".NETFramework")
            {
                if (version <= 40502)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableSchUseStrongCrypto", true);
                }
                if (version <= 40601)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.MemberDescriptorEqualsReturnsFalseIfEquivalent", true);
                }
                if (version <= 40602)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableSystemDefaultTlsVersions", true);
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableTlsAlerts", true);
                }
                if (version <= 40700)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.IO.Ports.DoNotCatchSerialStreamThreadExceptions", true);
                }
                if (version <= 40701)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.Uri.DontEnableStrictRFC3986ReservedCharacterSets", true);
                    LocalAppContext.DefineSwitchDefault("Switch.System.Uri.DontKeepUnicodeBidiFormattingCharacters", true);
                    LocalAppContext.DefineSwitchDefault("Switch.System.IO.Compression.DoNotUseNativeZipLibraryForDecompression", true);
                    return;
                }
            }
            else
            {
                if (!(platformIdentifier == "WindowsPhone") && !(platformIdentifier == "WindowsPhoneApp"))
                {
                    return;
                }
                if (version <= 80100)
                {
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableSchUseStrongCrypto", true);
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableSystemDefaultTlsVersions", true);
                    LocalAppContext.DefineSwitchDefault("Switch.System.Net.DontEnableTlsAlerts", true);
                }
            }
        }

        private static bool TryParseFrameworkName(string frameworkName, out string identifier, out int version, out string profile)
        {
            string empty = string.Empty;
            string str = empty;
            profile = empty;
            identifier = str;
            version = 0;
            if (frameworkName == null || frameworkName.Length == 0)
            {
                return false;
            }
            string[] strArrays = frameworkName.Split(new char[] { ',' });
            version = 0;
            if ((int)strArrays.Length < 2 || (int)strArrays.Length > 3)
            {
                return false;
            }
            identifier = strArrays[0].Trim();
            if (identifier.Length == 0)
            {
                return false;
            }
            bool flag = false;
            profile = null;
            for (int i = 1; i < (int)strArrays.Length; i++)
            {
                string[] strArrays1 = strArrays[i].Split(new char[] { '=' });
                if ((int)strArrays1.Length != 2)
                {
                    return false;
                }
                string str1 = strArrays1[0].Trim();
                string str2 = strArrays1[1].Trim();
                if (!str1.Equals("Version", StringComparison.OrdinalIgnoreCase))
                {
                    if (!str1.Equals("Profile", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (!string.IsNullOrEmpty(str2))
                    {
                        profile = str2;
                    }
                }
                else
                {
                    flag = true;
                    if (str2.Length > 0 && (str2[0] == 'v' || str2[0] == 'V'))
                    {
                        str2 = str2.Substring(1);
                    }
                    Version version1 = new Version(str2);
                    version = version1.Major * 10000;
                    if (version1.Minor > 0)
                    {
                        version = version + version1.Minor * 100;
                    }
                    if (version1.Build > 0)
                    {
                        version += version1.Build;
                    }
                }
            }
            if (!flag)
            {
                return false;
            }
            return true;
        }
    }
}