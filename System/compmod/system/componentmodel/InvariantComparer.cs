using System.Collections;
using System.Globalization;

namespace System
{
    [Serializable]
    internal class InvariantComparer : IComparer
    {
        private CompareInfo m_compareInfo;
        internal static readonly InvariantComparer Default = new InvariantComparer();

        internal InvariantComparer() => this.m_compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        public int Compare(object a, object b)
        {
            string string1 = a as string;
            string string2 = b as string;
            return string1 != null && string2 != null ? this.m_compareInfo.Compare(string1, string2) : Comparer.Default.Compare(a, b);
        }
    }
}