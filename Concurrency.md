# Concurrency Management
Concurrency is about handling multiple tasks at the same time, which can improve performance and responsiveness.

Why concurrency?   
To make efficient use of resources (CPU, I/O), avoid blocking, and keep apps responsive.

Challenges:   
When tasks share data, you must handle synchronization to avoid conflicts or data corruption (race conditions).

Tools:   
Synchronization primitives exist specifically to manage concurrency—they help multiple threads or processes coordinate so they don’t interfere with each other when accessing shared resources.

### In .Net   
Locks:
| Feature                 | `lock` (C# keyword)             | `Mutex` (System.Threading)          | `Semaphore` / `SemaphoreSlim`           |
|-------------------------|--------------------------------|------------------------------------|----------------------------------------|
| Scope                   | In-process only                | Cross-process (system-wide)         | Cross-process (`Semaphore`) / In-process (`SemaphoreSlim`) |
| Use case                | Protect critical section within a single process | Protect resources across processes | Control access to a limited number of resources            |
| Overhead                | Low                            | Higher (kernel object)              | Medium                                 |
| Performance             | Fast                          | Slower due to OS transitions        | `SemaphoreSlim` faster than `Semaphore` but both slower than `lock` |
| Recursion (re-entrant)  | Yes                           | No                                 | No                                    |
| Supports async/await    | No                            | No                                 | `SemaphoreSlim` supports async/await |
| Cross-process safe      | No                            | Yes                               | `Semaphore` yes / `SemaphoreSlim` no  |


### In SQL   
Isolation Levels:


### A few Concurrent Scenarios   
1. Handling Concurrent Access to One-Time Secret Keys in a Database   
Scenario:
A table stores one-time secret keys. Each time a user calls an API, the system must return an unused key and mark it as deprecated (used). Multiple users may call the API concurrently.

Question:
How can you safely handle concurrent requests to ensure that each user receives a unique, unused secret key without conflicts or duplicates?

Explanation:   
Default isolation level of a sql transaction is READ COMMITTED.
With READ COMMITTED, two users could read the same unused key before either of them updates its status—resulting in duplicate key assignment.
