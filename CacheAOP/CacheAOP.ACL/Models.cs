using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheAOP.ACL
{
    public class IdentityInfoResult : Result
    {
        public string Name { get; set; }
        public string LastName { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }
        public string Description { get; set; }

    }

    public class TaxInquiryResult : Result
    {
        public string TaxCode { get; set; }
    }
}
