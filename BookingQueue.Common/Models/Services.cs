using System.ComponentModel.DataAnnotations.Schema;

namespace BookingQueue.Common.Models;

[Table("services")]
public class Services
{
    [Column("id")]
    public long? Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }
    
    public long? prent_id { get; set; }

    [Column("translated_name")]
    public string? TranslatedName { get; set; }

    [Column("service_prefix")]
    public string? ServicePrefix { get; set; }

    [Column("deleted")]
    public DateTime? Deleted { get; set; }
}