namespace Producer
{
    internal class CommandContext(IStrategy strategy)
    {
        private IStrategy Strategy { set; get; } = strategy;

        async public Task<dynamic> RunCommand()
        {
            return await Strategy.Run();
        }
    }

    class ConsumerCommand : IStrategy
    {
        async public Task<dynamic> Run()
        {
            await Task.Delay(1);

            return "Consumer command started";
        }
    }

    class ProducerCommand : IStrategy
    {
        async public Task<dynamic> Run()
        {
            await Task.Delay(1);

            return "Producer command started";
        }


    }

    public interface IStrategy
    {
        abstract public Task<dynamic> Run();
    }
}