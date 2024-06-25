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
            .Publish(
                new RedisChannel(channel, RedisChannel.PatternMode.Literal),
                 JsonSerializer.Serialize(message),
                 CommandFlags.FireAndForget
                 );
        }

        public void Consume(string channel)
        {
            redisClient
            .GetSubscriber()
            .Subscribe(
                new RedisChannel(channel, RedisChannel.PatternMode.Literal),
                (channel, type) =>
            {
                Console.WriteLine(type);
            });

            Console.WriteLine("Listening for messages. Press any key to exit.");
            Console.ReadKey();
        }
    }
}