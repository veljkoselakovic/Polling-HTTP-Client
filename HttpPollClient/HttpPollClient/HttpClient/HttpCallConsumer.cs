namespace HttpPollClient.HttpClient
{
    using HttpPollClient.Common;
    using HttpPollClient.Common.Brokers;
    using HttpPollClient.Common.Metrics;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class HttpCallConsumer : Consumer<HttpRequestMessage>
    {
        private HttpCallConsumer(
            IMessageBroker<HttpRequestMessage>? messageBroker,
            List<Func<HttpRequestMessage, Task<HttpRequestMessage>>> messageProcessorPipeline,
            SemaphoreSlim maxConcurrentMessageLimiter,
            uint concurrencyLevel,
            IMetricsTracker? metricsTracker) : base(messageBroker, messageProcessorPipeline, maxConcurrentMessageLimiter, concurrencyLevel, metricsTracker)
        {
        }

        public static class Factory
        {
            public static HttpCallConsumer CreateDefaultWithChannelBroker()
            {
                return (HttpCallConsumer)
                    new ConsumerBuilder<HttpRequestMessage>()
                        .SetMessageBroker(ChannelBroker<HttpRequestMessage>.Instance)
                        .SetConcurrencyLevel(3)
                        .SetMaxConcurrentTasks(10)
                        .SetMetricsTracker(new DefaultMetricsTracker())
                        .AddMessageProcessorToPipeline(
                            async (httpRequestMessage) =>
                            {
                                Console.WriteLine(
                                    $"Processing request to {httpRequestMessage.RequestUri}"
                                );
                                await Task.Delay(1000); // Simulate some processing delay
                                return httpRequestMessage;
                            }
                        )
                        .SetFactory((broker, pipeline, limiter, concurrency, tracker) =>
                            new HttpCallConsumer(broker, pipeline, limiter, concurrency, tracker))
                        .Build();
            }
        }
    }
}
