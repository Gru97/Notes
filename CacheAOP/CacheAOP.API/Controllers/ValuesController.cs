using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CacheAOP.ACL;

namespace CacheAOP.API.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IInquiryServiceFacade _facade;

        public ValuesController(IInquiryServiceFacade facade)
        {
            _facade = facade;
        }

        // GET api/values
        public async Task<IdentityInfoResult> Get1()
        {
            var x= await _facade.IdentityInquiry("1200183345", new DateTime(1997, 08, 25));
            return x;
        }

    }
}
