**Caching method results in redis**   
Caching is a cross cutting concern. It's not part of the core business of an application and we might use caching in different modules and different part of an application.    
If we do not seperate caching from the logic of the application, it clutters the code, makes it hard to read, voilates SRP and DRY principle.   
Cross cutting concerns like caching (logging, authentication, exception handling and so on) can leverage from AOP.    
In dotnet, we can use attributes along interceptors (with the help of a DI cotainer) to implement such functionality.   
In the code, we have an ACL which has the responsibility of talking to some external services (e.g. with http call) and translate that result back to our domain language.    
I want to cache the result of these calls in redis. I have used ninject DI container to add interceptors to method calls.   
The main challange I had was that methods I wanted to cache were async, and they returned tasks, and I could not await them to get their results. I used ContinueWith to cache result of that task when it's awailable.   
**Question**: What does it mean that we cache Tasks? Like in https://stackoverflow.com/questions/36084495/when-to-cache-tasks
