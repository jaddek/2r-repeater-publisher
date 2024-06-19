namespace Producer
{
    internal class CommandContext(IStrategy Command)
    {
        private IStrategy _strategy { set; get; }

        public dynamic RunCommand()
        {
            return _strategy.Run();
        }
    }

    class ConsumerCommand : IStrategy
    {
        public dynamic Run()
        {
            Console.WriteLine("Yo");
            return "asdasd";
        }
    }

    class ProducerCommand : IStrategy
    {
        public dynamic Run()
        {
            Console.WriteLine("Yo");
            return "asdasd";
        }
    }

    public interface IStrategy
    {
        dynamic Run();
    }
}