namespace HttpPollClient.Common.Brokers
{
    public class DefaultMessageBroker : IMessageBroker<string>
    {
        public async Task<string> ConsumeMessageAsync()
        {
            return await Task.FromResult("Default Message");
        }

        public bool ProduceMessage(string message)
        {
            Console.WriteLine($"Producing message: {message}");
            return true;
        }
    }
}
