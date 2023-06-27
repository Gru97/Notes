public interface IParcelDeliveryHandler
{
    void Handle(Request request);
    void SetNext(IParcelDeliveryHandler? handler);
}