using System;
using System.Threading.Tasks;

namespace CacheAOP.ACL
{

    public interface IInquiryServiceFacade
    {
        Task<TaxInquiryResult> TaxInquiry(string nationalId, string postalCode);
        Task<IdentityInfoResult> IdentityInquiry(string nationalId , DateTime birth);
    }
    public class InquiryServiceFacade : IInquiryServiceFacade
    {
        [Cache]
        public virtual async Task<TaxInquiryResult> TaxInquiry(string nationalId, string postalCode)
        {
            return new TaxInquiryResult()
            {
                IsSuccessful = true,
                TaxCode = "1254866333"
            };
        }
        [Cache]
        public virtual async Task<IdentityInfoResult> IdentityInquiry(string nationalId, DateTime birth)
        {
            return new IdentityInfoResult()
            {
                IsSuccessful = true,
                Name = "MashtHasan",
                LastName = "Hasani"
            };
        }
    }

}
