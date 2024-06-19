
using Microsoft.Extensions.DependencyInjection;
using CommandLine;
using System.Collections.Specialized;
using System.Reflection;

namespace Producer;

public class Program
{
    public class Options
    {
        [Option('m', "message", Required = false, HelpText = "Message.")]
        public required string Message { get; set; }
        [Option('c', "command", Required = true, HelpText = "Command.", Default = "Producer")]
        public required string Command { get; set; }
    }
    static ServiceProvider InitContainer()
    {
        ServiceCollection services = new();
        services.AddSingleton<RedisClient>(new RedisClient(DotEnv.GetEnv("REDIS_DSN")));
        services.AddSingleton<ProducerCommand, ProducerCommand>();
        services.AddSingleton<ConsumerCommand, ConsumerCommand>();
        services.AddSingleton<SimpleProducer>((IServiceProvider provider) => new SimpleProducer(provider.GetService<RedisClient>() ?? throw new Exception("bla"), DotEnv.GetEnv("PRODUCER_CHANNEL")));
        ServiceProvider provider = services.BuildServiceProvider();

        return provider;
    }

    static async Task Main(string[] args)
    {
        DotEnv.Load(".env");
        string message = "";
        string command = "Producer";
        ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(options =>
                   {
                       message = options.Message;
                       command = options.Command;
                   });

        Dictionary<string, Type> commandMap = new();
        commandMap["Consumer"] = typeof(ConsumerCommand);
        commandMap["Producer"] = typeof(ProducerCommand);

        Type type = commandMap[command] ?? throw new Exception("invalid command");
        // Create an instance of that type
        Object Command = Activator.CreateInstance(type);

        if (Command is IStrategy)
        {
            CommandContext Context = new CommandContext(Command);

            var a = Context.RunCommand();
        }



        // SimpleProducer Producer = provider.GetRequiredService<SimpleProducer>();

        // await Producer.Publish(message);
    }
}