using Microsoft.Extensions.DependencyInjection;
using CommandLine;

namespace Producer
{
    public class Program
    {
        public readonly struct InputArguments(string command, string message)
        {
            public readonly string Command = command;
            public readonly string Message = message;
        }

        public class Options
        {
            [Option('m', "message", Required = false, HelpText = "Message.")]
            public required string Message { get; set; }
            [Option('c', "command", Required = true, HelpText = "Command.", Default = "Consumer")]
            public required string Command { get; set; }
        }

        static ServiceProvider InitContainer()
        {
            ServiceCollection services = new();
            services.AddSingleton<RedisClient>(new RedisClient(DotEnv.GetEnv("REDIS_DSN")));
            services.AddSingleton<SimpleProducer>((IServiceProvider provider) => new SimpleProducer(provider.GetRequiredService<RedisClient>(), DotEnv.GetEnv("PRODUCER_CHANNEL")));
            services.AddSingleton<ConsumerCommand>((IServiceProvider provider) => new ConsumerCommand(provider.GetRequiredService<SimpleProducer>()));
            services.AddSingleton<ProducerCommand, ProducerCommand>();
            ServiceProvider provider = services.BuildServiceProvider();

            return provider;
        }

        private static CommandsContainer InitCommandsContainer()
        {
            CommandsContainer CommandsContainer = new();
            CommandsContainer.RegisterCommand("producer", typeof(ProducerCommand));
            CommandsContainer.RegisterCommand("consumer", typeof(ConsumerCommand));

            return CommandsContainer;
        }

        private static InputArguments InitParsedOptions(string[] args)
        {
            string Message = "";
            string Command = "";
            ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
               .WithParsed(options =>
               {
                   Message = options.Message;
                   Command = options.Command;
               });

            return new InputArguments(Command, Message);
        }

        static async Task Main(string[] args)
        {
            DotEnv.Load(".env");
            // Init input arguments
            InputArguments InputArguments = InitParsedOptions(args);
            // Init DI container
            using ServiceProvider Provider = InitContainer();
            // Init Available commands
            CommandsContainer CommandContainer = InitCommandsContainer();

            try
            {
                // Get instance in DI by Command Type
                IStrategy CommandService = (IStrategy)Provider.GetRequiredService(
                    CommandContainer.GetRequiredCommandType(InputArguments.Command)
                );

                Console.WriteLine(await CommandService.Run());
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
            }
        }
    }
}