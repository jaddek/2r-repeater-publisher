
using Microsoft.Extensions.DependencyInjection;
using CommandLine;

namespace Producer;

public class Program
{
    public class Options
    {
        [Option('m', "message", Required = true, HelpText = "Message.")]
        public string? Message { get; set; }
    }
    static ServiceProvider InitContainer()
    {
        ServiceCollection services = new();
        services.AddSingleton<RedisClient>(new RedisClient(DotEnv.GetEnv("REDIS_DSN")));
        services.AddSingleton<SimpleProducer>((IServiceProvider provider) => new SimpleProducer(provider.GetService<RedisClient>() ?? throw new Exception("bla"), DotEnv.GetEnv("PRODUCER_CHANNEL")));
        ServiceProvider provider = services.BuildServiceProvider();

        return provider;
    }

    static async Task Main(string[] args)
    {
        var message = "";
        ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(options =>
                   {
                       message = options.Message;
                   });

        DotEnv.Load(".env");
        using ServiceProvider provider = InitContainer();

        SimpleProducer Producer = provider.GetRequiredService<SimpleProducer>();

        await Producer.Publish(message);
    }
}