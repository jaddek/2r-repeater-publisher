using Microsoft.Extensions.DependencyInjection;
using CommandLine;

namespace Producer
{
    public class Program
    {
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

        static async Task Main(string[] args)
        {
            DotEnv.Load(".env");
            string message = "";
            string command = "";
            ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
                       .WithParsed(options =>
                       {
                           message = options.Message;
                           command = options.Command;
                       });

            Dictionary<string, Type> commandMap = new()
            {
                ["producer"] = typeof(ProducerCommand),
                ["consumer"] = typeof(ConsumerCommand),

            };

            if (commandMap.Keys.ToList().Contains(command) == false)
            {
                Console.Error.WriteLine("Command '{0}' not registered", command);
                return;
            }

            using ServiceProvider provider = InitContainer();
            IStrategy Command = (IStrategy)provider.GetRequiredService(commandMap[command]);
            CommandContext CommandContext = new(Command);
            var result = await CommandContext.RunCommand();

            Console.WriteLine(result);
        }
    }
}