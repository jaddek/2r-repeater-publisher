namespace Producer
{
    public record Command()
    {
        public required string Type { get; init; }

        public required string Strategy { get; init; }
    }

    public record Request()
    {
        public string? Query { get; init; }
        public string? Headers { get; init; }
        public string? Body { get; init; }
        public required string Method { get; init; }
        public required string Host { get; init; }
        public required string Path { get; init; }
    }

    public record Message()
    {
        public required Command Command { get; init; }

        public required Dictionary<string, string> Data { get; init; }

        public required Request Request { get; init; }

        public required string Timestamp { get; init; }

        public required string Resource { get; init; }
    }
}