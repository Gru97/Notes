using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheAOP.ACL
{
    internal static class CacheKeys
    {
        public static string TaxInquiry(string nationalCode) => $"TaxInquiry-{nationalCode}";
        public static string IdentityInquiry(string nationalCodeSignatory) =>
            $"IdentityInquiry-{nationalCodeSignatory}";
    }
}
