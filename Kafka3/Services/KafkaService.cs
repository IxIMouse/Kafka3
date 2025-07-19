using Confluent.Kafka;
using Kafka3.Data;
using Kafka3.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Kafka3.Services;

public class KafkaService
{
    private readonly ILogger<KafkaService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly string _topic;
    private readonly string _tag;

    public KafkaService(ILogger<KafkaService> logger, ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _topic = configuration["Kafka:Topic"];
        _tag = configuration["Kafka:Tag"];
    }

    public async Task StartAsync()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(_topic);
            _logger.LogInformation("Started consuming messages from topic {Topic}", _topic);

            var messages = new List<DataMessage>();
            var startTime = DateTime.UtcNow;

            try
            {
                while (true)
                {
                    var cr = consumer.Consume();
                    if (cr.Message != null && cr.Message.Value.Contains(_tag))
                    {
                        var message = new DataMessage
                        {
                            CreatedAt = DateTime.UtcNow,
                            Value = new Random().Next(1, 1000) // Random positive number
                        };
                        messages.Add(message);
                        await _dbContext.DataMessages.AddAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming messages");
            }
            finally
            {
                await _dbContext.SaveChangesAsync();
                var checksum = CalculateChecksum(messages);
                var endTime = DateTime.UtcNow;
                _logger.LogInformation("Finished processing messages. Checksum: {Checksum}. Duration: {Duration} seconds.", checksum, (endTime - startTime).TotalSeconds);
            }
        }
    }

    private string CalculateChecksum(IEnumerable<DataMessage> messages)
    {
        using (var md5 = MD5.Create())
        {
            var hashInput = string.Join(",", messages.Select(m => m.Value));
            var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashInput));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
