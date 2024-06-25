using System.Text.Json;
using StackExchange.Redis;

namespace Producer
{
    public class SimpleProducer(RedisClient redisClient)
    {
        public void Publish(string channel, string message)
        {
            redisClient
            .GetSubscriber()
            .Publish(
                channel,
                 JsonSerializer.Serialize(message),
                 CommandFlags.FireAndForget
                 );
        }

        public void Consume(string channel)
        {
            redisClient
            .GetSubscriber()
            .Subscribe(channel, (channel, type) =>
            {
                Console.WriteLine(type);
            });

            Console.WriteLine("Listening for messages. Press any key to exit.");
            Console.ReadKey();
        }
    }
}