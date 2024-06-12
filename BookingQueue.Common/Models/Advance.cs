using System.ComponentModel.DataAnnotations.Schema;

namespace BookingQueue.Common.Models;

[Table("advance")]
public class Advance
{
    [Column("id")]
    public long? Id { get; set; }

    [Column("service_id")]
    public long? ServiceId { get; set; }

    [Column("advance_time")]
    public DateTime? AdvanceTime { get; set; }

    [Column("priority")]
    public int? Priority { get; set; }

    [Column("clients_authorization_id")]
    public long? ClientsAuthorizationId { get; set; }

    [Column("input_data")]
    public string? InputData { get; set; }

    [Column("comments")]
    public string? Comments { get; set; }
}