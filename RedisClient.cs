using StackExchange.Redis;

namespace Producer
{
    public class RedisClient(string DSN)
    {
        private readonly ConnectionMultiplexer _connection = ConnectionMultiplexer.Connect(DSN);

        public ISubscriber GetSubscriber()
        {
            return _connection.GetSubscriber();
        }
    }
}