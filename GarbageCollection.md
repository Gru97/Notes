### What is Qarbage collection in dotnet? ###
**Question**: Why do we apply _using_ statment (in C#) for manageing resources when we have gc?

There are two types ofr resources here that we are concerned about: Managed resources and unmanaged resources.   
Managed resources basically means "managed memory" that is managed by the garbage collector. When you no longer have any references to a managed object (which uses managed memory), the garbage collector will (eventually) release that memory for you.

Unmanaged resources are then everything that the garbage collector does not know about. For example: Open files, Open network connections, Open database connections, etc.
Normally you want to release those unmanaged resources before you lose all the references you have to the object managing them. You do this by calling Dispose on that object, or (in C#) using the using statement which will handle calling Dispose for you before the scope has ended.

Related references:   
https://andresantarosa.medium.com/heap-stack-e-garbage-collector-a-practical-guide-to-net-memory-management-system-7e60bbadf199
