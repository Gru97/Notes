# Parallel class in dotnet:
**Code**:   
``{
var contracts = await repository.GetAll().ToList();
await Parallel.ForEachAsync(contracts, new ParallelOptions { CancellationToken = stoppingToken, MaxDegreeOfParallelism = 8 },
async (contract, def) =>          
{                 
DoSomething(contract); //this is supposed to do http call and not change the state of the contract or execute any database command         
}          
}``

DoSomething() was actually doing some command and query execution on databased that caused this issue when registerd scope.   
Issue with shared database context (EF) between threads. We used a transient db context to solve this.    
**Quoting from microsoft docs:**    
Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads. Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute in parallel.
When EF Core detects an attempt to use a DbContext instance concurrently, you'll see an InvalidOperationException with a message like this:
A second operation started on this context before a previous operation completed. This is usually caused by different threads using the same instance of DbContext, however instance members are not guaranteed to be thread safe.
When concurrent access goes undetected, it can result in undefined behavior, application crashes and data corruption.
There are common mistakes that can inadvertently cause concurrent access on the same DbContext instance:
This is safe from concurrent access issues in most ASP.NET Core applications because there is only one thread executing each client request at a given time, and because each request gets a separate dependency injection scope (and therefore a separate DbContext instance).
Any code that explicitly executes multiple threads in parallel should ensure that DbContext instances aren't ever accessed concurrently.
Using dependency injection, this can be achieved by either registering the context as scoped, and creating scopes (using IServiceScopeFactory) for each thread, or by registering the DbContext as transient (using the overload of AddDbContext which takes a ServiceLifetime parameter).

**Question**: What is the difference between Parallel.ForEachAsync and Task.WhenAll ?   
**Question**: What is the difference between Parallel.ForEachAsync and Parallel.ForEach ?
