namespace Producer
{

    public interface IProducer
    {
        public Task<long> Publish(string message);
    }
}