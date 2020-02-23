namespace OrderProcessor
{
    public class RabbitMqOptions
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string InputQueue { get; set; }
        public string WarehouseQueue { get; set; }
        public string DispatcherQueue { get; set; }
        public string CashierQueue { get; set; }
    }
}