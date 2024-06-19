using Microsoft.Extensions.DependencyInjection;
using CommandLine;

namespace Producer;

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
        services.AddSingleton<ConsumerCommand, ConsumerCommand>();
        services.AddSingleton<ProducerCommand, ProducerCommand>();
        services.AddSingleton<SimpleProducer>((IServiceProvider provider) => new SimpleProducer(provider.GetService<RedisClient>() ?? throw new Exception("bla"), DotEnv.GetEnv("PRODUCER_CHANNEL")));
        ServiceProvider provider = services.BuildServiceProvider();

        return provider;
    }

    static async Task Main(string[] args)
    {
        DotEnv.Load(".env");
        string message = "";
        string command = "Consumer";
        ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(options =>
                   {
                       message = options.Message;
                       command = options.Command;
                   });

        Dictionary<string, Type> commandMap = new()
        {
            ["Producer"] = typeof(ProducerCommand),
            ["Consumer"] = typeof(ConsumerCommand),

        };

        using ServiceProvider provider = InitContainer();

        IStrategy Command = (IStrategy)provider.GetRequiredService(commandMap[command]);
        CommandContext Context = new(Command);
        var result = await Context.RunCommand();

        Console.WriteLine(result);
    }
}