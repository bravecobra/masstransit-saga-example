namespace Warehouse
{
    public class RabbitMqOptions
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string InputQueue { get; set; }
    }
}