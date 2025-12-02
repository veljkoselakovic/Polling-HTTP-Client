namespace HttpPollClient.Common.Consumer
{
    using HttpPollClient.Common.Brokers;
    using HttpPollClient.Common.Metrics;

    public class ConsumerFactory
    {
        public static Consumer<T> CreateConsumer<T>()
        {
            Consumer<T> consumer = new();
            return consumer;
        }

        public static Consumer<string> CreateConsumerWithDefaultBrokerAndSmallConcurrencyLevel()
        {
            IMessageBroker<string> messageBroker = new DefaultMessageBroker();
            Consumer<string> consumer = new Consumer<string>()
                .SetMessageBroker(messageBroker)
                .SetMaxConcurrentTasks(3)
                .SetConcurrencyLevel(5)
                .SetMetricsTracker(new DefaultMetricsTracker())
                .AddMessageProcessorToPipeline(
                    async (message) =>
                    {
                        string processedMessage = message.ToUpper();
                        Console.WriteLine($"{message} ->  {processedMessage}");
                        await Task.Delay(1000);
                        return processedMessage;
                    }
                )
                .AddMessageProcessorToPipeline(
                    async (message) =>
                    {
                        Console.WriteLine($"Processed message: {message}");
                        await Task.Delay(1000);
                        return message;
                    }
                );
            return consumer;
        }

        public static Consumer<string> CreateConsumerWithChannelrokerAndSmallConcurrencyLevel()
        {
            IMessageBroker<string> messageBroker = ChannelBroker<string>.Instance;
            Consumer<string> consumer = new Consumer<string>()
                .SetMessageBroker(messageBroker)
                .SetMaxConcurrentTasks(5)
                .SetConcurrencyLevel(5)
                .SetMetricsTracker(new DefaultMetricsTracker())
                .AddMessageProcessorToPipeline(
                    async (message) =>
                    {
                        string processedMessage = message.ToUpper();
                        Console.WriteLine($"{message} ->  {processedMessage}");
                        await Task.Delay(1000);
                        return processedMessage;
                    }
                )
                .AddMessageProcessorToPipeline(
                    async (message) =>
                    {
                        Console.WriteLine($"Processed message: {message}");
                        await Task.Delay(1000);
                        return message;
                    }
                );
            return consumer;
        }
    }
}
