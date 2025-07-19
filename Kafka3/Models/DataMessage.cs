using System.ComponentModel.DataAnnotations;

namespace Kafka3.Models;

public class DataMessage
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Value { get; set; }
}
