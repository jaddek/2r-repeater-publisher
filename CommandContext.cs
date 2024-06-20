using Microsoft.Extensions.DependencyInjection;

namespace Producer
{

    internal class CommandsContainer()
    {
        private readonly Dictionary<string, Type> _commands = [];

        public void registerCommand(string key, Type command)
        {
            _commands[key] = command;
        }

        public Type GetRequiredCommandType(string command)
        {
            if (_commands.Keys.ToList().Contains(command) == false)
            {
                throw new Exception(String.Format("Command '{0}' not registered", command));
            }

            return _commands[command];
        }
    }

    internal class ConsumerCommand(SimpleProducer producer) : IStrategy
    {
        async public Task<dynamic> Run()
        {
            await producer.Publish("hello");

            return "Consumer command started";
        }
    }

    internal class ProducerCommand : IStrategy
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