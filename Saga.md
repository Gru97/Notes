**What is Saga design pattern?**
- Applying the Saga Pattern • Caitie McCaffrey • GOTO 2015
- microXchg 2018 - Managing data consistency in a microservice architecture using Sagas


Originating from an article(https://www.cs.cornell.edu/andru/cs711/2002fa/reading/sagas.pdf) by Hector Garcaa-Molrna and Kenneth Salem in 1987, the main goal described in the article for the proposed SAGA pattern was to solve the issue of long-lived transactions. Quoting the article, "Long lived transactions (LLTs) hold on to database resources for relatively long periods of time, significantly delaying the termination of shorter and more common transactions". The idea is to break an LLT into a sequence of independent transactions that can be committed locally and reverted if necessary. It's basically modelling LLTs as a state machine with transactions T1,T2, etc. and their corresponding reverting actions (compensation) C1,C2, etc.   
   
A simple scenario that can leverage from this pattern is an online shopping application with Ordering, Stock, Payment, Shipping services each running independent of one another and has their own databases. After user submits an order (T1), user pays for the order(T2) and the item is going to be shipped. If in shipment, the stock is not available, we must revert the process. So we must undo the payment(C2) and change the state of the order to canceled(C1).   
    
Sometimes failure of a transaction doesn't need reverting. It might be enough to store the points up to which failure occurred and retry the operations left later.
There are two ways a saga can be modeled: Choreography and Orchestration. In Orchestration there is a central stateful component that has the information to command T1,T2, etc. to be executed or reverted once there is a failure. In Choreography, there is no central component to orchestrate transactions. The components react to messages coming from other components and send different messages for success or failure of their transactions.   
    
In applications that adapt microservice architecture, saga can be very useful when a certain process expands multiple services, or in DDD terms, when a process expands multiple bounded contexts. But that doesn't mean saga is not relevant for monolithic applications. Where ever we have a "Long lived transaction" we can break it into a sequence of transactions to leverage from this pattern.   
    
Saga is atomic, since it guarantees that all the transactions are either committed or their corresponding compensations would be executed.   
They are eventually consistence. But there's a lack of isolation that causes anomalies like lost updates (saga is in the middle of execution when some changes happen, and saga wouldn't know), dirty read (because transaction can be compensated after being committed), etc. We must take countermeasures to deal with these issues. There are several ways to handle these situations:   
- Introduce semantic lock (like pending state for an entity).    
- Try to model transactions in a way that the order of their execution doesn't matter   
- Put the risky transaction with compensations in the last step of the saga   
- Re-read data when possible,    
- Try to use saga to implement low risk processes.   
