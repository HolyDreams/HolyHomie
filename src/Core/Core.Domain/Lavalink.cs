namespace Core.Domain
{
    public class Lavalink
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string Password { get; set; }
    }
}
