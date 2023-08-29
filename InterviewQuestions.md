## SOLID
I'm always asked about SOLID principles in interviews. I would like to write my understanding of it here:
### Single Responsibility: 
The S in SOLID is the initial for Single Responsibility Principle (SRP) which can be a little vague for people to understand (or rather too abstract). 
It is stated that "each software module should have one and only one reason to change. 
Uncle Bob, who coined the term, said it's been stolen from a variety of classic sources, and one in particular is a paper from Parnas that talks about the decomposition of modules and states that" ... We propose instead that one begins with a list of difficult design decisions or design decisions that are likely to change. Each module is then designed to hide such a decision from the others."

So how do we detect the "reason" for change that we want to isolate? I quote from [Uncle Bob's article](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html):    
> Imagine you took your car to a mechanic in order to fix a broken electric window. He calls you the next day saying it’s all fixed. When you pick up your car, you find the window works fine; but the car won’t start. It’s not likely you will return to that mechanic because he’s clearly an idiot.

> That’s how customers and managers feel when we break things they care about that they did not ask us to change. This is the reason we do not put SQL in JSPs. This is the reason we do not generate HTML in the modules that compute results. This is the reason that business rules should not know the database schema. This is the reason we separate concerns.

> Another wording for the Single Responsibility Principle is:
Gather together the things that change for the same reasons. Separate those things that change for different reasons. If you think about this, you’ll realize that this is just another way to define cohesion and coupling. We want to increase the cohesion between things that change for the same reasons, and we want to decrease the coupling between those things that change for different reasons."



### Open Closed Principle
The O in SOLID stands for Open Closed Principle (OCP), coined initially by Bertrand Meyer, who stated that "software entities (classes, modules, functions, etc.) should be open for extension but closed for modification. One way to do so is by using inheritance to extend the behaviors of existing classes without modifying them. There is an example from Uncle Bob [here](https://drive.google.com/file/d/0BwhCYaYDn8EgN2M5MTkwM2EtNWFkZC00ZTI3LWFjZTUtNTFhZGZiYmUzODc1/view?resourcekey=0-FsS837CGML599A_o5D-nAw) where he creates a Shape abstraction to handle drawing of shapes while sticking to this principle.
