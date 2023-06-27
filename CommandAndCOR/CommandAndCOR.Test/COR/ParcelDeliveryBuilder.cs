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