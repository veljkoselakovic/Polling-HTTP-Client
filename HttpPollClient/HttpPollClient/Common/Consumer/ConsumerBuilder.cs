namespace HttpPollClient.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HttpPollClient.Common.Metrics;
    using HttpPollClient.Extension;

    public class ConsumerBuilder<TMessage>
    {
        private IMessageBroker<TMessage>? _messageBroker;
        private readonly List<Func<TMessage, Task<TMessage>>> _messageProcessorPipeline = [];

        private SemaphoreSlim _maxConcurrentMessageLimiter = new(1);
        private uint _concurrencyLevel;

        private IMetricsTracker? _metricsTracker;

        private Func<IMessageBroker<TMessage>?, List<Func<TMessage, Task<TMessage>>>, SemaphoreSlim, uint, IMetricsTracker?, Consumer<TMessage>>? _factory;

        public ConsumerBuilder<TMessage> SetMessageBroker(IMessageBroker<TMessage> messageBroker)
        {
            _messageBroker = messageBroker;
            return this;
        }

        public ConsumerBuilder<TMessage> SetMaxConcurrentTasks(int maxValue)
        {
            _maxConcurrentMessageLimiter = new SemaphoreSlim(maxValue);
            return this;
        }

        public ConsumerBuilder<TMessage> SetConcurrencyLevel(uint concurrencyLevel)
        {
            _concurrencyLevel = concurrencyLevel;
            return this;
        }

        public ConsumerBuilder<TMessage> SetMetricsTracker(IMetricsTracker metricsTracker)
        {
            _metricsTracker = metricsTracker;
            return this;
        }

        public ConsumerBuilder<TMessage> AddMessageProcessorToPipeline(Func<TMessage, Task<TMessage>> messageProcessor)
        {
            _messageProcessorPipeline.Add(messageProcessor);
            return this;
        }

        public ConsumerBuilder<TMessage> SetFactory(Func<IMessageBroker<TMessage>?, List<Func<TMessage, Task<TMessage>>>, SemaphoreSlim, uint, IMetricsTracker?, Consumer<TMessage>> factory)
        {
            _factory = factory;
            return this;
        }

        public Consumer<TMessage> Build()
        {
            if (_factory != null)
            {
                return _factory(_messageBroker, _messageProcessorPipeline, _maxConcurrentMessageLimiter, _concurrencyLevel, _metricsTracker);
            }

            return new Consumer<TMessage>(
                _messageBroker,
                _messageProcessorPipeline,
                _maxConcurrentMessageLimiter,
                _concurrencyLevel,
                _metricsTracker
            );
        }

    }
}
