using System.Text.Json;
using StackExchange.Redis;

namespace Producer
{
    public class SimpleProducer(RedisClient redisClient, string channel) : IProducer
    {
        public async Task<long> Publish(string message)
        {
            return await redisClient
            .GetSubscriber()
            .PublishAsync(
                channel,
                 JsonSerializer.Serialize(message),
                 CommandFlags.FireAndForget
                 );

        }
    }
}