using Producer;

DotEnv.Load(".env.local");

CLIApplication cli = CLIApplicationDirector.GetProducerApp(DotEnv.GetEnv("REDIS_DSN"));

cli.Run(args);