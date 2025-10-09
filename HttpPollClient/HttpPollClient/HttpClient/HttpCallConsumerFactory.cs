namespace HttpPollClient.HttpClient
{
    using HttpPollClient.Common.Brokers;

    public static class HttpCallConsumerFactory
    {
        public static HttpCallConsumer CreateDefaultWithChannelBroker()
        {
            return (HttpCallConsumer)
                new HttpCallConsumer()
                    .SetMessageBroker(ChannelBroker<HttpRequestMessage>.Instance)
                    .SetConcurrencyLevel(3)
                    .SetMaxConcurrentTasks(10)
                    .AddMessageProcessorToPipeline(
                        async (httpRequestMessage) =>
                        {
                            Console.WriteLine(
                                $"Processing request to {httpRequestMessage.RequestUri}"
                            );
                            await Task.Delay(1000); // Simulate some processing delay
                            return httpRequestMessage;
                        }
                    );
        }
    }
}
