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