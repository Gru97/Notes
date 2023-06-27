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