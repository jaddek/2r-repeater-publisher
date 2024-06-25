namespace Producer
{
    internal class ProducerCommand(SimpleProducer producer) : ICommandRun
    {
        public void Run(CLIOptions.InputArguments arguments)
        {
            producer.Publish(arguments.Channel, arguments.Message);
        }
    }

    internal class ConsumerCommand(SimpleProducer producer) : ICommandRun
    {
        public void Run(CLIOptions.InputArguments arguments)
        {
            producer.Consume(arguments.Channel);
        }
    }

    public interface ICommandRun
    {
        public void Run(CLIOptions.InputArguments arguments);
    }
}