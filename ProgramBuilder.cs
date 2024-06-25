using Microsoft.Extensions.DependencyInjection;
using CommandLine;

namespace Producer
{
    public class CLIApplicationDirector()
    {
        public static CLIApplication GetProducerApp(string redisDSN)
        {
            CliApplicationBuilder builder = CLIApplication.CreateBuilder();

            builder.InitDefaultContainer(redisDSN);
            builder.InitConsumerCommand("consumer");
            builder.InitProducerCommand("producer");
            CLIApplication app = builder.Build();

            return app;
        }
    }

    public class CliApplicationBuilder()
    {
        private readonly IServiceCollection services = new ServiceCollection();
        private readonly Dictionary<string, Type> aliases = [];
        private CLIApplication _app = new();

        public void InitDefaultContainer(string redisDSN)
        {
            services.AddSingleton<RedisClient>(new RedisClient(redisDSN));
            services.AddSingleton<SimpleProducer>((IServiceProvider provider) => new SimpleProducer(provider.GetRequiredService<RedisClient>()));
        }

        public void InitConsumerCommand(string command)
        {
            services.AddSingleton<ConsumerCommand>((IServiceProvider provider) => new ConsumerCommand(provider.GetRequiredService<SimpleProducer>()));
            aliases[command] = typeof(ConsumerCommand);
        }

        public void InitProducerCommand(string command)
        {
            services.AddSingleton<ProducerCommand>((IServiceProvider provider) => new ProducerCommand(provider.GetRequiredService<SimpleProducer>()));
            aliases[command] = typeof(ProducerCommand);
        }

        public void Flush()
        {
            _app = new CLIApplication();
        }

        public CLIApplication Build()
        {
            _app.SetServiceProvider(services.BuildServiceProvider());
            _app.SetAliases(aliases);

            return _app;
        }
    }

    public class CLIOptions()
    {
        public readonly struct InputArguments(string command, string channel, string message)
        {
            public readonly string Command = command;
            public readonly string Channel = channel;
            public readonly string Message = message;
        }

        public class Options
        {
            [Option('h', "channel", Required = true, HelpText = "Channel.")]
            public required string Channel { get; set; }
            [Option('m', "message", Required = false, HelpText = "Message.")]
            public required string Message { get; set; }
            [Option('c', "command", Required = true, HelpText = "Command.", Default = "Consumer")]
            public required string Command { get; set; }
        }

        public static InputArguments InitParsedOptions(string[] args)
        {
            string Message = "";
            string Channel = "";
            string Command = "";
            ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
               .WithParsed(options =>
               {
                   Message = options.Message;
                   Channel = options.Channel;
                   Command = options.Command;
               });

            return new InputArguments(Command, Channel, Message);
        }
    }

    public class CLIApplication
    {
        private ServiceProvider? ServiceProvider { get; set; } = null;
        private Dictionary<string, Type> CommandAliases { get; set; } = [];

        public void Run(string[] args)
        {
            CLIOptions.InputArguments Arguments = CLIOptions.InitParsedOptions(args);

            ICommandRun Command = GetCommand(Arguments.Command);

            Command.Run(Arguments);
        }

        private ICommandRun GetCommand(string command)
        {
            Type Command = CommandAliases[command];

            if (ServiceProvider is null)
            {
                throw new Exception("Service provider should be setted");
            }

            return (ICommandRun)ServiceProvider.GetRequiredService(Command);
        }

        public void SetServiceProvider(ServiceProvider services)
        {
            ServiceProvider = services;
        }

        public void SetAliases(Dictionary<string, Type> aliases)
        {
            CommandAliases = aliases;
        }


        public static CliApplicationBuilder CreateBuilder()
        {
            return new CliApplicationBuilder();
        }
    }
}

