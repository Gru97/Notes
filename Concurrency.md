# Concurrency Management
Concurrency is about handling multiple tasks at the same time, which can improve performance and responsiveness.

Why concurrency?   
To make efficient use of resources (CPU, I/O), avoid blocking, and keep apps responsive.

Challenges:   
When tasks share data, you must handle synchronization to avoid conflicts or data corruption (race conditions).

Tools:   
Synchronization primitives exist specifically to manage concurrency—they help multiple threads or processes coordinate so they don’t interfere with each other when accessing shared resources.

In Dotnet:
| Feature                 | `lock` (C# keyword)             | `Mutex` (System.Threading)          | `Semaphore` / `SemaphoreSlim`           |
|-------------------------|--------------------------------|------------------------------------|----------------------------------------|
| Scope                   | In-process only                | Cross-process (system-wide)         | Cross-process (`Semaphore`) / In-process (`SemaphoreSlim`) |
| Use case                | Protect critical section within a single process | Protect resources across processes | Control access to a limited number of resources            |
| Overhead                | Low                            | Higher (kernel object)              | Medium                                 |
| Performance             | Fast                          | Slower due to OS transitions        | `SemaphoreSlim` faster than `Semaphore` but both slower than `lock` |
| Recursion (re-entrant)  | Yes                           | No                                 | No                                    |
| Supports async/await    | No                            | No                                 | `SemaphoreSlim` supports async/await |
| Cross-process safe      | No                            | Yes                               | `Semaphore` yes / `SemaphoreSlim` no  |
