namespace BookingQueue.Common.Models.DTO;

public class ServiceDto
{
    public long ServiceId { get; set; }
    public string ServiceName { get; set; }
    public List<ServiceDto> SubServices { get; set; }
}