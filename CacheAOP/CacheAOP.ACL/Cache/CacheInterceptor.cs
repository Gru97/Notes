using Ninject.Extensions.Interception;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ninject.Extensions.Interception.Invocation;

namespace CacheAOP.ACL
{
    
    public class CacheInterceptor : IInterceptor
    {
        private readonly IDatabase _database;
        public CacheInterceptor()
        {
            _database = ConnectionMultiplexer
                .Connect("").GetDatabase();
        }
     
        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Request.Method.Name;
            var arguments = invocation.Request.Arguments.ToList();
            var method = typeof(CacheKeys).GetMethod(methodName);
            var returnType = invocation.Request.Method.ReturnType;
            var cachekey = method.Invoke(null, arguments.Take(1).ToArray()).ToString();

            var cachedData = _database.StringGet(cachekey);
            
            if (cachedData.HasValue)
            {
                if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    returnType = returnType.GenericTypeArguments[0];
                
                var obj = JsonConvert.DeserializeObject(cachedData, returnType);
                invocation.ReturnValue = Task.FromResult(obj.CastToReflected(returnType));
                return;
            }

            invocation.Proceed();

            //non-blocking approach
            var task = (Task)invocation.ReturnValue;
            task.ContinueWith((awaitedTask) =>
            {
                var result = invocation.ReturnValue.GetType().GetProperty("Result").GetValue(awaitedTask);
                
                if (result.CastTo<Result>().IsSuccessful)  //All methods from ACL must inherit from custom Result class
                    _database.StringSet(cachekey, JsonConvert.SerializeObject(result), TimeSpan.FromHours(1));
            });

            //blocking approach
            //task.Wait(); 
            //var result = invocation.ReturnValue.GetType().GetProperty("Result").GetValue(task);
            //_database.StringSet(cachekey, JsonConvert.SerializeObject(result), TimeSpan.FromHours(1));

        }
    }
}
