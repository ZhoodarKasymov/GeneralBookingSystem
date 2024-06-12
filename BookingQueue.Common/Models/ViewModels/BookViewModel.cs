namespace BookingQueue.Common.Models.ViewModels;

public class BookViewModel
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public long? ServiceId { get; set; }
    public DateTime? BookingDate { get; set; }
}