## Chain Of Responsibility Pattern
I came across a wrong way of using Chain of Responsibility pattern in a project and that became the motivation to learn about it and the right solution to that problem.    
In GoF the intent of COR is said to be, "avoid coupling the sender of a request to its receiver by giving more than one object a chance to handle the request, chain the receiving objects and pass the request along the chain until an object handles it".
In the provided example, assuming we want to process some parcels with different characteristics like different weight, we don't know which handler would handle the parcel a priori. We will chain all handlers that have the same interface, and pass the parcel down to the chain. Based on the size (weight) of each parcel, the handlers choose to handle the request or pass it along. So we can add handlers independent of one another, and the object (request) to be handled wouldn't need to know anything about how and what handler will handle it.

We have the request modeled:
```
public class Request
{
    public int ParcelSize { get; set; }
    public string ParcelCategory { get; set; }
    public string ParcelCode { get; set; }

    public Request(int parcelSize, string parcelCategory, string parcelCode)
    {
        ParcelSize = parcelSize;
        ParcelCategory = parcelCategory;
        ParcelCode = parcelCode;
    }
}
```
And we have an interface for any handler that wants to handle a parcel:
```
public interface IParcelDeliveryHandler
{
    void Handle(Request request);
    void SetNext(IParcelDeliveryHandler? handler);
}
```
Based on parcel size we defined three handlers:
```
public class SmallParcelHandler : IParcelDeliveryHandler
{
    private IParcelDeliveryHandler? _next;

    public void Handle(Request request)
    {
        if(request.ParcelSize<=10)
            Console.WriteLine("I handle small parcels.");

        else if (_next is not null)
            _next.Handle(request);
        
    }

    public void SetNext(IParcelDeliveryHandler? handler)
    {
        _next=handler;
    }
}
```
```
public class MediumParcelHandler : IParcelDeliveryHandler
{
    private IParcelDeliveryHandler? _next;

    public void Handle(Request request)
    {
        if (request.ParcelSize > 10  && request.ParcelSize <= 20)
            Console.WriteLine("I handle medium parcels.");

        else if(_next is not null)
            _next.Handle(request);
        
    }
    public void SetNext(IParcelDeliveryHandler? handler)
    {
        _next = handler;
    }
}
```
```
public class LargeParcelHandler : IParcelDeliveryHandler
{
    private IParcelDeliveryHandler? _next;

    public void Handle(Request request)
    {
        if (request.ParcelSize > 20 )
            Console.WriteLine("I handle large parcels.");

        else if (_next is not null)
            _next.Handle(request);
        
    }
    public void SetNext(IParcelDeliveryHandler? handler)
    {
        _next = handler;
    }
}
```
We chain the handlers using a builder:
```
public class ParcelDeliveryBuilder
{
    public static IParcelDeliveryHandler Create(Request request)
    {
        var handler1 = new SmallParcelHandler();
        var handler2 = new MediumParcelHandler();
        var handler3 = new LargeParcelHandler();

        handler1.SetNext(handler2);
        handler2.SetNext(handler3);
        return handler1;
    }
}
```
And we run a simple scenario using a test:
```
using Xunit;
{
    public class Test
    {
        [Fact]
        public void CORTest()
        {

            var request = new Request(10, "edible", "125");
            var handler = ParcelDeliveryBuilder.Create(request);
            handler.Handle(request);
        }       
    }
}

```
