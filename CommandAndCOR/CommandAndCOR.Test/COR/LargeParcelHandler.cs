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